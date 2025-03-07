using RoR2.ContentManagement;
using UnityEngine;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace PonePack
{
    public static class Items
    {
        public static ItemDef ShareHealthChangesWithNearbyAllies;
    }

    public static class Buffs
    {
        public static BuffDef ShareHealthChangesWithNearbyAlliesBuff;
    }

    public static class ItemObjects
    {
        public static GameObject ShareHealthChangesBonusIndicator;
    }

    public class PonePackContent : IContentPackProvider
    {
        public string identifier => PonePackMain.GUID;

        public static ReadOnlyContentPack readOnlyContentPack => new ReadOnlyContentPack(PonePackContentPack);
        internal static ContentPack PonePackContentPack { get; } = new ContentPack();

        private static AssetBundle _ponePackBundle;

        //private static ItemDef _geminiBands;

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
        }
        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
             ContentPack.Copy(PonePackContentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }
        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            //R2API.RecalculateStatsAPI.GetStatCoefficients += (body, statArgs) =>
            //{
            //    if (!body.inventory) return;

            //    int count = body.inventory.GetItemCount(PonePack.Items.ShareHealthChangesWithNearbyAllies);
            //    statArgs.moveSpeedMultAdd += 5f * count;
            //};

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
            //_geminiBands = _ponePackBundle.LoadAsset<ItemDef>("GeminiBands");
            //PonePackContentPack.itemDefs.Add(new ItemDef[] { _geminiBands });

            PonePack.Items.ShareHealthChangesWithNearbyAllies = _ponePackBundle.LoadAsset<ItemDef>("GeminiBands");
            PonePackContentPack.itemDefs.Add(new ItemDef[] { PonePack.Items.ShareHealthChangesWithNearbyAllies });

            //Test
            PonePack.ItemObjects.ShareHealthChangesBonusIndicator = _ponePackBundle.LoadAsset<GameObject>("ShareHealthChangesBonusIndicator");
            Debug.Log(PonePack.ItemObjects.ShareHealthChangesBonusIndicator.name);
            //PonePackContentPack.bodyPrefabs.Add(new GameObject[] { PonePack.ItemObjects.ShareHealthChangesBonusIndicator });
        }

        private void LoadBuffDefs()
        {
            PonePack.Buffs.ShareHealthChangesWithNearbyAlliesBuff = _ponePackBundle.LoadAsset<BuffDef>("ShareHealthChangesWithNearbyAlliesBuff");
            PonePackContentPack.buffDefs.Add(new BuffDef[] { PonePack.Buffs.ShareHealthChangesWithNearbyAlliesBuff });
        }
    }
}
