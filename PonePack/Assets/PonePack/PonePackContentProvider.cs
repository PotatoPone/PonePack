using RoR2.ContentManagement;
using UnityEngine;
using RoR2;
using System.Collections;
namespace PonePack
{
    public class PonePackContent : IContentPackProvider
    {
        public string identifier => PonePackMain.GUID;

        public static ReadOnlyContentPack readOnlyContentPack => new ReadOnlyContentPack(PonePackContentPack);
        internal static ContentPack PonePackContentPack { get; } = new ContentPack();

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            var asyncOperation = AssetBundle.LoadFromFileAsync(PonePackMain.assetBundleDir);
            while(!asyncOperation.isDone)
            {
                args.ReportProgress(asyncOperation.progress);
                yield return null;
            }

            //Write code here to initialize your mod post assetbundle load
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
    }
}
