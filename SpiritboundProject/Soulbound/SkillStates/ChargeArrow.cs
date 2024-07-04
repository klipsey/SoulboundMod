using RoR2.Projectile;
using SpiritboundMod.Modules.BaseStates;
using RoR2.UI;
using RoR2;
using UnityEngine;
using SpiritboundMod.Spiritbound.Content;
using R2API;
using UnityEngine.Networking;

namespace SpiritboundMod.Spiritbound.SkillStates
{
    public class ChargeArrow : BaseSpiritboundSkillState
    {
        private GameObject chargeEffectPrefab = SpiritboundAssets.chargeEffect;
        private GameObject chargeFullEffectPrefab = SpiritboundAssets.fullChargeEffect;
        private GameObject muzzleFlashEffectPrefab = SpiritboundAssets.arrowMuzzleFlashEffect;
        private GameObject arrowPrefab = SpiritboundAssets.arrowPrefab;
        private GameObject arrowChargedPrefab = SpiritboundAssets.chargedArrowPrefab;

        public static float baseChargeDuration = 1f;
        public static float baseMaxChargeDuration = 2f;
        public static float minBloomRadius = 0f;
        public static float maxBloomRadius = 0.5f;
        public static float minChargeDuration = 0.3f;
        public static string muzzleName = "BowMuzzle";
        public static float fullChargeAnimationDuration = 0.34f;

        public float chargeDuration;
        public float maxChargeDuration;
        public Animator animator;
        public Transform muzzleTransform;
        public GameObject chargeEffectInstance;
        public uint loopSoundInstanceId;
        public bool playedFullChargeEffects = false;

        public override void OnEnter()
        {
            base.OnEnter();

            chargeDuration = baseChargeDuration / attackSpeedStat;
            maxChargeDuration = baseMaxChargeDuration / attackSpeedStat;

            if(NetworkServer.active && characterBody.GetBuffCount(SpiritboundBuffs.quickShotBuff) > 0)
            {
                chargeDuration /= 2f;
                maxChargeDuration = chargeDuration;
            }

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

            Util.PlayAttackSpeedSound("Play_MULT_m1_snipe_charge", gameObject, attackSpeedStat);

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
            base.characterBody.SetSpreadBloom(base.age / chargeDuration);
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

            if (isAuthority && (!IsKeyDownAuthority() || fixedAge >= maxChargeDuration) && fixedAge >= minChargeDuration)
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

            loopSoundInstanceId = Util.PlaySound("Play_gravekeeper_attack1_fly_loop", gameObject);
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

            Quaternion randomRotation = Util.QuaternionSafeLookRotation(aimRay.direction);
            float x = Random.Range(0f, base.characterBody.spreadBloomAngle);
            float z = Random.Range(0f, 360f);
            Vector3 vector = Quaternion.Euler(0f, 0f, z) * (Quaternion.Euler(x, 0f, 0f) * Vector3.forward);
            float y = vector.y;
            vector.y = 0f;
            float angle = (Mathf.Atan2(vector.z, vector.x) * 57.29578f - 90f) * 1f;
            float angle2 = Mathf.Atan2(y, vector.magnitude) * 57.29578f * 1f;

            arrowChargedPrefab.GetComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(DamageTypes.MountingDread);
            if (base.characterBody.HasBuff(SpiritboundBuffs.spiritHealBuff))
            {
                arrowPrefab.GetComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(DamageTypes.Heal);
                arrowChargedPrefab.GetComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(DamageTypes.Heal);
            }

            var fireProjectileInfo = new FireProjectileInfo
            {
                damage = Util.Remap(charge, 0f, 1f, SpiritboundStaticValues.arrowBaseDamageCoefficient, SpiritboundStaticValues.arrowFullDamageCoefficient) * damageStat,
                useSpeedOverride = true,
                speedOverride = Util.Remap(charge, 0f, 1f, 50f + (base.characterBody.GetBuffCount(SpiritboundBuffs.soulStacksBuff) * 5), 150f + (base.characterBody.GetBuffCount(SpiritboundBuffs.soulStacksBuff) * 5)),
                crit = characterBody.RollCrit(),
                position = aimRay.origin,
                rotation = charge < 1f ? randomRotation : Util.QuaternionSafeLookRotation(aimRay.direction),
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
                if (base.isAuthority) FireArrowAuthority();
            }
            if (chargeEffectInstance) Destroy(chargeEffectInstance);
            if (NetworkServer.active && characterBody.GetBuffCount(SpiritboundBuffs.quickShotBuff) > 0)
            {
                characterBody.RemoveBuff(SpiritboundBuffs.quickShotBuff);
            }
            base.OnExit();
        }

        public override EntityStates.InterruptPriority GetMinimumInterruptPriority()
        {
            return EntityStates.InterruptPriority.PrioritySkill;
        }
    }
}

