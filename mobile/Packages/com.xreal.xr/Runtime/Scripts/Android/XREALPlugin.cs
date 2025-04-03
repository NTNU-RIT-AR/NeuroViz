#if UNITY_ANDROID && !UNITY_EDITOR
#define XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
#endif

using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.Management;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// Provides a set of utility functions and events for interacting with the XREAL XR Plugin.
    /// </summary>
    public static partial class XREALPlugin
    {
        /// <summary>
        /// Exits the application or editor, deinitializing the XR loader if active.
        /// </summary>
        public static void QuitApplication()
        {
            Debug.Log("[XREALPlugin] QuitApplication");
            if (XREALUtility.GetActiveLoader() != null)
            {
                XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            }
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_ANDROID
            if (XREALMultiResumeMediator.Singleton != null && XREALSettings.GetSettings().SupportMultiResume)
            {
                Debug.Log("[XREALPlugin] ForceKill");
                XREALMultiResumeMediator.Singleton.ForceKill();
            }
            else
            {
                Debug.Log("[XREALPlugin] Quit");
                Application.Quit();
            }
#else
            Debug.Log("[XREALPlugin] Quit");
            Application.Quit();
#endif
        }

        /// <summary>
        /// Sets the target frame rate for the application.
        /// </summary>
        /// <param name="targetFrameRate">The desired frame rate.</param>
        public static void SetTargetFrameRate(int targetFrameRate)
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            Internal.SetTargetFrameRate(targetFrameRate);
#endif
        }

        /// <summary>
        /// Gets the current target frame rate of the application.
        /// </summary>
        /// <returns>The target frame rate.</returns>
        public static int GetTargetFrameRate()
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            return Internal.GetTargetFrameRate();
#else
            return 0;
#endif
        }

        /// <summary>
        /// Sets the dominant hand for controller interaction.
        /// </summary>
        /// <param name="isRightHand">True if the right hand is dominant; false for the left hand.</param>
        public static void SetDominantHand(bool isRightHand)
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            Internal.SetDominantHand(isRightHand);
#endif
        }

        /// <summary>
        /// Recenters the controller's tracking rotation.
        /// </summary>
        public static bool RecenterController()
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            return Internal.RecenterController();
#else
            return false;
#endif
        }

        /// <summary>
        /// Gets the current input source for the XREAL input system.
        /// </summary>
        /// <returns>The current input source.</returns>
        public static InputSource GetInputSource()
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            return Internal.GetInputSource();
#else
            return InputSource.Controller;
#endif
        }

        /// <summary>
        /// Sets the input source for the XREAL input system.
        /// </summary>
        /// <param name="inputSource">The input source to set.</param>
        public static bool SetInputSource(InputSource inputSource)
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            return Internal.SetInputSource(inputSource);
#else
            return false;
#endif
        }

        /// <summary>
        /// Switches the current input source between hands and controller.
        /// </summary>
        /// <returns>True if the input source was successfully switched; otherwise, false.</returns>
        public static bool SwitchInputSource()
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            if (GetInputSource() == InputSource.Controller)
                return SetInputSource(InputSource.Hands);
            else
                return SetInputSource(InputSource.Controller);
#else
            return false;
#endif
        }

        /// <summary>
        /// Checks if hand tracking is supported by the XREAL input system.
        /// </summary>
        /// <returns>True if hand tracking is supported; otherwise, false.</returns>
        public static bool IsHandTrackingSupported()
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            return Internal.IsHandTrackingSupported();
#else
            return false;
#endif
        }

        private static partial class Internal
        {
            [DllImport(LibName)]
            public static extern void SetTargetFrameRate(int targetFrameRate);

            [DllImport(LibName)]
            public static extern int GetTargetFrameRate();

            [DllImport(LibName)]
            internal static extern void SetDominantHand(bool isRightHand);

            [DllImport(LibName)]
            internal static extern bool RecenterController();

            [DllImport(LibName)]
            internal static extern InputSource GetInputSource();

            [DllImport(LibName)]
            internal static extern bool SetInputSource(InputSource inputSource);

            [DllImport(LibName)]
            internal static extern bool IsHandTrackingSupported();
        }
    }
}
