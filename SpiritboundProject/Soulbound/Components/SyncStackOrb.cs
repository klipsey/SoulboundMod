using UnityEngine.Networking;
using R2API.Networking;
using R2API.Networking.Interfaces;
using UnityEngine;
using RoR2;
using SpiritboundMod.Spiritbound.Content;

namespace SpiritboundMod.Spiritbound.Components
{
    public class SyncStackOrb : INetMessage
    {
        private NetworkInstanceId netId;
        private GameObject target;

        public SyncStackOrb()
        {
        }

        public SyncStackOrb(NetworkInstanceId netId, GameObject target)
        {
            this.netId = netId;
            this.target = target;
        }

        public void Deserialize(NetworkReader reader)
        {
            this.netId = reader.ReadNetworkId();
            this.target = reader.ReadGameObject();
        }

        public void OnReceived()
        {
            GameObject bodyObject = Util.FindNetworkObject(this.netId);
            if (!bodyObject)
            {
                Chat.AddMessage("Fuck");
                return;
            }

            Transform modelTransform = target.GetComponent<CharacterBody>().modelLocator.modelTransform;
            if (modelTransform)
            {
                TemporaryOverlay temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = 1f;
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = SpiritboundAssets.absorbMaterial;
                temporaryOverlay.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.animateShaderAlpha = true;
            }

            Util.PlaySound("Play_item_proc_TPhealingNova", this.target.gameObject);
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(this.netId);
            writer.Write(this.target);
        }
    }
}