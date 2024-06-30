using SoulboundMod.Modules.BaseStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;
using SoulboundMod.Soulbound.Content;
using R2API;
using EntityStates;

namespace SoulboundMod.Soulbound.SkillStates
{
    public class Falsify : BaseSoulboundSkillState
    {
        private Transform modelTransform;

        public static GameObject dashPrefab = SoulboundAssets.dashEffect;

        public static float smallHopVelocity = 12f;

        public static float dashDelay = 0.2f;

        public static float dashDuration = 0.5f;

        public static float speedCoefficient = 5f;

        public static string beginSoundString = "sfx_driver_dodge";

        public static string endSoundString = "sfx_interrogator_dash";

        public static float damageCoefficient = SoulboundStaticValues.falsifyDamageCoefficient;

        public static GameObject hitEffectPrefab = SoulboundAssets.batHitEffectRed;

        public static float hitPauseDuration = 0.012f;

        private float stopwatch;

        private Vector3 dashVector = Vector3.zero;

        private Animator animator;

        private CharacterModel characterModel;

        private HurtBoxGroup hurtboxGroup;

        private OverlapAttack overlapAttack;

        private ChildLocator childLocator;

        private bool isDashing;

        private bool inHitPause;

        private float hitPauseTimer;

        private CameraTargetParams.AimRequest aimRequest;

        public bool hasHit { get; private set; }

        public int dashIndex { private get; set; }

        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();
            Util.PlaySound(beginSoundString, base.gameObject);
            modelTransform = GetModelTransform();
            if (base.cameraTargetParams)
            {
                aimRequest = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
            }
            if (modelTransform)
            {
                animator = modelTransform.GetComponent<Animator>();
                characterModel = modelTransform.GetComponent<CharacterModel>();
                childLocator = modelTransform.GetComponent<ChildLocator>();
                hurtboxGroup = modelTransform.GetComponent<HurtBoxGroup>();
            }
            SmallHop(base.characterMotor, smallHopVelocity);
            PlayAnimation("FullBody, Override", "Dash", "Dash.playbackRate", (dashDuration + dashDelay) * 1.5f);
            dashVector = base.inputBank.aimDirection;
            overlapAttack = InitMeleeOverlap(damageCoefficient, hitEffectPrefab, modelTransform, "MeleeHitbox");
            overlapAttack.damageType = DamageType.Stun1s;
            overlapAttack.teamIndex = TeamIndex.None;
            overlapAttack.procCoefficient = 1f;
            overlapAttack.impactSound = SoulboundAssets.batImpactSoundEvent.index;
            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility.buffIndex);
            }
        }

        private void CreateDashEffect()
        {
            Transform transform = childLocator.FindChild("Chest");
            if (transform && dashPrefab)
            {
                EffectManager.SpawnEffect(dashPrefab, new EffectData
                {
                    origin = transform.position,
                    rotation = Util.QuaternionSafeLookRotation(-dashVector)

                }, true);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.characterDirection.forward = dashVector;
            if (base.fixedAge >= dashDelay && !isDashing)
            {
                CreateDashEffect();
                isDashing = true;
                dashVector = base.inputBank.aimDirection;
                base.gameObject.layer = LayerIndex.fakeActor.intVal;
                base.characterMotor.Motor.RebuildCollidableLayers();
            }
            if (!isDashing)
            {
                stopwatch += Time.fixedDeltaTime;
            }
            else if (base.isAuthority)
            {
                base.characterMotor.velocity = Vector3.zero;
                if (!inHitPause)
                {
                    bool fired = overlapAttack.Fire();
                    stopwatch += Time.fixedDeltaTime;
                    if (fired)
                    {
                        if (!hasHit)
                        {
                            hasHit = true;
                        }
                        inHitPause = true;
                        hitPauseTimer = hitPauseDuration / attackSpeedStat;
                    }
                    base.characterMotor.rootMotion += dashVector * moveSpeedStat * speedCoefficient * Time.fixedDeltaTime;
                }
                else
                {
                    hitPauseTimer -= Time.fixedDeltaTime;
                    if (hitPauseTimer < 0f)
                    {
                        inHitPause = false;
                    }
                }
            }
            if (stopwatch >= dashDuration && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.gameObject.layer = LayerIndex.defaultLayer.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();
            Util.PlaySound(endSoundString, base.gameObject);
            if (base.isAuthority)
            {
                base.characterMotor.velocity *= 0.1f;
                SmallHop(base.characterMotor, smallHopVelocity);
            }
            aimRequest?.Dispose();
            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility.buffIndex);
            }
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {

            return InterruptPriority.PrioritySkill;
        }
        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write((byte)dashIndex);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            dashIndex = reader.ReadByte();
        }
    }
}
