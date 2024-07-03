using EntityStates;
using RoR2;
using SpiritboundMod.Spiritbound.Components;
using SpiritboundMod.Spiritbound.Content;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace SpiritboundMod.Modules.BaseStates
{
    public abstract class BaseSpiritboundSkillState : BaseSkillState
    {
        protected SpiritboundController spiritBoundController;
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
            if (!spiritBoundController)
            {
                spiritBoundController = base.GetComponent<SpiritboundController>();
            }
            if (!spiritMasterComponent)
            {
                spiritMasterComponent = base.GetComponent<SpiritMasterComponent>();
            }
        }
    }
}
