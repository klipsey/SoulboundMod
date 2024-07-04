using EntityStates;
using EntityStates.GravekeeperMonster.Weapon;
using RoR2;
using RoR2.Projectile;
using SpiritboundMod.Spirit.Components;
using SpiritboundMod.Spiritbound.Content;
using UnityEngine;
using UnityEngine.Networking;

namespace SpiritboundMod.Spirit.SkillStates
{
    public class SpiritBarrage : BaseState
    {
        public float baseDuration = 0.1f;

        public string muzzleString;

        public float missileForce = GravekeeperBarrage.missileForce;

        public float damageCoefficient = SpiritboundStaticValues.wispDamageCoefficient;

        public float maxSpread = 0f;

        public GameObject projectilePrefab = SpiritboundAssets.spiritOrbPrefab;

        private GameObject muzzleFlashEffect = SpiritboundAssets.arrowMuzzleFlashEffect;

        private float duration;

        private ChildLocator childLocator;

        private float missileStopwatch;

        private int spiritOrbAmount;
        public override void OnEnter()
        {
            childLocator = GetModelChildLocator();

            base.OnEnter();

            duration = baseDuration / attackSpeedStat;
            if(base.characterBody.baseNameToken == "KENKO_SPIRIT_NAME")
            {
                spiritOrbAmount = base.GetComponent<SpiritController>().owner.GetComponent<CharacterBody>().GetBuffCount(SpiritboundBuffs.soulStacksBuff) + 1;

                int rand = Random.Range(0, 2);
                muzzleString = rand == 0 ? "MuzzleFlashL" : "MuzzleFlashR";
            }
            else
            {
                spiritOrbAmount = characterBody.GetBuffCount(SpiritboundBuffs.soulStacksBuff) + 1;

                muzzleString = "FirePack";
            }
            //PlayAnimation("Jar, Override", "BeginGravekeeperBarrage");
            characterBody.SetAimTimer(duration + 1f);
        }

        private void FireBlob(Ray projectileRay, float bonusPitch, float bonusYaw)
        {
            projectileRay.direction = Util.ApplySpread(projectileRay.direction, 0f, maxSpread, 1f, 1f, bonusYaw, bonusPitch);
            EffectManager.SimpleMuzzleFlash(muzzleFlashEffect, base.gameObject, muzzleString, true);
            if (NetworkServer.active)
            {
                ProjectileManager.instance.FireProjectile(projectilePrefab, projectileRay.origin, Util.QuaternionSafeLookRotation(projectileRay.direction), gameObject, damageStat * damageCoefficient, missileForce, Util.CheckRoll(critStat, characterBody.master));
            }
        }

        public override void OnExit()
        {
            //PlayCrossfade("Jar, Override", "EndGravekeeperBarrage", 0.06f);
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            missileStopwatch += Time.fixedDeltaTime;

            if (missileStopwatch >= duration / spiritOrbAmount)
            {
                missileStopwatch = 0f;
                Transform transform = childLocator.FindChild(muzzleString);
                if (transform)
                {
                    Ray projectileRay = default;
                    projectileRay.origin = transform.position;
                    projectileRay.direction = GetAimRay().direction;
                    float maxDistance = 1000f;
                    if (Physics.Raycast(GetAimRay(), out var hitInfo, maxDistance, LayerIndex.world.mask))
                    {
                        projectileRay.direction = hitInfo.point - transform.position;
                    }
                    FireBlob(projectileRay, 0f, 0f);
                }
            }
            if (base.fixedAge >= duration * 1.25f && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
