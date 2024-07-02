using EntityStates;
using MonoMod.RuntimeDetour;
using RoR2;
using SoulboundMod.Modules.BaseStates;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SoulboundMod.Soulbound.SkillStates
{
    public class RedirectSpirit : BaseSoulboundSkillState
    {
        public static float baseDuration = 0.7f;
        protected float duration;
        private Vector3 position;
        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / base.attackSpeedStat;

            base.StartAimMode(duration + 0.1f, false);

            base.PlayCrossfade("Gesture, Override", "Point", "Hand.playbackRate", duration, 0.1f);

            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();

                BulletAttack fakeAimRay = new BulletAttack
                {
                    bulletCount = 1,
                    aimVector = aimRay.direction,
                    origin = aimRay.origin,
                    damage = 0f,
                    damageColorIndex = DamageColorIndex.Default,
                    falloffModel = BulletAttack.FalloffModel.None,
                    maxDistance = 75f,
                    force = 0f,
                    hitMask = LayerIndex.CommonMasks.bullet,
                    minSpread = 0f,
                    maxSpread = 0f,
                    isCrit = false,
                    owner = base.gameObject,
                    muzzleName = "",
                    smartCollision = true,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = 0f,
                    radius = 1f,
                    sniper = false,
                    stopperMask = LayerIndex.CommonMasks.bullet,
                    weapon = null,
                    tracerEffectPrefab = null,
                    spreadPitchScale = 0f,
                    spreadYawScale = 0f,
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                    hitEffectPrefab = null
                };
                fakeAimRay.modifyOutgoingDamageCallback = delegate (BulletAttack _bulletAttack, ref BulletAttack.BulletHit hitInfo, DamageInfo damageInfo)
                {
                    damageInfo.rejected = true;

                    position = hitInfo.point;

                };
                fakeAimRay.Fire();
            }

            Util.PlaySound("sfx_interrogator_point", base.gameObject);

            spiritMasterComponent.RedirectOrder(position, 10f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
