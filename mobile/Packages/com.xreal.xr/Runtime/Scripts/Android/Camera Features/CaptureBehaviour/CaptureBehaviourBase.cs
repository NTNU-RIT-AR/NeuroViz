using System;
using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary> A capture behaviour base. </summary>
    public class CaptureBehaviourBase : MonoBehaviour, IFrameConsumer
    {
        /// <summary> The RGB camera rig. </summary>
        [SerializeField] Transform CameraRig;
        /// <summary> The capture camera. </summary>
        public Camera CaptureCamera;
        public Camera CaptureCamera2;
        private FrameCaptureContext m_FrameCaptureContext;

        /// <summary> Gets the context. </summary>
        /// <returns> The context. </returns>
        public FrameCaptureContext GetContext()
        {
            return m_FrameCaptureContext;
        }

        /// <summary> Initializes this object. </summary>
        /// <param name="context">     The context.</param>
        /// <param name="blendCamera"> The blend camera.</param>
        public virtual void Init(FrameCaptureContext context)
        {
            this.m_FrameCaptureContext = context;
        }

        /// <summary> Updates the capture behaviour. </summary>
        protected virtual void Update()
        {
            if (m_FrameCaptureContext != null && m_FrameCaptureContext.RequestCameraParam().lockRoll)
            {
                Vector3 eulerAngles = CaptureCamera.transform.eulerAngles;
                eulerAngles.z = 0;
                CaptureCamera.transform.eulerAngles = eulerAngles;
                Vector3 eulerAngles2 = CaptureCamera2.transform.eulerAngles;
                eulerAngles2.z = 0;
                CaptureCamera2.transform.eulerAngles = eulerAngles2;
            }
        }

        public void SetCameraMask(int mask)
        {
            CaptureCamera.cullingMask = mask;
            CaptureCamera2.cullingMask = mask;
        }

        public void SetBackGroundColor(Color color)
        {
            CaptureCamera.backgroundColor = color;
            CaptureCamera2.backgroundColor = color;
        }

        /// <summary> Executes the 'frame' action. </summary>
        /// <param name="frame"> The frame.</param>
        public virtual void OnFrame(UniversalTextureFrame frame)
        {
            var mode = m_FrameCaptureContext.GetBlender().BlendMode;
            switch (mode)
            {
                case BlendMode.CameraOnly:
                    MoveToGod();
                    break;
                case BlendMode.Blend:
                case BlendMode.VirtualOnly:
                    // update camera pose
                    UpdateHeadPoseByTimestamp(frame.timeStamp);
                    break;
                default:
                    break;
            }
        }

        private void MoveToGod()
        {
            CameraRig.transform.position = Vector3.one * 9999;
        }

        /// <summary> Updates the head pose by timestamp described by timestamp. </summary>
        /// <param name="timestamp"> The timestamp.</param>
        private void UpdateHeadPoseByTimestamp(UInt64 timestamp)
        {
            Pose head_pose = Pose.identity;
            //             var result = NRSessionManager.Instance.NRHMDPoseTracker.GetHeadPoseByTimeInUnityWorld(ref head_pose, timestamp);
            //             if (result)
            //             {
            //                 // Debug.Log("UpdateHeadPoseByTimestamp: timestamp={0}, pos={1}", timestamp, head_pose.ToString("F2"));
            //                 RGBCameraRig.transform.position = head_pose.position;
            //                 RGBCameraRig.transform.rotation = head_pose.rotation;
            //             }
        }
    }
}
