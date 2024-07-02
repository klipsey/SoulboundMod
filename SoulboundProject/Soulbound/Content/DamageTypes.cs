using HG;
using Newtonsoft.Json.Linq;
using R2API;
using RoR2;
using RoR2.Projectile;
using SoulboundMod.Modules;
using SoulboundMod.Soulbound.Components;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using static RoR2.DotController;

namespace SoulboundMod.Soulbound.Content
{
    public static class DamageTypes
    {
        public static DamageAPI.ModdedDamageType Default;
        public static DamageAPI.ModdedDamageType MountingDread;
        internal static void Init()
        {
            Default = DamageAPI.ReserveDamageType();
            MountingDread = DamageAPI.ReserveDamageType();
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
            SoulboundController soulBoundController = attackerBody.GetComponent<SoulboundController>();
            SpiritMasterComponent spiritMasterController = attackerBody.GetComponent<SpiritMasterComponent>();
            if (NetworkServer.active)
            {
                if (soulBoundController && attackerBody.baseNameToken == "KENKO_SOULBOUND_NAME")
                {
                    if(damageInfo.HasModdedDamageType(MountingDread) && victimBody && spiritMasterController)
                    {
                        if(victimBody.GetBuffCount(SoulboundBuffs.mountingDreadBuff) >= 2)
                        {
                            victimBody.SetBuffCount(SoulboundBuffs.mountingDreadBuff.buffIndex, 0);

                            DamageInfo biteDamage = new DamageInfo
                            {
                                position = victimBody.corePosition,
                                attacker = attackerBody.gameObject,
                                inflictor = spiritMasterController.spiritController.gameObject,
                                damage = SoulboundStaticValues.spiritBiteDamageCoefficient * attackerBody.damage,
                                damageColorIndex = DamageColorIndex.Default,
                                damageType = DamageType.BonusToLowHealth | DamageType.SlowOnHit,
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
                            victimBody.AddBuff(SoulboundBuffs.mountingDreadBuff);
                        }
                    }
                }
            }
        }
    }
}
