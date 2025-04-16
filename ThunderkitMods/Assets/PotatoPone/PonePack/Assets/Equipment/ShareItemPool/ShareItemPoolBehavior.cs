using RoR2;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx;
using R2API;
using System;
using System.Linq;

namespace PonePack
{
    [BepInDependency(ItemAPI.PluginGUID)]

    public class ShareItemPoolBehavior : BaseModdedEquipment
    {
        private float shareDuration = 4f;

        private CharacterBody recipientBody;
        private Inventory activatorInventoryCache;
        private Inventory recipientInventoryCache;
        private Inventory sharedInventoryCache;
        private GameObject shareItemPoolAttachment;

        private bool isActive = false;
        private float timer = 0f;

        private void Awake()
        {
            this.enabled = false;
        }

        private void Update()
        {
            if (!this.isActive) return;

            if (this.timer > 0f)
            {
                this.timer -= Time.deltaTime;
            }
            else
            {
                this.isActive = false;
                RevertItemPools();
            }
        }

        private void OnEnable()
        {
            if (NetworkServer.active && this.body && this.body.isPlayerControlled)
            {
                this.shareItemPoolAttachment = Instantiate(PonePack.NetworkedObjects.ShareItemPoolAttachment, this.body.coreTransform);

                this.activatorInventoryCache = this.shareItemPoolAttachment.AddComponent<Inventory>();
                this.recipientInventoryCache = this.shareItemPoolAttachment.AddComponent<Inventory>();

                On.RoR2.EquipmentSlot.PerformEquipmentAction += OnEquipmentUsed;
                On.RoR2.EquipmentSlot.UpdateTargets += OnUpdateTargets;
                On.RoR2.Inventory.RpcItemAdded += OnItemAdded;
            }
        }

        private void OnDisable()
        {
            if (NetworkServer.active && this.body && this.body.isPlayerControlled)
            {
                if (this.shareItemPoolAttachment) Destroy(this.shareItemPoolAttachment);

                On.RoR2.EquipmentSlot.PerformEquipmentAction -= OnEquipmentUsed;
                On.RoR2.EquipmentSlot.UpdateTargets -= OnUpdateTargets;
                On.RoR2.Inventory.RpcItemAdded -= OnItemAdded;
            }
        }

        private void RevertItemPools()
        {
            if (this.body)
            {
                this.body.inventory.CopyItemsFrom(this.activatorInventoryCache);
                this.activatorInventoryCache.CleanInventory();
            }

            if (this.recipientBody)
            {
                this.recipientBody.inventory.CopyItemsFrom(this.recipientInventoryCache);
                this.recipientInventoryCache.CleanInventory();
            }
        }

        // If either player adds an item, add that item to their cached inventories as well
        private void OnItemAdded(On.RoR2.Inventory.orig_RpcItemAdded orig, Inventory self, ItemIndex itemIndex)
        {
            orig(self, itemIndex);

            CharacterBody characterBody = self.gameObject.GetComponent<CharacterBody>();

            if (this.isActive && characterBody && self != this.activatorInventoryCache && self != this.recipientInventoryCache)
            {
                if (characterBody == this.recipientBody || characterBody == this.body)
                {
                    this.activatorInventoryCache.GiveItem(itemIndex);
                    this.recipientInventoryCache.GiveItem(itemIndex);
                }
            }
        }

        private void OnUpdateTargets(On.RoR2.EquipmentSlot.orig_UpdateTargets orig, EquipmentSlot self, EquipmentIndex targetingEquipmentIndex, bool userShouldAnticipateTarget)
        {
            if (targetingEquipmentIndex == PonePack.Equipment.ShareItemPool.equipmentIndex)
            {
                //orig(self, RoR2Content.Equipment.PassiveHealing.equipmentIndex, true);
                base.UpdateTargets(targetingEquipmentIndex, true);
            }
            else
            {
                orig(self, targetingEquipmentIndex, userShouldAnticipateTarget);
            }
        }

        private bool OnEquipmentUsed(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            bool result = orig(self, equipmentDef);

            if (equipmentDef == PonePack.Equipment.ShareItemPool)
            {
                if (base.currentTarget.rootObject == null) return false;
                if (base.currentTarget.rootObject.GetComponent<CharacterBody>() == null) return false;

                CharacterBody targetBody = base.currentTarget.rootObject.GetComponent<CharacterBody>();
                this.recipientBody = targetBody;

                //Set caches
                this.activatorInventoryCache.CopyItemsFrom(this.body.inventory);
                this.recipientInventoryCache.CopyItemsFrom(this.recipientBody.inventory);

                //Sync inventories
                this.body.inventory.AddItemsFrom(this.recipientBody.inventory);
                this.recipientBody.inventory.AddItemsFrom(this.body.inventory);

                this.isActive = true;
                this.timer = this.shareDuration;

                return true;
            }

            return result;
        }
    }
}