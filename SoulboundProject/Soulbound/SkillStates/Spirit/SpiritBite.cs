using RoR2;
using SoulboundMod.Modules.BaseStates;
using EntityStates;
using SoulboundMod.Soulbound.Content;
using UnityEngine;

namespace SoulboundMod.Spirit.SkillStates
{
    public class SpiritBite : BaseMeleeAttack
    {
        protected GameObject swingEffectInstance;
        public override void OnEnter()
        {
            RefreshState();
            hitboxGroupName = "MeleeHitbox";

            damageType = DamageType.Generic;
            damageCoefficient = SoulboundStaticValues.spiritBiteDamageCoefficient;
            procCoefficient = 1f;
            pushForce = 300f;
            bonusForce = Vector3.zero;
            baseDuration = 1f;

            //0-1 multiplier of baseduration, used to time when the hitbox is out (usually based on the run time of the animation)
            //for example, if attackStartPercentTime is 0.5, the attack will start hitting halfway through the ability. if baseduration is 3 seconds, the attack will start happening at 1.5 seconds
            attackStartPercentTime = 0.2f;
            attackEndPercentTime = 0.4f;

            //this is the point at which the attack can be interrupted by itself, continuing a combo
            earlyExitPercentTime = 0.5f;

            hitStopDuration = 0.05f;
            attackRecoil = 2f / attackSpeedStat;
            hitHopVelocity = 0f;

            swingSoundString = "";
            hitSoundString = "";
            playbackRateParam = "Bite.playbackRate";
            swingEffectPrefab = SoulboundAssets.spiritBiteEffect;
            hitEffectPrefab = SoulboundAssets.soulboundHitEffect;

            impactSound = SoulboundAssets.biteImpactSoundEvent.index;

            muzzleString = "HeadCenter";

            base.OnEnter();
        }

        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();
        }

        protected override void FireAttack()
        {
            if (base.isAuthority)
            {
                Vector3 direction = this.GetAimRay().direction;
                direction.y = Mathf.Max(direction.y, direction.y * 0.5f);
                this.FindModelChild("MeleePivot").rotation = Util.QuaternionSafeLookRotation(direction);
            }

            base.FireAttack();
        }

        protected override void PlaySwingEffect()
        {
            Util.PlaySound(this.swingSoundString, this.gameObject);
            if (this.swingEffectPrefab)
            {
                Transform muzzleTransform = this.FindModelChild(this.muzzleString);
                if (muzzleTransform)
                {
                    this.swingEffectInstance = Object.Instantiate<GameObject>(this.swingEffectPrefab, muzzleTransform);
                }
            }
        }

        protected override void PlayAttackAnimation()
        {
            RefreshState();
            PlayCrossfade("Gesture, Override", "Bite", playbackRateParam, duration * 1.3f, 0.05f);
        }

        public override void OnExit()
        {
            base.OnExit();

            if (this.swingEffectInstance) EntityState.Destroy(this.swingEffectInstance);
        }
    }
}
