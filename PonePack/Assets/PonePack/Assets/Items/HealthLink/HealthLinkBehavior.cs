using RoR2;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx;
using R2API;
using UnityEngine.AddressableAssets;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace PonePack
{
    [BepInDependency(ItemAPI.PluginGUID)]

    public class HealthLinkBehavior : BaseItemBodyBehavior
    {
        private NetworkedBodyAttachment attachment;
        private GameObject healthLinkBodyAttachment;
        //private static GameObject prefab;

        [BaseItemBodyBehavior.ItemDefAssociationAttribute(useOnServer = true, useOnClient = false)]
        public static ItemDef GetItemDef()
        {
            return PonePack.Items.HealthLink;
        }

        private void OnEnable()
        {
            if (NetworkServer.active)
            {
                this.healthLinkBodyAttachment = Instantiate(PonePack.ItemObjects.HealthLinkBodyAttachment);
                this.attachment = this.healthLinkBodyAttachment.GetComponent<NetworkedBodyAttachment>();

                if (!this.attachment) Debug.LogWarning("healthLinkBodyAttachment doesn't have a NetworkedBodyAttachment");

                this.attachment.AttachToGameObjectAndSpawn(this.body.gameObject);
            }
        }

        private void OnDisable()
        {
            if (this.body && this.body.HasBuff(PonePack.Buffs.ShareHealthChangesWithNearbyAlliesBuff))
            {
                this.body.SetBuffCount(PonePack.Buffs.ShareHealthChangesWithNearbyAlliesBuff.buffIndex, 0);
            }

            if (this.attachment)
            {
                UnityEngine.Object.Destroy(this.attachment.gameObject);
                this.attachment = null;
            }
        }
    }
}