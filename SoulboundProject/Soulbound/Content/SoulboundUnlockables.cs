using RoR2;
using UnityEngine;
using SoulboundMod.Soulbound;
using SoulboundMod.Soulbound.Achievements;

namespace SoulboundMod.Soulbound.Content
{
    public static class SoulboundUnlockables
    {
        public static UnlockableDef characterUnlockableDef = null;
        public static UnlockableDef masterySkinUnlockableDef = null;

        public static void Init()
        {
            /*
            masterySkinUnlockableDef = Modules.Content.CreateAndAddUnlockableDef(SpyMasteryAchievement.unlockableIdentifier,
                Modules.Tokens.GetAchievementNameToken(SpyMasteryAchievement.unlockableIdentifier),
                SoulboundSurvivor.instance.assetBundle.LoadAsset<Sprite>("texMonsoonSkin"));
            */
            /*
            if (true == false)
            {
                characterUnlockableDef = Modules.Content.CreateAndAddUnlockableDef(SpyUnlockAchievement.unlockableIdentifier,
                Modules.Tokens.GetAchievementNameToken(SpyUnlockAchievement.unlockableIdentifier),
                SpySurvivor.instance.assetBundle.LoadAsset<Sprite>("texSpyIcon"));
            }
            */
        }
    }
}
