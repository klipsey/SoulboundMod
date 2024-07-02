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
    public abstract class BaseSoulboundState : BaseState
    {
        protected SoulboundController soulboundController;

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
            if (!soulboundController)
            {
                soulboundController = base.GetComponent<SoulboundController>();
            }
        }
    }
}
