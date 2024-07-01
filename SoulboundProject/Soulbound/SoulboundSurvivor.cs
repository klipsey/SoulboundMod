using BepInEx.Configuration;
using SoulboundMod.Modules;
using SoulboundMod.Modules.Characters;
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
using SoulboundMod.Soulbound.Components;
using SoulboundMod.Soulbound.Content;
using SoulboundMod.Soulbound.SkillStates;
using HG;
using EntityStates;
using R2API.Networking.Interfaces;
using EmotesAPI;
using System.Runtime.CompilerServices;

namespace SoulboundMod.Soulbound
{
    public class SoulboundSurvivor : SurvivorBase<SoulboundSurvivor>
    {
        public override string assetBundleName => "soulbound";
        public override string bodyName => "SoulboundBody";
        public override string masterName => "SoulboundMonsterMaster";
        public override string modelPrefabName => "mdlSoulbound";
        public override string displayPrefabName => "SoulboundDisplay";

        public const string SOULBOUND_PREFIX = SoulboundPlugin.DEVELOPER_PREFIX + "_SOULBOUND_";
        public override string survivorTokenPrefix => SOULBOUND_PREFIX;

        internal static GameObject characterPrefab;

        public static SkillDef convictScepterSkillDef;

        public override BodyInfo bodyInfo => new BodyInfo
        {
            bodyName = bodyName,
            bodyNameToken = SOULBOUND_PREFIX + "NAME",
            subtitleNameToken = SOULBOUND_PREFIX + "SUBTITLE",

            characterPortrait = assetBundle.LoadAsset<Texture>("texSoulboundIcon"),
            bodyColor = SoulboundAssets.soulBoundColor,
            sortPosition = 6f,

            crosshair = Modules.Assets.LoadCrosshair("Standard"),
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

        public override UnlockableDef characterUnlockableDef => SoulboundUnlockables.characterUnlockableDef;

        public override ItemDisplaysBase itemDisplays => new SoulboundItemDisplays();
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
            SoulboundConfig.Init();

            SoulboundUnlockables.Init();

            base.InitializeCharacter();

            CameraParams.InitializeParams();

            ChildLocator childLocator = bodyPrefab.GetComponentInChildren<ChildLocator>();

            DamageTypes.Init();

            SoulboundStates.Init();
            SoulboundTokens.Init();

            SoulboundAssets.Init(assetBundle);

            SoulboundBuffs.Init(assetBundle);

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
            bodyPrefab.AddComponent<SoulboundController>();
            bodyPrefab.AddComponent<SoulboundTracker>();
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
            bodyPrefab.AddComponent<SoulboundPassive>();
            Skills.CreateSkillFamilies(bodyPrefab);
            AddPassiveSkills();
            AddPrimarySkills();
            AddSecondarySkills();
            AddUtilitySkills();
            AddSpecialSkills();
            //if (SoulboundPlugin.scepterInstalled) InitializeScepter();
        }

        private void AddPassiveSkills()
        {
            SoulboundPassive passive = bodyPrefab.GetComponent<SoulboundPassive>();

            SkillLocator skillLocator = bodyPrefab.GetComponent<SkillLocator>();

            skillLocator.passiveSkill.enabled = false;

            passive.interrogatorPassive = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = SOULBOUND_PREFIX + "PASSIVE_NAME",
                skillNameToken = SOULBOUND_PREFIX + "PASSIVE_NAME",
                skillDescriptionToken = SOULBOUND_PREFIX + "PASSIVE_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texSoulboundPassive"),
                keywordTokens = new string[] { Tokens.interrogatorGuiltyKeyword },
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

            Skills.AddPassiveSkills(passive.passiveSkillSlot.skillFamily, passive.interrogatorPassive);
        }

        private void AddPrimarySkills()
        {
            SkillDef bow = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "Affray",
                skillNameToken = SOULBOUND_PREFIX + "SECONDARY_AFFRAY_NAME",
                skillDescriptionToken = SOULBOUND_PREFIX + "SECONDARY_AFFRAY_DESCRIPTION",
                keywordTokens = new string[] { Tokens.interrogatorPressuredKeyword, Tokens.slayerKeyword },
                skillIcon = assetBundle.LoadAsset<Sprite>("texSoulboundCleaverIcon"),

                activationState = new SerializableEntityStateType(typeof(MainState)),

                activationStateMachineName = "Weapon2",
                interruptPriority = InterruptPriority.Skill,

                baseMaxStock = 1,
                baseRechargeInterval = 5f,
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
                forceSprintDuringState = false,
            });
            Skills.AddPrimarySkills(bodyPrefab, bow);
        }

        private void AddSecondarySkills()
        {
            SkillDef hop = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "Affray",
                skillNameToken = SOULBOUND_PREFIX + "SECONDARY_AFFRAY_NAME",
                skillDescriptionToken = SOULBOUND_PREFIX + "SECONDARY_AFFRAY_DESCRIPTION",
                keywordTokens = new string[] { Tokens.interrogatorPressuredKeyword, Tokens.slayerKeyword },
                skillIcon = assetBundle.LoadAsset<Sprite>("texSoulboundCleaverIcon"),

                activationState = new SerializableEntityStateType(typeof(MainState)),

                activationStateMachineName = "Weapon2",
                interruptPriority = InterruptPriority.Skill,

                baseMaxStock = 1,
                baseRechargeInterval = 5f,
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
                forceSprintDuringState = false,
            });

            Skills.AddSecondarySkills(bodyPrefab, hop);
        }

        private void AddUtilitySkills()
        {
            SkillDef dashToSpirit = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "Falsify",
                skillNameToken = SOULBOUND_PREFIX + "UTILITY_FALSIFY_NAME",
                skillDescriptionToken = SOULBOUND_PREFIX + "UTILITY_FALSIFY_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texFalsifyIcon"),

                activationState = new SerializableEntityStateType(typeof(MainState)),
                activationStateMachineName = "Weapon2",
                interruptPriority = InterruptPriority.Skill,

                baseRechargeInterval = 6f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
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
            SkillDef convict = Skills.CreateSkillDef<SoulboundSkillDef>(new SkillDefInfo
            {
                skillName = "Convict",
                skillNameToken = SOULBOUND_PREFIX + "SPECIAL_CONVICT_NAME",
                skillDescriptionToken = SOULBOUND_PREFIX + "SPECIAL_CONVICT_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texConvictIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(MainState)),
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

            Skills.AddSpecialSkills(bodyPrefab, convict);
        }

        private void InitializeScepter()
        {
            convictScepterSkillDef = Skills.CreateSkillDef<SoulboundSkillDef>(new SkillDefInfo
            {
                skillName = "Convict Scepter",
                skillNameToken = SOULBOUND_PREFIX + "SPECIAL_SCEPTER_CONVICT_NAME",
                skillDescriptionToken = SOULBOUND_PREFIX + "SPECIAL_SCEPTER_CONVICT_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texConvictScepter"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(MainState)),
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

            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(convictScepterSkillDef, bodyName, SkillSlot.Special, 0);
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
                SoulboundUnlockables.masterySkinUnlockableDef);

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
            masterySkin.rendererInfos[0].defaultMaterial = SoulboundAssets.spyMonsoonMat;
            masterySkin.rendererInfos[1].defaultMaterial = SoulboundAssets.spyMonsoonMat;
            masterySkin.rendererInfos[2].defaultMaterial = SoulboundAssets.spyMonsoonMat;
            masterySkin.rendererInfos[3].defaultMaterial = SoulboundAssets.spyMonsoonMat;
            masterySkin.rendererInfos[5].defaultMaterial = SoulboundAssets.spyVisorMonsoonMat;

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
            SoulboundAI.Init(bodyPrefab, masterName);

            //how to load a master set up in unity, can be an empty gameobject with just AISkillDriver components
            //assetBundle.LoadMaster(bodyPrefab, masterName);
        }

        private void AddHooks()
        {
            //HUD.onHudTargetChangedGlobal += HUDSetup;
            On.RoR2.UI.LoadoutPanelController.Rebuild += LoadoutPanelController_Rebuild;
            On.RoR2.HealthComponent.TakeDamage += new On.RoR2.HealthComponent.hook_TakeDamage(HealthComponent_TakeDamage);
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;

            if(SoulboundPlugin.emotesInstalled) Emotes();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void Emotes()
        {
            On.RoR2.SurvivorCatalog.Init += (orig) =>
            {
                orig();
                var skele = SoulboundAssets.mainAssetBundle.LoadAsset<GameObject>("soulbound_emoteskeleton");
                CustomEmotesAPI.ImportArmature(SoulboundSurvivor.characterPrefab, skele);
            };
        }


        private static void LoadoutPanelController_Rebuild(On.RoR2.UI.LoadoutPanelController.orig_Rebuild orig, LoadoutPanelController self)
        {
            orig(self);

            if (self.currentDisplayData.bodyIndex == BodyCatalog.FindBodyIndex("SoulboundBody"))
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
                if(self.baseNameToken == "KENKO_SOULBOUND_NAME")
                {
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
                   
                }
            }

            orig.Invoke(self, damageInfo);
        }
        private static void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            CharacterBody attackerBody = damageReport.attackerBody;
            if (attackerBody && damageReport.attackerMaster && damageReport.victim)
            {
               
            }
        }
        internal static void HUDSetup(HUD hud)
        {
            /*
            if (hud.targetBodyObject && hud.targetMaster && hud.targetMaster.bodyPrefab == SoulboundSurvivor.characterPrefab)
            {
                if (!hud.targetMaster.hasAuthority) return;
                Transform skillsContainer = hud.equipmentIcons[0].gameObject.transform.parent;

                // ammo display for atomic
                Transform healthbarContainer = hud.transform.Find("MainContainer").Find("MainUIArea").Find("SpringCanvas").Find("BottomLeftCluster").Find("BarRoots").Find("LevelDisplayCluster");

                GameObject stealthTracker = GameObject.Instantiate(healthbarContainer.gameObject, hud.transform.Find("MainContainer").Find("MainUIArea").Find("SpringCanvas").Find("BottomLeftCluster"));
                stealthTracker.name = "AmmoTracker";
                stealthTracker.transform.SetParent(hud.transform.Find("MainContainer").Find("MainUIArea").Find("CrosshairCanvas").Find("CrosshairExtras"));

                GameObject.DestroyImmediate(stealthTracker.transform.GetChild(0).gameObject);
                MonoBehaviour.Destroy(stealthTracker.GetComponentInChildren<LevelText>());
                MonoBehaviour.Destroy(stealthTracker.GetComponentInChildren<ExpBar>());

                stealthTracker.transform.Find("LevelDisplayRoot").Find("ValueText").gameObject.SetActive(false);
                GameObject.DestroyImmediate(stealthTracker.transform.Find("ExpBarRoot").gameObject);

                stealthTracker.transform.Find("LevelDisplayRoot").GetComponent<RectTransform>().anchoredPosition = new Vector2(-12f, 0f);

                RectTransform rect = stealthTracker.GetComponent<RectTransform>();
                rect.localScale = new Vector3(0.8f, 0.8f, 1f);
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 0f);
                rect.offsetMin = new Vector2(120f, -40f);
                rect.offsetMax = new Vector2(120f, -40f);
                rect.pivot = new Vector2(0.5f, 0f);
                //positional data doesnt get sent to clients? Manually making offsets works..
                rect.anchoredPosition = new Vector2(50f, 0f);
                rect.localPosition = new Vector3(120f, -40f, 0f);

                GameObject chargeBarAmmo = GameObject.Instantiate(SoulboundAssets.mainAssetBundle.LoadAsset<GameObject>("WeaponChargeBar"));
                chargeBarAmmo.name = "StealthMeter";
                chargeBarAmmo.transform.SetParent(hud.transform.Find("MainContainer").Find("MainUIArea").Find("CrosshairCanvas").Find("CrosshairExtras"));

                rect = chargeBarAmmo.GetComponent<RectTransform>();

                rect.localScale = new Vector3(0.75f, 0.1f, 1f);
                rect.anchorMin = new Vector2(100f, 2f);
                rect.anchorMax = new Vector2(100f, 2f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(100f, 2f);
                rect.localPosition = new Vector3(100f, 2f, 0f);
                rect.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));

                ConvictHudController stealthComponent = stealthTracker.AddComponent<ConvictHudController>();

                stealthComponent.targetHUD = hud;
                stealthComponent.targetText = stealthTracker.transform.Find("LevelDisplayRoot").Find("PrefixText").gameObject.GetComponent<LanguageTextMeshController>();
                stealthComponent.durationDisplay = chargeBarAmmo;
                stealthComponent.durationBar = chargeBarAmmo.transform.GetChild(1).gameObject.GetComponent<UnityEngine.UI.Image>();
                stealthComponent.durationBarColor = chargeBarAmmo.transform.GetChild(0).gameObject.GetComponent<UnityEngine.UI.Image>();
            }
            */
        }
    }
}