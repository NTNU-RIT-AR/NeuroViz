using System;
using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary>Manages RGB camera texture data capture and YUV format processing.</summary>
    public class XREALRGBCameraTexture : SingletonMonoBehaviour<XREALRGBCameraTexture>
    {
        /// <summary>Triggered when new camera frame data is available.</summary>
        public Action OnRGBCameraUpdate;

        private Texture2D m_TextureY, m_TextureU, m_TextureV;
        private ulong m_Timestamp = 0;
        private Vector2Int m_Resolution = Vector2Int.zero;

        /// <summary>Indicates if camera capture is currently active.</summary>
        private bool m_IsCapturing = false;
        public bool IsCapturing => m_IsCapturing;

        /// <summary>Starts RGB camera data capture through native plugin.</summary>
        public void StartCapture()
        {
            Debug.Log($"[XREALRGBCameraTexture] StartCapture");
            XREALPlugin.StartRGBCameraDataCapture();
            m_IsCapturing = true;
        }

        /// <summary>Stops active camera data capture.</summary>
        public void StopCapture()
        {
            Debug.Log($"[XREALRGBCameraTexture] StopCapture");
            XREALPlugin.StopRGBCameraDataCapture();
            m_IsCapturing = false;
        }

        /// <summary>Gets YUV plane textures in order: Y, U, V.</summary>
        /// <returns>Array of three Texture2D objects</returns>
        public Texture2D[] GetYUVFormatTextures()
        {
            return new Texture2D[] { m_TextureY, m_TextureU, m_TextureV };
        }

        /// <summary>Gets current capture resolution.</summary>
        /// <returns>Resolution as Vector2Int (x=width, y=height)</returns>
        public Vector2Int GetResolution()
        {
            return m_Resolution;
        }

        /// <summary>Gets timestamp of latest captured frame.</summary>
        /// <returns>Timestamp in native units</returns>
        public ulong GetTimeStamp()
        {
            return m_Timestamp;
        }

        /// <summary>Main update loop for frame processing.</summary>
        private void Update()
        {
            if (XREALPlugin.TryGetRGBCameraFrame(ref m_Timestamp))
            {
                int frameHandle = 0;
                if (XREALPlugin.TryAcquireLatestImage(ref frameHandle, ref m_Resolution, ref m_Timestamp))
                {
                    try
                    {
                        LoadYUVFormatTexture(frameHandle);
                    }
                    finally
                    {
                        XREALPlugin.DisposeRGBCameraDataHandle(frameHandle);
                    }
                    OnRGBCameraUpdate?.Invoke();
                }
            }
        }

        /// <summary>Loads YUV data into separate textures.</summary>
        /// <param name="frameHandle">Native frame data reference</param>
        private unsafe void LoadYUVFormatTexture(int frameHandle)
        {
            // Initialize textures on first frame
            if (m_TextureY == null)
            {
                int Width = m_Resolution.x;
                int Height = m_Resolution.y;

                // Y plane: full resolution (Alpha8 format for 8-bit luminance)
                m_TextureY = new Texture2D(Width, Height, TextureFormat.Alpha8, false);

                // U/V planes: half resolution (4:2:0 chroma subsampling)
                m_TextureU = new Texture2D(Width / 2, Height / 2, TextureFormat.Alpha8, false);
                m_TextureV = new Texture2D(Width / 2, Height / 2, TextureFormat.Alpha8, false);
            }

            // Get native data pointers for each plane
            XREALPlugin.TryGetRGBCameraDataPlane(frameHandle, 0, out IntPtr YData, out Vector2Int sizeY); 
            XREALPlugin.TryGetRGBCameraDataPlane(frameHandle, 1, out IntPtr VData, out Vector2Int sizeV); 
            XREALPlugin.TryGetRGBCameraDataPlane(frameHandle, 2, out IntPtr UData, out Vector2Int sizeU); 

            // Load raw byte data into textures
            m_TextureY.LoadRawTextureData(YData, sizeY.x * sizeY.y);
            m_TextureV.LoadRawTextureData(VData, sizeV.x * sizeV.y);
            m_TextureU.LoadRawTextureData(UData, sizeU.x * sizeU.y);

            // Apply texture updates to GPU
            m_TextureY.Apply();
            m_TextureU.Apply();
            m_TextureV.Apply();
        }
    }
}