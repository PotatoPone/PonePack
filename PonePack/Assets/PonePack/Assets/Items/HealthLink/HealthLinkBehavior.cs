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
                //-- WORKED?
                //Debug.Log("Spawning HealthLinkBodyAttachment...");
                //this.attachment = UnityEngine.Object.Instantiate<GameObject>(prefab).GetComponent<NetworkedBodyAttachment>();
                //this.attachment.AttachToGameObjectAndSpawn(this.body.gameObject, null);



                //Debug.Log("Spawning healthLinkBodyAttachment...");
                //this.healthLinkBodyAttachment = PonePack.ItemObjects.HealthLinkBodyAttachment.InstantiateClone(name + "healthLinkBodyAttachment", true);
                //this.healthLinkBodyAttachment.RegisterNetworkPrefab();
                //this.attachment = this.healthLinkBodyAttachment.GetComponent<NetworkedBodyAttachment>();

                //if (!this.attachment)
                //{
                //    Debug.LogWarning("healthLinkBodyAttachment doesn't have a NetworkedBodyAttachment. Creating one...");
                //    this.attachment = this.healthLinkBodyAttachment.AddComponent<NetworkedBodyAttachment>();
                //    this.attachment.shouldParentToAttachedBody = true;
                //}

                //this.attachment.AttachToGameObjectAndSpawn(this.body.gameObject);


                //-- healthLinkBodyAttachment
                this.healthLinkBodyAttachment = Instantiate(PonePack.ItemObjects.HealthLinkBodyAttachment);
                this.attachment = this.healthLinkBodyAttachment.GetComponent<NetworkedBodyAttachment>();

                if (!this.attachment) Debug.LogWarning("healthLinkBodyAttachment doesn't have a NetworkedBodyAttachment");

                this.attachment.AttachToGameObjectAndSpawn(this.body.gameObject);


                //-- ReplicationCube
                Debug.Log("Spawning ReplicationCube...");
                GameObject cube = Instantiate(PonePack.ItemObjects.ReplicationCube);
                NetworkServer.Spawn(cube);
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