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
    public class HealOrb : Orb
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
                    this.target.healthComponent.Heal((1 - this.target.healthComponent.health / this.target.healthComponent.fullHealth) * (this.target.healthComponent.fullHealth * SpiritboundStaticValues.healCoefficient + (this.target.healthComponent.body.GetBuffCount(SpiritboundBuffs.soulStacksBuff) * SpiritboundStaticValues.healStacking)), default(ProcChainMask));

                    if (NetworkServer.active)
                    {
                        int num = 5;
                        float num2 = 2f;
                        this.target.healthComponent.body.ClearTimedBuffs(SpiritboundBuffs.spiritMovespeedStacksBuff);
                        for (int i = 0; i < num; i++)
                        {
                            this.target.healthComponent.body.AddTimedBuff(SpiritboundBuffs.spiritMovespeedStacksBuff, num2 * (i + 1) / num);
                        }
                    }

                    NetworkIdentity identity = this.target.healthComponent.gameObject.GetComponent<NetworkIdentity>();
                    if (!identity) return;
                    new SyncHealOrb(identity.netId, this.target.healthComponent.gameObject).Send(NetworkDestination.Clients);
                }
            }
        }
    }
}