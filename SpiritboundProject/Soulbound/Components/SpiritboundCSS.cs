using UnityEngine;
using RoR2;
using SpiritboundMod.Spiritbound.Content;

namespace SpiritboundMod.Spiritbound.Components
{
    public class SpiritboundCSS : MonoBehaviour
    {
        private ChildLocator childLocator;
        private bool hasPlayed = false;
        private bool hasPlayed2 = false;
        private float timer = 0f;
        private void Awake()
        {
            this.childLocator = this.GetComponent<ChildLocator>();
            this.Invoke("SkinShit", 0.3f);
        }
        private void SkinShit()
        {
            if (this.childLocator)
            {
                this.childLocator.FindChild("FireR").gameObject.GetComponent<ParticleSystemRenderer>().material = SpiritboundAssets.fireMatInFront;
                this.childLocator.FindChild("FireL").gameObject.GetComponent<ParticleSystemRenderer>().material = SpiritboundAssets.fireMatInFront;
                this.childLocator.FindChild("FireHead").gameObject.GetComponent<ParticleSystemRenderer>().material = SpiritboundAssets.fireMatInFront;
                this.childLocator.FindChild("FirePack").gameObject.GetComponent<ParticleSystemRenderer>().material = SpiritboundAssets.fireMatInFront;
            }
        }
        private void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if (!hasPlayed && timer >= 0.8f)
            {
                hasPlayed = true;
            }

            if (!hasPlayed2 && timer >= 1.25f)
            {
                hasPlayed2 = true;
            }
        }
    }
}
