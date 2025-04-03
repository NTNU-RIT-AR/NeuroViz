using System;
using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// A generic singleton MonoBehaviour base class that ensures only one instance of the type exists in the scene.
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T s_Singleton;

        /// <summary>
        /// Gets the singleton instance of the type <typeparamref name="T"/>.
        /// </summary>
        public static T Singleton => s_Singleton;

        /// <summary>
        /// Creates or returns the singleton instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <returns>The singleton instance of the type <typeparamref name="T"/>.</returns>
        public static T CreateSingleton()
        {
            if (s_Singleton == null)
            {
                new GameObject(typeof(T).Name, typeof(T));
            }

            return s_Singleton;
        }

        protected virtual void Awake()
        {
            if (s_Singleton != null)
            {
                Debug.LogWarning($"[SingletonMonoBehaviour] There has been an instance already! {typeof(T).Name}");
                Destroy(this);
                return;
            }
            s_Singleton = this as T;
            if (gameObject.transform.parent == null)
                DontDestroyOnLoad(gameObject);
        }

        protected virtual void OnDestroy()
        {
            if (s_Singleton == this as T)
            {
                s_Singleton = null;
            }
        }
    }

    /// <summary>
    /// A simple thread-safe singleton class for non-MonoBehaviour types.
    /// </summary>
    public class Singleton<T> where T : class, new()
    {
        static readonly Lazy<T> _lazyInstance = new();

        /// <summary>
        /// Gets the singleton instance of the type <typeparamref name="T"/>.
        /// </summary>
        public static T Instance => _lazyInstance.Value;
    }
}
