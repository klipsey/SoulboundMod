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
    public abstract class BaseSpiritboundState : BaseState
    {
        protected SpiritboundController spiritBoundController;

        public override void OnEnter()
        {
            base.OnEnter();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            RefreshState();
        }
        protected void RefreshState()
        {
            if (!spiritBoundController)
            {
                spiritBoundController = base.GetComponent<SpiritboundController>();
            }
        }
    }
}
