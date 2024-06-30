using BepInEx.Configuration;
using SoulboundMod.Modules;

namespace SoulboundMod.Soulbound.Content
{
    public static class SoulboundConfig
    {
        public static ConfigEntry<bool> forceUnlock;
        public static ConfigEntry<float> stabDamageCoefficient;
        public static ConfigEntry<float> maxCloakDefault;
        public static ConfigEntry<float> cloakHealthCost;
        public static ConfigEntry<float> maxCloakDead;
        public static ConfigEntry<float> bigEarnerHealthPunishment;
        public static ConfigEntry<bool> bigEarnerFullyResets;
        public static ConfigEntry<float> sapperRange;
        public static void Init()
        {
            string section = "01 - General";
            string section2 = "02 - Stats";

            //add more here or else you're cringe
            forceUnlock = Config.BindAndOptions(
                section,
                "Unlock Soulbound",
                false,
                "Unlock Soulbound.", true);
        }
    }
}
