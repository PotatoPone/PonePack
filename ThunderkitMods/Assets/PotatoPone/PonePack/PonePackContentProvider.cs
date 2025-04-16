using RoR2.ContentManagement;
using UnityEngine;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using R2API;
using System;
using EntityStates;

namespace PonePack
{
    public static class Deployables
    {
        public static DeployableSlot HazelTurretDeployableSlot;
    }

    public static class Items
    {
        public static ItemDef HealthLink;
    }

    public static class Equipment
    {
        public static EquipmentDef ShareItemPool;
    }

    public static class Buffs
    {
        public static BuffDef HealthLink;
    }

    public static class NetworkedObjects
    {
        public static GameObject HealthLinkBodyAttachment;
        public static GameObject ShareItemPoolAttachment;
    }

    public static class ModdedProcTypes
    {
        public static ModdedProcType HealthLink;
    }

    public static class EntityStateConfigurations
    {
        public static EntityStateConfiguration FireHazelTurret;
    }

    public class PonePackContent : IContentPackProvider
    {
        public string identifier => PonePackMain.GUID;

        public static ReadOnlyContentPack readOnlyContentPack => new ReadOnlyContentPack(PonePackContentPack);
        internal static ContentPack PonePackContentPack { get; } = new ContentPack();

        protected static AssetBundle _ponePackBundle { get; private set; }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            var asyncOperation = AssetBundle.LoadFromFileAsync(PonePackMain.assetBundleDir);
            while(!asyncOperation.isDone)
            {
                args.ReportProgress(asyncOperation.progress);
                yield return null;
            }

            //Write code here to initialize your mod post assetbundle load
            _ponePackBundle = asyncOperation.assetBundle;

            LoadItemDefs();
            LoadEquipmentDefs();
            LoadBuffDefs();
            LoadProcTypes();
            LoadNetworkObjectPrefabs();
            LoadHooks();
        }
        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
             ContentPack.Copy(PonePackContentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }
        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        private void AddSelf(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }
        internal PonePackContent()
        {
            ContentManager.collectContentPackProviders += AddSelf;
        }

        private void LoadItemDefs()
        {
            PonePack.Items.HealthLink = _ponePackBundle.LoadAsset<ItemDef>("HealthLink");
            PonePackContentPack.itemDefs.Add(new ItemDef[] { PonePack.Items.HealthLink });
        }

        private void LoadEquipmentDefs()
        {
            PonePack.Equipment.ShareItemPool = _ponePackBundle.LoadAsset<EquipmentDef>("ShareItemPool");
            PonePackContentPack.equipmentDefs.Add(new EquipmentDef[] { PonePack.Equipment.ShareItemPool });
        }

        private void LoadBuffDefs()
        {
            PonePack.Buffs.HealthLink = _ponePackBundle.LoadAsset<BuffDef>("HealthLinkBuff");
            PonePackContentPack.buffDefs.Add(new BuffDef[] { PonePack.Buffs.HealthLink });
        }

        private void LoadProcTypes()
        {
            ModdedProcTypes.HealthLink = R2API.ProcTypeAPI.ReserveProcType();
        }

        private void LoadNetworkObjectPrefabs()
        {
            AddBodyAttachment("HealthLinkBodyAttachment", ref PonePack.NetworkedObjects.HealthLinkBodyAttachment);
            AddBodyAttachment("ShareItemPoolAttachment", ref PonePack.NetworkedObjects.ShareItemPoolAttachment);

            //// HealthLinkBodyAttachment
            //GameObject healthLinkBodyAttachmentAsset = _ponePackBundle.LoadAsset<GameObject>("HealthLinkBodyAttachment");
            //healthLinkBodyAttachmentAsset.AddComponent<NetworkedBodyAttachment>();

            //PonePack.NetworkedObjects.HealthLinkBodyAttachment = PrefabAPI.InstantiateClone(healthLinkBodyAttachmentAsset, "HealthLinkBodyAttachment");
            //PonePack.NetworkedObjects.HealthLinkBodyAttachment.RegisterNetworkPrefab();
            //PonePackContentPack.networkedObjectPrefabs.Add(new GameObject[] { PonePack.NetworkedObjects.HealthLinkBodyAttachment });

            //// ShareItemPoolAttachment
            //GameObject shareItemPoolAttachmentAsset = _ponePackBundle.LoadAsset<GameObject>("ShareItemPoolAttachment");
            //shareItemPoolAttachmentAsset.AddComponent<NetworkedBodyAttachment>();

            //PonePack.NetworkedObjects.ShareItemPoolAttachment = PrefabAPI.InstantiateClone(shareItemPoolAttachmentAsset, "ShareItemPoolAttachment");
            //PonePack.NetworkedObjects.ShareItemPoolAttachment.RegisterNetworkPrefab();
            //PonePackContentPack.networkedObjectPrefabs.Add(new GameObject[] { PonePack.NetworkedObjects.ShareItemPoolAttachment });
        }

        private void AddItem(string defName, ref ItemDef itemDef)
        {
            itemDef = _ponePackBundle.LoadAsset<ItemDef>(defName);
            PonePackContentPack.itemDefs.Add(new ItemDef[] { itemDef });
        }

        private void AddBodyAttachment(string objectName, ref GameObject objectReference)
        {
            GameObject _attachmentAsset = _ponePackBundle.LoadAsset<GameObject>(objectName);
            _attachmentAsset.AddComponent<NetworkedBodyAttachment>();

            objectReference = PrefabAPI.InstantiateClone(_attachmentAsset, objectName);
            objectReference.RegisterNetworkPrefab();
            PonePackContentPack.networkedObjectPrefabs.Add(new GameObject[] { objectReference });
        }

        private void LoadHooks()
        {
            On.RoR2.CharacterBody.OnEquipmentGained += OnEquipmentGained;
            On.RoR2.CharacterBody.OnEquipmentLost += OnEquipmentLost;
        }

        //-- Hooks

        private void OnEquipmentGained(On.RoR2.CharacterBody.orig_OnEquipmentGained orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            if (NetworkServer.active)
            {
                if (equipmentDef == PonePack.Equipment.ShareItemPool) self.AddItemBehavior<ShareItemPoolBehavior>(1);
            }

            orig(self, equipmentDef);
        }

        private void OnEquipmentLost(On.RoR2.CharacterBody.orig_OnEquipmentLost orig, CharacterBody self, EquipmentDef equipmentDef)
        {
            if (NetworkServer.active)
            {
                if (equipmentDef == PonePack.Equipment.ShareItemPool) self.AddItemBehavior<ShareItemPoolBehavior>(0);
            }

            orig(self, equipmentDef);
        }
    }
}
