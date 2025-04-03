#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.Management.AndroidManifest.Editor;

namespace Unity.XR.XREAL.Editor
{
    public class XREALManifestProvider : IAndroidManifestRequirementProvider
    {
        public ManifestRequirement ProvideManifestRequirement()
        {
            var newElements = new List<ManifestElement>
            {
                new ManifestElement()
                {
                    ElementPath = new List<string>
                    {
                        "manifest", "application", "meta-data"
                    },
                    Attributes = new Dictionary<string, string>
                    {
                        { "name", "nreal_sdk" },
                        { "value", "true" },
                    },
                },
                new ManifestElement()
                {
                    ElementPath = new List<string>
                    {
                        "manifest", "application", "meta-data"
                    },
                    Attributes = new Dictionary<string, string>
                    {
                        { "name", "com.nreal.supportDevices" },
                        { "value", string.Join("|", XREALSettings.GetSettings().SupportDevices.Select(d =>
                            $"{(int)d}|{(d == XREALDeviceCategory.XREAL_DEVICE_CATEGORY_REALITY ? "XrealLight" : "XrealAir")}")) },
                    },
                }
            };

            foreach (var permission in XREALSettings.GetSettings().AddtionalPermissions)
            {
                newElements.Add(new ManifestElement()
                {
                    ElementPath = new List<string>
                    {
                        "manifest", "uses-permission"
                    },
                    Attributes = new Dictionary<string, string>
                    {
                        { "name", "android.permission." + permission },
                    },
                });
            }

            var removeElements = new List<ManifestElement>();

            if (XREALSettings.GetSettings().SupportMultiResume)
            {
                removeElements.Add(new ManifestElement()
                {
                    ElementPath = new List<string>
                    {
                        "manifest", "application", "activity", "intent-filter"
                    },
                });
            }
            else
            {
                newElements.Add(new ManifestElement()
                {
                    ElementPath = new List<string>
                    {
                        "manifest", "application", "activity", "intent-filter", "action"
                    },
                    Attributes = new Dictionary<string, string>
                    {
                        { "name", "android.intent.action.MAIN" },
                    },
                });
                newElements.Add(new ManifestElement()
                {
                    ElementPath = new List<string>
                    {
                        "manifest", "application", "activity", "intent-filter", "category"
                    },
                    Attributes = new Dictionary<string, string>
                    {
                        { "name", "android.intent.category.LAUNCHER" },
                    },
                });
            }

            return new ManifestRequirement()
            {
                SupportedXRLoaders = new HashSet<Type>() { typeof(XREALXRLoader) },
                NewElements = newElements,
                RemoveElements = removeElements,
            };
        }
    }
}
#endif
