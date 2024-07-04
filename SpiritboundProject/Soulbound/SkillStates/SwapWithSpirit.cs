using RoR2.Projectile;
using SpiritboundMod.Modules.BaseStates;
using RoR2.UI;
using RoR2;
using UnityEngine;
using SpiritboundMod.Spiritbound.Content;
using R2API;
using UnityEngine.Networking;
using EntityStates;
using SpiritboundMod.Spirit.SkillStates;
using SpiritboundMod.Spirit.Components;


namespace SpiritboundMod.Spiritbound.SkillStates
{
    public class SwapWithSpirit : BaseSpiritboundSkillState
    {
        private GameObject chargeEffectPrefab = SpiritboundAssets.chargeEffect;

        private GameObject swapMuzzleFlash = SpiritboundAssets.spiritJarOpenEffect;

        private GameObject wolf;

        private Vector3 lambPosition;

        private Vector3 wolfPosition;

        private CameraTargetParams.AimRequest aimRequest;

        private GameObject chargeEffectInstance;
        private GameObject chargeEffectInstance2;
        private bool hasTeleported;
        public override void OnEnter()
        {
            base.OnEnter();

            if (base.cameraTargetParams)
            {
                aimRequest = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
            }
            wolf = this.spiritMasterComponent.spiritController.gameObject;

            lambPosition = this.characterBody.corePosition;
            wolfPosition = wolf.transform.position;

            chargeEffectInstance = Object.Instantiate(chargeEffectPrefab, lambPosition, base.transform.rotation);
            chargeEffectInstance.transform.parent = characterBody.coreTransform;

            var scaleParticleSystemDuration = chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
            if (scaleParticleSystemDuration) scaleParticleSystemDuration.newDuration = 0.5f;
            var objectScaleCurve = chargeEffectInstance.GetComponent<ObjectScaleCurve>();
            if (objectScaleCurve) objectScaleCurve.timeMax = 0.5f;

            chargeEffectInstance2 = Object.Instantiate(chargeEffectPrefab, wolfPosition, base.transform.rotation);
            chargeEffectInstance2.transform.parent = wolf.GetComponent<CharacterBody>().coreTransform;

            var scaleParticleSystemDuration2 = chargeEffectInstance2.GetComponent<ScaleParticleSystemDuration>();
            if (scaleParticleSystemDuration2) scaleParticleSystemDuration2.newDuration = 0.5f;
            var objectScaleCurve2 = chargeEffectInstance2.GetComponent<ObjectScaleCurve>();
            if (objectScaleCurve2) objectScaleCurve2.timeMax = 0.5f;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(base.fixedAge >= 0.5f && !hasTeleported)
            {
                hasTeleported = true;

                this.spiritMasterComponent.spiritController.gameObject.GetComponent<RigidbodyMotor>().rigid.MovePosition(lambPosition);
                this.characterMotor.Motor.SetPosition(wolfPosition);

                if (chargeEffectInstance) Destroy(chargeEffectInstance);
                if (chargeEffectInstance2) Destroy(chargeEffectInstance2);

                BlastAttack lambBlast = new BlastAttack
                {
                    attacker = base.gameObject,
                    inflictor = base.gameObject,
                    attackerFiltering = AttackerFiltering.NeverHitSelf,
                    baseDamage = base.damageStat * SpiritboundStaticValues.swapDamageCoefficient,
                    baseForce = 500f,
                    crit = RollCrit(),
                    damageType = DamageType.Generic,
                    falloffModel = BlastAttack.FalloffModel.Linear,
                    position = wolfPosition,
                    procChainMask = default,
                    procCoefficient = 1f,
                    radius = 10f + characterBody.GetBuffCount(SpiritboundBuffs.soulStacksBuff),
                    teamIndex = teamComponent.teamIndex
                };
                EffectManager.SimpleEffect(swapMuzzleFlash, wolfPosition, base.transform.rotation, true);
                lambBlast.Fire();

                BlastAttack wolfBlast = new BlastAttack
                {
                    attacker = wolf,
                    inflictor = wolf,
                    attackerFiltering = AttackerFiltering.NeverHitSelf,
                    baseDamage = base.damageStat * SpiritboundStaticValues.swapDamageCoefficient,
                    baseForce = 500f,
                    crit = RollCrit(),
                    damageType = DamageType.Generic,
                    falloffModel = BlastAttack.FalloffModel.Linear,
                    position = lambPosition,
                    procChainMask = default,
                    procCoefficient = 1f,
                    radius = 10f + characterBody.GetBuffCount(SpiritboundBuffs.soulStacksBuff),
                    teamIndex = teamComponent.teamIndex
                };
                EffectManager.SimpleEffect(swapMuzzleFlash, lambPosition, base.transform.rotation, true);
                wolfBlast.Fire();

                EntityStateMachine.FindByCustomName(base.gameObject, "Weapon2").SetInterruptState(new SpiritBarrage(), InterruptPriority.PrioritySkill);
                spiritMasterComponent.spiritController.EnterFollowMode();
                spiritMasterComponent.SpiritOrbOrder();
            }

            if(base.isAuthority && base.fixedAge >= 0.65f)
            {
                outer.SetNextStateToMain();
            }
        }
        public override void OnExit()
        {
            aimRequest?.Dispose();

            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
