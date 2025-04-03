using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.XR.XREAL
{
    /// <summary>Mediates multi-resume functionality and Android native interactions.</summary>
    public class XREALMultiResumeMediator : SingletonMonoBehaviour<XREALMultiResumeMediator>
    {
        /// <summary>Indicates if the app is running in multi-resume background mode.</summary>
        public bool IsMultiResumeBackground { get; private set; } = false;

        /// <summary>Triggered when floating window state changes.</summary>
        public static event Action<bool> FloatingWindowStateChanged;

        /// <summary>Triggered when floating window receives click event.</summary>
        public static event Action FloatingWindowClicked;

        /// <summary>Proxy for handling XR display events from native code.</summary>
        XRDisplayProxy mXRDisplayProxy = null;

        /// <summary>Cached Java class instance for native multi-resume operations.</summary>
        AndroidJavaClass m_MultiResumeNativeInstance;

        /// <summary>Provides access to the native Android NRXRApp class instance.</summary>
        AndroidJavaClass NativeInstance
        {
            get
            {
                m_MultiResumeNativeInstance ??= new AndroidJavaClass("ai.nreal.activitylife.NRXRApp");
                return m_MultiResumeNativeInstance;
            }
        }

        /// <summary>Android callback handler for floating window state changes.</summary>
        class FloatingManagerListener : AndroidJavaProxy
        {
            /// <summary>Initializes callback handler with Java interface name.</summary>
            public FloatingManagerListener() : base("ai.nreal.activitylife.IFloatingManagerCallback") { }

            /// <summary>Native callback: Floating window became visible.</summary>
            public void onFloatingViewShown() => FloatingWindowStateChanged?.Invoke(true);

            /// <summary>Native callback: Floating window was dismissed.</summary>
            public void onFloatingViewDismissed() => FloatingWindowStateChanged?.Invoke(false);

            /// <summary>Native callback: Floating window received click event.</summary>
            public void onFloatingViewClicked() => FloatingWindowClicked?.Invoke();
        }

#if !UNITY_EDITOR && UNITY_ANDROID
        /// <summary>Initializes multi-resume system on Android platform startup.</summary>
        [RuntimeInitializeOnLoadMethod]
        static void OnLoad()
        {
            Debug.Log("[XREALMultiResumeMediator] OnLoad");
            if (XREALSettings.GetSettings().SupportMultiResume)
            {
                if (Singleton == null)
                    CreateSingleton();
                Singleton.gameObject.name = "NRNativeMediator";

                var cls = new AndroidJavaClass("ai.nreal.activitylife.FloatingManager");
                cls.CallStatic("setNRXRAppCallback", new FloatingManagerListener());
                XREALFloatingViewProvider.Instance.RegisterFloatViewProxy(new XREALDefaultFloatingViewProxy());
            }
        }
#endif

        /// <summary>Native callback: Updates multi-resume background state.</summary>
        /// <param name="state">"true" = enter background mode, "false" = exit background mode</param>
        [Preserve]
        void SetMultiResumeBackground(string state)
        {
            IsMultiResumeBackground = state == "true";
            Debug.LogFormat("SetMultiResumeBackground: state={0}, isMultiResumeBackground={1}", state, IsMultiResumeBackground);
        }

        /// <summary>Broadcasts controller display mode to native system.</summary>
        /// <param name="displayMode">Target display mode identifier</param>
        public void BroadcastControllerDisplayMode(int displayMode)
        {
            NativeInstance.CallStatic("broadcastControllerDisplayMode", displayMode);
        }

        /// <summary>Triggers dynamic display switch procedure.</summary>
        public void BroadcastDynamicSwitchDP()
        {
            NativeInstance.CallStatic("broadcastDynamicSwitchDP");
        }

        /// <summary>Forces termination of the application through native system.</summary>
        public void ForceKill()
        {
            NativeInstance.CallStatic("forceKill");
        }

        /// <summary>Requests native system to move app to background state.</summary>
        public void MoveToBackOnNR()
        {
            NativeInstance.CallStatic("moveToBackOnNR");
        }

        /// <summary>Retrieves native Android activity instance for UI operations.</summary>
        /// <returns>AndroidJavaObject representing proxy activity</returns>
        public AndroidJavaObject GetFakeActivity()
        {
            return NativeInstance.CallStatic<AndroidJavaObject>("getFakeActivity");
        }

        /// <summary>Gets connected XREAL glasses display ID from native system.</summary>
        /// <returns>Display identifier integer</returns>
        public int GetXrealGlassesDisplayId()
        {
            return NativeInstance.CallStatic<int>("getNrealGlassesDisplayId");
        }

        /// <summary>Prepares native system for dynamic display switching.</summary>
        public void PrepareDynamicSwitchDP()
        {
            NativeInstance.CallStatic("prepareDynamicSwitchDP");
        }

        /// <summary>Checks native system readiness for display switch.</summary>
        /// <returns>True if system is ready for switching</returns>
        public bool ReadyForDynamicSwitchDP()
        {
            return NativeInstance.CallStatic<bool>("readyForDynamicSwitchDP");
        }

        /// <summary>Checks native system readiness for session restart.</summary>
        /// <returns>True if session can be restarted</returns>
        public bool ReadyForRestartSession()
        {
            return NativeInstance.CallStatic<bool>("readyForRestartSession");
        }

        /// <summary>Registers listener for XR display events.</summary>
        /// <param name="listener">Implementation of display event interface</param>
        public void AddXRDisplayListener(IXRDisplayListener listener)
        {
            if (mXRDisplayProxy == null)
                mXRDisplayProxy = new XRDisplayProxy(NativeInstance);
            mXRDisplayProxy.AddListener(listener);
        }

        /// <summary>Unregisters XR display event listener.</summary>
        /// <param name="listener">Previously registered listener</param>
        public void RemoveXRDisplayListener(IXRDisplayListener listener)
        {
            mXRDisplayProxy?.RemoveListener(listener);
        }
    }
}