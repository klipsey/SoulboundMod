using SpiritboundMod.Modules.BaseStates;
using RoR2;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;
using SpiritboundMod.Spiritbound.Content;
using R2API;
using EntityStates;
using SpiritboundMod.Spiritbound.Components;

namespace SpiritboundMod.Spirit.SkillStates
{
    public class SpiritLunge : BaseState
    {
        public static float giveUpDuration = 3f;

        public static float speedCoefficient = 2f;

        public Vector3 position;

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

            bool reachedDistance = Vector3.Distance(position, base.transform.position) <= minDistanceFromPoint;
            bool failedToReachDistance = fixedAge >= giveUpDuration;

            if ((reachedDistance || failedToReachDistance) && base.isAuthority)
            {
                PerformInputs();
                outer.SetNextStateToMain();
            }

            if (base.isAuthority)
            {
                base.rigidbodyDirection.aimDirection = (position - base.transform.position).normalized;
                base.rigidbodyMotor.rootMotion += (position - base.transform.position).normalized * (speedCoefficient * moveSpeedStat * Time.fixedDeltaTime);
            }
        }

        private void PerformInputs()
        {
            if (skillLocator)
            {
                if (inputBank.skill1.down && skillLocator.primary) skillLocator.primary.ExecuteIfReady();
                if (inputBank.skill2.down && skillLocator.secondary) skillLocator.secondary.ExecuteIfReady();
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
