using EntityStates;
using EntityStates.GravekeeperMonster.Weapon;
using RoR2;
using RoR2.Orbs;
using SpiritboundMod.Modules.BaseStates;
using SpiritboundMod.Spiritbound.Content;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Projectile;
using SpiritboundMod.Spirit.SkillStates;

namespace SpiritboundMod.Spiritbound.SkillStates
{
    public class HopFire : BaseSpiritboundSkillState
    {
        private GameObject muzzleFlashEffect = SpiritboundAssets.arrowMuzzleFlashEffect;
        protected Vector3 hopVector;
        public float duration = 0.3f;
        public float speedCoefficient = 7f;
        public float orbDamageCoefficient = SpiritboundStaticValues.wispDamageCoefficient;
        private ChildLocator childLocator;
        protected CameraTargetParams.AimRequest request;

        private bool fired = false;
        private bool isCrit;

        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();

            characterBody.SetAimTimer(2f);

            Transform modelTransform = GetModelTransform();
            if (modelTransform)
            {
                childLocator = modelTransform.GetComponent<ChildLocator>();
            }

            if (cameraTargetParams)
            {
                request = cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
            }
            hopVector = GetHopVector();

            characterMotor.velocity = Vector3.zero;
            Vector3 to = inputBank.aimDirection;
            to.y = 0f;
            if (inputBank.aimDirection.y < 0f && (Vector3.Angle(to, hopVector) <= 90) || inputBank.aimDirection.y > 0f && (Vector3.Angle(to, hopVector) >= 90))
            {
                hopVector.y *= -1;
            }
            if (Vector3.Angle(inputBank.aimDirection, to) <= 45)
            {
                hopVector.y = 0.25f;
            }

            hopVector.y = Mathf.Clamp(hopVector.y, 0.1f, 0.75f);

            characterDirection.moveVector = hopVector;

            base.PlayCrossfade("FullBody, Override", "Dash", "Dash.playbackRate", this.duration * 1.5f, 0.05f);
            base.PlayAnimation("Gesture, Override", "BufferEmpty");

            if (EntityStates.BrotherMonster.BaseSlideState.slideEffectPrefab && base.characterBody)
            {
                Vector3 position = base.characterBody.footPosition;
                Quaternion rotation = Quaternion.identity;
                Transform transform = base.FindModelChild("Base");

                if (transform)
                {
                    position = transform.position;
                }

                if (base.characterDirection)
                {
                    rotation = Util.QuaternionSafeLookRotation(hopVector);
                }

                EffectManager.SimpleEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/FeatherEffect"), position, rotation, true);
            }

            speedCoefficient = 0.3f * characterBody.jumpPower * Mathf.Clamp((characterBody.moveSpeed) / 4f, 5f, 20f);

            isCrit = Util.CheckRoll(base.characterBody.crit, base.characterBody.master);

            if (NetworkServer.active)
            {
                characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            }

            Util.PlaySound("sfx_driver_dash", this.gameObject);
        }
        protected virtual Vector3 GetHopVector()
        {
            Vector3 aimDirection = inputBank.aimDirection;
            aimDirection.y = 0f;
            Vector3 axis = -Vector3.Cross(Vector3.up, aimDirection);
            float num = Vector3.Angle(inputBank.aimDirection, aimDirection);
            if (inputBank.aimDirection.y < 0f)
            {
                num = 0f - num;
            }
            return Vector3.Normalize(Quaternion.AngleAxis(num, axis) * inputBank.moveVector);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.duration / 2f && !fired)
            {
                fired = true;
                Fire();
                EntityStateMachine.FindByCustomName(base.gameObject, "Weapon2").SetInterruptState(new SpiritBarrage(), InterruptPriority.PrioritySkill);
                spiritMasterComponent.SpiritOrbOrder();
            }

            if (characterMotor && characterDirection && base.isAuthority)
            {
                characterMotor.Motor.ForceUnground();
                characterMotor.velocity = hopVector * speedCoefficient;
            }

            if (fixedAge >= this.duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }
        protected virtual GenericDamageOrb CreateArrowOrb()
        {
            return new HuntressArrowOrb();
        }
        private void Fire()
        {
            if (NetworkServer.active)
            {
                HurtBox[] hurtBoxes = new SphereSearch
                {
                    origin = base.transform.position,
                    radius = 20f,
                    mask = LayerIndex.entityPrecise.mask
                }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(base.characterBody.teamComponent.teamIndex)).OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

                if (hurtBoxes.Length > 0)
                {
                    int num = Mathf.Clamp(hurtBoxes.Length, 1, 3 + base.characterBody.GetBuffCount(SpiritboundBuffs.soulStacksBuff));

                    for(int i = 0; i < num; i++)
                    {
                        GenericDamageOrb genericDamageOrb = CreateArrowOrb();
                        genericDamageOrb.damageValue = base.characterBody.damage * SpiritboundStaticValues.arrowBaseDamageCoefficient;
                        genericDamageOrb.isCrit = isCrit;
                        genericDamageOrb.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
                        genericDamageOrb.attacker = base.gameObject;
                        genericDamageOrb.procCoefficient = 0.7f;
                        HurtBox hurtBox = hurtBoxes[i];
                        if (hurtBox)
                        {
                            Transform transform = childLocator.FindChild("BowMuzzle");
                            EffectManager.SimpleMuzzleFlash(muzzleFlashEffect, base.gameObject, "BowMuzzle", transmit: true);
                            genericDamageOrb.origin = transform.position;
                            genericDamageOrb.target = hurtBox;
                            OrbManager.instance.AddOrb(genericDamageOrb);
                        }
                    }
                }
            }
        }
        public override void OnExit()
        {
            //just in case
            if(!fired)
            {
                Fire();
            }
            if (!outer.destroying)
            {
                if (cameraTargetParams)
                {
                    request.Dispose();
                }
            }
            base.OnExit();
            if (NetworkServer.active)
            {
                characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
                for(int i = 5 - characterBody.GetBuffCount(SpiritboundBuffs.quickShotBuff);  i > 0; i--) 
                {
                    characterBody.AddTimedBuff(SpiritboundBuffs.quickShotBuff, 4f);
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
