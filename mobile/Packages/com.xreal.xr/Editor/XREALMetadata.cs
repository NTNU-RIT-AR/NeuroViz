using System.Collections.Generic;
using UnityEditor;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;

namespace Unity.XR.XREAL.Editor
{
    class XREALMetadata : IXRPackage
    {
        class XREALLoaderMetadata : IXRLoaderMetadata
        {
            public string loaderName { get; set; }
            public string loaderType { get; set; }
            public List<BuildTargetGroup> supportedBuildTargets { get; set; }
        }

        class XREALPackageMetadata : IXRPackageMetadata
        {
            public string packageName { get; set; }
            public string packageId { get; set; }
            public string settingsType { get; set; }
            public List<IXRLoaderMetadata> loaderMetadata { get; set; }
        }

        static IXRPackageMetadata s_Metadata = new XREALPackageMetadata()
        {
            packageName = "XREAL XR Plugin",
            packageId = XREALUtility.k_Identifier,
            settingsType = typeof(XREALSettings).FullName,
            loaderMetadata = new List<IXRLoaderMetadata>()
            {
                new XREALLoaderMetadata()
                {
                    loaderName = "XREAL",
                    loaderType = typeof(XREALXRLoader).FullName,
                    supportedBuildTargets = new List<BuildTargetGroup>()
                    {
                        BuildTargetGroup.Android,
#if XREAL_ENTERPRISE
                        BuildTargetGroup.Standalone,
#endif
                    }
                }
            }
        };

        public IXRPackageMetadata metadata => s_Metadata;

        public bool PopulateNewSettingsInstance(ScriptableObject obj)
        {
            if (obj is XREALSettings settings)
            {
#if UNITY_ANDROID
                settings.VirtualController = FindPrefabInPackage("XREALVirtualController");
#endif
                EditorUtility.SetDirty(settings);
                return true;
            }

            return false;
        }

#if UNITY_ANDROID
        GameObject FindPrefabInPackage(string prefabName)
        {
            string packagePath = $"Packages/{XREALUtility.k_Identifier}";
            string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab", new[] { packagePath });

            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            return null;
        }
#endif
    }
}
