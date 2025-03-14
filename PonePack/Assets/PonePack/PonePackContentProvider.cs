using RoR2.ContentManagement;
using UnityEngine;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using R2API;

namespace PonePack
{
    public static class Items
    {
        public static ItemDef HealthLink;
    }

    public static class Buffs
    {
        public static BuffDef ShareHealthChangesWithNearbyAlliesBuff;
    }

    public static class ItemObjects
    {
        public static GameObject HealthLinkBodyAttachment;
    }

    public static class ModdedProcTypes
    {
        public static ModdedProcType HealthLink;
    }

    public class PonePackContent : IContentPackProvider
    {
        public string identifier => PonePackMain.GUID;

        public static ReadOnlyContentPack readOnlyContentPack => new ReadOnlyContentPack(PonePackContentPack);
        internal static ContentPack PonePackContentPack { get; } = new ContentPack();

        private static AssetBundle _ponePackBundle;

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
            LoadBuffDefs();
            LoadProcTypes();
            LoadNetworkObjectPrefabs();
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

        private void LoadBuffDefs()
        {
            PonePack.Buffs.ShareHealthChangesWithNearbyAlliesBuff = _ponePackBundle.LoadAsset<BuffDef>("ShareHealthChangesWithNearbyAlliesBuff");
            PonePackContentPack.buffDefs.Add(new BuffDef[] { PonePack.Buffs.ShareHealthChangesWithNearbyAlliesBuff });
        }

        private void LoadProcTypes()
        {
            ModdedProcTypes.HealthLink = R2API.ProcTypeAPI.ReserveProcType();
        }

        private void LoadNetworkObjectPrefabs()
        {
            // HealthLinkBodyAttachment
            GameObject healthLinkBodyAttachmentAsset = _ponePackBundle.LoadAsset<GameObject>("HealthLinkBodyAttachment");
            healthLinkBodyAttachmentAsset.AddComponent<NetworkedBodyAttachment>();

            PonePack.ItemObjects.HealthLinkBodyAttachment = PrefabAPI.InstantiateClone(healthLinkBodyAttachmentAsset, "HealthLinkBodyAttachment");
            PonePack.ItemObjects.HealthLinkBodyAttachment.RegisterNetworkPrefab();
            PonePackContentPack.networkedObjectPrefabs.Add(new GameObject[] { PonePack.ItemObjects.HealthLinkBodyAttachment });
        }
    }
}
