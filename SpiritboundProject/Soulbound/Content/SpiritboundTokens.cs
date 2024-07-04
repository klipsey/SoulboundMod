using System;
using SpiritboundMod.Modules;
using SpiritboundMod.Spiritbound;
using SpiritboundMod.Spiritbound.Achievements;
using UnityEngine.UIElements;

namespace SpiritboundMod.Spiritbound.Content
{
    public static class SpiritboundTokens
    {
        public static void Init()
        {
            AddSpiritboundTokens();

            ////uncomment this to spit out a lanuage file with all the above tokens that people can translate
            ////make sure you set Language.usingLanguageFolder and printingEnabled to true
            //Language.PrintOutput("Spy.txt");
            //todo guide
            ////refer to guide on how to build and distribute your mod with the proper folders
        }

        public static void AddSpiritboundTokens()
        {
            #region Spiritbound
            string prefix = SpiritboundSurvivor.SOULBOUND_PREFIX;
            string prefixWolf = SpiritCharacter.SPIRIT_PREFIX;
            string desc = "mmmmfgfmmf kindred....<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > https://leagueoflegends.fandom.com/wiki/Kindred" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > https://leagueoflegends.fandom.com/wiki/Kindred" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > https://leagueoflegends.fandom.com/wiki/Kindred" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > https://leagueoflegends.fandom.com/wiki/Kindred" + Environment.NewLine + Environment.NewLine;

            string lore = "Tell me again, little Lamb, which things are mine to take? \n\n All things, dear Wolf.";
            string outro = "..and so they left, where Lamb went... Wolf was sure to follow.";
            string outroFailure = "..and so they vanished, standing still in life.";
            
            Language.Add(prefix + "NAME", "Spiritbound");
            Language.Add(prefixWolf + "NAME", "Spirit");
            Language.Add(prefix + "DESCRIPTION", desc);
            Language.Add(prefix + "SUBTITLE", "The Eternal");
            Language.Add(prefix + "LORE", lore);
            Language.Add(prefix + "OUTRO_FLAVOR", outro);
            Language.Add(prefix + "OUTRO_FAILURE", outroFailure);
            //Lambs Color Code #BBC6ED
            //Wolfs Color Code #39456e
            #region Skins
            Language.Add(prefix + "MASTERY_SKIN_NAME", "Alternate");
            #endregion

            #region Passive
            Language.Add(prefix + "PASSIVE_NAME", "Mark of the Grove");
            Language.Add(prefix + "PASSIVE_DESCRIPTION", $"<color=#7E8DC4>Spiritbound</color> collects <color=#7E8DC4>spirits</color> upon <style=cIsHealth>defeating</style> boss enemies. " +
                $"Each <color=#7E8DC4>spirit</color> permanently <style=cIsUtility>empowers</style> certain abilities.");
            #endregion

            #region Primary
            Language.Add(prefix + "PRIMARY_BOW_NAME", "Puncture");
            Language.Add(prefix + "PRIMARY_BOW_DESCRIPTION", Tokens.healingPrefix + $". <color=#BBC6ED>Lamb</color> charges an arrow dealing <style=cIsDamage>{SpiritboundStaticValues.arrowBaseDamageCoefficient * 100f}%-{SpiritboundStaticValues.arrowFullDamageCoefficient * 100f}% damage</style>. " +
                $"Fully charged arrows apply <color=#39456e>mounting dread</color>. Upon reaching 3 stacks of <color=#39456e>mounting dread</color>, <color=#39456e>Wolf</color> lunges dealing " +
                $"<style=cIsDamage>{SpiritboundStaticValues.missingHPExecuteDamage * 100f}%</style> of the targets <style=cIsHealth>missing health</style>.");
            #endregion

            #region Secondary
            Language.Add(prefix + "SECONDARY_HOP_NAME", "Frolick");
            Language.Add(prefix + "SECONDARY_HOP_DESCRIPTION", $"<color=#BBC6ED>Lamb</color> hops in a direction firing <style=cIsDamage>3</style> arrows dealing " +
                $"<style=cIsDamage>{SpiritboundStaticValues.leapArrowDamageCoefficient * 100f}%</style> and gains <style=cIsDamage>temporary attackspeed</style> for <color=#7E8DC4>Puncture</color>. " +
                $"<color=#7E8DC4>Spiritbound</color> then fire homing wisps that deal <style=cIsDamage>{SpiritboundStaticValues.wispDamageCoefficient * 100f}% damage</style>.");
            #endregion

            #region Utility 
            Language.Add(prefix + "UTILITY_SWAP_NAME", "Interlinked");
            Language.Add(prefix + "UTILITY_SWAP_DESCRIPTION", $"<color=#BBC6ED>Lamb</color> swaps positions with <color=#39456e>Wolf</color>. " +
                $"<color=#7E8DC4>Spiritbound</color> deal <style=cIsDamage>{SpiritboundStaticValues.swapDamageCoefficient * 100f}% damage</style> in an area and fire " +
                $"homing wisps that deal <style=cIsDamage>{SpiritboundStaticValues.wispDamageCoefficient * 100f}% damage</style>.");

            #endregion

            #region Special
            Language.Add(prefix + "SPECIAL_REDIRECT_NAME", "Frenzy");
            Language.Add(prefix + "SPECIAL_REDIRECT_DESCRIPTION", $"<color=#39456e>Wolf</color> is sent into a <style=cIsDamage>frenzy</style> and lunges to a targeted location. During the <style=cIsDamage>frenzy</style>, " +
                $"<color=#7E8DC4>Frolick's</color> <style=cIsUtility>cooldown is lowered by 60%</style>. After the <style=cIsDamage>frenzy</style> ends, <color=#39456e>Wolf</color> returns to <color=#BBC6ED>Lamb</color>.");
            #endregion

            #region Achievements
            Language.Add(Tokens.GetAchievementNameToken(SpiritboundMasterAchievement.identifier), "Spiritbound: Mastery");
            Language.Add(Tokens.GetAchievementDescriptionToken(SpiritboundMasterAchievement.identifier), "As Spiritbound, beat the game or obliterate on Monsoon.");
            /*
            Language.Add(Tokens.GetAchievementNameToken(SpyUnlockAchievement.identifier), "Dressed to Kill");
            Language.Add(Tokens.GetAchievementDescriptionToken(SpyUnlockAchievement.identifier), "Get a Backstab.");
            */
            #endregion

            #endregion
        }
    }
}