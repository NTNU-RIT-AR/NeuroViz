using Unity.XR.CoreUtils.Editor;
using UnityEditor;
using UnityEngine.Rendering;

namespace Unity.XR.XREAL.Editor
{
    static class XREALProjectValidator
    {
        const string k_Category = "XREAL";

        [InitializeOnLoadMethod]
        static void AddXREALValidationRules()
        {
            var buildTargetRules = new BuildValidationRule[]
            {
#if UNITY_ANDROID
                new BuildValidationRule()
                {
                    Category = k_Category,
                    Message = "The Android minimum API level should be set to 29 or higher.",
                    Error = true,
                    IsRuleEnabled = XREALUtility.IsLoaderActive,
                    CheckPredicate = () =>
                    {
                        return PlayerSettings.Android.minSdkVersion >= AndroidSdkVersions.AndroidApiLevel29
                            || PlayerSettings.Android.minSdkVersion == AndroidSdkVersions.AndroidApiLevelAuto;
                    },
                    FixIt = () =>
                    {
                        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
                    }
                },
                new BuildValidationRule()
                {
                    Category = k_Category,
                    Message = "The build scripting backend should be set to IL2CPP.",
                    Error = true,
                    IsRuleEnabled = XREALUtility.IsLoaderActive,
                    CheckPredicate = () =>
                    {
                        return PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP;
                    },
                    FixIt = () =>
                    {
                        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                    }
                },
                new BuildValidationRule()
                {
                    Category = k_Category,
                    Message = "The target architectures should be set to ARM64.",
                    Error = true,
                    IsRuleEnabled = XREALUtility.IsLoaderActive,
                    CheckPredicate = () =>
                    {
                        return PlayerSettings.Android.targetArchitectures == AndroidArchitecture.ARM64;
                    },
                    FixIt = () =>
                    {
                        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                    }
                },
                new BuildValidationRule()
                {
                    Category = k_Category,
                    Message = "The graphics API should be set to OpenGLES3.",
                    Error = true,
                    IsRuleEnabled = XREALUtility.IsLoaderActive,
                    CheckPredicate = () =>
                    {
                        bool autoGraphicsAPI = PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android);
                        var graphicsAPI = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
                        return !autoGraphicsAPI && graphicsAPI != null && graphicsAPI.Length == 1 && graphicsAPI[0] == GraphicsDeviceType.OpenGLES3;
                    },
                    FixIt = () =>
                    {
                        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
                        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new GraphicsDeviceType[1] { GraphicsDeviceType.OpenGLES3 });
                    }
                },
#endif
#if UNITY_STANDALONE_WIN
                new BuildValidationRule()
                {
                    Category = k_Category,
                    Message = "The graphics API should be set to Vulkan.",
                    Error = true,
                    IsRuleEnabled = XREALUtility.IsLoaderActive,
                    CheckPredicate = () =>
                    {
                        bool autoGraphicsAPI = PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64);
                        var graphicsAPI = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows64);
                        return !autoGraphicsAPI && graphicsAPI != null && graphicsAPI.Length == 1 && graphicsAPI[0] == GraphicsDeviceType.Vulkan;
                    },
                    FixIt = () =>
                    {
                        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
                        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new GraphicsDeviceType[1] { GraphicsDeviceType.Vulkan });
                    }
                },
#endif
            };

            BuildValidator.AddRules(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), buildTargetRules);
        }
    }
}
