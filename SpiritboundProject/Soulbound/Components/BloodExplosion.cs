using UnityEngine;
using RoR2;

namespace SpiritboundMod.Spiritbound.Components
{
    public class BloodExplosion : MonoBehaviour
    {
        private void Awake()
        {
            CharacterBody characterBody = this.GetComponent<CharacterBody>();

            if (this.transform)
            {
                EffectManager.SpawnEffect(Content.SpiritboundAssets.bloodExplosionEffect, new EffectData
                {
                    origin = this.transform.position,
                    rotation = Quaternion.identity,
                    scale = 0.5f
                }, false);

                GameObject.Instantiate(Content.SpiritboundAssets.bloodSpurtEffect, this.transform);
                Util.PlaySound("sfx_blood_gurgle", base.gameObject);
            }
        }

        private void LateUpdate()
        {
            if (this.transform) this.transform.localScale = Vector3.zero;
        }
    }
}