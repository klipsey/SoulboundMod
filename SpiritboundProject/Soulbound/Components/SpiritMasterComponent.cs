using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;
using SpiritboundMod.Spiritbound;
using System.Linq;
using SpiritboundMod.Spirit.Components;
using UnityEngine.UIElements;

namespace SpiritboundMod.Spiritbound.Components
{
    public class SpiritMasterComponent : MonoBehaviour, ILifeBehavior
    {
        internal static GameObject summonPrefab;

        private CharacterMaster spiritMaster;
        private CharacterMaster characterMaster;
        private CharacterBody characterBody;

        private Vector3 verticalOffset = new Vector3(0f, 10f, 0f);

        public SpiritController spiritController;
        private void Start()
        {
            characterBody = base.GetComponent<CharacterBody>();
            characterMaster = characterBody.master;

            FindOrSummonSpirit();
            Subscriptions();
        }

        private void Subscriptions()
        {
            characterBody.onInventoryChanged += SelfBody_onInventoryChanged;
        }

        private void SelfBody_onInventoryChanged()
        {
            if (spiritMaster)
            {
                spiritMaster.inventory.CopyItemsFrom(characterBody.inventory);
                spiritMaster.inventory.CopyEquipmentFrom(characterBody.inventory);
                CleanSpiritInventory(spiritMaster.inventory);

                if (spiritMaster.inventory.GetItemCount(RoR2Content.Items.MinionLeash) < 1) { }
                spiritMaster.inventory.GiveItem(RoR2Content.Items.MinionLeash);
            }
        }

        private void FindOrSummonSpirit()
        {
            if (NetworkServer.active)
            {
                var minions = CharacterMaster.readOnlyInstancesList.Where(el => el.minionOwnership.ownerMaster == characterMaster);
                foreach (CharacterMaster minion in minions)
                {
                    if (minion.masterIndex == MasterCatalog.FindMasterIndex(summonPrefab))
                    {
                        spiritMaster = minion;
                        if (!spiritMaster.hasBody) spiritMaster.Respawn(base.transform.position + verticalOffset, Quaternion.identity);
                        if (!spiritMaster.godMode) spiritMaster.ToggleGod();
                        spiritController = minion.bodyInstanceObject.GetComponent<SpiritController>();
                        spiritController.owner = base.gameObject;
                        spiritController.ApplySpiritSkin(characterBody);
                        return;
                    }
                }

                if (!spiritMaster)
                {
                    SpawnSpirit(characterBody);
                }
            }
        }

        private void SpawnSpirit(CharacterBody characterBody)
        {
            MasterSummon minionSummon = new MasterSummon();
            minionSummon.masterPrefab = summonPrefab;
            minionSummon.ignoreTeamMemberLimit = false;
            minionSummon.teamIndexOverride = TeamIndex.Player;
            minionSummon.summonerBodyObject = characterBody.gameObject;
            minionSummon.inventoryToCopy = characterBody.inventory;
            minionSummon.position = characterBody.corePosition + verticalOffset;
            minionSummon.rotation = characterBody.transform.rotation;

            if (spiritMaster = minionSummon.Perform())
            {
                if (!spiritMaster.godMode) spiritMaster.ToggleGod();
                spiritMaster.inventory.GiveItem(RoR2Content.Items.MinionLeash);
                CleanSpiritInventory(spiritMaster.inventory);
                spiritController = spiritMaster.bodyInstanceObject.GetComponent<SpiritController>();
                spiritController.owner = base.gameObject;
            }
        }

        private void CleanSpiritInventory(Inventory inventory)
        {
            if (inventory.itemAcquisitionOrder.Count == 0) return;

            foreach (string itemName in SpiritCharacter.spiritItemBlackList)
            {
                var itemIndex = ItemCatalog.FindItemIndex(itemName);
                var itemCount = inventory.GetItemCount(itemIndex);
                if (itemCount > 0)
                {
                    inventory.RemoveItem(itemIndex, itemCount);
                }
            }
        }

        public void LungeOrder(CharacterBody victimBody)
        {
            if (!spiritController) FindOrSummonSpirit();
            if (victimBody)
            {
                spiritController.LungeAtEnemy(victimBody);
                spiritController.SetTarget(victimBody.mainHurtBox);
                spiritController.EnterAttackMode();
            }
        }
        public void SpiritOrbOrder()
        {
            if (!spiritController) FindOrSummonSpirit();
            spiritController.FreeOrb();
            spiritController.EnterAttackMode();
        }
        public void FollowOrder()
        {
            if (!spiritController) FindOrSummonSpirit();

            if (Vector3.Distance(characterBody.corePosition, spiritController.characterBody.corePosition) >= 1000f)
            {
                Vector3 teleportPosition = characterBody.corePosition + verticalOffset;
                TeleportHelper.TeleportBody(spiritController.characterBody, teleportPosition);

                GameObject teleportEffectPrefab = Run.instance.GetTeleportEffectPrefab(base.gameObject);
                if (teleportEffectPrefab)
                {
                    EffectManager.SimpleEffect(teleportEffectPrefab, teleportPosition, Quaternion.identity, true);
                }
            }
            else
            {
                Vector3 travelPosition = characterBody.corePosition + verticalOffset;
                spiritController.Redirect(travelPosition, 10f, false);
            }

            spiritController.EnterFollowMode();
        }

        public void RedirectOrder(Vector3 position, float minDistance)
        {
            if (!spiritController) FindOrSummonSpirit();

            spiritController.Redirect(position, minDistance);
        }

        public void OnDeathStart()
        {
            if (spiritMaster)
            {
                if (spiritMaster.godMode) spiritMaster.ToggleGod();

                spiritMaster.TrueKill();
            }
        }
    }
}
