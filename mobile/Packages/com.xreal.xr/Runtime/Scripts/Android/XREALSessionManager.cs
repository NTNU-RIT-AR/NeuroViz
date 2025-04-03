#if UNITY_ANDROID && UNITY_INPUT_SYSTEM
using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// A singleton MonoBehaviour class that manages session-related functionality for the XREAL SDK.
    /// This class is responsible for managing the home menu actions, recentering, and caching the camera's position and rotation.
    /// It ensures that the session can be resumed with the appropriate camera state when the application is paused and resumed.
    /// </summary>
    public class XREALSessionManager : SingletonMonoBehaviour<XREALSessionManager>
    {
        [SerializeField]
        InputActionReference m_MenuAction;
        [SerializeField]
        InputActionReference m_RecenterAction;
        [SerializeField]
        GameObject m_MenuPrefab;
        [SerializeField]
        bool m_RecenterVibrationEnabled = true;
        [SerializeField]
        float m_VibrationAmplitude = 0.25f;
        [SerializeField]
        float m_VibrationDuration = 0.15f;
        [SerializeField]
        bool m_CacheCameraXRotation = false;
        [SerializeField]
        bool m_CacheCameraYRotation = true;
        [SerializeField]
        bool m_CacheCameraPosition = true;

        Transform m_Camera;
        Transform m_CameraOffset;
        float m_CacheXRotation = 0;
        float m_CacheYRotation = 0;
        Vector3 m_CachePosition = Vector3.zero;
        Coroutine m_CacheCoroutine;

        void Start()
        {
            if (XREALUtility.MainCamera != null)
            {
                m_Camera = XREALUtility.MainCamera.transform;
                m_CameraOffset = m_Camera.parent;
            }
        }

        void OnEnable()
        {
            if (m_MenuAction != null && m_MenuAction.action != null)
            {
                m_MenuAction.action.performed += OnMenuActionPerformed;
            }
            if (m_RecenterAction != null && m_RecenterAction.action != null)
            {
                m_RecenterAction.action.performed += OnRecenterActionPerformed;
            }
            XREALPlugin.OnTrackingTypeChangedInternal += OnTrackingTypeChangedInternal;
        }

        void OnDisable()
        {
            if (m_MenuAction != null && m_MenuAction.action != null)
            {
                m_MenuAction.action.performed -= OnMenuActionPerformed;
            }
            if (m_RecenterAction != null && m_RecenterAction.action != null)
            {
                m_RecenterAction.action.performed -= OnRecenterActionPerformed;
            }
            XREALPlugin.OnTrackingTypeChangedInternal -= OnTrackingTypeChangedInternal;
        }

        void OnMenuActionPerformed(InputAction.CallbackContext context)
        {
            if (context.interaction is PressInteraction)
            {
                if (XREALHomeMenu.Singleton == null)
                {
                    if (m_MenuPrefab != null)
                        Instantiate(m_MenuPrefab);
                }
                else
                {
                    XREALHomeMenu.Singleton.Toggle();
                }
            }
        }

        void OnRecenterActionPerformed(InputAction.CallbackContext context)
        {
            if (context.interaction is HoldInteraction)
            {
                XREALPlugin.RecenterController();
                if (m_RecenterVibrationEnabled && XREALVirtualController.Singleton != null)
                    XREALVirtualController.Singleton.Controller.SendHapticImpulse(0, m_VibrationAmplitude, m_VibrationDuration);
            }
        }

        void OnApplicationPause(bool pause)
        {
            if (m_Camera == null || m_CameraOffset == null)
                return;
            if (pause)
            {
                if (m_CacheCameraXRotation || m_CacheCameraYRotation)
                {
                    Quaternion combinedRotation = m_CameraOffset.localRotation * m_Camera.localRotation;
                    if (m_CacheCameraXRotation)
                        m_CacheXRotation = combinedRotation.eulerAngles.x;
                    if (m_CacheCameraYRotation)
                        m_CacheYRotation = combinedRotation.eulerAngles.y;
                }
                if (m_CacheCameraPosition && XREALPlugin.GetTrackingType() == TrackingType.MODE_6DOF)
                {
                    m_CachePosition = m_CameraOffset.localPosition + m_CameraOffset.localRotation * m_Camera.localPosition;
                    if (m_CacheCoroutine != null)
                    {
                        StopCoroutine(m_CacheCoroutine);
                        m_CacheCoroutine = null;
                    }
                }
            }
            else
            {
                if (m_CacheCameraXRotation || m_CacheCameraYRotation)
                    m_CameraOffset.localEulerAngles = new Vector3(m_CacheXRotation, m_CacheYRotation, 0);
                if (m_CacheCameraPosition && XREALPlugin.GetTrackingType() == TrackingType.MODE_6DOF)
                {
                    m_CameraOffset.localPosition = m_CachePosition;
                    m_Camera.localPosition = Vector3.zero;
                    m_CacheCoroutine = StartCoroutine(AdjustCameraOffset());
                }
            }
        }

        IEnumerator AdjustCameraOffset()
        {
            while (m_Camera.localPosition == Vector3.zero)
                yield return null;
            m_CameraOffset.localPosition -= m_CameraOffset.localRotation * m_Camera.localPosition;
            m_CacheCoroutine = null;
        }

        void OnTrackingTypeChangedInternal(bool result, TrackingType targetTrackingType)
        {
            if (result)
            {
                if (m_CameraOffset != null && m_CameraOffset.GetComponentInParent<XROrigin>() is XROrigin origin)
                    m_CameraOffset.SetLocalPositionAndRotation(origin.CameraYOffset * Vector3.up, Quaternion.identity);
                m_CacheXRotation = 0;
                m_CacheYRotation = 0;
                m_CachePosition = Vector3.zero;
                if (m_CacheCoroutine != null)
                {
                    StopCoroutine(m_CacheCoroutine);
                    m_CacheCoroutine = null;
                }
            }
        }
    }
}
#endif
