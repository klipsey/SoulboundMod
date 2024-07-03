using HG;
using Newtonsoft.Json.Linq;
using R2API;
using RoR2;
using RoR2.Projectile;
using SpiritboundMod.Modules;
using SpiritboundMod.Spirit.Components;
using SpiritboundMod.Spiritbound.Components;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using static RoR2.DotController;

namespace SpiritboundMod.Spiritbound.Content
{
    public static class DamageTypes
    {
        public static DamageAPI.ModdedDamageType Default;
        public static DamageAPI.ModdedDamageType MountingDread;
        public static DamageAPI.ModdedDamageType CurrentHealthSpirit;
        internal static void Init()
        {
            Default = DamageAPI.ReserveDamageType();
            MountingDread = DamageAPI.ReserveDamageType();
            CurrentHealthSpirit = DamageAPI.ReserveDamageType();
            Hook();
        }
        private static void Hook()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }
        private static void GlobalEventManager_onServerDamageDealt(DamageReport damageReport)
        {
            DamageInfo damageInfo = damageReport.damageInfo;
            if (!damageReport.attackerBody || !damageReport.victimBody)
            {
                return;
            }
            HealthComponent victimHealth = damageReport.victim;
            GameObject inflictorObject = damageInfo.inflictor;
            CharacterBody victimBody = damageReport.victimBody;
            EntityStateMachine victimMachine = victimBody.GetComponent<EntityStateMachine>();
            CharacterBody attackerBody = damageReport.attackerBody;
            GameObject attackerObject = damageReport.attacker.gameObject;
            SpiritboundController spiritBoundController = attackerBody.GetComponent<SpiritboundController>();
            SpiritMasterComponent spiritMasterController = attackerBody.GetComponent<SpiritMasterComponent>();
            if (NetworkServer.active)
            {
                if (spiritBoundController && attackerBody.baseNameToken == "KENKO_SPIRITBOUND_NAME")
                {
                    if (damageReport.victim && attackerBody.HasBuff(SpiritboundBuffs.spiritHealBuff) && spiritBoundController.healAmount == 100f && attackerBody.healthComponent.health < attackerBody.healthComponent.fullHealth)
                    {
                        attackerBody.SetBuffCount(SpiritboundBuffs.spiritHealBuff.buffIndex, 0);
                        spiritBoundController.healAmount = 0f;
                        HealOrb orb = new HealOrb();
                        orb.origin = damageReport.victim.transform.position;
                        orb.target = Util.FindBodyMainHurtBox(attackerBody);
                        RoR2.Orbs.OrbManager.instance.AddOrb(orb);
                    }
                    if (damageInfo.HasModdedDamageType(MountingDread) && victimBody && spiritMasterController)
                    {
                        if(victimBody.GetBuffCount(SpiritboundBuffs.mountingDreadBuff) >= 2)
                        {
                            victimBody.SetBuffCount(SpiritboundBuffs.mountingDreadBuff.buffIndex, 0);

                            DamageInfo biteDamage = new DamageInfo
                            {
                                position = victimBody.corePosition,
                                attacker = attackerBody.gameObject,
                                inflictor = spiritMasterController.spiritController.gameObject,
                                damage = (victimBody.healthComponent.fullCombinedHealth - victimBody.healthComponent.health) * (0.05f + (0.005f * attackerBody.GetBuffCount(SpiritboundBuffs.soulStacksBuff)))  + SpiritboundStaticValues.spiritBiteDamageCoefficient * attackerBody.damage ,
                                damageColorIndex = DamageColorIndex.Default,
                                damageType = DamageType.SlowOnHit,
                                crit = damageInfo.crit,
                                force = Vector3.zero,
                                procChainMask = default,
                                procCoefficient = 1f
                            };

                            victimBody.healthComponent.TakeDamage(biteDamage);
                            GlobalEventManager.instance.OnHitEnemy(biteDamage, victimBody.gameObject);
                            GlobalEventManager.instance.OnHitAll(biteDamage, victimBody.gameObject);

                            spiritMasterController.LungeOrder(victimBody);
                        }
                        else
                        {
                            victimBody.AddBuff(SpiritboundBuffs.mountingDreadBuff);
                        }
                    }
                }
            }
        }
    }
}
