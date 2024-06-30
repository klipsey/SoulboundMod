using UnityEngine;
using RoR2;
using EntityStates;
using UnityEngine.AddressableAssets;
using RoR2.Projectile;
using SoulboundMod.Soulbound.Content;
using SoulboundMod.Soulbound.Components;
using R2API;

namespace SoulboundMod.Soulbound.SkillStates
{
    public class ThrowCleaver : GenericProjectileBaseState
    {
        public static float baseDuration = 0.2f;
        public static float baseDelayDuration = 0.3f * baseDuration;
        public GameObject cleaver = SoulboundAssets.arrowPrefab;
        public SoulboundController soulboundController;
        private ChildLocator childLocator;
        public override void OnEnter()
        {
            soulboundController = base.gameObject.GetComponent<SoulboundController>();
            base.attackSoundString = "sfx_scout_baseball_hit";

            base.baseDuration = baseDuration;
            base.baseDelayBeforeFiringProjectile = baseDelayDuration;

            base.damageCoefficient = damageCoefficient;
            base.force = 120f;

            base.projectilePitchBonus = -3.5f;

            base.OnEnter();
        }

        public override void FireProjectile()
        {
            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                aimRay = this.ModifyProjectileAimRay(aimRay);
                aimRay.direction = Util.ApplySpread(aimRay.direction, 0f, 0f, 1f, 1f, 0f, this.projectilePitchBonus);
                ProjectileManager.instance.FireProjectile(cleaver, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), this.gameObject, this.damageStat * SoulboundStaticValues.cleaverDamageCoefficient, this.force, this.RollCrit(), DamageColorIndex.Default, null, -1f);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }

        public override void PlayAnimation(float duration)
        {
            if (base.GetModelAnimator())
            {
                base.PlayAnimation("Gesture, Override", "SwingCleaver", "Swing.playbackRate", this.duration * 5.5f);
            }
        }
    }
}