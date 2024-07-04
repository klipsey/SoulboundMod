using UnityEngine;
using RoR2;
using RoR2.Orbs;
using SpiritboundMod.Spiritbound.Content;
using UnityEngine.Networking;
using R2API.Networking;
using R2API.Networking.Interfaces;
using System.Reflection;

namespace SpiritboundMod.Spiritbound.Components
{
    public class StackOrb : Orb
    {
        public override void Begin()
        {
            base.duration = Mathf.Clamp(base.distanceToTarget / 5f, 0.25f, 0.5f);

            EffectData effectData = new EffectData
            {
                origin = this.origin,
                genericFloat = base.duration
            };

            effectData.SetHurtBoxReference(this.target);

            GameObject effectPrefab = SpiritboundAssets.consumeOrb;

            EffectManager.SpawnEffect(effectPrefab, effectData, true);
        }

        public override void OnArrival()
        {
            if (this.target)
            {
                if (this.target.healthComponent)
                {
                    this.target.GetComponent<SpiritboundController>().AddStackInternally();

                    NetworkIdentity identity = this.target.healthComponent.gameObject.GetComponent<NetworkIdentity>();
                    if (!identity) return;
                    new SyncStackOrb(identity.netId, this.target.healthComponent.gameObject).Send(NetworkDestination.Clients);
                }
            }
        }
    }
}