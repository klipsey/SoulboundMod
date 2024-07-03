using RoR2;
using SpiritboundMod.Modules.Achievements;
using SpiritboundMod.Spiritbound;

namespace SpiritboundMod.Spiritbound.Achievements
{
    //automatically creates language tokens "ACHIEVMENT_{identifier.ToUpper()}_NAME" and "ACHIEVMENT_{identifier.ToUpper()}_DESCRIPTION" 
    [RegisterAchievement(identifier, unlockableIdentifier, null, null)]
    public class SpiritboundMasterAchievement : BaseMasteryAchievement
    {
        public const string identifier = SpiritboundSurvivor.SOULBOUND_PREFIX + "masteryAchievement";
        public const string unlockableIdentifier = SpiritboundSurvivor.SOULBOUND_PREFIX + "masteryUnlockable";

        public override string RequiredCharacterBody => SpiritboundSurvivor.instance.bodyName;

        //difficulty coeff 3 is monsoon. 3.5 is typhoon for grandmastery skins
        public override float RequiredDifficultyCoefficient => 3;
    }
}