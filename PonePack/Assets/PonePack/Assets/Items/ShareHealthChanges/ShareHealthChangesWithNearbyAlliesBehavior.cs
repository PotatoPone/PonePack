//using RoR2;
//using RoR2.Items;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Networking;
//using BepInEx;
//using R2API;

//namespace PonePack
//{
//    public class ShareHealthChangesWithNearbyAlliesBehavior : CharacterBody.ItemBehavior
//    {
//        private GameObject lanternCollider;
//        private static GameObject prefab;

//        private void Start()
//        {
//            //Not starting because I need an event to add this behavior
//            Debug.Log("ShareHealthChangesWithNearbyAlliesBehavior Started");

//            if (prefab == null)
//            {
//                prefab = LegacyResourcesAPI.Load<GameObject>("ShareHealthChangesCollider");
//            }
//            if (NetworkServer.active)
//            {
//                this.lanternCollider = UnityEngine.Object.Instantiate<GameObject>(prefab, this.body.corePosition, Quaternion.identity);
//                this.lanternCollider.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(base.gameObject, null);
//            }
//        }

//        private void OnEnable()
//        {

//        }

//        private void OnDisable()
//        {
//            if (this.body && this.body.HasBuff(PonePack.Buffs.ShareHealthChangesWithNearbyAlliesBuff))
//            {
//                this.body.SetBuffCount(PonePack.Buffs.ShareHealthChangesWithNearbyAlliesBuff.buffIndex, 0);
//            }
//            if (this.lanternCollider)
//            {
//                UnityEngine.Object.Destroy(this.lanternCollider);
//                this.lanternCollider = null;
//            }
//        }
//    }
//}



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

    public class ShareHealthChangesWithNearbyAlliesBehavior : BaseItemBodyBehavior
    {
        private GameObject lanternCollider;
        private static GameObject prefab;

        [BaseItemBodyBehavior.ItemDefAssociationAttribute(useOnServer = true, useOnClient = false)]
        public static ItemDef GetItemDef()
        {
            return PonePack.Items.ShareHealthChangesWithNearbyAllies;
        }

        private void OnEnable()
        {
            if (prefab == null)
            {
                prefab = PonePack.ItemObjects.ShareHealthChangesBonusIndicator;
            }

            if (NetworkServer.active)
            {
                this.lanternCollider = UnityEngine.Object.Instantiate<GameObject>(prefab, this.body.corePosition, Quaternion.identity);

                this.lanternCollider.AddComponent<NetworkedBodyAttachment>();
                this.lanternCollider.GetComponent<NetworkedBodyAttachment>().shouldParentToAttachedBody = true;

                //this.lanternCollider.GetComponent<ShareHealthChangesWithNearbyAllies>().body = this.body;

                this.lanternCollider.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(base.gameObject, null); //Ob ref not set to instance
            }


            //R2API.RecalculateStatsAPI.GetStatCoefficients += (body, statArgs) =>
            //{
            //    if (!body.inventory) return;

            //    int count = body.inventory.GetItemCount(PonePack.Items.ShareHealthChangesWithNearbyAllies);
            //    statArgs.moveSpeedMultAdd += 5f * count; //Applies to ALL entities??
            //};
        }

        private void OnDisable()
        {
            if (this.body && this.body.HasBuff(PonePack.Buffs.ShareHealthChangesWithNearbyAlliesBuff))
            {
                this.body.SetBuffCount(PonePack.Buffs.ShareHealthChangesWithNearbyAlliesBuff.buffIndex, 0);
            }
            if (this.lanternCollider)
            {
                UnityEngine.Object.Destroy(this.lanternCollider);
                this.lanternCollider = null;
            }
        }
    }
}