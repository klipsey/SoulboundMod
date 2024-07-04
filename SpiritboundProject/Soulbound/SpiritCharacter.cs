using BepInEx.Configuration;
using SpiritboundMod.Modules;
using SpiritboundMod.Modules.Characters;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using UnityEngine;
using R2API;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using SpiritboundMod.Spiritbound.Components;
using SpiritboundMod.Spiritbound.Content;
using EntityStates;
using RoR2.CharacterAI;
using EntityStates.LemurianMonster;
using SpiritboundMod.Spirit.Components;
using SpiritboundMod.Spirit.SkillStates;


namespace SpiritboundMod.Spiritbound
{
    public class SpiritCharacter : CharacterBase<SpiritCharacter>
    {
        public override string assetBundleName => "spiritbound";
        public override string bodyName => "SpiritBody";
        public override string modelPrefabName => "mdlSpirit";

        public const string SPIRIT_PREFIX = SpiritboundPlugin.DEVELOPER_PREFIX + "_SPIRIT_";

        internal static GameObject characterPrefab;

        public override BodyInfo bodyInfo => new BodyInfo
        {
            bodyNameToClone = "FlameDrone",
            bodyName = bodyName,
            bodyNameToken = SPIRIT_PREFIX + "NAME",
            crosshair = Modules.Assets.LoadCrosshair("Standard"),
            subtitleNameToken = SPIRIT_PREFIX + "SUBTITLE",
            characterPortrait = assetBundle.LoadAsset<Texture>("texWolfIcon"),
            maxHealth = 100f,
            healthGrowth = 100f * 0.3f,
            healthRegen = 1f,
            armor = 0f,
            moveSpeed = 10f,
            acceleration = 150f,
        };

        public override CustomRendererInfo[] customRendererInfos => new CustomRendererInfo[]
        {
                new CustomRendererInfo
                {
                    childName = "Model",
                    material = SpiritboundAssets.spiritBodyMat
                },
                new CustomRendererInfo
                {
                    childName = "FurModel",
                    material = SpiritboundAssets.spiritFurMat
                },
                new CustomRendererInfo
                {
                    childName = "MaskModel",
                    material = SpiritboundAssets.spiritBodyMat
                },
        };
        public override ItemDisplaysBase itemDisplays => new SpiritItemDisplays();
        public override AssetBundle assetBundle { get; protected set; }
        public override GameObject bodyPrefab { get; protected set; }
        public override CharacterBody prefabCharacterBody { get; protected set; }
        public override GameObject characterModelObject { get; protected set; }
        public override CharacterModel prefabCharacterModel { get; protected set; }

        internal static List<string> attackDrivers = new List<string>();
        internal static List<string> spiritItemBlackList = new List<string>()
        {
            "FreeChest",
            "TreasureCache",
            "TreasureCacheVoid",
            "AutoCastEquipment",
            "ExtraLife",
            "ExtraLifeVoid",
            "DroneWeapons"
        };
        public override void InitializeCharacter()
        {
            InitializeCharacterBodyPrefab();

            InitializeCharacterMaster();

            InitializeEntityStateMachines();

            InitializeSkills();

            AddHitboxes();

            InitializeSkins();

            AdditionalBodySetup();

            characterPrefab = bodyPrefab;
        }

        private void AdditionalBodySetup()
        {
            bodyPrefab.AddComponent<SpiritController>();
        }
        public void AddHitboxes()
        {
            Prefabs.SetupHitBoxGroup(characterModelObject, "MeleeHitbox", "MeleeHitbox");
        }

        public override void InitializeEntityStateMachines()
        {
            Prefabs.ClearEntityStateMachines(bodyPrefab);

            Prefabs.AddMainEntityStateMachine(bodyPrefab, "Body", typeof(SpiritMainState), typeof(EntityStates.SpawnTeleporterState));

            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon");
            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon2");

            foreach (var i in bodyPrefab.GetComponents<AkEvent>())
            {
                UnityEngine.Object.DestroyImmediate(i);
            }
        }
        public override void InitializeCharacterMaster()
        {
            var masterPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/FlameDroneMaster.prefab").WaitForCompletion(), "SpiritMaster");

            var newMaster = masterPrefab.GetComponent<CharacterMaster>();
            newMaster.bodyPrefab = this.bodyPrefab;

            var baseAI = masterPrefab.GetComponent<RoR2.CharacterAI.BaseAI>();
            baseAI.aimVectorMaxSpeed = 5000f;
            baseAI.aimVectorDampTime = 0.01f;
            //baseAI.enemyAttentionDuration = float.PositiveInfinity;

            //SpiritController spiritController = bodyPrefab.GetComponent<SpiritController>();
            //spiritController.followDrivers = new List<string>();
            //spiritController.attackDrivers = new List<string>();

            AddSkillDrivers(masterPrefab);

            Modules.Content.AddMasterPrefab(masterPrefab);

            SpiritMasterComponent.summonPrefab = masterPrefab;
        }

        private void AddSkillDrivers(GameObject masterPrefab)
        {
            #region AI
            foreach (AISkillDriver i in masterPrefab.GetComponentsInChildren<AISkillDriver>())
            {
                UnityEngine.Object.DestroyImmediate(i);
            }
            AISkillDriver biteDriver = masterPrefab.AddComponent<AISkillDriver>();
            biteDriver.customName = "BiteOffNodeGraph";
            biteDriver.skillSlot = SkillSlot.Primary;
            biteDriver.requireSkillReady = true;
            biteDriver.minUserHealthFraction = float.NegativeInfinity;
            biteDriver.maxUserHealthFraction = float.PositiveInfinity;
            biteDriver.minTargetHealthFraction = float.NegativeInfinity;
            biteDriver.maxTargetHealthFraction = float.PositiveInfinity;
            biteDriver.minDistance = 0f;
            biteDriver.maxDistance = 5f;
            biteDriver.activationRequiresAimConfirmation = false;
            biteDriver.activationRequiresTargetLoS = false;
            biteDriver.selectionRequiresTargetLoS = true;
            biteDriver.maxTimesSelected = -1;
            biteDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            biteDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            biteDriver.aimType = AISkillDriver.AimType.AtMoveTarget;
            biteDriver.moveInputScale = 1f;
            biteDriver.ignoreNodeGraph = true;
            biteDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            if (!attackDrivers.Contains(biteDriver.customName)) attackDrivers.Add(biteDriver.customName);

            AISkillDriver fireWisp = masterPrefab.AddComponent<AISkillDriver>();
            fireWisp.customName = "FireWisps";
            fireWisp.skillSlot = SkillSlot.Secondary;
            fireWisp.requireSkillReady = true;
            fireWisp.minUserHealthFraction = float.NegativeInfinity;
            fireWisp.maxUserHealthFraction = float.PositiveInfinity;
            fireWisp.minTargetHealthFraction = float.NegativeInfinity;
            fireWisp.maxTargetHealthFraction = float.PositiveInfinity;
            fireWisp.minDistance = 0f;
            fireWisp.maxDistance = 20f;
            fireWisp.activationRequiresAimConfirmation = true;
            fireWisp.activationRequiresTargetLoS = true;
            fireWisp.selectionRequiresTargetLoS = true;
            fireWisp.maxTimesSelected = -1;
            fireWisp.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            fireWisp.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            fireWisp.aimType = AISkillDriver.AimType.AtMoveTarget;
            fireWisp.moveInputScale = 1f;
            fireWisp.ignoreNodeGraph = false;
            fireWisp.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            if (!attackDrivers.Contains(fireWisp.customName)) attackDrivers.Add(fireWisp.customName);

            AISkillDriver strafeDriver = masterPrefab.AddComponent<AISkillDriver>();
            strafeDriver.customName = "StrafeNearTarget";
            strafeDriver.skillSlot = SkillSlot.None;
            strafeDriver.requireSkillReady = true;
            strafeDriver.minUserHealthFraction = float.NegativeInfinity;
            strafeDriver.maxUserHealthFraction = float.PositiveInfinity;
            strafeDriver.minTargetHealthFraction = float.NegativeInfinity;
            strafeDriver.maxTargetHealthFraction = float.PositiveInfinity;
            strafeDriver.minDistance = 0f;
            strafeDriver.maxDistance = 5f;
            strafeDriver.activationRequiresAimConfirmation = false;
            strafeDriver.activationRequiresTargetLoS = false;
            strafeDriver.selectionRequiresTargetLoS = true;
            strafeDriver.maxTimesSelected = -1;
            strafeDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            strafeDriver.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            strafeDriver.aimType = AISkillDriver.AimType.AtMoveTarget;
            strafeDriver.moveInputScale = 1f;
            strafeDriver.ignoreNodeGraph = true;
            strafeDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            if (!attackDrivers.Contains(strafeDriver.customName)) attackDrivers.Add(strafeDriver.customName);

            AISkillDriver chaseNearDriver = masterPrefab.AddComponent<AISkillDriver>();
            chaseNearDriver.customName = "ChaseTargetClose";
            chaseNearDriver.skillSlot = SkillSlot.None;
            chaseNearDriver.requireSkillReady = true;
            chaseNearDriver.minUserHealthFraction = float.NegativeInfinity;
            chaseNearDriver.maxUserHealthFraction = float.PositiveInfinity;
            chaseNearDriver.minTargetHealthFraction = float.NegativeInfinity;
            chaseNearDriver.maxTargetHealthFraction = float.PositiveInfinity;
            chaseNearDriver.minDistance = 0f;
            chaseNearDriver.maxDistance = 10f;
            chaseNearDriver.activationRequiresAimConfirmation = false;
            chaseNearDriver.activationRequiresTargetLoS = false;
            chaseNearDriver.selectionRequiresTargetLoS = true;
            chaseNearDriver.maxTimesSelected = -1;
            chaseNearDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            chaseNearDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            chaseNearDriver.aimType = AISkillDriver.AimType.AtMoveTarget;
            chaseNearDriver.moveInputScale = 1f;
            chaseNearDriver.ignoreNodeGraph = true;
            chaseNearDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            if (!attackDrivers.Contains(chaseNearDriver.customName)) attackDrivers.Add(chaseNearDriver.customName);

            AISkillDriver chaseFarDriver = masterPrefab.AddComponent<AISkillDriver>();
            chaseFarDriver.customName = "ChaseFromAfar";
            chaseFarDriver.skillSlot = SkillSlot.None;
            chaseFarDriver.requireSkillReady = true;
            chaseFarDriver.minUserHealthFraction = float.NegativeInfinity;
            chaseFarDriver.maxUserHealthFraction = float.PositiveInfinity;
            chaseFarDriver.minTargetHealthFraction = float.NegativeInfinity;
            chaseFarDriver.maxTargetHealthFraction = float.PositiveInfinity;
            chaseFarDriver.minDistance = 0f;
            chaseFarDriver.maxDistance = float.PositiveInfinity;
            chaseFarDriver.activationRequiresAimConfirmation = false;
            chaseFarDriver.activationRequiresTargetLoS = false;
            chaseFarDriver.selectionRequiresTargetLoS = false;
            chaseFarDriver.maxTimesSelected = -1;
            chaseFarDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            chaseFarDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            chaseFarDriver.aimType = AISkillDriver.AimType.AtMoveTarget;
            chaseFarDriver.moveInputScale = 1f;
            chaseFarDriver.ignoreNodeGraph = false;
            chaseFarDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            chaseFarDriver.shouldSprint = true;
            if (!attackDrivers.Contains(chaseFarDriver.customName)) attackDrivers.Add(chaseFarDriver.customName);

            AISkillDriver idleDriver = masterPrefab.AddComponent<AISkillDriver>();
            idleDriver.customName = "DoNothing";
            idleDriver.skillSlot = SkillSlot.None;
            idleDriver.requireSkillReady = true;
            idleDriver.minUserHealthFraction = float.NegativeInfinity;
            idleDriver.maxUserHealthFraction = float.PositiveInfinity;
            idleDriver.minTargetHealthFraction = float.NegativeInfinity;
            idleDriver.maxTargetHealthFraction = float.PositiveInfinity;
            idleDriver.minDistance = 0f;
            idleDriver.maxDistance = float.PositiveInfinity;
            idleDriver.activationRequiresAimConfirmation = false;
            idleDriver.activationRequiresTargetLoS = false;
            idleDriver.selectionRequiresTargetLoS = false;
            idleDriver.maxTimesSelected = -1;
            idleDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            idleDriver.movementType = AISkillDriver.MovementType.Stop;
            idleDriver.aimType = AISkillDriver.AimType.None;
            idleDriver.moveInputScale = 1f;
            idleDriver.ignoreNodeGraph = false;
            idleDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            if (!attackDrivers.Contains(idleDriver.customName)) attackDrivers.Add(idleDriver.customName);

            AISkillDriver hardLeash = masterPrefab.AddComponent<AISkillDriver>();
            hardLeash.customName = "HardLeashToLeader";
            hardLeash.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            hardLeash.moveTargetType = AISkillDriver.TargetType.CurrentLeader;
            hardLeash.activationRequiresAimConfirmation = false;
            hardLeash.activationRequiresTargetLoS = false;
            hardLeash.selectionRequiresTargetLoS = false;
            hardLeash.maxTimesSelected = -1;
            hardLeash.maxDistance = float.PositiveInfinity;
            hardLeash.minDistance = 120f;
            hardLeash.requireSkillReady = false;
            hardLeash.aimType = AISkillDriver.AimType.AtCurrentLeader;
            hardLeash.ignoreNodeGraph = false;
            hardLeash.moveInputScale = 1f;
            hardLeash.driverUpdateTimerOverride = -1f;
            hardLeash.shouldSprint = true;
            hardLeash.shouldFireEquipment = false;
            hardLeash.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            hardLeash.minTargetHealthFraction = Mathf.NegativeInfinity;
            hardLeash.maxTargetHealthFraction = Mathf.Infinity;
            hardLeash.minUserHealthFraction = float.NegativeInfinity;
            hardLeash.maxUserHealthFraction = float.PositiveInfinity;
            hardLeash.skillSlot = SkillSlot.None;

            AISkillDriver softLeash = masterPrefab.AddComponent<AISkillDriver>();
            softLeash.customName = "SoftLeashToLeader";
            softLeash.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            softLeash.moveTargetType = AISkillDriver.TargetType.CurrentLeader;
            softLeash.activationRequiresAimConfirmation = false;
            softLeash.activationRequiresTargetLoS = false;
            softLeash.selectionRequiresTargetLoS = false;
            softLeash.maxTimesSelected = -1;
            softLeash.maxDistance = float.PositiveInfinity;
            softLeash.minDistance = 10f;
            softLeash.requireSkillReady = false;
            softLeash.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            softLeash.ignoreNodeGraph = false;
            softLeash.moveInputScale = 1f;
            softLeash.driverUpdateTimerOverride = -1f;
            softLeash.shouldSprint = false;
            softLeash.shouldFireEquipment = false;
            softLeash.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            softLeash.minTargetHealthFraction = Mathf.NegativeInfinity;
            softLeash.maxTargetHealthFraction = Mathf.Infinity;
            softLeash.minUserHealthFraction = float.NegativeInfinity;
            softLeash.maxUserHealthFraction = float.PositiveInfinity;
            softLeash.skillSlot = SkillSlot.None;

            AISkillDriver idleNear = masterPrefab.AddComponent<AISkillDriver>();
            idleNear.customName = "IdleNearLeader";
            idleNear.movementType = AISkillDriver.MovementType.Stop;
            idleNear.moveTargetType = AISkillDriver.TargetType.CurrentLeader;
            idleNear.activationRequiresAimConfirmation = false;
            idleNear.activationRequiresTargetLoS = false;
            idleNear.selectionRequiresTargetLoS = false;
            idleNear.maxTimesSelected = -1;
            idleNear.maxDistance = float.PositiveInfinity;
            idleNear.minDistance = 0f;
            idleNear.requireSkillReady = false;
            idleNear.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            idleNear.ignoreNodeGraph = false;
            idleNear.moveInputScale = 1f;
            idleNear.driverUpdateTimerOverride = -1f;
            idleNear.shouldSprint = false;
            idleNear.shouldFireEquipment = false;
            idleNear.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            idleNear.minTargetHealthFraction = Mathf.NegativeInfinity;
            idleNear.maxTargetHealthFraction = Mathf.Infinity;
            idleNear.minUserHealthFraction = float.NegativeInfinity;
            idleNear.maxUserHealthFraction = float.PositiveInfinity;
            idleNear.skillSlot = SkillSlot.None;

            #endregion
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
                skillName = SPIRIT_PREFIX + "PASSIVE_NAME",
                skillNameToken = SPIRIT_PREFIX + "PASSIVE_NAME",
                skillDescriptionToken = SPIRIT_PREFIX + "PASSIVE_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texDefaultSkin"),
                keywordTokens = new string[] { },
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
            SteppedSkillDef bite = Skills.CreateSkillDef<SteppedSkillDef>(new SkillDefInfo
                (
                    "Bite",
                    SPIRIT_PREFIX + "PRIMARY_BITE_NAME",
                    SPIRIT_PREFIX + "PRIMARY_BITE_DESCRIPTION",
                    assetBundle.LoadAsset<Sprite>("texSpiritBiteIcon"),
                    new SerializableEntityStateType(typeof(SpiritBite)),
                    "Weapon"
                ));
            bite.stepCount = 1;
            bite.stepGraceDuration = 0.1f;
            Skills.AddPrimarySkills(bodyPrefab, bite);
        }

        private void AddSecondarySkills()
        {
            SkillDef fireSpirit = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "Fire Orb",
                skillNameToken = SPIRIT_PREFIX + "SECONDARY_ORB_NAME",
                skillDescriptionToken = SPIRIT_PREFIX + "SECONDARY_ORB_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texSpiritOrbIcon"),

                activationState = new SerializableEntityStateType(typeof(SpiritBarrage)),

                activationStateMachineName = "Weapon2",
                interruptPriority = InterruptPriority.PrioritySkill,

                baseMaxStock = 1,
                baseRechargeInterval = 3f,
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
                forceSprintDuringState = false
            });

            Skills.AddSecondarySkills(bodyPrefab, fireSpirit);
        }

        private void AddUtilitySkills()
        {
            SkillDef idle = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "Idle",
                skillNameToken = SPIRIT_PREFIX + "IDLE_NAME",
                skillDescriptionToken = SPIRIT_PREFIX + "IDLE_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texFalsifyIcon"),

                activationState = new SerializableEntityStateType(typeof(SpiritMainState)),
                activationStateMachineName = "Weapon",
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

            Skills.AddUtilitySkills(bodyPrefab, idle);
        }

        private void AddSpecialSkills()
        {
            SkillDef idle2 = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "Idle",
                skillNameToken = SPIRIT_PREFIX + "IDLE_NAME",
                skillDescriptionToken = SPIRIT_PREFIX + "IDLE_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texFalsifyIcon"),

                activationState = new SerializableEntityStateType(typeof(SpiritMainState)),
                activationStateMachineName = "Weapon",
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

            Skills.AddSpecialSkills(bodyPrefab, idle2);
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
                "meshSpiritBody",
                "meshFur",
                "meshSpiritMask");

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
    }
}