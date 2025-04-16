using RoR2.ContentManagement;
using UnityEngine;
using RoR2;
using System.Collections;
namespace Druid
{
    public class DruidContent : IContentPackProvider
    {
        public string identifier => DruidMain.GUID;

        public static ReadOnlyContentPack readOnlyContentPack => new ReadOnlyContentPack(DruidContentPack);
        internal static ContentPack DruidContentPack { get; } = new ContentPack();
        protected static AssetBundle DruidBundle { get; private set; }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            var asyncOperation = AssetBundle.LoadFromFileAsync(DruidMain.assetBundleDir);
            while(!asyncOperation.isDone)
            {
                args.ReportProgress(asyncOperation.progress);
                yield return null;
            }

            //Write code here to initialize your mod post assetbundle load
            DruidBundle = asyncOperation.assetBundle;

            Druid.DruidSurvivor.Initialize();
        }
        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
             ContentPack.Copy(DruidContentPack, args.output);
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
        internal DruidContent()
        {
            ContentManager.collectContentPackProviders += AddSelf;
        }
    }
}
