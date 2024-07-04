using UnityEngine;
using UnityEngine.UI;
using RoR2;
using RoR2.UI;
using SpiritboundMod.Spiritbound.Components;
using SpiritboundMod.Spiritbound.Content;

namespace SpiritboundMod.Spiritbound.Components
{
    public class PassiveHealHudController : MonoBehaviour
    {
        public HUD targetHUD;
        public SpiritboundController spiritBoundController;

        public LanguageTextMeshController targetText;
        public GameObject durationDisplay;
        public Image durationBar;
        public Image durationBarColor;

        private void Start()
        {
            this.spiritBoundController = this.targetHUD?.targetBodyObject?.GetComponent<SpiritboundController>();
            this.spiritBoundController.onHealGained += SetDisplay;

            this.durationDisplay.SetActive(false);
            SetDisplay();
        }

        private void OnDestroy()
        {
            if (this.spiritBoundController) this.spiritBoundController.onHealGained -= SetDisplay;

            this.targetText.token = string.Empty;
            this.durationDisplay.SetActive(false);
            GameObject.Destroy(this.durationDisplay);
        }

        private void Update()
        {
            if (targetText.token != string.Empty) { targetText.token = string.Empty; }

            if (this.spiritBoundController && this.spiritBoundController.healCounter >= 0f)
            {
                float fill;
                fill = Util.Remap(this.spiritBoundController.healCounter, 0f, 100f, 0f, 1f);

                if (this.durationBarColor)
                {
                    if (fill >= 1f) this.durationBarColor.fillAmount = 1f;
                    this.durationBarColor.fillAmount = Mathf.Lerp(this.durationBarColor.fillAmount, fill, Time.deltaTime * 2f);
                }

                this.durationBar.fillAmount = fill;
            }
        }

        private void SetDisplay()
        {
            if (this.spiritBoundController)
            {
                this.durationDisplay.SetActive(true);
                this.targetText.token = string.Empty;

                this.durationBar.color = SpiritboundAssets.spiritBoundColor;
            }
            else
            {
                this.durationDisplay.SetActive(false);
            }
        }
    }
}
