using UnityEngine;
using EntityStates;
using SoulboundMod.Modules.BaseStates;
using RoR2;
using UnityEngine.AddressableAssets;
using SoulboundMod.Soulbound.Content;
using static R2API.DamageAPI;
using UnityEngine.Networking;
using R2API.Networking;
using SoulboundMod.Soulbound.Components;
using R2API.Networking.Interfaces;

namespace SoulboundMod.Soulbound.SkillStates
{
    public class Swing : BaseMeleeAttack
    {
        public override void OnEnter()
        {
            RefreshState();
            hitboxGroupName = "MeleeHitbox";

            damageType = DamageType.Generic;
            damageCoefficient = SoulboundStaticValues.swingDamageCoefficient;
            procCoefficient = 1f;
            pushForce = 300f;
            bonusForce = Vector3.zero;
            baseDuration = 0.625f;

            //0-1 multiplier of baseduration, used to time when the hitbox is out (usually based on the run time of the animation)
            //for example, if attackStartPercentTime is 0.5, the attack will start hitting halfway through the ability. if baseduration is 3 seconds, the attack will start happening at 1.5 seconds
            attackStartPercentTime = 0.4f;
            attackEndPercentTime = 0.6f;

            //this is the point at which the attack can be interrupted by itself, continuing a combo
            earlyExitPercentTime = 1f;

            hitStopDuration = 0.012f;
            attackRecoil = 0.5f;
            hitHopVelocity = this.isConvicting ? 2f : 4f;

            swingSoundString = this.isConvicting ? "Play_merc_sword_swing" : "sfx_driver_swing";
            hitSoundString = "";
            muzzleString = swingIndex % 2 == 0 ? "SwingMuzzle1" : "SwingMuzzle2";
            playbackRateParam = "Swing.playbackRate";
            swingEffectPrefab = this.isConvicting ? SoulboundAssets.swordSwingEffect : SoulboundAssets.batSwingEffect;
            hitEffectPrefab = SoulboundAssets.batHitEffect;

            impactSound = this.isConvicting ? SoulboundAssets.swordImpactSoundEvent.index : SoulboundAssets.batImpactSoundEvent.index;

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
                base.FireAttack();
            }

        }

        protected override void PlaySwingEffect()
        {
            Util.PlaySound(this.swingSoundString, this.gameObject);
            if (this.swingEffectPrefab)
            {
                Transform muzzleTransform = this.FindModelChild(this.muzzleString);
                if (muzzleTransform)
                {
                    this.swingEffectPrefab = Object.Instantiate<GameObject>(this.swingEffectPrefab, muzzleTransform);
                }
            }
        }

        protected override void PlayAttackAnimation()
        {
            PlayCrossfade("Gesture, Override", "Swing" + (1 + swingIndex), playbackRateParam, duration * 2.2f, 0.05f);
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}