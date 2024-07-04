using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.HudOverlay;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using SpiritboundMod.Spiritbound.Content;
using System;

namespace SpiritboundMod.Spiritbound.Components
{
    public class SpiritboundController : MonoBehaviour
    {
        private CharacterBody characterBody;
        private ModelSkinController skinController;
        private ChildLocator childLocator;
        private CharacterModel characterModel;
        private Animator animator;
        private SkillLocator skillLocator;
        private SpiritMasterComponent spiritMasterComponent;
        public string currentSkinNameToken => this.skinController.skins[this.skinController.currentSkinIndex].nameToken;
        public string altSkinNameToken => SpiritboundSurvivor.SOULBOUND_PREFIX + "MASTERY_SKIN_NAME";

        public bool pauseTimer = false;

        private int currentStacks;

        public static float maxHealGain = 100f;

        public float healCounter;

        private float healStopwatchInterval;

        private Vector3 previousPosition = Vector3.zero;

        public Action onHealGained;
        private void Awake()
        {
            this.characterBody = this.GetComponent<CharacterBody>();
            ModelLocator modelLocator = this.GetComponent<ModelLocator>();
            this.childLocator = modelLocator.modelBaseTransform.GetComponentInChildren<ChildLocator>();
            this.animator = modelLocator.modelBaseTransform.GetComponentInChildren<Animator>();
            this.characterModel = modelLocator.modelBaseTransform.GetComponentInChildren<CharacterModel>();
            this.skillLocator = this.GetComponent<SkillLocator>();
            this.skinController = modelLocator.modelTransform.gameObject.GetComponent<ModelSkinController>();
            this.spiritMasterComponent = this.GetComponent<SpiritMasterComponent>();
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
                this.childLocator.FindChild("FireR").gameObject.GetComponent<ParticleSystemRenderer>().material = SpiritboundAssets.fireMatInFront;
                this.childLocator.FindChild("FireL").gameObject.GetComponent<ParticleSystemRenderer>().material = SpiritboundAssets.fireMatInFront;
                this.childLocator.FindChild("FireHead").gameObject.GetComponent<ParticleSystemRenderer>().material = SpiritboundAssets.fireMatInFront;
                this.childLocator.FindChild("FirePack").gameObject.GetComponent<ParticleSystemRenderer>().material = SpiritboundAssets.fireMatInFront;
            }
        }
        private void FixedUpdate()
        {
            healStopwatchInterval += Time.fixedDeltaTime;

            if (currentStacks != characterBody.GetBuffCount(SpiritboundBuffs.soulStacksBuff))
            {
                if (NetworkServer.active) characterBody.SetBuffCount(SpiritboundBuffs.soulStacksBuff.buffIndex, currentStacks);
            }

            if (healStopwatchInterval >= 0.25f && base.transform)
            {
                healStopwatchInterval = 0f;
                if (healCounter < 100f) healCounter += (base.transform.position - previousPosition).magnitude / 2f;
                else
                {
                    if (!characterBody.HasBuff(SpiritboundBuffs.spiritHealBuff))
                    {
                        if (NetworkServer.active) characterBody.SetBuffCount(SpiritboundBuffs.spiritHealBuff.buffIndex, 1);
                    }
                    healCounter = 100f;
                }
                previousPosition = base.transform.position;
                onHealGained?.Invoke();
            }

            if(spiritMasterComponent && spiritMasterComponent.spiritController.gameObject)
            {
                if(spiritMasterComponent.spiritController.gameObject.GetComponent<CharacterBody>().HasBuff(RoR2Content.Buffs.WarCryBuff))
                {
                    if(this.skillLocator.secondary.rechargeStopwatch < this.skillLocator.utility.finalRechargeInterval * 0.6f && this.skillLocator.secondary.rechargeStopwatch > 0f)
                    {
                        this.skillLocator.secondary.rechargeStopwatch = this.skillLocator.utility.finalRechargeInterval * 0.6f;
                    }
                }
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
