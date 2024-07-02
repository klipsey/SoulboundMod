using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SoulboundMod.Soulbound.Content
{
    public static class SoulboundBuffs
    {
        public static BuffDef mountingDreadBuff;
        public static BuffDef soulStacksBuff;
        public static void Init(AssetBundle assetBundle)
        {
            mountingDreadBuff = Modules.Content.CreateAndAddBuff("SoulboundMountingDreadBuff", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/AttackSpeedOnCrit/texBuffAttackSpeedOnCritIcon.tif").WaitForCompletion(),
                SoulboundAssets.soulBoundColor, true, false, false);
            soulStacksBuff = Modules.Content.CreateAndAddBuff("SoulStacksBuff", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/AttackSpeedOnCrit/texBuffAttackSpeedOnCritIcon.tif").WaitForCompletion(),
                SoulboundAssets.soulBoundColor, true, false, false);
        }
    }
}
