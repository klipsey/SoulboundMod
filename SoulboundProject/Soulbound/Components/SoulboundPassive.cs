using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace SoulboundMod.Soulbound.Components
{
    public class SoulboundPassive : MonoBehaviour
    {
        public SkillDef soulboundPassive;

        public GenericSkill passiveSkillSlot;

        public bool isJump
        {
            get
            {
                if (soulboundPassive && passiveSkillSlot)
                {
                    return passiveSkillSlot.skillDef == soulboundPassive;
                }

                return false;
            }
        }
    }
}