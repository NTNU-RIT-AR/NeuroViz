using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary> A nr device param initializer. </summary>
    [RequireComponent(typeof(Camera))]
    public class XREALCameraInitializer : MonoBehaviour
    {
        /// <summary> Type of the device. </summary>
        [SerializeField] XREALComponent m_DeviceType = XREALComponent.XREAL_COMPONENT_RGB_CAMERA;

        private Camera m_TargetCamera;
        public bool IsInitialized { get; private set; } = false;

        void Start()
        {
            Initialize();
        }

        private void Initialize()
        {

            if (m_TargetCamera == null)
            {
                m_TargetCamera = gameObject.GetComponent<Camera>();
            }
#if !UNITY_EDITOR
            if (m_DeviceType == XREALComponent.XREAL_COMPONENT_RGB_CAMERA && !XREALPlugin.IsHMDFeatureSupported(XREALSupportedFeature.XREAL_FEATURE_RGB_CAMERA))
            {
                Debug.LogWarningFormat("[NRCameraInitializer] Auto adaption for DevieType : {0} ==> {1}", m_DeviceType, XREALComponent.XREAL_COMPONENT_DISPLAY_LEFT);
                m_DeviceType = XREALComponent.XREAL_COMPONENT_DISPLAY_LEFT;
            }
            else if ((m_DeviceType == XREALComponent.XREAL_COMPONENT_GRAYSCALE_CAMERA_LEFT || m_DeviceType == XREALComponent.XREAL_COMPONENT_GRAYSCALE_CAMERA_RIGHT))
            {
                Vector2Int size = Vector2Int.zero;
                bool result = XREALPlugin.GetDeviceResolution(m_DeviceType, ref size);

                if (!result || size.x == 0)
                {
                    if (m_DeviceType == XREALComponent.XREAL_COMPONENT_GRAYSCALE_CAMERA_LEFT)
                    {
                        m_DeviceType = XREALComponent.XREAL_COMPONENT_DISPLAY_LEFT;
                    }
                    else if (m_DeviceType == XREALComponent.XREAL_COMPONENT_GRAYSCALE_CAMERA_RIGHT)
                    {
                        m_DeviceType = XREALComponent.XREAL_COMPONENT_DISPLAY_RIGHT;
                    }
                }
            }
            Matrix4x4 matrix = Matrix4x4.identity;
            XREALPlugin.GetCameraProjectionMatrix(m_DeviceType, m_TargetCamera.nearClipPlane, m_TargetCamera.farClipPlane, ref matrix);
            m_TargetCamera.projectionMatrix = matrix;
            Pose pose = Pose.identity;
            XREALPlugin.GetDevicePoseFromHead(m_DeviceType, ref pose);
            transform.localPosition = pose.position;
            transform.localRotation = pose.rotation;
            Debug.Log($"[NRCameraInitializer] Initialize: m_DeviceType={m_DeviceType}, projectionMatrix={m_TargetCamera.projectionMatrix}, position={transform.localPosition},rotation={transform.localRotation}");
#endif
            IsInitialized = true;
        }

        /// <summary> Switch to eye parameter. </summary>
        /// <param name="eye"> The eye.</param>
        public void SwitchToEyeParam(XREALComponent eye)
        {
            if (m_DeviceType == eye)
            {
                return;
            }

            m_DeviceType = eye;
#if !UNITY_EDITOR
            Initialize();
#endif
        }
    }
}
