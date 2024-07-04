using SpiritboundMod.Spiritbound.Content;

namespace SpiritboundMod.Modules
{
    internal static class Tokens
    {
        public const string agilePrefix = "<style=cIsUtility>Agile</style>";

        public const string healingPrefix = "<color=#BBC6ED>Respite</color>";

        public static string agileKeyword = KeywordText("Agile", "The skill can be used while sprinting.");

        public static string slayerKeyword = KeywordText("Slayer", "The skill deals 2% more damage per 1% of health the target has lost, up to <style=cIsDamage>3x</style> damage.");

        public static string respiteKeyword = KeywordText("Respite", $"Moving builds up <color=#BBC6ED>Respite</color>. Hitting an enemy with <color=#7E8DC4>Puncture</color> when below <style=cIsHealth>100% max health</style> " +
            $"<style=cIsHealing>heals for {SpiritboundStaticValues.healCoefficient * 100f}%</style> <style=cIsHealth>missing health</style>.");

        public static string spiritBoundStacksKeyword = KeywordText("Stacks", $"Primary: Increased arrow velocity and <style=cIsDamage>{SpiritboundStaticValues.missingHPExecuteStacking * 100f}%</style> extra <style=cIsHealth>missing health</style> <style=cIsDamage>damage.</style> \n" +
            $"Secondary: Fires an additional arrow and an additional wisp. \n" +
            $"Utility: Increase explosion radius and fire an additional wisp. \n" +
            $"Special: <color=#39456e>Wolf</color> deals <style=cIsDamage>{SpiritboundStaticValues.currentHpStacking * 100f}%</style> extra <style=cIsHealth>current health</style> <style=cIsDamage>damage</style>. ");
        public static string DamageText(string text)
        {
            return $"<style=cIsDamage>{text}</style>";
        }
        public static string DamageValueText(float value)
        {
            return $"<style=cIsDamage>{value * 100}% damage</style>";
        }
        public static string UtilityText(string text)
        {
            return $"<style=cIsUtility>{text}</style>";
        }
        public static string RedText(string text) => HealthText(text);
        public static string HealthText(string text)
        {
            return $"<style=cIsHealth>{text}</style>";
        }
        public static string KeywordText(string keyword, string sub)
        {
            return $"<style=cKeywordName>{keyword}</style><style=cSub>{sub}</style>";
        }
        public static string ScepterDescription(string desc)
        {
            return $"\n<color=#d299ff>SCEPTER: {desc}</color>";
        }

        public static string GetAchievementNameToken(string identifier)
        {
            return $"ACHIEVEMENT_{identifier.ToUpperInvariant()}_NAME";
        }
        public static string GetAchievementDescriptionToken(string identifier)
        {
            return $"ACHIEVEMENT_{identifier.ToUpperInvariant()}_DESCRIPTION";
        }
    }
}