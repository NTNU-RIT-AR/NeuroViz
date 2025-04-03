using System.Threading;
using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary> A RGB camera frame provider. </summary>
    public class RGBCameraFrameProvider : AbstractFrameProvider
    {
        /// <summary> The RGB tex. </summary>
        private XREALRGBCameraTexture m_CameraTexture;
        private UniversalTextureFrame frameInfo;

        /// <summary> Default constructor. </summary>
        public RGBCameraFrameProvider()
        {
            m_CameraTexture = XREALRGBCameraTexture.CreateSingleton();
            m_CameraTexture.OnRGBCameraUpdate += UpdateYUVFrame;
            frameInfo.textures = new Texture[3];
            frameInfo.textureType = TextureType.YUV;
        }

        private void UpdateYUVFrame()
        {
            var textures = m_CameraTexture.GetYUVFormatTextures();
            frameInfo.timeStamp = m_CameraTexture.GetTimeStamp();
            frameInfo.textures[0] = textures[0];
            frameInfo.textures[1] = textures[1];
            frameInfo.textures[2] = textures[2];
            OnUpdate?.Invoke(frameInfo);
            m_IsFrameReady = true;
        }

        /// <summary> Gets frame information. </summary>
        /// <returns> The frame information. </returns>
        public override Resolution GetFrameInfo()
        {
            var resolution = m_CameraTexture.GetResolution();
            return new Resolution() { width = resolution.x, height = resolution.y };
        }

        /// <summary> Plays this object. </summary>
        public override void Play()
        {
            Debug.Log($"[RGBCameraFrameProvider] Play");
            m_CameraTexture.StartCapture();
        }

        /// <summary> Stops this object. </summary>
        public override void Stop()
        {
            Debug.Log($"[RGBCameraFrameProvider] Stop");
            ThreadPool.QueueUserWorkItem(_ =>
            {
                m_CameraTexture.StopCapture();
            });

        }

        /// <summary> Releases this object. </summary>
        public override void Release()
        {
            Debug.Log($"[RGBCameraFrameProvider] Release");
            if (m_CameraTexture != null)
            {
                m_CameraTexture.OnRGBCameraUpdate -= UpdateYUVFrame;
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    m_CameraTexture.StopCapture();
                    m_CameraTexture = null;
                });
            }

            Debug.Log($"[RGBCameraFrameProvider] Release end");
        }
    }
}
