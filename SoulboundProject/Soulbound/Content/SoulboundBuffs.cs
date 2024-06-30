using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SoulboundMod.Soulbound.Content
{
    public static class SoulboundBuffs
    {
        public static BuffDef interrogatorGuiltyDebuff;
        public static BuffDef interrogatorGuiltyBuff;
        public static BuffDef interrogatorPressuredBuff;
        public static BuffDef interrogatorConvictBuff;
        public static void Init(AssetBundle assetBundle)
        {
            interrogatorGuiltyBuff = Modules.Content.CreateAndAddBuff("SoulboundGuiltyBuff", assetBundle.LoadAsset<Sprite>("texGuiltyBuff"),
                SoulboundAssets.interrogatorColor, true, false, false);

            interrogatorGuiltyDebuff = Modules.Content.CreateAndAddBuff("SoulboundGuiltyDebuff", assetBundle.LoadAsset<Sprite>("texGuiltyDebuff"),
                SoulboundAssets.interrogatorColor, false, true, false);

            interrogatorPressuredBuff = Modules.Content.CreateAndAddBuff("SoulboundPressuredDebuff", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/CritOnUse/texBuffFullCritIcon.tif").WaitForCompletion(),
                SoulboundAssets.interrogatorColor, false, false, false);
            
            interrogatorConvictBuff = Modules.Content.CreateAndAddBuff("SoulboundConvictBuff", assetBundle.LoadAsset<Sprite>("texConvictBuff"), 
                SoulboundAssets.interrogatorColor, true, false, false);
        }
    }
}
