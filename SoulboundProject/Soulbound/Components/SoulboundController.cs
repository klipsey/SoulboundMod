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

        public bool pauseTimer = false;

        private int currentStacks;
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
            currentStacks = 0;
        }
       
        private void ApplySkin()
        {
            if(this.skinController && this.childLocator)
            {
                this.childLocator.FindChild("FireR").gameObject.GetComponent<ParticleSystemRenderer>().material = SoulboundAssets.fireMatInFront;
                this.childLocator.FindChild("FireL").gameObject.GetComponent<ParticleSystemRenderer>().material = SoulboundAssets.fireMatInFront;
                this.childLocator.FindChild("FireHead").gameObject.GetComponent<ParticleSystemRenderer>().material = SoulboundAssets.fireMatInFront;
                this.childLocator.FindChild("FirePack").gameObject.GetComponent<ParticleSystemRenderer>().material = SoulboundAssets.fireMatInFront;
            }
        }
        private void FixedUpdate()
        {
            if(currentStacks != characterBody.GetBuffCount(SoulboundBuffs.soulStacksBuff))
            {
                if (NetworkServer.active) characterBody.SetBuffCount(SoulboundBuffs.soulStacksBuff.buffIndex, currentStacks);
            }
        }

        public void AddStackInternally()
        {
            currentStacks++;
        }
        private void OnDestroy()
        {
        }
    }
}
