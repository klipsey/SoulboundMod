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
using RoR2.CharacterAI;
using EmotesAPI;
using System.Runtime.CompilerServices;
using EntityStates.LemurianMonster;
using EntityStates.GravekeeperMonster.Weapon;
using static RoR2.TeleporterInteraction;

namespace SoulboundMod.Soulbound
{
    public class SpiritCharacter : CharacterBase<SpiritCharacter>
    {
        public override string assetBundleName => "soulbound";
        public override string bodyName => "SpiritBody";
        public override string modelPrefabName => "mdlSpirit";

        public const string SPIRIT_PREFIX = SoulboundPlugin.DEVELOPER_PREFIX + "_SPIRIT_";

        internal static GameObject characterPrefab;

        public static SkillDef convictScepterSkillDef;

        public override BodyInfo bodyInfo => new BodyInfo
        {
            bodyName = bodyName,
            bodyNameToken = SPIRIT_PREFIX + "NAME",
            crosshair = Modules.Assets.LoadCrosshair("Standard"),
            subtitleNameToken = SPIRIT_PREFIX + "SUBTITLE",
            characterPortrait = assetBundle.LoadAsset<Texture>("texSpiritIcon"),
            maxHealth = 100f,
            healthGrowth = 100f * 0.3f,
            healthRegen = 1f,
            armor = 0f,
            moveSpeed = 24f,
            acceleration = 150f,
        };

        public override CustomRendererInfo[] customRendererInfos => new CustomRendererInfo[]
        {
                new CustomRendererInfo
                {
                    childName = "Model",
                    material = SoulboundAssets.spiritBodyMat
                },
                new CustomRendererInfo
                {
                    childName = "FurModel",
                    material = SoulboundAssets.spiritFurMat
                },
                new CustomRendererInfo
                {
                    childName = "MaskModel",
                    material = SoulboundAssets.spiritBodyMat
                },
        };
        public override ItemDisplaysBase itemDisplays => new SoulboundItemDisplays();
        public override AssetBundle assetBundle { get; protected set; }
        public override GameObject bodyPrefab { get; protected set; }
        public override CharacterBody prefabCharacterBody { get; protected set; }
        public override GameObject characterModelObject { get; protected set; }
        public override CharacterModel prefabCharacterModel { get; protected set; }
        internal static List<string> followDrivers = new List<string>();
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
            base.InitializeCharacter();
            AdditionalBodySetup();
            InitializeEntityStateMachines();
            characterPrefab = bodyPrefab;
        }

        private void AdditionalBodySetup()
        {
            AddHitboxes();
            //bodyPrefab.AddComponent<SpiritController>();
            InitializeCharacterMaster();
        }
        public void AddHitboxes()
        {
            Prefabs.SetupHitBoxGroup(characterModelObject, "MeleeHitbox", "MeleeHitbox");
        }

        public override void InitializeEntityStateMachines()
        {
            Prefabs.ClearEntityStateMachines(bodyPrefab);

            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon");

            foreach (var i in bodyPrefab.GetComponents<AkEvent>())
            {
                UnityEngine.Object.DestroyImmediate(i);
            }
        }
        public override void InitializeCharacterMaster()
        {
            var masterPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/Drone1Master.prefab").WaitForCompletion(), "SpiritMaster");

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
            //FalconerComponent.summonPrefab = masterPrefab;
        }

        private void AddSkillDrivers(GameObject masterPrefab)
        {
            #region AI
            foreach (AISkillDriver i in masterPrefab.GetComponentsInChildren<AISkillDriver>())
            {
                UnityEngine.Object.DestroyImmediate(i);
            }

            AISkillDriver strafeMissile = masterPrefab.AddComponent<AISkillDriver>();
            strafeMissile.customName = "ShootMissilesStrafe";
            strafeMissile.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            strafeMissile.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            strafeMissile.activationRequiresAimConfirmation = false;
            strafeMissile.activationRequiresTargetLoS = true;
            strafeMissile.selectionRequiresTargetLoS = false;
            strafeMissile.maxTimesSelected = -1;
            strafeMissile.maxDistance = 35f;
            strafeMissile.minDistance = 0f;
            strafeMissile.requireSkillReady = true;
            strafeMissile.aimType = AISkillDriver.AimType.AtMoveTarget;
            strafeMissile.ignoreNodeGraph = false;
            strafeMissile.moveInputScale = 1f;
            strafeMissile.driverUpdateTimerOverride = -1f;
            strafeMissile.shouldSprint = false;
            strafeMissile.shouldFireEquipment = false;
            strafeMissile.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            strafeMissile.minTargetHealthFraction = Mathf.NegativeInfinity;
            strafeMissile.maxTargetHealthFraction = Mathf.Infinity;
            strafeMissile.minUserHealthFraction = float.NegativeInfinity;
            strafeMissile.maxUserHealthFraction = float.PositiveInfinity;
            strafeMissile.skillSlot = SkillSlot.Secondary;
            if (!attackDrivers.Contains(strafeMissile.customName)) attackDrivers.Add(strafeMissile.customName);

            AISkillDriver chaseMissile = masterPrefab.AddComponent<AISkillDriver>();
            chaseMissile.customName = "ShootMissilesChase";
            chaseMissile.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            chaseMissile.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            chaseMissile.activationRequiresAimConfirmation = false;
            chaseMissile.activationRequiresTargetLoS = true;
            chaseMissile.selectionRequiresTargetLoS = false;
            chaseMissile.maxTimesSelected = -1;
            chaseMissile.maxDistance = 100f;
            chaseMissile.minDistance = 30f;
            chaseMissile.requireSkillReady = true;
            chaseMissile.aimType = AISkillDriver.AimType.AtMoveTarget;
            chaseMissile.ignoreNodeGraph = false;
            chaseMissile.moveInputScale = 1f;
            chaseMissile.driverUpdateTimerOverride = -1f;
            chaseMissile.shouldSprint = true;
            chaseMissile.shouldFireEquipment = false;
            chaseMissile.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            chaseMissile.minTargetHealthFraction = Mathf.NegativeInfinity;
            chaseMissile.maxTargetHealthFraction = Mathf.Infinity;
            chaseMissile.minUserHealthFraction = float.NegativeInfinity;
            chaseMissile.maxUserHealthFraction = float.PositiveInfinity;
            chaseMissile.skillSlot = SkillSlot.Secondary;
            if (!attackDrivers.Contains(chaseMissile.customName)) attackDrivers.Add(chaseMissile.customName);

            AISkillDriver strafeGun = masterPrefab.AddComponent<AISkillDriver>();
            strafeGun.customName = "ShootGunsStrafe";
            strafeGun.movementType = AISkillDriver.MovementType.StrafeMovetarget;
            strafeGun.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            strafeGun.activationRequiresAimConfirmation = true;
            strafeGun.activationRequiresTargetLoS = true;
            strafeGun.selectionRequiresTargetLoS = false;
            strafeGun.maxTimesSelected = -1;
            strafeGun.maxDistance = 35f;
            strafeGun.minDistance = 0f;
            strafeGun.requireSkillReady = true;
            strafeGun.aimType = AISkillDriver.AimType.AtMoveTarget;
            strafeGun.ignoreNodeGraph = false;
            strafeGun.moveInputScale = 1f;
            strafeGun.driverUpdateTimerOverride = -1f;
            strafeGun.shouldSprint = false;
            strafeGun.shouldFireEquipment = false;
            strafeGun.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            strafeGun.minTargetHealthFraction = Mathf.NegativeInfinity;
            strafeGun.maxTargetHealthFraction = Mathf.Infinity;
            strafeGun.minUserHealthFraction = float.NegativeInfinity;
            strafeGun.maxUserHealthFraction = float.PositiveInfinity;
            strafeGun.skillSlot = SkillSlot.Primary;
            if (!attackDrivers.Contains(strafeGun.customName)) attackDrivers.Add(strafeGun.customName);

            AISkillDriver chaseGun = masterPrefab.AddComponent<AISkillDriver>();
            chaseGun.customName = "ShootGunsChase";
            chaseGun.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            chaseGun.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            chaseGun.activationRequiresAimConfirmation = true;
            chaseGun.activationRequiresTargetLoS = true;
            chaseGun.selectionRequiresTargetLoS = false;
            chaseGun.maxTimesSelected = -1;
            chaseGun.maxDistance = 100f;
            chaseGun.minDistance = 30f;
            chaseGun.requireSkillReady = true;
            chaseGun.aimType = AISkillDriver.AimType.AtMoveTarget;
            chaseGun.ignoreNodeGraph = false;
            chaseGun.moveInputScale = 1f;
            chaseGun.driverUpdateTimerOverride = -1f;
            chaseGun.shouldSprint = true;
            chaseGun.shouldFireEquipment = false;
            chaseGun.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            chaseGun.minTargetHealthFraction = Mathf.NegativeInfinity;
            chaseGun.maxTargetHealthFraction = Mathf.Infinity;
            chaseGun.minUserHealthFraction = float.NegativeInfinity;
            chaseGun.maxUserHealthFraction = float.PositiveInfinity;
            chaseGun.skillSlot = SkillSlot.Primary;
            if (!attackDrivers.Contains(chaseGun.customName)) attackDrivers.Add(chaseGun.customName);

            AISkillDriver chaseEnemies = masterPrefab.AddComponent<AISkillDriver>();
            chaseEnemies.customName = "ChaseEnemies";
            chaseEnemies.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            chaseEnemies.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            chaseEnemies.activationRequiresAimConfirmation = false;
            chaseEnemies.activationRequiresTargetLoS = false;
            chaseEnemies.selectionRequiresTargetLoS = false;
            chaseEnemies.maxTimesSelected = -1;
            chaseEnemies.maxDistance = float.PositiveInfinity;
            chaseEnemies.minDistance = 0f;
            chaseEnemies.requireSkillReady = true;
            chaseEnemies.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            chaseEnemies.ignoreNodeGraph = false;
            chaseEnemies.moveInputScale = 1f;
            chaseEnemies.driverUpdateTimerOverride = -1f;
            chaseEnemies.shouldSprint = false;
            chaseEnemies.shouldFireEquipment = false;
            chaseEnemies.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            chaseEnemies.minTargetHealthFraction = Mathf.NegativeInfinity;
            chaseEnemies.maxTargetHealthFraction = Mathf.Infinity;
            chaseEnemies.minUserHealthFraction = float.NegativeInfinity;
            chaseEnemies.maxUserHealthFraction = float.PositiveInfinity;
            chaseEnemies.skillSlot = SkillSlot.None;
            if (!attackDrivers.Contains(chaseEnemies.customName)) attackDrivers.Add(chaseEnemies.customName);

            AISkillDriver doNothing = masterPrefab.AddComponent<AISkillDriver>();
            doNothing.customName = "DoNothing";
            doNothing.movementType = AISkillDriver.MovementType.Stop;
            doNothing.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            doNothing.activationRequiresAimConfirmation = true;
            doNothing.activationRequiresTargetLoS = true;
            doNothing.selectionRequiresTargetLoS = false;
            doNothing.maxTimesSelected = -1;
            doNothing.maxDistance = float.PositiveInfinity;
            doNothing.minDistance = 0f;
            doNothing.requireSkillReady = true;
            doNothing.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            doNothing.ignoreNodeGraph = false;
            doNothing.moveInputScale = 1f;
            doNothing.driverUpdateTimerOverride = -1f;
            doNothing.shouldSprint = false;
            doNothing.shouldFireEquipment = false;
            doNothing.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            doNothing.minTargetHealthFraction = Mathf.NegativeInfinity;
            doNothing.maxTargetHealthFraction = Mathf.Infinity;
            doNothing.minUserHealthFraction = float.NegativeInfinity;
            doNothing.maxUserHealthFraction = float.PositiveInfinity;
            doNothing.skillSlot = SkillSlot.Primary;
            if (!attackDrivers.Contains(doNothing.customName)) attackDrivers.Add(doNothing.customName);

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
            softLeash.minDistance = 20f;
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
            bodyPrefab.AddComponent<SoulboundPassive>();
            Skills.CreateSkillFamilies(bodyPrefab);
            AddPrimarySkills();
            AddSecondarySkills();
            AddUtilitySkills();
            AddSpecialSkills();
            //if (SoulboundPlugin.scepterInstalled) InitializeScepter();
        }

        private void AddPrimarySkills()
        {
            SkillDef headbutt = Skills.CreateSkillDef<SkillDef>(new SkillDefInfo
            {
                skillName = "Bite",
                skillNameToken = SPIRIT_PREFIX + "SECONDARY_AFFRAY_NAME",
                skillDescriptionToken = SPIRIT_PREFIX + "SECONDARY_AFFRAY_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texSoulboundCleaverIcon"),

                activationState = new SerializableEntityStateType(typeof(Bite)),

                activationStateMachineName = "Weapon",
                interruptPriority = InterruptPriority.Skill,

                baseMaxStock = 1,
                baseRechargeInterval = 1f,
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

            Skills.AddPrimarySkills(bodyPrefab, headbutt);
        }

        private void AddSecondarySkills()
        {
            SkillDef fireSpirit = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "SpiritOrb",
                skillNameToken = SPIRIT_PREFIX + "SECONDARY_AFFRAY_NAME",
                skillDescriptionToken = SPIRIT_PREFIX + "SECONDARY_AFFRAY_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texSoulboundCleaverIcon"),

                activationState = new SerializableEntityStateType(typeof(SpiritBarrage)),

                activationStateMachineName = "Weapon",
                interruptPriority = InterruptPriority.Skill,

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
                forceSprintDuringState = false,
            });

            Skills.AddSecondarySkills(bodyPrefab, fireSpirit);
        }

        private void AddUtilitySkills()
        {
            SkillDef dashToSpirit = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = "Idle",
                skillNameToken = SPIRIT_PREFIX + "UTILITY_FALSIFY_NAME",
                skillDescriptionToken = SPIRIT_PREFIX + "UTILITY_FALSIFY_DESCRIPTION",
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

            Skills.AddUtilitySkills(bodyPrefab, dashToSpirit);
        }

        private void AddSpecialSkills()
        {
            SkillDef convict = Skills.CreateSkillDef<SoulboundSkillDef>(new SkillDefInfo
            {
                skillName = "Idle",
                skillNameToken = SPIRIT_PREFIX + "UTILITY_FALSIFY_NAME",
                skillDescriptionToken = SPIRIT_PREFIX + "UTILITY_FALSIFY_DESCRIPTION",
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

            Skills.AddSpecialSkills(bodyPrefab, convict);
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
    }
}