using BepInEx.Configuration;
using SpiritboundMod.Modules;
using SpiritboundMod.Modules.Characters;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using RoR2.UI;
using R2API;
using R2API.Networking;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using SpiritboundMod.Spiritbound.Components;
using SpiritboundMod.Spiritbound.Content;
using SpiritboundMod.Spiritbound.SkillStates;
using HG;
using EntityStates;
using R2API.Networking.Interfaces;
using EmotesAPI;
using System.Runtime.CompilerServices;
using SpiritboundMod.Spirit.Components;

namespace SpiritboundMod.Spiritbound
{
    public class SpiritboundSurvivor : SurvivorBase<SpiritboundSurvivor>
    {
        public override string assetBundleName => "spiritbound";
        public override string bodyName => "SpiritboundBody";
        public override string masterName => "SpiritboundMonsterMaster";
        public override string modelPrefabName => "mdlSpiritbound";
        public override string displayPrefabName => "SpiritboundDisplay";

        public const string SOULBOUND_PREFIX = SpiritboundPlugin.DEVELOPER_PREFIX + "_SPIRITBOUND_";
        public override string survivorTokenPrefix => SOULBOUND_PREFIX;

        internal static GameObject characterPrefab;

        public static SkillDef convictScepterSkillDef;

        public override BodyInfo bodyInfo => new BodyInfo
        {
            bodyName = bodyName,
            bodyNameToken = SOULBOUND_PREFIX + "NAME",
            subtitleNameToken = SOULBOUND_PREFIX + "SUBTITLE",

            characterPortrait = assetBundle.LoadAsset<Texture>("texSpiritboundIcon"),
            bodyColor = SpiritboundAssets.spiritBoundColor,
            sortPosition = 6f,

            spreadBloomDecayTime = 0.7f,
            spreadBloomCurve = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainBody.prefab").WaitForCompletion().gameObject.GetComponent<CharacterBody>().spreadBloomCurve,
            crosshair = SpiritboundAssets.spiritboundCrosshair,
            podPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod"),

            maxHealth = 110f,
            damage = 12f,

            jumpCount = 1,
        };

        public override CustomRendererInfo[] customRendererInfos => new CustomRendererInfo[]
        {
                new CustomRendererInfo
                {
                    childName = "Model",
                },
                new CustomRendererInfo
                {
                    childName = "HairModel",
                },
                new CustomRendererInfo
                {
                    childName = "HornModel",
                },
                new CustomRendererInfo
                {
                    childName = "MaskModel",
                },
                new CustomRendererInfo
                {
                    childName = "TrinketModel",
                },
                new CustomRendererInfo
                {
                    childName = "PackModel",
                },
                new CustomRendererInfo
                {
                    childName = "BowModel",
                }
        };

        public override UnlockableDef characterUnlockableDef => SpiritboundUnlockables.characterUnlockableDef;

        public override ItemDisplaysBase itemDisplays => new SpiritboundItemDisplays();
        public override AssetBundle assetBundle { get; protected set; }
        public override GameObject bodyPrefab { get; protected set; }
        public override CharacterBody prefabCharacterBody { get; protected set; }
        public override GameObject characterModelObject { get; protected set; }
        public override CharacterModel prefabCharacterModel { get; protected set; }
        public override GameObject displayPrefab { get; protected set; }
        public override void Initialize()
        {

            //uncomment if you have multiple characters
            //ConfigEntry<bool> characterEnabled = Config.CharacterEnableConfig("Survivors", "Henry");

            //if (!characterEnabled.Value)
            //    return;

            //need the character unlockable before you initialize the survivordef
            base.Initialize();
        }

        public override void InitializeCharacter()
        {
            SpiritboundAssets.Init(assetBundle);

            SpiritboundConfig.Init();

            SpiritboundUnlockables.Init();

            base.InitializeCharacter();

            CameraParams.InitializeParams();

            DamageTypes.Init();

            SpiritboundStates.Init();
            SpiritboundTokens.Init();

            SpiritboundBuffs.Init(assetBundle);

            InitializeEntityStateMachines();
            InitializeSkills();
            InitializeSkins();
            InitializeCharacterMaster();

            AdditionalBodySetup();

            characterPrefab = bodyPrefab;

            AddHooks();
        }

        private void AdditionalBodySetup()
        {
            AddHitboxes();
            bodyPrefab.AddComponent<SpiritboundController>();
            bodyPrefab.AddComponent<SpiritMasterComponent>();
        }
        public void AddHitboxes()
        {
        }

        public override void InitializeEntityStateMachines()
        {
            //clear existing state machines from your cloned body (probably commando)
            //omit all this if you want to just keep theirs
            Prefabs.ClearEntityStateMachines(bodyPrefab);

            //the main "Body" state machine has some special properties
            Prefabs.AddMainEntityStateMachine(bodyPrefab, "Body", typeof(SkillStates.MainState), typeof(EntityStates.SpawnTeleporterState));
            //if you set up a custom main characterstate, set it up here
            //don't forget to register custom entitystates in your HenryStates.cs

            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon");
            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon2");
            Prefabs.AddEntityStateMachine(bodyPrefab, "Dash");
        }

        #region skills
        public override void InitializeSkills()
        {
            bodyPrefab.AddComponent<SpiritboundPassive>();
            Skills.CreateSkillFamilies(bodyPrefab);
            AddPassiveSkills();
            AddPrimarySkills();
            AddSecondarySkills();
            AddUtilitySkills();
            AddSpecialSkills();
            //if (SpiritboundPlugin.scepterInstalled) InitializeScepter();
        }

        private void AddPassiveSkills()
        {
            SpiritboundPassive passive = bodyPrefab.GetComponent<SpiritboundPassive>();

            SkillLocator skillLocator = bodyPrefab.GetComponent<SkillLocator>();

            skillLocator.passiveSkill.enabled = false;

            passive.spiritboundPassive = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = SOULBOUND_PREFIX + "PASSIVE_NAME",
                skillNameToken = SOULBOUND_PREFIX + "PASSIVE_NAME",
                skillDescriptionToken = SOULBOUND_PREFIX + "PASSIVE_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texSpiritboundPassive"),
                keywordTokens = new string[] { Tokens.spiritBoundStacksKeyword },
                activationState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Idle)),
                activationStateMachineName = "",
                baseMaxStock = 1,
                baseRechargeInterval = 0f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Any,
                resetCooldownTimerOnUse = false,
                isCombatSkill = false,
                mustKeyPress = false,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 2,
                stockToConsume = 1
            });

            Skills.AddPassiveSkills(passive.passiveSkillSlot.skillFamily, passive.spiritboundPassive);
        }

        private void AddPrimarySkills()
        {
            SkillDef bow = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "Bow",
                skillNameToken = SOULBOUND_PREFIX + "PRIMARY_BOW_NAME",
                skillDescriptionToken = SOULBOUND_PREFIX + "PRIMARY_BOW_DESCRIPTION",
                keywordTokens = new string[] { Tokens.agileKeyword, Tokens.respiteKeyword },
                skillIcon = assetBundle.LoadAsset<Sprite>("tex"),

                activationState = new SerializableEntityStateType(typeof(ChargeArrow)),

                activationStateMachineName = "Weapon",
                interruptPriority = InterruptPriority.Any,

                baseMaxStock = 1,
                baseRechargeInterval = 1f,
                rechargeStock = 0,
                requiredStock = 0,
                stockToConsume = 0,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                beginSkillCooldownOnSkillEnd = true,
                mustKeyPress = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,
            });

            Skills.AddPrimarySkills(bodyPrefab, bow);
        }

        private void AddSecondarySkills()
        {
            SkillDef hop = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "Hop",
                skillNameToken = SOULBOUND_PREFIX + "SECONDARY_HOP_NAME",
                skillDescriptionToken = SOULBOUND_PREFIX + "SECONDARY_HOP_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("tex"),

                activationState = new SerializableEntityStateType(typeof(HopFire)),

                activationStateMachineName = "Weapon",
                interruptPriority = InterruptPriority.PrioritySkill,

                baseMaxStock = 1,
                baseRechargeInterval = 7f,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                beginSkillCooldownOnSkillEnd = true,
                mustKeyPress = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = true,
            });

            Skills.AddSecondarySkills(bodyPrefab, hop);
        }

        private void AddUtilitySkills()
        {
            SkillDef dashToSpirit = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "Spirit Swap",
                skillNameToken = SOULBOUND_PREFIX + "UTILITY_SWAP_NAME",
                skillDescriptionToken = SOULBOUND_PREFIX + "UTILITY_SWAP_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("tex"),

                activationState = new SerializableEntityStateType(typeof(SwapWithSpirit)),
                activationStateMachineName = "Dash",
                interruptPriority = InterruptPriority.Skill,

                baseRechargeInterval = 8f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = false,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = true,

            });

            Skills.AddUtilitySkills(bodyPrefab, dashToSpirit);
        }

        private void AddSpecialSkills()
        {
            SkillDef redirectSpirit = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "Redirect",
                skillNameToken = SOULBOUND_PREFIX + "SPECIAL_REDIRECT_NAME",
                skillDescriptionToken = SOULBOUND_PREFIX + "SPECIAL_REDIRECT_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texConvictIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(RedirectSpirit)),
                activationStateMachineName = "Weapon2",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseRechargeInterval = 16f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,
            });

            Skills.AddSpecialSkills(bodyPrefab, redirectSpirit);
        }
        #endregion skills

        #region skins
        public override void InitializeSkins()
        {
            ModelSkinController skinController = prefabCharacterModel.gameObject.AddComponent<ModelSkinController>();
            ChildLocator childLocator = prefabCharacterModel.GetComponent<ChildLocator>();

            CharacterModel.RendererInfo[] defaultRendererinfos = prefabCharacterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            //this creates a SkinDef with all default fields
            SkinDef defaultSkin = Skins.CreateSkinDef("DEFAULT_SKIN",
                assetBundle.LoadAsset<Sprite>("texDefaultSkin"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject);

            //these are your Mesh Replacements. The order here is based on your CustomRendererInfos from earlier
            //pass in meshes as they are named in your assetbundle
            //currently not needed as with only 1 skin they will simply take the default meshes
            //uncomment this when you have another skin
            defaultSkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
                "meshBody",
                "meshHair",
                "meshHorns",
                "meshLambMask",
                "meshTrinket",
                "meshPack",
                "meshBow");

            //add new skindef to our list of skindefs. this is what we'll be passing to the SkinController
            /*
            defaultSkin.gameObjectActivations = new SkinDef.GameObjectActivation[]
            {
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChildGameObject("Tie"),
                    shouldActivate = true,
                }
            };
            */
            skins.Add(defaultSkin);
            #endregion

            //uncomment this when you have a mastery skin
            /*
            #region MasterySkin

            ////creating a new skindef as we did before
            SkinDef masterySkin = Modules.Skins.CreateSkinDef(INTERROGATOR_PREFIX + "MASTERY_SKIN_NAME",
                assetBundle.LoadAsset<Sprite>("texMonsoonSkin"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject,
                SpiritboundUnlockables.masterySkinUnlockableDef);

            ////adding the mesh replacements as above. 
            ////if you don't want to replace the mesh (for example, you only want to replace the material), pass in null so the order is preserved
            masterySkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
                "meshSpyAlt",
                "meshRevolverAlt",//no gun mesh replacement. use same gun mesh
                "meshKnifeAlt",
                "meshWatchAlt",
                null,
                "meshVisorAlt");

            ////masterySkin has a new set of RendererInfos (based on default rendererinfos)
            ////you can simply access the RendererInfos' materials and set them to the new materials for your skin.
            masterySkin.rendererInfos[0].defaultMaterial = SpiritboundAssets.spyMonsoonMat;
            masterySkin.rendererInfos[1].defaultMaterial = SpiritboundAssets.spyMonsoonMat;
            masterySkin.rendererInfos[2].defaultMaterial = SpiritboundAssets.spyMonsoonMat;
            masterySkin.rendererInfos[3].defaultMaterial = SpiritboundAssets.spyMonsoonMat;
            masterySkin.rendererInfos[5].defaultMaterial = SpiritboundAssets.spyVisorMonsoonMat;

            ////here's a barebones example of using gameobjectactivations that could probably be streamlined or rewritten entirely, truthfully, but it works
            masterySkin.gameObjectActivations = new SkinDef.GameObjectActivation[]
            {
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChildGameObject("Tie"),
                    shouldActivate = false,
                }
            };
            ////simply find an object on your child locator you want to activate/deactivate and set if you want to activate/deacitvate it with this skin

            skins.Add(masterySkin);

            #endregion
            */
            skinController.skins = skins.ToArray();
        }
        #endregion skins


        //Character Master is what governs the AI of your character when it is not controlled by a player (artifact of vengeance, goobo)
        public override void InitializeCharacterMaster()
        {
            //if you're lazy or prototyping you can simply copy the AI of a different character to be used
            //Modules.Prefabs.CloneDopplegangerMaster(bodyPrefab, masterName, "Merc");

            //how to set up AI in code
            SpiritboundAI.Init(bodyPrefab, masterName);

            //how to load a master set up in unity, can be an empty gameobject with just AISkillDriver components
            //assetBundle.LoadMaster(bodyPrefab, masterName);
        }

        private void AddHooks()
        {
            HUD.onHudTargetChangedGlobal += HUDSetup;
            On.RoR2.UI.LoadoutPanelController.Rebuild += LoadoutPanelController_Rebuild;
            On.RoR2.HealthComponent.TakeDamage += new On.RoR2.HealthComponent.hook_TakeDamage(HealthComponent_TakeDamage);
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;

            if(SpiritboundPlugin.emotesInstalled) Emotes();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void Emotes()
        {
            On.RoR2.SurvivorCatalog.Init += (orig) =>
            {
                orig();
                var skele = SpiritboundAssets.mainAssetBundle.LoadAsset<GameObject>("spiritbound_emoteskeleton");
                CustomEmotesAPI.ImportArmature(SpiritboundSurvivor.characterPrefab, skele);
            };
        }


        private static void LoadoutPanelController_Rebuild(On.RoR2.UI.LoadoutPanelController.orig_Rebuild orig, LoadoutPanelController self)
        {
            orig(self);

            if (self.currentDisplayData.bodyIndex == BodyCatalog.FindBodyIndex("SpiritboundBody"))
            {
                foreach (LanguageTextMeshController i in self.gameObject.GetComponentsInChildren<LanguageTextMeshController>())
                {
                    if (i && i.token == "LOADOUT_SKILL_MISC") i.token = "Passive";
                }
            }
        }
        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            if (self)
            {
                if(self.baseNameToken == "KENKO_SPIRITBOUND_NAME")
                {
                    if (self.HasBuff(SpiritboundBuffs.spiritMovespeedStacksBuff)) self.moveSpeed += (self.GetBuffCount(SpiritboundBuffs.spiritMovespeedStacksBuff));
                }
            }
        }
        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (NetworkServer.active && self.alive || !self.godMode || self.ospTimer <= 0f)
            {
                CharacterBody victimBody = self.body;
                CharacterBody attackerBody = null;

                if (damageInfo.attacker)
                {
                    attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                }

                if (damageInfo.damage > 0 && !damageInfo.rejected && victimBody && attackerBody)
                {
                    if (attackerBody.baseNameToken == "KENKO_SPIRIT_NAME")
                    {
                        SpiritController spiritController = attackerBody.GetComponent<SpiritController>();
                        if (spiritController && victimBody && damageInfo.HasModdedDamageType(DamageTypes.CurrentHealthSpirit))
                        {
                            damageInfo.damage += victimBody.healthComponent.health * (SpiritboundStaticValues.currentHPDamage + 
                                (spiritController.owner.GetComponent<CharacterBody>().GetBuffCount(SpiritboundBuffs.soulStacksBuff) * SpiritboundStaticValues.currentHpStacking));
                        }
                    }
                }
            }

            orig.Invoke(self, damageInfo);
        }
        private static void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            CharacterBody attackerBody = damageReport.attackerBody;
            if (NetworkServer.active && attackerBody && damageReport.victim)
            {
                if(damageReport.victim.body.isBoss || damageReport.victim.body.isChampion)
                {
                    if (attackerBody.baseNameToken == "KENKO_SPIRITBOUND_NAME")
                    {
                        StackOrb orb = new StackOrb();
                        orb.origin = damageReport.victim.transform.position;
                        orb.target = Util.FindBodyMainHurtBox(attackerBody);
                        RoR2.Orbs.OrbManager.instance.AddOrb(orb);
                    }
                    else if(attackerBody.baseNameToken == "KENKO_SPIRIT_NAME")
                    {
                        StackOrb orb = new StackOrb();
                        orb.origin = damageReport.victim.transform.position;
                        orb.target = Util.FindBodyMainHurtBox(attackerBody.GetComponent<SpiritController>().owner);
                        RoR2.Orbs.OrbManager.instance.AddOrb(orb);
                    }
                }
            }
        }
        private static void HUDSetup(HUD hud)
        {
            if (hud.targetBodyObject && hud.targetMaster && hud.targetMaster.bodyPrefab == SpiritboundSurvivor.characterPrefab)
            {
                if (!hud.targetMaster.hasAuthority) return;
                Transform skillsContainer = hud.equipmentIcons[0].gameObject.transform.parent;

                // ammo display for atomic
                Transform healthbarContainer = hud.transform.Find("MainContainer").Find("MainUIArea").Find("SpringCanvas").Find("BottomLeftCluster").Find("BarRoots").Find("LevelDisplayCluster");

                GameObject healTracker = GameObject.Instantiate(healthbarContainer.gameObject, hud.transform.Find("MainContainer").Find("MainUIArea").Find("SpringCanvas").Find("BottomLeftCluster"));
                healTracker.name = "HealthTracker";
                healTracker.transform.SetParent(hud.transform.Find("MainContainer").Find("MainUIArea").Find("CrosshairCanvas").Find("CrosshairExtras"));

                GameObject.DestroyImmediate(healTracker.transform.GetChild(0).gameObject);
                MonoBehaviour.Destroy(healTracker.GetComponentInChildren<LevelText>());
                MonoBehaviour.Destroy(healTracker.GetComponentInChildren<ExpBar>());

                healTracker.transform.Find("LevelDisplayRoot").Find("ValueText").gameObject.SetActive(false);
                GameObject.DestroyImmediate(healTracker.transform.Find("ExpBarRoot").gameObject);

                healTracker.transform.Find("LevelDisplayRoot").GetComponent<RectTransform>().anchoredPosition = new Vector2(-12f, 0f);

                RectTransform rect = healTracker.GetComponent<RectTransform>();
                rect.localScale = new Vector3(0.8f, 0.8f, 1f);
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 0f);
                rect.offsetMin = new Vector2(120f, -40f);
                rect.offsetMax = new Vector2(120f, -40f);
                rect.pivot = new Vector2(0.5f, 0f);
                //positional data doesnt get sent to clients? Manually making offsets works..
                rect.anchoredPosition = new Vector2(50f, 0f);
                rect.localPosition = new Vector3(120f, -40f, 0f);

                GameObject chargeBarAmmo = GameObject.Instantiate(SpiritboundAssets.mainAssetBundle.LoadAsset<GameObject>("WeaponChargeBar"));
                chargeBarAmmo.name = "SpiritShieldMeter";
                chargeBarAmmo.transform.SetParent(hud.transform.Find("MainContainer").Find("MainUIArea").Find("CrosshairCanvas").Find("CrosshairExtras"));

                rect = chargeBarAmmo.GetComponent<RectTransform>();

                rect.localScale = new Vector3(0.75f, 0.1f, 1f);
                rect.anchorMin = new Vector2(100f, 2f);
                rect.anchorMax = new Vector2(100f, 2f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(100f, 2f);
                rect.localPosition = new Vector3(100f, 2f, 0f);
                rect.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));

                PassiveHealHudController healHudController = healTracker.AddComponent<PassiveHealHudController>();

                healHudController.targetHUD = hud;
                healHudController.targetText = healTracker.transform.Find("LevelDisplayRoot").Find("PrefixText").gameObject.GetComponent<LanguageTextMeshController>();
                healHudController.durationDisplay = chargeBarAmmo;
                healHudController.durationBar = chargeBarAmmo.transform.GetChild(1).gameObject.GetComponent<UnityEngine.UI.Image>();
                healHudController.durationBarColor = chargeBarAmmo.transform.GetChild(0).gameObject.GetComponent<UnityEngine.UI.Image>();
            }
        }
    }
}