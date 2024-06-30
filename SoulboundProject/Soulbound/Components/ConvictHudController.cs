/*
using UnityEngine;
using UnityEngine.UI;
using RoR2;
using RoR2.UI;
using SoulboundMod.Soulbound.Components;
using SoulboundMod.Soulbound.Content;

namespace SoulboundMod.Soulbound.Components
{
    public class ConvictHudController : MonoBehaviour
    {
        public HUD targetHUD;
        public SoulboundController soulboundController;

        public LanguageTextMeshController targetText;
        public GameObject durationDisplay;
        public Image durationBar;
        public Image durationBarColor;

        private void Start()
        {
            this.soulboundController = this.targetHUD?.targetBodyObject?.GetComponent<SoulboundController>();
            this.soulboundController.onConvictDurationChange += SetDisplay;

            this.durationDisplay.SetActive(false);
            SetDisplay();
        }

        private void OnDestroy()
        {
            if (this.soulboundController) this.soulboundController.onConvictDurationChange -= SetDisplay;

            this.targetText.token = string.Empty;
            this.durationDisplay.SetActive(false);
            GameObject.Destroy(this.durationDisplay);
        }

        private void Update()
        {
            if(targetText.token != string.Empty) { targetText.token = string.Empty; }

            if(this.soulboundController && this.soulboundController.convictTimer > 0f)
            {
                float fill = this.soulboundController.convictTimer;

                if (this.durationBarColor)
                {
                    if (fill >= 1f) this.durationBarColor.fillAmount = 1f;
                    this.durationBarColor.fillAmount = Mathf.Lerp(this.durationBarColor.fillAmount, fill, Time.deltaTime * 2f);
                }

                this.durationBar.fillAmount = fill;
            }
            else if(this.durationDisplay.activeSelf == true && this.soulboundController.convictTimer <= 0f)
            {
                this.durationDisplay.SetActive(false);
            }
        }

        private void SetDisplay()
        {
            if (this.soulboundController)
            {
                this.durationDisplay.SetActive(true);
                this.targetText.token = string.Empty;

                this.durationBar.color = SoulboundAssets.interrogatorColor;
            }
            else
            {
                this.durationDisplay.SetActive(false);
            }
        }
    }
}
*/