using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary>Interface for managing Android floating window operations.</summary>
    public interface IFloatingViewProxy
    {
        /// <summary>Creates native floating view object.</summary>
        /// <returns>AndroidJavaObject representing the view.</returns>
        AndroidJavaObject CreateFloatingView();

        /// <summary>Displays the floating window.</summary>
        void Show();

        /// <summary>Hides the floating window.</summary>
        void Hide();

        /// <summary>Destroys the floating window resources.</summary>
        void DestroyFloatingView();
    }

    /// <summary>Default implementation for XREAL floating window management.</summary>
    public class XREALDefaultFloatingViewProxy : IFloatingViewProxy
    {
        private AndroidJavaObject mJavaProxyObject;

        /// <summary>Initializes Android proxy with Unity activity context.</summary>
        public XREALDefaultFloatingViewProxy()
        {
            var clsUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = clsUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            mJavaProxyObject = new AndroidJavaObject("ai.nreal.activitylife.NRDefaultFloatingViewProxy", activity);
        }

        /// <summary>Creates floating view through JNI.</summary>
        public AndroidJavaObject CreateFloatingView()
        {
            return mJavaProxyObject.Call<AndroidJavaObject>("CreateFloatingView");
        }

        /// <summary>Hides floating view via native call.</summary>
        public void Hide()
        {
            mJavaProxyObject.Call("Hide");
        }

        /// <summary>Shows floating view via native call.</summary>
        public void Show()
        {
            mJavaProxyObject.Call("Show");
        }

        /// <summary>Destroys floating view resources via native call.</summary>
        public void DestroyFloatingView()
        {
            mJavaProxyObject.Call("DestroyFloatingView");
        }
    }

    /// <summary>Singleton manager for floating view lifecycle.</summary>
    public class XREALFloatingViewProvider : Singleton<XREALFloatingViewProvider>
    {
        /// <summary>Cached Java instance of floating view manager.</summary>
        protected AndroidJavaObject mJavaFloatingViewManager;

        /// <summary>Lazy-loaded Android floating manager singleton.</summary>
        protected AndroidJavaObject JavaFloatingViewManager
        {
            get
            {
                if (mJavaFloatingViewManager == null)
                {
                    var cls = new AndroidJavaClass("ai.nreal.activitylife.FloatingManager");
                    mJavaFloatingViewManager = cls.CallStatic<AndroidJavaObject>("getInstance");
                }
                return mJavaFloatingViewManager;
            }
        }

        /// <summary>Proxy wrapper for C#/Java interop.</summary>
        protected class XREALFloatingViewProxyWrapper : AndroidJavaProxy
        {
            private IFloatingViewProxy mProxy;
            /// <summary>Get associated  proxy instance.</summary>
            public IFloatingViewProxy FloatingViewProxy => mProxy;

            /// <summary>Initialize proxy bridge with implementation.</summary>
            /// <param name="proxy">Concrete proxy implementation.</param>
            public XREALFloatingViewProxyWrapper(IFloatingViewProxy proxy) : base("ai.nreal.activitylife.IFloatingViewProxy")
            {
                mProxy = proxy;
            }

            /// <summary>Delegate create call to proxy.</summary>
            public AndroidJavaObject CreateFloatingView()
            {
                return mProxy?.CreateFloatingView();
            }

            /// <summary>Delegate show call to proxy.</summary>
            public void Show()
            {
                mProxy?.Show();
            }

            /// <summary>Delegate hide call to proxy.</summary>
            public void Hide()
            {
                mProxy?.Hide();
            }

            /// <summary>Delegate destroy call to proxy.</summary>
            public void DestroyFloatingView()
            {
                mProxy?.DestroyFloatingView();
            }
        }

        private XREALFloatingViewProxyWrapper mProxyWrapper = null;

        /// <summary>Register proxy implementation with Android system.</summary>
        /// <param name="proxy">Proxy implementation to register.</param>
        public void RegisterFloatViewProxy(IFloatingViewProxy proxy)
        {
            mProxyWrapper = new XREALFloatingViewProxyWrapper(proxy);
            JavaFloatingViewManager.Call("setFloatingViewProxy", mProxyWrapper);
        }

        /// <summary>Get currently active proxy instance.</summary>
        /// <returns>Registered proxy or null.</returns>
        public IFloatingViewProxy GetCurrentFloatViewProxy()
        {
            return mProxyWrapper?.FloatingViewProxy;
        }
    }
}