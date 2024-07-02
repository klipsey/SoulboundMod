using EntityStates;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SoulboundMod.Spirit.SkillStates
{
    internal class SpiritMainState : BaseState
    {
        private Animator modelAnimator;

        private bool skill1InputReceived;
        private bool skill2InputReceived;
        private bool skill3InputReceived;
        private bool skill4InputReceived;
        private bool sprintInputReceived;

        public override void OnEnter()
        {
            base.OnEnter();
            modelAnimator = GetModelAnimator();
            PlayAnimation("Body", "Idle");
            modelAnimator.SetFloat("Fly.rate", 1f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            PerformInputs();
        }

        public override void Update()
        {
            base.Update();
            if (Time.deltaTime <= 0) return;
            UpdateAnimParams();
        }

        private void PerformInputs()
        {
            if (isAuthority)
            {
                if (inputBank)
                {
                    if (rigidbodyMotor)
                    {
                        rigidbodyMotor.moveVector = inputBank.moveVector * characterBody.moveSpeed;
                    }
                    if (rigidbodyDirection)
                    {
                        if (inputBank.moveVector != Vector3.zero)
                            rigidbodyDirection.aimDirection = inputBank.moveVector;
                        else
                            rigidbodyDirection.aimDirection.y = 0f;
                    }

                    skill1InputReceived = inputBank.skill1.down;
                    skill2InputReceived = inputBank.skill2.down;
                    skill3InputReceived = inputBank.skill3.down;
                    skill4InputReceived = inputBank.skill4.down;
                    sprintInputReceived |= inputBank.sprint.down;

                    if (inputBank.moveVector.magnitude <= 0.5f) sprintInputReceived = false;
                    characterBody.isSprinting = sprintInputReceived;
                    if (sprintInputReceived) modelAnimator.SetFloat("Fly.rate", 1.5f);
                    sprintInputReceived = false;
                }

                if (skillLocator)
                {
                    if (skill1InputReceived && skillLocator.primary) skillLocator.primary.ExecuteIfReady();
                    if (skill2InputReceived && skillLocator.secondary) skillLocator.secondary.ExecuteIfReady();
                    if (skill3InputReceived && skillLocator.utility) skillLocator.utility.ExecuteIfReady();
                    if (skill4InputReceived && skillLocator.special) skillLocator.special.ExecuteIfReady();
                }
            }
        }

        private void UpdateAnimParams()
        {
            if (!modelAnimator) return;

            Vector3 moveVector = inputBank.moveVector;
            bool value = moveVector != Vector3.zero && characterBody.moveSpeed > Mathf.Epsilon;

            modelAnimator.SetBool("isMoving", value);
        }
    }
}
