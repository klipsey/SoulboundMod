using EntityStates;
using EntityStates.GravekeeperMonster.Weapon;
using RoR2;
using RoR2.Projectile;
using SoulboundMod.Modules.BaseStates;
using SoulboundMod.Spirit.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace SoulboundMod.Spirit.SkillStates
{
    public class SpiritRedirect : BaseState
    {
        public static float giveUpDuration = 3f;
        public static float speedCoefficient = 1.5f;

        public Vector3 travelPosition;

        public float minDistanceFromPoint;

        public bool isEmpower;

        public override void OnEnter()
        {
            base.OnEnter();

            characterBody.isSprinting = true;

            if (NetworkServer.active && isEmpower)
            {
                characterBody.AddTimedBuff(RoR2Content.Buffs.TeamWarCry, 6f);
                gameObject.GetComponent<SpiritController>().inFrenzy = true;
            }

            if (minDistanceFromPoint <= 0f) minDistanceFromPoint = 1f;

            PlayAnimation("FullBody, Override", "Dash", "Dash.playbackRate", giveUpDuration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            bool reachedDistance = Vector3.Distance(travelPosition, transform.position) <= minDistanceFromPoint;
            bool failedToReachDistance = fixedAge >= giveUpDuration;

            if ((reachedDistance || failedToReachDistance) && base.isAuthority)
            {
                if(reachedDistance && isEmpower) PerformInputs();
                outer.SetNextStateToMain();
            }

            if (isAuthority)
            {
                rigidbodyDirection.aimDirection = (travelPosition - transform.position).normalized;
                rigidbodyMotor.rootMotion += (travelPosition - transform.position).normalized * (speedCoefficient * moveSpeedStat * Time.fixedDeltaTime);
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
