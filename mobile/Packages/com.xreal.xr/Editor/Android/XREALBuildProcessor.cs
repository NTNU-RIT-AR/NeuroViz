#if UNITY_ANDROID
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;
#if UNITY_6000_0_OR_NEWER
using System.Text.RegularExpressions;
using UnityEditor.Android;
#endif

namespace Unity.XR.XREAL.Editor
{
    public partial class XREALBuildProcessor : XRBuildHelper<XREALSettings>
#if UNITY_6000_0_OR_NEWER
        , IPostGenerateGradleAndroidProject
#endif
    {
        internal static string PackagePath => $"Packages/{XREALUtility.k_Identifier}";
        internal static string PackageRuntime => $"{PackagePath}/Runtime";
        internal static string PackagePlugins => $"{PackageRuntime}/Plugins/Android";
        static List<string> s_CopiedAssets = new List<string>();

        public override void OnPreprocessBuild(BuildReport report)
        {
            base.OnPreprocessBuild(report);
            if (XREALSettings.GetSettings().SupportMultiResume)
            {
#if UNITY_6000_0_OR_NEWER
                ImportAsset("nractivitylife_6-release");
#else
                ImportAsset("nractivitylife-release");
#endif
#if UNITY_6000_0_OR_NEWER
                PlayerSettings.Android.applicationEntry = AndroidApplicationEntry.Activity;
#endif
            }
        }

        void ImportAsset(string name)
        {
            foreach (var asset in AssetDatabase.FindAssets(name, new string[] { PackagePlugins }))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(asset);
                string relativePath = assetPath.Substring(PackageRuntime.Length + 1);
                string destPath = Path.Combine("Assets", relativePath);
                string destFolder = Path.GetDirectoryName(destPath);
                if (!Directory.Exists(destFolder))
                    Directory.CreateDirectory(destFolder);
                AssetDatabase.CopyAsset(assetPath, destPath);
                s_CopiedAssets.Add(destPath);
                if (AssetImporter.GetAtPath(destPath) is PluginImporter importer)
                {
                    importer.SetCompatibleWithPlatform(BuildTarget.Android, true);
                    importer.SaveAndReimport();
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public override void OnPostprocessBuild(BuildReport report)
        {
            base.OnPostprocessBuild(report);
            foreach (var assetPath in s_CopiedAssets)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
            s_CopiedAssets.Clear();
        }

#if UNITY_6000_0_OR_NEWER
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string gradleFilePath = Path.Combine(path, "../launcher/build.gradle");
            if (File.Exists(gradleFilePath))
            {
                string gradleContent = File.ReadAllText(gradleFilePath);
                string pickFirstConfig = "\n        pickFirst 'lib/arm64-v8a/libc++_shared.so'";
                string packagingOptions = $"\n    packagingOptions {{{pickFirstConfig}\n    }}";
                Match packagingMatch = Regex.Match(gradleContent, @"^\s*packagingOptions\s*{", RegexOptions.Multiline);

                if (packagingMatch.Success)
                {
                    int insertIndex = gradleContent.IndexOf("{", packagingMatch.Index) + 1;
                    gradleContent = gradleContent.Insert(insertIndex, pickFirstConfig);
                }
                else
                {
                    Match androidMatch = Regex.Match(gradleContent, @"^\s*android\s*{", RegexOptions.Multiline);
                    if (androidMatch.Success)
                    {
                        int insertIndex = gradleContent.IndexOf("{", androidMatch.Index) + 1;
                        gradleContent = gradleContent.Insert(insertIndex, packagingOptions);
                    }
                }

                File.WriteAllText(gradleFilePath, gradleContent);
            }
        }
#endif
    }
}
#endif
