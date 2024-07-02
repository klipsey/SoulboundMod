using SoulboundMod.Modules.BaseStates;
using SoulboundMod.Soulbound.SkillStates;
using SoulboundMod.Spirit.SkillStates;

namespace SoulboundMod.Soulbound.Content
{
    public static class SoulboundStates
    {
        public static void Init()
        {
            Modules.Content.AddEntityState(typeof(BaseSoulboundSkillState));
            Modules.Content.AddEntityState(typeof(MainState));
            Modules.Content.AddEntityState(typeof(BaseSoulboundState));

            Modules.Content.AddEntityState(typeof(HopFire));
            Modules.Content.AddEntityState(typeof(RedirectSpirit));
            Modules.Content.AddEntityState(typeof(SwapWithSpirit));
            Modules.Content.AddEntityState(typeof(ChargeArrow));

            Modules.Content.AddEntityState(typeof(SpiritBarrage));
            Modules.Content.AddEntityState(typeof(SpiritBite));
            Modules.Content.AddEntityState(typeof(SpiritLunge));
            Modules.Content.AddEntityState(typeof(SpiritRedirect));
            Modules.Content.AddEntityState(typeof(SpiritMainState));
        }
    }
}
