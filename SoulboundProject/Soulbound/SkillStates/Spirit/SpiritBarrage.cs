using EntityStates;
using EntityStates.GravekeeperMonster.Weapon;
using RoR2;
using RoR2.Projectile;
using SoulboundMod.Soulbound.Content;
using UnityEngine;
using UnityEngine.Networking;

namespace SoulboundMod.Spirit.SkillStates
{
    public class SpiritBarrage : BaseState
    {
        private float stopwatch;

        private float missileStopwatch;

        public float baseDuration = 1.5f;

        public string muzzleString;

        public float missileSpawnFrequency = 0.75f;

        public float missileSpawnDelay = 0f;

        public float missileForce = GravekeeperBarrage.missileForce;

        public float damageCoefficient = SoulboundStaticValues.wispDamageCoefficient;

        public float maxSpread = 0f;

        public GameObject projectilePrefab = GravekeeperBarrage.projectilePrefab;

        public GameObject muzzleflashPrefab = GravekeeperBarrage.muzzleflashPrefab;

        public GameObject fireEffectPrefab = GravekeeperBarrage.jarOpenEffectPrefab;

        public string fireSoundString = GravekeeperBarrage.jarOpenSoundString;

        private ChildLocator childLocator;

        public override void OnEnter()
        {
            int rand = Random.Range(0, 2);
            muzzleString = rand == 0 ? "MuzzleFlashL" : "MuzzleFlashR";
            childLocator = GetModelChildLocator();
            base.OnEnter();
            //PlayAnimation("Jar, Override", "BeginGravekeeperBarrage");
            Util.PlaySound(fireSoundString, gameObject);
            characterBody.SetAimTimer(baseDuration + 2f);
        }

        private void FireBlob(Ray projectileRay, float bonusPitch, float bonusYaw)
        {
            projectileRay.direction = Util.ApplySpread(projectileRay.direction, 0f, maxSpread, 1f, 1f, bonusYaw, bonusPitch);
            EffectManager.SimpleMuzzleFlash(muzzleflashPrefab, gameObject, muzzleString, transmit: false);
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
            stopwatch += Time.fixedDeltaTime;
            missileStopwatch += Time.fixedDeltaTime;
            if (missileStopwatch >= missileSpawnFrequency)
            {
                missileStopwatch = 0f;
                Transform transform = childLocator.FindChild(muzzleString);
                if ((bool)transform)
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
            if (stopwatch >= baseDuration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}
