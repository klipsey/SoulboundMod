using UnityEngine;
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using SpiritboundMod.Spirit.SkillStates;
using SpiritboundMod.Spiritbound.Content;
using SpiritboundMod.Spiritbound.Components;
using RoR2.Skills;
using SpiritboundMod.Spiritbound;

namespace SpiritboundMod.Spirit.Components
{
    public class SpiritController : MonoBehaviour
    {
        public static SkillDef missileSkillDef;

        public GameObject owner;

        private GameObject masterObject;
        private BaseAI baseAI;
        private AISkillDriver[] aISkillDrivers;
        public GameObject currentTarget { get { return baseAI.currentEnemy.gameObject; } }
        public HurtBox currentBestHurtbox { get { return baseAI.currentEnemy.bestHurtBox; } }

        private EntityStateMachine weaponMachine;
        private EntityStateMachine bodyMachine;
        public SkillLocator skillLocator;

        private bool attackMode = true;
        public bool inAttackMode { get { return attackMode; } }

        //private SpiritVFXComponents spiritVFX;

        public CharacterBody characterBody;

        public Highlight targetHighlight;

        public bool inFrenzy;

        private void Awake()
        {
            skillLocator = base.GetComponent<SkillLocator>();

            this.Invoke("ApplyEffects", 0.3f);
        }

        private void Start()
        {
            characterBody = base.GetComponent<CharacterBody>();

            if (owner && characterBody)
            {
                CharacterBody ownerBody = owner.GetComponent<CharacterBody>();
                if (ownerBody.skinIndex != characterBody.skinIndex)
                {
                    ApplySpiritSkin(ownerBody);
                }
            }

            masterObject = characterBody.master.gameObject;
            aISkillDrivers = masterObject.GetComponents<AISkillDriver>();
            baseAI = masterObject.GetComponent<BaseAI>();
            weaponMachine = EntityStateMachine.FindByCustomName(base.gameObject, "Weapon");
            bodyMachine = EntityStateMachine.FindByCustomName(base.gameObject, "Body");
            EnterFollowMode();
        }

        public void ApplySpiritSkin(CharacterBody ownerBody)
        {
            if (ownerBody && characterBody)
            {
                characterBody.skinIndex = ownerBody.skinIndex;

                ModelLocator modelLocator = base.GetComponent<ModelLocator>();
                if (modelLocator)
                {
                    GameObject modelObject = modelLocator.modelTransform.gameObject;
                    if (modelObject)
                    {
                        ModelSkinController MSC = modelObject.GetComponent<ModelSkinController>();
                        MSC.ApplySkin((int)characterBody.skinIndex);
                    }
                }
            }
        }

        public void ApplyEffects()
        {
            ModelLocator modelLocator = base.GetComponent<ModelLocator>();

            ChildLocator childLocator = modelLocator.modelBaseTransform.GetComponentInChildren<ChildLocator>();

            childLocator.FindChild("FireR").gameObject.GetComponent<ParticleSystemRenderer>().material = SpiritboundAssets.fireMatInFront;
            childLocator.FindChild("FireL").gameObject.GetComponent<ParticleSystemRenderer>().material = SpiritboundAssets.fireMatInFront;
        }

        public void SetTarget(HurtBox target)
        {
            HealthComponent healthComponent = target.healthComponent;
            GameObject bodyObject = healthComponent.gameObject;
            if (target && healthComponent && healthComponent.alive && bodyObject)
            {
                CreateHighlight(bodyObject);

                baseAI.currentEnemy.gameObject = bodyObject;
                baseAI.currentEnemy.bestHurtBox = target;
                baseAI.enemyAttention = 1f; //baseAI.enemyAttentionDuration;
                baseAI.BeginSkillDriver(baseAI.EvaluateSkillDrivers());
            }
        }

        public void EnterAttackMode()
        {
            if (Util.HasEffectiveAuthority(owner))
            {
                Util.PlaySound("", base.gameObject);
            }

            if (attackMode) return;

            attackMode = true;

            foreach (AISkillDriver driver in aISkillDrivers)
            {
                if (driver.enabled) continue;
                if (SpiritCharacter.attackDrivers.Contains(driver.customName))
                {
                    driver.enabled = true;
                }
            }

            //spiritVFX.SetLineColor(Color.red);
            //spiritVFX.SetTrailColor(Color.red);
        }

        public void EnterFollowMode()
        {
            if (Util.HasEffectiveAuthority(owner))
            {
                Util.PlaySound("Play_gravekeeper_death_01", base.gameObject);
            }

            if (!attackMode) return;

            attackMode = false;

            if (targetHighlight) UnityEngine.Object.Destroy(targetHighlight);

            foreach (AISkillDriver driver in aISkillDrivers)
            {
                if (!driver.enabled) continue;
                if (SpiritCharacter.attackDrivers.Contains(driver.customName))
                {
                    driver.enabled = false;
                }
            }

            baseAI.currentEnemy.gameObject = null;
            baseAI.currentEnemy.bestHurtBox = null;
            baseAI.BeginSkillDriver(baseAI.EvaluateSkillDrivers());

            //spiritVFX.SetLineColor(Color.green);
            //spiritVFX.SetTrailColor(Color.green);
        }

        public void LungeAtEnemy(CharacterBody victimBody)
        {
            this.bodyMachine.SetInterruptState(new SpiritLunge{ position = victimBody.transform.position }, EntityStates.InterruptPriority.PrioritySkill);
        }
        public void FreeOrb()
        {
            this.weaponMachine.SetInterruptState(new SpiritBarrage(), EntityStates.InterruptPriority.PrioritySkill);
        }
        public void Redirect(Vector3 position, float minDistance, bool empower = true)
        {
            this.bodyMachine.SetInterruptState(new SpiritRedirect{ position = position, minDistanceFromPoint = minDistance, isEmpower = empower }, EntityStates.InterruptPriority.PrioritySkill);
        }


        private void FixedUpdate()
        {
            if (characterBody.isPlayerControlled) return;

            if (!owner && masterObject)
            {
                owner = masterObject.GetComponent<CharacterMaster>().minionOwnership.ownerMaster.bodyInstanceObject;
                if (!owner) return;
                SpiritMasterComponent spiritMasterComponent = owner.GetComponent<SpiritMasterComponent>();
                if (!spiritMasterComponent)
                {
                    if (characterBody.master.godMode) characterBody.master.ToggleGod();
                    characterBody.master.TrueKill();
                }
                spiritMasterComponent.spiritController = this;
            }

            if (!owner) return;

            if (inFrenzy)
            {
                if (!characterBody.HasBuff(RoR2.RoR2Content.Buffs.WarCryBuff))
                {
                    inFrenzy = false;
                    owner.GetComponent<SpiritMasterComponent>().FollowOrder();
                }
            }
        }

        #region UI

        private void CreateHighlight(GameObject target)
        {
            if (target.GetComponent<Highlight>()) return;

            if (targetHighlight) UnityEngine.Object.Destroy(targetHighlight);

            var modelLocator = target.GetComponent<ModelLocator>();
            if (modelLocator)
            {
                var modelTransform = modelLocator.modelTransform;
                if (modelTransform)
                {
                    var characterModel = modelTransform.GetComponent<CharacterModel>();
                    if (characterModel)
                    {
                        foreach (CharacterModel.RendererInfo rendererInfo in characterModel.baseRendererInfos)
                        {
                            if (!rendererInfo.ignoreOverlays)
                            {
                                targetHighlight = target.AddComponent<Highlight>();
                                targetHighlight.highlightColor = Highlight.HighlightColor.teleporter;
                                targetHighlight.strength = 1.5f;
                                targetHighlight.targetRenderer = rendererInfo.renderer;
                                targetHighlight.isOn = true;
                                return;
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
