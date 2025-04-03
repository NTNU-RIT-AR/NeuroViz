#if (UNITY_ANDROID && !UNITY_EDITOR) || UNITY_STANDALONE
#define XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
#endif

using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.XR.XREAL
{
    public enum TrackingType
    {
        MODE_6DOF = 0,
        MODE_3DOF = 1,
        MODE_0DOF = 2,
        MODE_0DOF_STAB = 3,
    }

    public enum InputSource
    {
        None = 0,
        Controller = 1,
        Hands = 2,
        ControllerAndHands = 3,
    }

    public enum XREALComponent
    {
        XREAL_COMPONENT_DISPLAY_LEFT = 0,
        XREAL_COMPONENT_DISPLAY_RIGHT,
        XREAL_COMPONENT_RGB_CAMERA,
        XREAL_COMPONENT_GRAYSCALE_CAMERA_LEFT,
        XREAL_COMPONENT_GRAYSCALE_CAMERA_RIGHT,
        XREAL_COMPONENT_MAGNETIC,
        XREAL_COMPONENT_HEAD,
        XREAL_COMPONENT_IMU,
        XREAL_COMPONENT_NUM,
    }

    public enum XREALDeviceType
    {
        XREAL_DEVICE_TYPE_INVALID = 0,
        XREAL_DEVICE_TYPE_LIGHT = 1,
        XREAL_DEVICE_TYPE_AIR = 2,
        XREAL_DEVICE_TYPE_AIR2_PRO = 3,
        XREAL_DEVICE_TYPE_AIR2 = 4,
        XREAL_DEVICE_TYPE_AIR2_ULTRA = 5,
        XREAL_DEVICE_TYPE_ONE = 10,
        XREAL_DEVICE_TYPE_ONE_PROM = 11,
        XREAL_DEVICE_TYPE_ONE_PROL = 12,
        XREAL_DEVICE_TYPE_HONOR_AIR = 1001,
    }

    public enum XREALDeviceCategory
    {
        XREAL_DEVICE_CATEGORY_INVALID = 0,
        XREAL_DEVICE_CATEGORY_REALITY = 1,
        XREAL_DEVICE_CATEGORY_VISION = 2,
    }

    public enum XREALSupportedFeature
    {
        XREAL_FEATURE_RGB_CAMERA = 1,
        XREAL_FEATURE_WEARING_STATUS_OF_GLASSES = 2,
        XREAL_FEATURE_CONTROLLER = 3,
        XREAL_FEATURE_PERCEPTION_HEAD_TRACKING_ROTATION = 4,
        XREAL_FEATURE_PERCEPTION_HEAD_TRACKING_POSITION = 5,
    }

    public enum XREALGlassesDisconnectReason
    {
        GLASSES_DEVICE_DISCONNECT = 1,
        NOTIFY_TO_QUIT_APP = 2,
        NOTIFY_GOTO_SLEEP = 3,
    }

    public enum XREALRGBCameraPlugState
    {
        UNKNOWN = 0,
        PLUGIN = 1,
        PLUGOUT = 2,
    }

    public enum XREALWearingStatus
    {
        UNKNOWN = 0,
        PUT_ON,
        TAKE_OFF,
    }

    public enum XREALTemperatureLevel
    {
        LEVEL_NORMAL = 0,
        LEVEL_WARM = 1,
        LEVEL_HOT = 2,
    }

    public enum XREALKeyType
    {
        NONE = 0,
        MULTI_KEY = 1,
        INCREASE_KEY = 2,
        DECREASE_KEY = 3,
        MENU_KEY = 4,
        ALL_KEY = 1000,
    }

    public enum XREALClickType
    {
        CLICK = 1,
        DOUBLE_CLICK = 2,
        LONG_PRESS = 3,
    }

    public enum XREALKeyState
    {
        UNKNOWN = 0,
        KEY_DOWN = 1,
        KEY_UP = 2,
    }

    public delegate void BeginChangeTrackingTypeEvent(TrackingType from, TrackingType to);
    public delegate void TrackingTypeChangedCallback(bool result, TrackingType targetTrackingType);

    /// <summary>
    /// Provides a set of utility functions and events for interacting with the XREAL XR Plugin.
    /// </summary>
    public static partial class XREALPlugin
    {
        const string LibName = "XREALXRPlugin";

        /// <summary>
        /// Event triggered before the tracking type is switched.
        /// </summary>
        public static event BeginChangeTrackingTypeEvent OnBeginChangeTrackingType;

        /// <summary>
        /// Event triggered after the tracking type is switched.
        /// </summary>
        public static event TrackingTypeChangedCallback OnTrackingTypeChanged;

        internal static event TrackingTypeChangedCallback OnTrackingTypeChangedInternal;

        /// <summary>
        /// Gets the current device type of the XREAL hardware.
        /// </summary>
        /// <returns>The XREAL device type.</returns>
        public static XREALDeviceType GetDeviceType()
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            return Internal.GetDeviceType();
#else
            return XREALDeviceType.XREAL_DEVICE_TYPE_INVALID;
#endif
        }

        /// <summary>
        /// Determines whether the connected device is part of the XREAL One series.
        /// </summary>
        /// <returns>True if the device is an XREAL One series device; otherwise, false.</returns>
        public static bool IsOneSeriesGlasses()
        {
            var deviceType = GetDeviceType();
            return deviceType == XREALDeviceType.XREAL_DEVICE_TYPE_ONE || deviceType == XREALDeviceType.XREAL_DEVICE_TYPE_ONE_PROM || deviceType == XREALDeviceType.XREAL_DEVICE_TYPE_ONE_PROL;
        }

        /// <summary>
        /// Gets the category of the connected XREAL device.
        /// </summary>
        /// <returns>The device category.</returns>
        public static XREALDeviceCategory GetDeviceCategory()
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            return Internal.GetDeviceCategory();
#else
            return XREALDeviceCategory.XREAL_DEVICE_CATEGORY_INVALID;
#endif
        }

        /// <summary>
        /// Retrieves the current tracking type.
        /// </summary>
        /// <returns>The current <see cref="TrackingType"/>.</returns>
        public static TrackingType GetTrackingType()
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            return Internal.GetTrackingType();
#else
            return TrackingType.MODE_3DOF;
#endif
        }

        /// <summary>
        /// Switches the tracking type asynchronously, invoking callbacks and events.
        /// </summary>
        /// <param name="targetTrackingType">The target tracking type to switch to.</param>
        /// <param name="callback">Optional callback to invoke after the switch is complete.</param>
        /// <returns>A task that resolves to true if the switch was successful, otherwise false.</returns>
        public static async Task<bool> SwitchTrackingTypeAsync(TrackingType targetTrackingType, TrackingTypeChangedCallback callback = null)
        {
            OnBeginChangeTrackingType?.Invoke(GetTrackingType(), targetTrackingType);

            const int MaxFrameRate = 60;
            int targetFrameRate = GetTargetFrameRate();
            targetFrameRate = targetFrameRate > 0 ? Mathf.Min(targetFrameRate, MaxFrameRate) : MaxFrameRate;
            await Task.Delay(1000 / targetFrameRate * 5); //Pass multiple frames of black screens to underlying layer

            bool result = await Task.Run(() =>
            {
                return SwitchTrackingType(targetTrackingType);
            });
            await Task.Delay(1000 / targetFrameRate * 5); //Wait for 5 frames to ensure tracking stability and prevent screen flickering.
            OnTrackingTypeChangedInternal?.Invoke(result, targetTrackingType);
            callback?.Invoke(result, targetTrackingType);
            OnTrackingTypeChanged?.Invoke(result, targetTrackingType);
            return result;
        }

        internal static bool SwitchTrackingType(TrackingType type)
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            return Internal.SwitchTrackingType(type);
#else
            return false;
#endif
        }

        /// <summary>
        /// Retrieves the pose of a device relative to the head.
        /// </summary>
        /// <param name="device">The device component.</param>
        /// <param name="pose">The output pose of the device.</param>
        /// <returns>True if the pose was successfully retrieved, otherwise false.</returns>
        public static bool GetDevicePoseFromHead(XREALComponent device, ref Pose pose)
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            return Internal.GetDevicePoseFromHead(device, ref pose);
#else
            return false;
#endif
        }

        /// <summary>
        /// Retrieves the resolution of the device.
        /// </summary>
        /// <param name="component">The device component.</param>
        /// <param name="size">The output resolution as a <see cref="Vector2Int"/>.</param>
        /// <returns>True if the resolution was successfully retrieved, otherwise false.</returns>
        public static bool GetDeviceResolution(XREALComponent component, ref Vector2Int size)
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            return Internal.GetDeviceResolution(component, ref size);
#else
            return false;
#endif
        }

        /// <summary>
        /// Retrieves the refresh rate of the display device.
        /// </summary>
        /// <param name="refreshRate">The output refresh rate.</param>
        /// <returns>True if the refresh rate was successfully retrieved, otherwise false.</returns>
        public static bool GetDeviceRefreshRate(ref uint refreshRate)
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            return Internal.GetDeviceRefreshRate(ref refreshRate);
#else
            return false;
#endif
        }

        /// <summary>
        /// Checks if a specific feature is supported.
        /// </summary>
        /// <param name="feature">The feature to check.</param>
        /// <returns>True if the feature is supported, otherwise false.</returns>
        public static bool IsHMDFeatureSupported(XREALSupportedFeature feature)
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            return Internal.IsHMDFeatureSupported(feature);
#else
            return false;
#endif
        }

        /// <summary>
        /// Retrieves the camera projection matrix for a given camera component.
        /// </summary>
        /// <param name="component">The camera component.</param>
        /// <param name="z_near">The near clipping plane.</param>
        /// <param name="z_far">The far clipping plane.</param>
        /// <param name="mat">The output projection matrix.</param>
        /// <returns>True if the matrix was successfully retrieved, otherwise false.</returns>
        public static bool GetCameraProjectionMatrix(XREALComponent component, float z_near, float z_far, ref Matrix4x4 mat)
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            return Internal.GetCameraProjectionMatrix(component, z_near, z_far, ref mat);
#else
            return false;
#endif
        }

        /// <summary>
        /// Retrieves the intrinsic parameters of the camera.
        /// </summary>
        /// <param name="component">The camera component.</param>
        /// <param name="focalLength">The output focal length.</param>
        /// <param name="principalPoint">The output principal point.</param>
        /// <returns>True if the intrinsic parameters were successfully retrieved, otherwise false.</returns>
        public static bool GetCameraIntrinsic(XREALComponent component, ref Vector2 focalLength, ref Vector2 principalPoint)
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            return Internal.GetCameraIntrinsic(component, ref focalLength, ref principalPoint);
#else
            return false;
#endif
        }

        private static partial class Internal
        {
            [DllImport(LibName)]
            internal static extern XREALDeviceType GetDeviceType();

            [DllImport(LibName)]
            internal static extern XREALDeviceCategory GetDeviceCategory();

            [DllImport(LibName)]
            internal static extern TrackingType GetTrackingType();

            [DllImport(LibName)]
            internal static extern bool SwitchTrackingType(TrackingType type);

            [DllImport(LibName)]
            internal static extern bool GetDevicePoseFromHead(XREALComponent device, ref Pose pose);

            [DllImport(LibName)]
            internal static extern bool GetDeviceResolution(XREALComponent component, ref Vector2Int size);

            [DllImport(LibName)]
            internal static extern bool GetDeviceRefreshRate(ref uint refreshRate);

            [DllImport(LibName)]
            internal static extern bool IsHMDFeatureSupported(XREALSupportedFeature feature);

            [DllImport(LibName)]
            internal static extern bool GetCameraProjectionMatrix(XREALComponent component, float z_near, float z_far, ref Matrix4x4 mat);

            [DllImport(LibName)]
            internal static extern bool GetCameraIntrinsic(XREALComponent component, ref Vector2 focalLength, ref Vector2 principalPoint);
        }
    }
}
