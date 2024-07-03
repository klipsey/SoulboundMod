using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace SpiritboundMod.Spiritbound.Components
{
    public class SpiritboundPassive : MonoBehaviour
    {
        public SkillDef spiritboundPassive;

        public GenericSkill passiveSkillSlot;

        public bool isJump
        {
            get
            {
                if (spiritboundPassive && passiveSkillSlot)
                {
                    return passiveSkillSlot.skillDef == spiritboundPassive;
                }

                return false;
            }
        }
    }
}