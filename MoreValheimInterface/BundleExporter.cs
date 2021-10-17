using UnityEditor;

namespace MoreValheimInterface
{
    class BundleExporter
    {
        [MenuItem("Assets/Build AssetBundles")]
        static void BuildAllBundles()
        {
            BuildPipeline.BuildAssetBundles("../MoreValheim/Resources", BuildAssetBundleOptions.StrictMode | BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.StandaloneWindows64);
        }
    }
}
