using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SpiritboundMod.Spiritbound.Content
{
    public static class SpiritboundBuffs
    {
        public static BuffDef mountingDreadBuff;
        public static BuffDef soulStacksBuff;
        public static BuffDef spiritHealBuff;
        public static BuffDef spiritMovespeedStacksBuff;
        public static BuffDef quickShotBuff;
        public static void Init(AssetBundle assetBundle)
        {
            mountingDreadBuff = Modules.Content.CreateAndAddBuff("SpiritboundMountingDreadBuff", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/AttackSpeedOnCrit/texBuffAttackSpeedOnCritIcon.tif").WaitForCompletion(),
                SpiritboundAssets.spiritBoundColor, true, false, false);
            soulStacksBuff = Modules.Content.CreateAndAddBuff("SoulStacksBuff", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/AttackSpeedOnCrit/texBuffAttackSpeedOnCritIcon.tif").WaitForCompletion(),
                SpiritboundAssets.spiritBoundColor, true, false, false);
            spiritHealBuff = Modules.Content.CreateAndAddBuff("SpiritHealBuff", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Croco/texBuffRegenBoostIcon.tif").WaitForCompletion(),
                SpiritboundAssets.spiritBoundColor, false, false, false);
            spiritMovespeedStacksBuff = Modules.Content.CreateAndAddBuff("SpiritSpeedBuff", Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/MoveSpeedOnKill/texBuffKillMoveSpeed.tif").WaitForCompletion(),
                    SpiritboundAssets.spiritBoundColor, true, false, false);
            quickShotBuff = Modules.Content.CreateAndAddBuff("SpiritQuickShotBuff", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/AttackSpeedOnCrit/texBuffAttackSpeedOnCritIcon.tif").WaitForCompletion(),
                    SpiritboundAssets.spiritBoundColor, true, false, false);
        }
    }
}
