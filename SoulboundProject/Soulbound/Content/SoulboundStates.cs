using SoulboundMod.Modules.BaseStates;
using SoulboundMod.Soulbound.SkillStates;

namespace SoulboundMod.Soulbound.Content
{
    public static class SoulboundStates
    {
        public static void Init()
        {
            Modules.Content.AddEntityState(typeof(BaseSoulboundSkillState));
            Modules.Content.AddEntityState(typeof(MainState));
            Modules.Content.AddEntityState(typeof(BaseSoulboundState));
            Modules.Content.AddEntityState(typeof(SpiritBarrage));
            Modules.Content.AddEntityState(typeof(SpiritMainState));
        }
    }
}
