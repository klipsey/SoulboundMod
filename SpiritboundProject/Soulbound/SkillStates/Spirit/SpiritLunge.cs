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

        public static float speedCoefficient = 6f;

        public Vector3 position;

        public float minDistanceFromPoint = 10f;

        public override void OnEnter()
        {
            base.OnEnter();

            base.characterBody.isSprinting = true;
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
                if (failedToReachDistance) this.characterMotor.Motor.SetPosition(position);
                this.outer.SetNextStateToMain();
            }

            if (base.isAuthority)
            {
                base.rigidbodyDirection.aimDirection = (position - base.transform.position).normalized;
                base.rigidbodyMotor.rootMotion += (position - base.transform.position).normalized * (speedCoefficient * moveSpeedStat * Time.fixedDeltaTime);
            }
        }

        private void PerformInputs()
        {
            if (base.skillLocator)
            {
                if (base.inputBank.skill1.down && base.skillLocator.primary) base.skillLocator.primary.ExecuteIfReady();
                if (base.inputBank.skill2.down && base.skillLocator.secondary) base.skillLocator.secondary.ExecuteIfReady();
                if (base.inputBank.skill3.down && base.skillLocator.utility) base.skillLocator.utility.ExecuteIfReady();
                if (base.inputBank.skill4.down && base.skillLocator.special) base.skillLocator.special.ExecuteIfReady();
            }
        }

        public override void OnExit()
        {
            base.characterBody.isSprinting = false;
            base.PlayCrossfade("FullBody, Override", "BufferEmpty", 0.2f);
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
