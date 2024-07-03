using RoR2;
using UnityEngine;
using SpiritboundMod.Spiritbound;
using SpiritboundMod.Spiritbound.Achievements;

namespace SpiritboundMod.Spiritbound.Content
{
    public static class SpiritboundUnlockables
    {
        public static UnlockableDef characterUnlockableDef = null;
        public static UnlockableDef masterySkinUnlockableDef = null;

        public static void Init()
        {
            /*
            masterySkinUnlockableDef = Modules.Content.CreateAndAddUnlockableDef(SpyMasteryAchievement.unlockableIdentifier,
                Modules.Tokens.GetAchievementNameToken(SpyMasteryAchievement.unlockableIdentifier),
                SpiritboundSurvivor.instance.assetBundle.LoadAsset<Sprite>("texMonsoonSkin"));
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
