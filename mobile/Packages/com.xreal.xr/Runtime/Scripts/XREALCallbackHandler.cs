using AOT;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.XR.XREAL
{
    public enum XREALActionType
    {
        ACTION_TYPE_UNKNOWN = 0,
        ACTION_TYPE_CLICK = 1,
        ACTION_TYPE_DOUBLE_CLICK = 2,
        ACTION_TYPE_LONG_PRESS = 3,
        ACTION_TYPE_OPEN_SCREEN = 4,
        ACTION_TYPE_CLOSE_SCREEN = 5,
        ACTION_TYPE_INCREASE_BRIGHTNESS = 6,
        ACTION_TYPE_DECREASE_BRIGHTNESS = 7,
        ACTION_TYPE_INCREASE_VOLUME = 8,
        ACTION_TYPE_DECREASE_VOLUME = 9,
        ACTION_TYPE_SWITCH_TO_MONO = 10,
        ACTION_TYPE_SWITCH_TO_STEORO = 11,
        ACTION_TYPE_NEXT_EC_LEVEL = 12,
        ACTION_TYPE_SWITCH_TO_DP_VOICE = 13,
        ACTION_TYPE_SWITCH_TO_UVC_VOICE = 14,
        ACTION_TYPE_RESERVED0 = 15,
        ACTION_TYPE_RESERVED1 = 16,
        ACTION_TYPE_RESERVED2 = 17,
        ACTION_TYPE_RESERVED3 = 18,
        ACTION_TYPE_RESERVED4 = 19,
        ACTION_TYPE_SWITCH_SLEEP_TIME_LEVEL = 30,
        ACTION_TYPE_SWITCH_DISPLAY_COLOR_CALIBRATION = 31,
        ACTION_TYPE_STARTUP_STATE = 32,
        ACTION_TYPE_TRIGGER_SWITCH_SPACE_MODE = 33,
        ACTION_TYPE_TRIGGER_RECENTER = 34,
        ACTION_TYPE_TRIGGER_OSD_MAIN_MENU = 35,
        ACTION_TYPE_TRIGGER_TAKE_PHOTO = 36,
        ACTION_TYPE_TRIGGER_TAKE_VIDEO = 37,
        ACTION_TYPE_RESERVED5 = 1000,
        ACTION_TYPE_SCREEN_STATUS_NOTIFY = 1010,
        ACTION_TYPE_DISCONNECT = 2000,
        ACTION_TYPE_FORCE_QUIT = 2001,
        ACTION_TYPE_EVENT = 2002,
        ACTION_TYPE_SYSTEM_DISPLAY_CHANGE = 2003,
        ACTION_TYPE_AUDIO_ALGORITHM_CHANGE = 2020,
        ACTION_TYPE_DISPLAY_STATE = 2021,
        ACTION_TYPE_DP_WORKING_STATE = 2022,
        ACTION_TYPE_KEY_STATE = 2023,
        ACTION_TYPE_PROXIMITY_WEARING_STATE = 2024,
        ACTION_TYPE_RGB_CAMERA_PLUGIN_STATE = 2025,
        ACTION_TYPE_TEMPERATURE_DATA = 2026,
        ACTION_TYPE_POWER_SAVE_STATE = 2027,
        ACTION_TYPE_TEMPERATURE_STATE = 2028,
    }

    public struct GlassesEventData
    {
        public XREALActionType actionType;
        public uint para;
        public uint para2;
        public float para3;
    }

    internal delegate void XREALLogCallback(LogType logType, string message);

    /// <summary>
    /// Delegate for handling errors from the XREAL plugin.
    /// </summary>
    /// <param name="errorCode">The error code indicating the type of error.</param>
    /// <param name="message">Optional error message providing additional context or details.</param>
    public delegate void XREALErrorCallback(XREALErrorCode errorCode, string message);

    /// <summary>
    /// Delegate for handling generic glasses events.
    /// </summary>
    /// <param name="data">The data associated with the glasses event.</param>
    public delegate void XREALGlassesEventCallback(GlassesEventData data);

    /// <summary>
    /// Delegate for handling key click events on XREAL glasses.
    /// </summary>
    /// <param name="actionType">The type of click action.</param>
    /// <param name="keyType">The type of key that was clicked.</param>
    public delegate void XREALGlassesKeyClickCallback(XREALClickType actionType, XREALKeyType keyType);

    /// <summary>
    /// Delegate for handling key state changes on XREAL glasses.
    /// </summary>
    /// <param name="keyType">The type of key whose state has changed.</param>
    /// <param name="state">The new state of the key.</param>
    public delegate void XREALGlassesKeyStateCallback(XREALKeyType keyType, XREALKeyState state);

    /// <summary>
    /// Delegate for handling glasses disconnection events.
    /// </summary>
    /// <param name="reason">The reason for the disconnection.</param>
    public delegate void XREALGlassesDisconnectCallback(XREALGlassesDisconnectReason reason);

    /// <summary>
    /// Delegate for handling volume change events on XREAL glasses.
    /// </summary>
    /// <param name="volume">The new volume level.</param>
    public delegate void XREALGlassesVolumeCallback(uint volume);

    /// <summary>
    /// Delegate for handling RGB camera plug/unplug state changes.
    /// </summary>
    /// <param name="state">The current state of the RGB camera.</param>
    public delegate void XREALGlassesRGBCameraPlugStateCallback(XREALRGBCameraPlugState state);

    /// <summary>
    /// Delegate for handling changes in the electro chromic level of XREAL glasses.
    /// </summary>
    /// <param name="level">The current electrochromic level.</param>
    public delegate void XREALGlassesECLevelCallback(uint level);

    /// <summary>
    /// Delegate for handling changes in the wearing state of XREAL glasses.
    /// </summary>
    /// <param name="state">The current wearing status.</param>
    public delegate void XREALGlassesWearingStateCallback(XREALWearingStatus state);

    /// <summary>
    /// Delegate for handling changes in the brightness level of XREAL glasses.
    /// </summary>
    /// <param name="brightness">The current brightness level.</param>
    public delegate void XREALGlassesBrightnessCallback(uint brightness);

    /// <summary>
    /// Delegate for handling changes in the temperature level of XREAL glasses.
    /// </summary>
    /// <param name="level">The current temperature level.</param>
    public delegate void XREALGlassesTemperatureLevelCallback(XREALTemperatureLevel level);

    /// <summary>
    /// Handles callbacks from the native XREAL plugin and forwards them to events.
    /// </summary>
    public static class XREALCallbackHandler
    {
        internal static event XREALGlassesEventCallback OnGlassesEventEnterprise;
        public static event XREALErrorCallback OnXREALError;
        public static event XREALGlassesKeyClickCallback OnXREALGlassesKeyClick;
        public static event XREALGlassesKeyStateCallback OnXREALGlassesKeyState;
        public static event XREALGlassesDisconnectCallback OnXREALGlassesDisconnect;
        public static event XREALGlassesVolumeCallback OnXREALGlassesVolume;
        public static event XREALGlassesRGBCameraPlugStateCallback OnXREALGlassesRGBCameraPlugState;
        public static event XREALGlassesECLevelCallback OnXREALGlassesECLevel;
        public static event XREALGlassesWearingStateCallback OnXREALGlassesWearingState;
        public static event XREALGlassesBrightnessCallback OnXREALGlassesBrightness;
        public static event XREALGlassesTemperatureLevelCallback OnXREALGlassesTemperatureLevel;

#if (UNITY_ANDROID && !UNITY_EDITOR) || UNITY_STANDALONE
        [RuntimeInitializeOnLoadMethod]
#endif
        internal static void OnLoad()
        {
            Debug.Log("[XREALCallbackHandler] OnLoad");
            XREALPlugin.SetGlassesEventCallback(OnGlassesEventCallback);
            XREALPlugin.SetNativeErrorCallback(OnNativeErrorCallback);
        }

        [MonoPInvokeCallback(typeof(XREALGlassesEventCallback))]
        private static void OnGlassesEventCallback(GlassesEventData data)
        {
            if (XREALMainThreadDispather.Singleton != null)
            {
                XREALMainThreadDispather.Singleton.QueueOnMainThread(() =>
                {
                    try
                    {
                        OnGlassesEventHandler(data);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                });
            }
        }

        [MonoPInvokeCallback(typeof(XREALErrorCallback))]
        private static void OnNativeErrorCallback(XREALErrorCode errorCode, string message)
        {
            Debug.Log($"[XREALCallbackHandler] OnNativeErrorCallback: err={errorCode}, msg={message}");
            if (XREALMainThreadDispather.Singleton != null)
            {
                XREALMainThreadDispather.Singleton.QueueOnMainThread(() =>
                {
                    InvokeXREALError(errorCode, message);
                });
            }
        }

        /// <summary>
        /// Invokes the XREAL error callback event.
        /// </summary>
        /// <param name="errorCode">The error code received from the native plugin.</param>
        /// <param name="message">Optional error message to provide additional details.</param>
        public static void InvokeXREALError(XREALErrorCode errorCode, string message = null)
        {
            OnXREALError?.Invoke(errorCode, message);
        }

        private static void OnGlassesEventHandler(GlassesEventData data)
        {
            switch (data.actionType)
            {
                case XREALActionType.ACTION_TYPE_CLICK:
                    OnXREALGlassesKeyClick?.Invoke(XREALClickType.CLICK, (XREALKeyType)data.para);
                    break;
                case XREALActionType.ACTION_TYPE_DOUBLE_CLICK:
                    OnXREALGlassesKeyClick?.Invoke(XREALClickType.DOUBLE_CLICK, (XREALKeyType)data.para);
                    break;
                case XREALActionType.ACTION_TYPE_LONG_PRESS:
                    OnXREALGlassesKeyClick?.Invoke(XREALClickType.LONG_PRESS, (XREALKeyType)data.para);
                    break;
                case XREALActionType.ACTION_TYPE_INCREASE_BRIGHTNESS:
                case XREALActionType.ACTION_TYPE_DECREASE_BRIGHTNESS:
                    OnXREALGlassesBrightness?.Invoke(data.para);
                    break;
                case XREALActionType.ACTION_TYPE_INCREASE_VOLUME:
                case XREALActionType.ACTION_TYPE_DECREASE_VOLUME:
                    OnXREALGlassesVolume?.Invoke(data.para);
                    break;
                case XREALActionType.ACTION_TYPE_NEXT_EC_LEVEL:
                    OnXREALGlassesECLevel?.Invoke(data.para);
                    break;
                case XREALActionType.ACTION_TYPE_DISCONNECT:
                    OnXREALGlassesDisconnect?.Invoke(XREALGlassesDisconnectReason.GLASSES_DEVICE_DISCONNECT);
#if UNITY_ANDROID
                    XREALPlugin.QuitApplication();
#endif
                    break;
                case XREALActionType.ACTION_TYPE_FORCE_QUIT:
                    OnXREALGlassesDisconnect?.Invoke(XREALGlassesDisconnectReason.NOTIFY_TO_QUIT_APP);
#if UNITY_ANDROID
                    XREALPlugin.QuitApplication();
#endif
                    break;
                case XREALActionType.ACTION_TYPE_KEY_STATE:
                    OnXREALGlassesKeyState?.Invoke((XREALKeyType)data.para, (XREALKeyState)data.para2);
                    break;
                case XREALActionType.ACTION_TYPE_PROXIMITY_WEARING_STATE:
                    XREALWearingStatus wearingState = (XREALWearingStatus)data.para;
                    if (wearingState == XREALWearingStatus.PUT_ON || wearingState == XREALWearingStatus.TAKE_OFF)
                        OnXREALGlassesWearingState?.Invoke(wearingState);
                    break;
                case XREALActionType.ACTION_TYPE_RGB_CAMERA_PLUGIN_STATE:
                    OnXREALGlassesRGBCameraPlugState?.Invoke((XREALRGBCameraPlugState)data.para);
                    break;
                case XREALActionType.ACTION_TYPE_TEMPERATURE_STATE:
                    OnXREALGlassesTemperatureLevel?.Invoke((XREALTemperatureLevel)data.para);
                    break;
                default:
                    OnGlassesEventEnterprise?.Invoke(data);
                    break;
            }
        }
    }

    public static partial class XREALPlugin
    {
        [DllImport(LibName)]
        internal static extern void SetGlassesEventCallback(XREALGlassesEventCallback callback);

        [DllImport(LibName)]
        internal static extern void SetNativeLogCallback(XREALLogCallback callback);

        [DllImport(LibName)]
        internal static extern void SetNativeErrorCallback(XREALErrorCallback callback);
    }
}
