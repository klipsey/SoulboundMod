using RoR2;
using SpiritboundMod.Modules.BaseStates;
using EntityStates;
using SpiritboundMod.Spiritbound.Content;
using UnityEngine;

namespace SpiritboundMod.Spirit.SkillStates
{
    public class SpiritBite : BaseMeleeAttack
    {
        protected GameObject swingEffectInstance;
        public override void OnEnter()
        {
            RefreshState();
            hitboxGroupName = "MeleeHitbox";

            damageType = DamageType.SlowOnHit;
            damageCoefficient = SpiritboundStaticValues.spiritBiteDamageCoefficient;
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
            swingEffectPrefab = SpiritboundAssets.spiritBiteEffect;
            hitEffectPrefab = SpiritboundAssets.spiritboundHitEffect;

            impactSound = SpiritboundAssets.biteImpactSoundEvent.index;

            muzzleString = "BiteMuzzleFlash";

            if(characterBody.HasBuff(RoR2.RoR2Content.Buffs.WarCryBuff)) moddedDamageTypeHolder.Add(DamageTypes.CurrentHealthSpirit);
            base.OnEnter();
        }

        protected override void OnHitEnemyAuthority()
        {
            Util.PlaySound(hitSoundString, gameObject);
        }

        protected override void FireAttack()
        {
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
