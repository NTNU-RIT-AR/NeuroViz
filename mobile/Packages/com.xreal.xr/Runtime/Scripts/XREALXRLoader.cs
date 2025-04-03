using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using System.Runtime.InteropServices;
#if UNITY_INPUT_SYSTEM
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem;
#endif
#if XR_ARFOUNDATION
using UnityEngine.XR.ARSubsystems;
#endif
#if XR_HANDS
using UnityEngine.XR.Hands;
#endif

namespace Unity.XR.XREAL
{
#if UNITY_INPUT_SYSTEM
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    static class InputLayoutLoader
    {
        static InputLayoutLoader()
        {
            RegisterInputLayouts();
        }

        public static void RegisterInputLayouts()
        {
            InputSystem.RegisterLayout<XREALController>(
                matches: new InputDeviceMatcher()
                    .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                    .WithProduct(@"(^(XREAL Controller))"));
            InputSystem.RegisterLayout<XREALHandTracking>(
                matches: new InputDeviceMatcher()
                    .WithInterface(XRUtilities.InterfaceMatchAnyVersion)
                    .WithProduct(@"(^(XREAL Hand Tracking))"));
        }
    }
#endif

    /// <summary>
    /// Manages the lifecycle of XREAL subsystems.
    /// </summary>
    public class XREALXRLoader : XRLoaderHelper
#if UNITY_EDITOR
        , IXRLoaderPreInit
#endif
    {
        static List<XRDisplaySubsystemDescriptor> s_DisplaySubsystemDescriptors = new List<XRDisplaySubsystemDescriptor>();
        static List<XRInputSubsystemDescriptor> s_InputSubsystemDescriptors = new List<XRInputSubsystemDescriptor>();
#if XR_ARFOUNDATION
        static List<XRSessionSubsystemDescriptor> s_SessionSubsystemDescriptors = new List<XRSessionSubsystemDescriptor>();
        static List<XRPlaneSubsystemDescriptor> s_PlaneSubsystemDescriptors = new List<XRPlaneSubsystemDescriptor>();
        static List<XRMeshSubsystemDescriptor> s_MeshSubsystemDescriptors = new List<XRMeshSubsystemDescriptor>();
        static List<XRImageTrackingSubsystemDescriptor> s_ImageTrackingSubsystemDescriptors = new List<XRImageTrackingSubsystemDescriptor>();
        static List<XRAnchorSubsystemDescriptor> s_AnchorSubsystemDescriptors = new List<XRAnchorSubsystemDescriptor>();
        static List<XRCameraSubsystemDescriptor> s_CameraSubsystemDescriptors = new List<XRCameraSubsystemDescriptor>();
#endif
#if XR_HANDS
        static List<XRHandSubsystemDescriptor> s_HandSubsystemDescriptors = new List<XRHandSubsystemDescriptor>();
#endif

        /// <summary>
        /// Triggered when the XR Loader starts, just before starting all XR subsystems.
        /// This event allows subscribers to execute custom logic before the XR system becomes fully operational.
        /// </summary>
        public static event Action OnXRLoaderStart;

        /// <summary>
        /// Triggered when the XR Loader stops, just before stopping all XR subsystems.
        /// This event allows subscribers to execute custom logic before the XR system is completely shut down.
        /// </summary>
        public static event Action OnXRLoaderStop;

        /// <summary>
        /// Initializes the loader.
        /// </summary>
        /// <returns>`True` if the input subsystem and the display subsystem were successfully created, otherwise `false`.</returns>
        public override bool Initialize()
        {
            Debug.Log("[XREALXRLoader] Init");
#if UNITY_INPUT_SYSTEM
            InputLayoutLoader.RegisterInputLayouts();
#endif
            if (Resources.Load<TextAsset>("XREALPackageVersion") is TextAsset versionFile)
            {
                Debug.LogFormat("[XREALXRLoader] XREALPackageVersion: {0}", versionFile.text);
            }
            else
            {
                Debug.LogWarning("[XREALXRLoader] Fail to load XREALPackageVersion");
            }
#if XREAL_ENTERPRISE
            if (Resources.Load<TextAsset>("XREALEnterprisePackageVersion") is TextAsset entVersionFile)
            {
                Debug.LogFormat("[XREALXRLoader] XREALEnterprisePackageVersion: {0}", entVersionFile.text);
            }
            else
            {
                Debug.LogWarning("[XREALXRLoader] Fail to load XREALEnterprisePackageVersion");
            }
#endif
            string version = Marshal.PtrToStringAnsi(XREALPlugin.GetPluginVersion());
            Debug.LogFormat("[XREALXRLoader] GetPluginVersion: {0}", version);

            XREALSettings settings = XREALSettings.GetSettings();
            if (settings == null)
            {
                Debug.LogError("Unable to start XREAL XR Plugin. Failed to get XREAL Settings.");
                return false;
            }
            UserDefinedSettings userDefinedSettings = new UserDefinedSettings()
            {
                colorSpace = (int)QualitySettings.activeColorSpace,
                stereoRenderingMode = (int)settings.StereoRendering,
                trackingType = (int)settings.InitialTrackingType,
#if UNITY_ANDROID && XREAL_ENTERPRISE
                supportMonoMode = settings.SupportMonoMode,
#else
                supportMonoMode = false,
#endif
#if UNITY_ANDROID
                unityActivity = XREALUtility.UnityActivity.GetRawObject(),
                inputSource = (int)settings.InitialInputSource,
#endif
            };
#if UNITY_ANDROID
            settings.InitLicenseData();
#endif
            XREALPlugin.InitUserDefinedSettings(userDefinedSettings);
#if DEVELOPMENT_BUILD && !XR_PROFILER
            XREALPlugin.SetNativeLogLevel((LogType)5);
#endif
#if UNITY_ANDROID
            if (!XREALPlugin.CreateSession())
            {
                Debug.LogError("Unable to start XREAL XR Plugin. Failed to create XREAL session.");
                return false;
            }
#endif
            CreateSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>(s_InputSubsystemDescriptors, "XREAL Head Tracking");
            if (GetLoadedSubsystem<XRInputSubsystem>() == null)
            {
                Debug.LogError("Unable to start XREAL XR Plugin. Failed to load input subsystem.");
                return false;
            }
            CreateSubsystem<XRDisplaySubsystemDescriptor, XRDisplaySubsystem>(s_DisplaySubsystemDescriptors, "XREAL Display");
            if (GetLoadedSubsystem<XRDisplaySubsystem>() == null)
            {
                Debug.LogError("Unable to start XREAL XR Plugin. Failed to load display subsystem.");
                return false;
            }
#if XR_ARFOUNDATION
            CreateSubsystem<XRSessionSubsystemDescriptor, XRSessionSubsystem>(s_SessionSubsystemDescriptors, "XREAL Session");
            CreateSubsystem<XRPlaneSubsystemDescriptor, XRPlaneSubsystem>(s_PlaneSubsystemDescriptors, "XREAL Plane Detection");
            CreateSubsystem<XRMeshSubsystemDescriptor, XRMeshSubsystem>(s_MeshSubsystemDescriptors, "XREAL Meshing");
            CreateSubsystem<XRImageTrackingSubsystemDescriptor, XRImageTrackingSubsystem>(s_ImageTrackingSubsystemDescriptors, "XREAL ImageTracking");
            CreateSubsystem<XRAnchorSubsystemDescriptor, XRAnchorSubsystem>(s_AnchorSubsystemDescriptors, "XREAL Anchor");
            CreateSubsystem<XRCameraSubsystemDescriptor, XRCameraSubsystem>(s_CameraSubsystemDescriptors, "XREAL Camera");
#endif
#if XR_HANDS
            CreateSubsystem<XRHandSubsystemDescriptor, XRHandSubsystem>(s_HandSubsystemDescriptors, "XREAL Hands");
#endif
            Debug.Log("[XREALXRLoader] Init End");
            return true;
        }

        /// <summary>
        /// Starts all subsystems.
        /// </summary>
        public override bool Start()
        {
            Debug.Log("[XREALXRLoader] Start");
            OnXRLoaderStart?.Invoke();
            StartSubsystem<XRInputSubsystem>();
            if (!XREALSettings.GetSettings().IsDeviceSupported())
                return false;

            StartSubsystem<XRDisplaySubsystem>();
#if XR_HANDS
            StartSubsystem<XRHandSubsystem>();
#endif
            Debug.Log("[XREALXRLoader] Start End");
            return true;
        }

        /// <summary>
        /// Stops all subsystems.
        /// </summary>
        public override bool Stop()
        {
            Debug.Log("[XREALXRLoader] Stop");
            OnXRLoaderStop?.Invoke();
            StopSubsystem<XRDisplaySubsystem>();
            StopSubsystem<XRInputSubsystem>();
#if XR_HANDS
            StopSubsystem<XRHandSubsystem>();
#endif
            Debug.Log("[XREALXRLoader] Stop End");
            return true;
        }

        /// <summary>
        /// Destroys all subsystems.
        /// </summary>
        public override bool Deinitialize()
        {
            Debug.Log("[XREALXRLoader] Deinitialize");
            DestroySubsystem<XRDisplaySubsystem>();
            DestroySubsystem<XRInputSubsystem>();
#if XR_ARFOUNDATION
            DestroySubsystem<XRSessionSubsystem>();
            DestroySubsystem<XRPlaneSubsystem>();
            DestroySubsystem<XRMeshSubsystem>();
            DestroySubsystem<XRImageTrackingSubsystem>();
            DestroySubsystem<XRAnchorSubsystem>();
#endif
#if XR_HANDS
            DestroySubsystem<XRHandSubsystem>();
#endif

#if UNITY_ANDROID
            XREALPlugin.DestroySession();
#endif
            Debug.Log("[XREALXRLoader] Deinitialize End");

            return true;
        }

#if UNITY_EDITOR
        public string GetPreInitLibraryName(BuildTarget buildTarget, BuildTargetGroup buildTargetGroup)
        {
            return "XREALXRPlugin";
        }
#endif
    }

    internal struct UserDefinedSettings
    {
        public int colorSpace;
        public int stereoRenderingMode;
        public int trackingType;
        public bool supportMonoMode;
        public IntPtr unityActivity;
        public int inputSource;
    }

    public static partial class XREALPlugin
    {
        [DllImport(LibName)]
        internal static extern IntPtr GetPluginVersion();

        [DllImport(LibName)]
        internal static extern void InitUserDefinedSettings(UserDefinedSettings settings);

        [DllImport(LibName)]
        internal static extern bool CreateSession();

        [DllImport(LibName)]
        internal static extern void ResumeSession();

        [DllImport(LibName)]
        internal static extern void PauseSession();

        [DllImport(LibName)]
        internal static extern void DestroySession();

        [DllImport(LibName)]
        internal static extern void SetNativeLogLevel(LogType logType);
    }
}
