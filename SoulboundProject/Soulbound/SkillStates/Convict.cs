using UnityEngine;
using EntityStates;
using SoulboundMod.Modules.BaseStates;
using RoR2;
using UnityEngine.AddressableAssets;
using SoulboundMod.Soulbound.Content;
using UnityEngine.Networking;
using SoulboundMod.Soulbound.Components;
using static RoR2.OverlapAttack;
using System;

namespace SoulboundMod.Soulbound.SkillStates
{
    public class Convict : BaseSoulboundSkillState
    {
        public GameObject markedPrefab = SoulboundAssets.interrogatorConvictedConsume;

        private float baseDuration = 0.5f;

        private float duration;

        private SoulboundTracker tracker;

        private HurtBox victim;

        private CharacterBody victimBody;

        private CameraTargetParams.AimRequest aimRequest;
        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            tracker = this.GetComponent<SoulboundTracker>();
            if(tracker)
            {
                victim = tracker.GetTrackingTarget();
                if(victim)
                {
                    victimBody = victim.healthComponent.body;
                    if((victimBody.HasBuff(SoulboundBuffs.interrogatorGuiltyDebuff) || characterBody.skillLocator.special.skillNameToken == SoulboundSurvivor.SOULBOUND_PREFIX + "SPECIAL_SCEPTER_CONVICT_NAME") && !victimBody.HasBuff(SoulboundBuffs.interrogatorConvictBuff))
                    {
                        if (base.cameraTargetParams)
                        {
                            aimRequest = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
                        }
                        StartAimMode(duration);
                        PlayAnimation("Gesture, Override", "Point", "Swing.playbackRate", duration * 1.5f);
                        EffectManager.SpawnEffect(markedPrefab, new EffectData
                        {
                            origin = victimBody.corePosition,
                            scale = 1.5f
                        }, transmit: true);

                        this.soulboundController.convictDurationMax = SoulboundStaticValues.baseConvictTimerMax + (this.characterBody.inventory.GetItemCount(DLC1Content.Items.EquipmentMagazineVoid) * 0.5f);

                        if (NetworkServer.active)
                        {
                            victimBody.AddTimedBuff(SoulboundBuffs.interrogatorConvictBuff, this.soulboundController.convictDurationMax);
                            characterBody.AddTimedBuff(SoulboundBuffs.interrogatorConvictBuff, this.soulboundController.convictDurationMax);
                        }
                    }
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.fixedAge >= duration)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            aimRequest?.Dispose();
        }
    }
}