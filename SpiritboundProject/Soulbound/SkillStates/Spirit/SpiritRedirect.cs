using EntityStates;
using EntityStates.GravekeeperMonster.Weapon;
using RoR2;
using UnityEngine.Networking;
using SpiritboundMod.Spirit.Components;
using UnityEngine;
using RoR2.Navigation;
using System.Linq;
using R2API;

namespace SpiritboundMod.Spirit.SkillStates
{
    public class SpiritRedirect : SpiritLunge
    {
        public bool isEmpower;
        public override void OnEnter()
        {
            base.OnEnter();

            if (NetworkServer.active && isEmpower)
            {
                characterBody.AddTimedBuff(RoR2Content.Buffs.WarCryBuff, 8f);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if(isEmpower) base.gameObject.GetComponent<SpiritController>().inFrenzy = true;
        }
    }
}
