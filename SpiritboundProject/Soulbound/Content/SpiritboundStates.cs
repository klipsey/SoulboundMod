using SpiritboundMod.Modules.BaseStates;
using SpiritboundMod.Spiritbound.SkillStates;
using SpiritboundMod.Spirit.SkillStates;

namespace SpiritboundMod.Spiritbound.Content
{
    public static class SpiritboundStates
    {
        public static void Init()
        {
            Modules.Content.AddEntityState(typeof(BaseSpiritboundSkillState));
            Modules.Content.AddEntityState(typeof(MainState));
            Modules.Content.AddEntityState(typeof(BaseSpiritboundState));

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
