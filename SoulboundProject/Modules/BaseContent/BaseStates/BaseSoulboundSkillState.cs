using EntityStates;
using RoR2;
using SoulboundMod.Soulbound.Components;
using SoulboundMod.Soulbound.Content;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace SoulboundMod.Modules.BaseStates
{
    public abstract class BaseSoulboundSkillState : BaseSkillState
    {
        protected SoulboundController soulboundController;
        protected SpiritMasterComponent spiritMasterComponent;
        public virtual void AddRecoil2(float x1, float x2, float y1, float y2)
        {
            this.AddRecoil(x1, x2, y1, y2);
        }
        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
        protected void RefreshState()
        {
            if (!soulboundController)
            {
                soulboundController = base.GetComponent<SoulboundController>();
            }
            if (!spiritMasterComponent)
            {
                base.GetComponent<SpiritMasterComponent>();
            }
        }
    }
}
