using RoR2.Projectile;
using SoulboundMod.Modules.BaseStates;
using System;
using RoR2;
using UnityEngine;
using SoulboundMod.Soulbound.Content;

namespace SoulboundMod.Soulbound.SkillStates
{
    public class ChargeArrow : BaseSoulboundSkillState
    {
        private GameObject chargeEffectPrefab = SoulboundAssets.chargeEffect;
        private GameObject fullChargeEffectPrefab = SoulboundAssets.fullChargeEffect;
        private GameObject muzzleFlashEffectPrefab = SoulboundAssets.arrowMuzzleFlashEffect;
        private GameObject arrowPrefab = SoulboundAssets.arrowPrefab;
        private GameObject arrowChargedPrefab = SoulboundAssets.chargedArrowPrefab;
        private GameObject chargeFullEffectPrefab = SoulboundAssets.fullChargeEffect;

        public static float baseChargeDuration = 1f;
        public static float minBloomRadius = 0f;
        public static float maxBloomRadius = 0.5f;
        public static float minChargeDuration = 0.3f;
        public static string muzzleName = "BowMuzzle";
        public static float fullChargeAnimationDuration = 0.34f;

        public float chargeDuration;
        public Animator animator;
        public Transform muzzleTransform;
        public GameObject chargeEffectInstance;
        public uint loopSoundInstanceId;
        public bool playedFullChargeEffects = false;

        public override void OnEnter()
        {
            base.OnEnter();

            chargeDuration = baseChargeDuration / attackSpeedStat;

            animator = GetModelAnimator();

            var childLocator = GetModelChildLocator();
            if (childLocator)
            {
                muzzleTransform = childLocator.FindChild(muzzleName) ?? characterBody.coreTransform;
                if (muzzleTransform && chargeEffectPrefab)
                {
                    chargeEffectInstance = UnityEngine.Object.Instantiate(chargeEffectPrefab, muzzleTransform.position, muzzleTransform.rotation);
                    chargeEffectInstance.transform.parent = muzzleTransform;

                    var scaleParticleSystemDuration = chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                    if (scaleParticleSystemDuration) scaleParticleSystemDuration.newDuration = chargeDuration;
                    var objectScaleCurve = chargeEffectInstance.GetComponent<ObjectScaleCurve>();
                    if (objectScaleCurve) objectScaleCurve.timeMax = chargeDuration;
                }
            }

            loopSoundInstanceId = Util.PlayAttackSpeedSound("Play_huntress_R_aim_loop", gameObject, attackSpeedStat);

            PlayAnimation("Gesture, Override", "ChargeArrow");
        }

        public float CalcCharge()
        {
            return Mathf.Clamp01(fixedAge / chargeDuration);
        }

        public override void Update()
        {
            base.Update();
            characterBody.SetAimTimer(1f);
            characterBody.SetSpreadBloom(Util.Remap(CalcCharge(), 0f, 1f, minBloomRadius, maxBloomRadius), true);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            var charge = CalcCharge();
            if (!playedFullChargeEffects && charge >= 1f)
            {
                playedFullChargeEffects = true;

                PlayFullChargeEffects();
            }

            if (isAuthority && !IsKeyDownAuthority() && fixedAge >= minChargeDuration)
            {
                outer.SetNextStateToMain();
            }
        }

        public void PlayFullChargeEffects()
        {
            AkSoundEngine.StopPlayingID(loopSoundInstanceId);
            if (chargeEffectInstance) Destroy(chargeEffectInstance);

            EffectManager.SimpleMuzzleFlash(muzzleFlashEffectPrefab, gameObject, muzzleName, false);

            chargeEffectInstance = UnityEngine.Object.Instantiate(chargeFullEffectPrefab, muzzleTransform.position, muzzleTransform.rotation);
            chargeEffectInstance.transform.parent = muzzleTransform;

            loopSoundInstanceId = Util.PlaySound("Play_item_proc_fireRingTornado_start", gameObject);
        }

        public void FireArrowAuthority()
        {
            var charge = CalcCharge();
            var aimRay = GetAimRay();

            Util.PlaySound("Play_huntress_m1_shoot", gameObject);
            if (charge >= 1f)
            {
                Util.PlaySound("Play_clayboss_m1_shoot", gameObject);
            }

            var fireProjectileInfo = new FireProjectileInfo
            {
                damage = Util.Remap(charge, 0f, 1f, SoulboundStaticValues.arrowBaseDamageCoefficient, SoulboundStaticValues.arrowFullDamageCoefficient) * damageStat,
                crit = characterBody.RollCrit(),
                position = aimRay.origin,
                rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                owner = gameObject,
                force = 400f * charge,
                projectilePrefab = charge < 1f ? arrowPrefab : arrowChargedPrefab
            };
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        }

        public override void OnExit()
        {
            AkSoundEngine.StopPlayingID(loopSoundInstanceId);
            if (!outer.destroying)
            {
                PlayAnimation("Gesture, Override", "FireArrow");
                if (isAuthority) FireArrowAuthority();
            }
            if (chargeEffectInstance) Destroy(chargeEffectInstance);
            base.OnExit();
        }

        public override EntityStates.InterruptPriority GetMinimumInterruptPriority()
        {
            return EntityStates.InterruptPriority.PrioritySkill;
        }
    }
}

