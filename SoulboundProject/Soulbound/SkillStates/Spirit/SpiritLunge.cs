using SoulboundMod.Modules.BaseStates;
using RoR2;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;
using SoulboundMod.Soulbound.Content;
using R2API;
using EntityStates;
using SoulboundMod.Soulbound.Components;

namespace SoulboundMod.Spirit.SkillStates
{
    public class SpiritLunge : BaseState
    {
        public static float giveUpDuration = 3f;

        public static float speedCoefficient = 6f;

        public CharacterBody victimBody;

        public float minDistanceFromPoint = 10f;

        public override void OnEnter()
        {
            base.OnEnter();

            characterBody.isSprinting = true;
            if (minDistanceFromPoint <= 0f) minDistanceFromPoint = 1f;

            PlayAnimation("FullBody, Override", "Dash", "Dash.playbackRate", giveUpDuration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            PerformInputs();

            bool reachedDistance = Vector3.Distance(victimBody.gameObject.transform.position - victimBody.footPosition, transform.position) <= minDistanceFromPoint;
            bool failedToReachDistance = fixedAge >= giveUpDuration;

            if ((reachedDistance || failedToReachDistance) && base.isAuthority)
            {
                PerformInputs();
                outer.SetNextStateToMain();
            }

            if (base.isAuthority)
            {
                rigidbodyDirection.aimDirection = (victimBody.gameObject.transform.position - victimBody.footPosition).normalized;
                rigidbodyMotor.rootMotion += (victimBody.gameObject.transform.position - victimBody.footPosition).normalized * (speedCoefficient * moveSpeedStat * Time.fixedDeltaTime);
            }
        }

        private void PerformInputs()
        {
            if (skillLocator)
            {
                if (inputBank.skill1.down && skillLocator.primary) skillLocator.primary.ExecuteIfReady();
            }
        }

        public override void OnExit()
        {
            characterBody.isSprinting = false;
            PlayCrossfade("FullBody, Override", "BufferEmpty", 0.2f);
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
