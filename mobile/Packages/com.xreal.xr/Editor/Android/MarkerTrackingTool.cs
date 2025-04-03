#if UNITY_ANDROID
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity.XR.XREAL.Editor
{
    /// <summary>
    /// Provides tools for managing the Marker Tracking module in XREAL projects.
    /// This includes installing, uninstalling, and checking the status of the marker tracking module.
    /// </summary>
    public static class MarkerTrackingTool
    {
        internal static string MARKER_RES_ROOT => Path.Combine(XREALBuildProcessor.PackagePath, "Marker~");

        const string NR_PLUGIN_JSON = "nr_plugins.json";

        static string NR_PLUGIN_JSON_PATH => Path.Combine(Application.streamingAssetsPath, NR_PLUGIN_JSON);

        [MenuItem("XREAL/MarkerTracking/Install", validate = true)]
        static bool ValitateInstallMarkerTrackingModule()
        {
            return !IsMarkerTrackingInstalled();
        }

        /// <summary>
        /// Installs the marker tracking module by copying the plugin configuration file 
        /// from the marker resources directory to the StreamingAssets directory.
        /// </summary>
        [MenuItem("XREAL/MarkerTracking/Install")]
        static void InstallMarkerTrackingModule()
        {
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }

            File.Copy(Path.Combine(MARKER_RES_ROOT, NR_PLUGIN_JSON), NR_PLUGIN_JSON_PATH);
            AssetDatabase.Refresh();
        }

        [MenuItem("XREAL/MarkerTracking/Uninstall", validate = true)]
        static bool ValitateUninstallMarkerTrackingModule()
        {
            return IsMarkerTrackingInstalled();
        }

        /// <summary>
        /// Uninstalls the marker tracking module by deleting the plugin configuration file 
        /// from the StreamingAssets directory.
        /// </summary>
        [MenuItem("XREAL/MarkerTracking/Uninstall")]
        static void UninstallMarkerTrackingModule()
        {
            File.Delete(NR_PLUGIN_JSON_PATH);
            AssetDatabase.Refresh();
        }

        public static bool IsMarkerTrackingInstalled()
        {
            return File.Exists(NR_PLUGIN_JSON_PATH);
        }
    }
}
#endif
