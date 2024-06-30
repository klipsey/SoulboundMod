using RoR2;
using SoulboundMod.Modules.Achievements;
using SoulboundMod.Soulbound;

namespace SoulboundMod.Soulbound.Achievements
{
    //automatically creates language tokens "ACHIEVMENT_{identifier.ToUpper()}_NAME" and "ACHIEVMENT_{identifier.ToUpper()}_DESCRIPTION" 
    [RegisterAchievement(identifier, unlockableIdentifier, null, null)]
    public class SoulboundMasterAchievement : BaseMasteryAchievement
    {
        public const string identifier = SoulboundSurvivor.SOULBOUND_PREFIX + "masteryAchievement";
        public const string unlockableIdentifier = SoulboundSurvivor.SOULBOUND_PREFIX + "masteryUnlockable";

        public override string RequiredCharacterBody => SoulboundSurvivor.instance.bodyName;

        //difficulty coeff 3 is monsoon. 3.5 is typhoon for grandmastery skins
        public override float RequiredDifficultyCoefficient => 3;
    }
}