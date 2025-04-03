using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.Management;

namespace Unity.XR.XREAL
{
    public enum StereoRenderingMode
    {
        MultiPass = UnityEngine.XR.XRSettings.StereoRenderingMode.MultiPass,
        SinglePassInstanced = UnityEngine.XR.XRSettings.StereoRenderingMode.SinglePassInstanced,
    }

    [XRConfigurationData("XREAL", k_SettingsKey)]
    public class XREALSettings : ScriptableObject
    {
        public const string k_SettingsKey = "com.unity.xr.management.xrealsettings";

        public StereoRenderingMode StereoRendering = StereoRenderingMode.SinglePassInstanced;
        public TrackingType InitialTrackingType = TrackingType.MODE_6DOF;
#if UNITY_ANDROID
        public InputSource InitialInputSource = InputSource.Controller;
        public GameObject VirtualController;
        public bool SupportMultiResume = true;
        public List<string> AddtionalPermissions = new List<string>();
#if XREAL_ENTERPRISE
        public bool SupportMonoMode = false;
#endif
#endif
        public List<XREALDeviceCategory> SupportDevices = new List<XREALDeviceCategory>() { XREALDeviceCategory.XREAL_DEVICE_CATEGORY_REALITY, XREALDeviceCategory.XREAL_DEVICE_CATEGORY_VISION };
        public TextAsset LicenseAsset = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnLoad()
        {
            Debug.Log($"[XREALSettings] OnLoad");
#if UNITY_ANDROID
            if (XREALUtility.IsLoaderActive())
            {
                if (GetSettings() != null && GetSettings().VirtualController != null)
                {
                    Instantiate(GetSettings().VirtualController);
                }
                else
                {
                    XREALVirtualController.CreateSingleton();
                }
            }
#endif
#if UNITY_STANDALONE
            if (GetSettings() != null)
            {
                GetSettings().InitLicenseData();
            }
#endif
            if (XREALMainThreadDispather.Singleton == null)
            {
                XREALMainThreadDispather.CreateSingleton();
            }
        }

#if !UNITY_EDITOR
        static XREALSettings s_Settings;

        void Awake()
        {
            s_Settings = this;
        }
#endif

        internal unsafe void InitLicenseData()
        {
            byte[] licenseData = null;
            var licenseFile = Path.Combine(Application.persistentDataPath, "nrsdk_license.bin");
            if (File.Exists(licenseFile))
            {
                try
                {
                    licenseData = File.ReadAllBytes(licenseFile);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            if (licenseData == null && LicenseAsset != null)
                licenseData = LicenseAsset.bytes;
            if (licenseData != null && licenseData.Length > 0)
            {
                fixed (byte* ptr = licenseData)
                {
                    XREALPlugin.InitLicenseData(new NativeView
                    {
                        data = ptr,
                        count = licenseData.Length
                    });
                }
            }
        }

        /// <summary>
        /// Checks whether the current device is supported.
        /// </summary>
        /// <returns> Returns true if the device is supported; otherwise, false. </returns>
        public bool IsDeviceSupported()
        {
            var deviceCategory = XREALPlugin.GetDeviceCategory();
            return SupportDevices.Contains(deviceCategory);
        }

        public static XREALSettings GetSettings()
        {
#if UNITY_EDITOR
            return UnityEditor.EditorBuildSettings.TryGetConfigObject(k_SettingsKey, out XREALSettings settings) ? settings : null;
#else
            return s_Settings;
#endif
        }
    }

    public static partial class XREALPlugin
    {
        [DllImport(LibName)]
        internal static extern void InitLicenseData(NativeView nativeView);
    }
}
