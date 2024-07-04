using EntityStates;
using MonoMod.RuntimeDetour;
using RoR2;
using SpiritboundMod.Modules.BaseStates;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SpiritboundMod.Spiritbound.SkillStates
{
    public class RedirectSpirit : BaseSpiritboundSkillState
    {
        public static float baseDuration = 0.7f;
        protected float duration;
        private Vector3 position;
        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / base.attackSpeedStat;

            base.StartAimMode(duration + 0.1f, false);

            base.PlayCrossfade("Gesture, Override", "Point", "Hand.playbackRate", duration, 0.1f);

            if (base.isAuthority)
            {
                RaycastHit hitInfo;
                Vector3 calcPosition = (!base.inputBank.GetAimRaycast(100f, out hitInfo) ? Vector3.MoveTowards(base.inputBank.GetAimRay().GetPoint(100f), base.transform.position, 5f) : Vector3.MoveTowards(hitInfo.point, base.transform.position, 5f));
                position = calcPosition;
            }

            spiritMasterComponent.RedirectOrder(position, 10f);

            Util.PlaySound("sfx_interrogator_point", base.gameObject);

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
