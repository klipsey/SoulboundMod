using RoR2.Projectile;
using UnityEngine;

namespace SpiritboundMod.Spiritbound.Components
{
    public class AlignArrowComponent : MonoBehaviour
    {
        public Rigidbody rigidbody;
        public float minimumVelocitySqrMagnitude = 1f;
        public ProjectileStickOnImpact projectileStickOnImpact;

        public void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            projectileStickOnImpact = GetComponent<ProjectileStickOnImpact>();
        }

        public void LateUpdate()
        {
            if (rigidbody && rigidbody.velocity.sqrMagnitude >= minimumVelocitySqrMagnitude &&
                (!projectileStickOnImpact || !projectileStickOnImpact.stuck))
                transform.forward = rigidbody.velocity.normalized;
        }
    }
}
