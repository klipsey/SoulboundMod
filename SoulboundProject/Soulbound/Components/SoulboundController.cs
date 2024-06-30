using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.HudOverlay;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using SoulboundMod.Soulbound.Content;
using System;

namespace SoulboundMod.Soulbound.Components
{
    public class SoulboundController : MonoBehaviour
    {
        private CharacterBody characterBody;
        private ModelSkinController skinController;
        private ChildLocator childLocator;
        private CharacterModel characterModel;
        private Animator animator;
        private SkillLocator skillLocator;
        private Material[] swordMat;
        private Material[] batMat;
        public string currentSkinNameToken => this.skinController.skins[this.skinController.currentSkinIndex].nameToken;
        public string altSkinNameToken => SoulboundSurvivor.SOULBOUND_PREFIX + "MASTERY_SKIN_NAME";

        private bool hasPlayed = false;
        public bool isConvicted => this.characterBody.HasBuff(SoulboundBuffs.interrogatorConvictBuff);
        private bool stopwatchOut = false;
        public bool pauseTimer = false;
        public bool hitSelf { get; private set; }

        public float convictDurationMax;
        private void Awake()
        {
            this.characterBody = this.GetComponent<CharacterBody>();
            ModelLocator modelLocator = this.GetComponent<ModelLocator>();
            this.childLocator = modelLocator.modelBaseTransform.GetComponentInChildren<ChildLocator>();
            this.animator = modelLocator.modelBaseTransform.GetComponentInChildren<Animator>();
            this.characterModel = modelLocator.modelBaseTransform.GetComponentInChildren<CharacterModel>();
            this.skillLocator = this.GetComponent<SkillLocator>();
            this.skinController = modelLocator.modelTransform.gameObject.GetComponent<ModelSkinController>();

            this.Invoke("ApplySkin", 0.3f);
        }
        private void Start()
        {
        }
        #region tooMuchCrap
       
        private void ApplySkin()
        {
            if(this.skinController && this.childLocator)
            {
                this.childLocator.FindChild("FireR").gameObject.GetComponent<ParticleSystemRenderer>().material = SoulboundAssets.fireMat;
                this.childLocator.FindChild("FireL").gameObject.GetComponent<ParticleSystemRenderer>().material = SoulboundAssets.fireMat;
                this.childLocator.FindChild("FireHead").gameObject.GetComponent<ParticleSystemRenderer>().material = SoulboundAssets.fireMat;
                this.childLocator.FindChild("FirePack").gameObject.GetComponent<ParticleSystemRenderer>().material = SoulboundAssets.fireMat;
            }
        }
        #endregion
        private void FixedUpdate()
        {
        }

        private void OnDestroy()
        {
        }
    }
}
