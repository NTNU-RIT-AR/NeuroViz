using System.Collections.Generic;
using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary> A frame blender. </summary>
    public class FrameBlender : BlenderBase
    {
        /// <summary> Target camera. </summary>
        protected Camera[] m_TargetCamera;
        /// <summary> The encoder. </summary>
        protected IEncoder m_Encoder;
        /// <summary> The blend material. </summary>
        private Material m_BackGroundMat;
        private List<XREALBackGroundRender> m_NRBackGroundRender;
        private List<XREALCameraInitializer> m_DeviceParamInitializer;

        private CaptureSide m_CaputreSide;
        private RenderTexture m_BlendTexture;
        private RenderTexture m_BlendTextureLeft;
        private RenderTexture m_BlendTextureRight;
        /// <summary> Gets or sets the blend texture. </summary>
        /// <value> The blend texture. </value>
        public override RenderTexture BlendTexture
        {
            get
            {
                return m_BlendTexture;
            }
        }

        /// <summary> Initializes this object. </summary>
        /// <param name="cameraArray">  The camera.</param>
        /// <param name="encoder"> The encoder.</param>
        /// <param name="param">   The parameter.</param>
        public override void Init(Camera[] cameraArray, IEncoder encoder, CameraParameters param)
        {
            base.Init(cameraArray, encoder, param);

            Width = param.cameraResolutionWidth;
            Height = param.cameraResolutionHeight;
            m_TargetCamera = cameraArray;
            m_Encoder = encoder;
            BlendMode = param.blendMode;
            m_CaputreSide = param.captureSide;

            m_NRBackGroundRender = new List<XREALBackGroundRender>();
            m_DeviceParamInitializer = new List<XREALCameraInitializer>();
            for (var i = 0; i < m_TargetCamera.Length; ++i)
            {
                var backGroundRender = m_TargetCamera[i].gameObject.GetComponent<XREALBackGroundRender>();
                if (backGroundRender == null)
                {
                    backGroundRender = m_TargetCamera[i].gameObject.AddComponent<XREALBackGroundRender>();
                }
                backGroundRender.enabled = false;
                m_NRBackGroundRender.Add(backGroundRender);

                m_DeviceParamInitializer.Add(m_TargetCamera[i].gameObject.GetComponent<XREALCameraInitializer>());

                m_TargetCamera[i].enabled = false;
            }

            if (m_CaputreSide != CaptureSide.Both)
            {
                m_BlendTexture = CreateRenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            }
            else
            {
                m_BlendTextureLeft = CreateRenderTexture((int)(0.5f * Width), (int)(0.5f * Height), 24, RenderTextureFormat.ARGB32);
                m_BlendTextureRight = CreateRenderTexture((int)(0.5f * Width), (int)(0.5f * Height), 24, RenderTextureFormat.ARGB32);
                m_BlendTexture = CreateRenderTexture(Width, (int)(0.5f * Height), 24, RenderTextureFormat.ARGB32);
            }
        }

        /// <summary> Executes the 'frame' action. </summary>
        /// <param name="frame"> The frame.</param>
        public override void OnFrame(UniversalTextureFrame frame)
        {
            base.OnFrame(frame);

            for (var i = 0; i < m_DeviceParamInitializer.Count; ++i)
            {
                if (!m_DeviceParamInitializer[i].IsInitialized)
                {
                    return;
                }
            }

            if (m_BackGroundMat == null)
            {
                m_BackGroundMat = CreatBlendMaterial(frame.textureType);
                for (var i = 0; i < m_NRBackGroundRender.Count; ++i)
                {
                    m_NRBackGroundRender[i].SetMaterial(m_BackGroundMat);
                }
            }

            bool isyuv = frame.textureType == TextureType.YUV;
            const string MainTextureStr = "_MainTex";
            const string UTextureStr = "_UTex";
            const string VTextureStr = "_VTex";

            switch (BlendMode)
            {
                case BlendMode.VirtualOnly:
                    CameraRenderToTarget(false);
                    break;
                case BlendMode.CameraOnly:
                case BlendMode.Blend:
                    if (isyuv)
                    {
                        m_BackGroundMat.SetTexture(MainTextureStr, frame.textures[0]);
                        m_BackGroundMat.SetTexture(UTextureStr, frame.textures[1]);
                        m_BackGroundMat.SetTexture(VTextureStr, frame.textures[2]);
                    }
                    else
                    {
                        m_BackGroundMat.SetTexture(MainTextureStr, frame.textures[0]);
                    }
                    CameraRenderToTarget(true);
                    break;
            }

            // Commit frame                
            m_Encoder.Commit(BlendTexture, frame.timeStamp);
            FrameCount++;
        }
        private void CameraRenderToTarget(bool enableBackGround)
        {
            if (m_CaputreSide != CaptureSide.Both)
            {
                m_NRBackGroundRender[0].enabled = enableBackGround;
                m_TargetCamera[0].targetTexture = m_BlendTexture;
                m_TargetCamera[0].Render();
            }
            else
            {
                m_NRBackGroundRender[0].enabled = enableBackGround;
                m_NRBackGroundRender[1].enabled = false;
                m_TargetCamera[0].targetTexture = m_BlendTextureLeft;
                m_TargetCamera[0].Render();

                m_NRBackGroundRender[0].enabled = false;
                m_NRBackGroundRender[1].enabled = enableBackGround;
                m_TargetCamera[1].targetTexture = m_BlendTextureRight;
                m_TargetCamera[1].Render();

                MergeRenderTextures(m_BlendTextureLeft, m_BlendTextureRight, m_BlendTexture);
            }
        }
        private void MergeRenderTextures(Texture leftSrc, Texture rightSrc, RenderTexture target)
        {
            //Set the RTT in order to render to it
            Graphics.SetRenderTarget(target);

            //Setup 2D matrix in range 0..1, so nobody needs to care about sized
            GL.LoadPixelMatrix(0, 1, 1, 0);

            //Then clear & draw the texture to fill the entire RTT.
            GL.Clear(true, true, new Color(0, 0, 0, 0));

            Graphics.DrawTexture(new Rect(0, 0, 0.5f, 1.0f), leftSrc);
            Graphics.DrawTexture(new Rect(0.5f, 0, 0.5f, 1.0f), rightSrc);
        }
        private Material CreatBlendMaterial(TextureType texturetype)
        {
            string shader_name = string.Format("Shaders/CaptureBackground{0}", texturetype == TextureType.RGB ? "" : "YUV");
            return new Material(Resources.Load<Shader>(shader_name));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources. </summary>
        public override void Dispose()
        {
            base.Dispose();

            m_BlendTexture?.Release();
            m_BlendTexture = null;
            m_BlendTextureLeft?.Release();
            m_BlendTextureLeft = null;
            m_BlendTextureRight?.Release();
            m_BlendTextureRight = null;
        }

        private RenderTexture CreateRenderTexture(int width, int height, int depth = 24, RenderTextureFormat format = RenderTextureFormat.ARGB32, bool usequaAnti = true)
        {
            var rt = new RenderTexture(width, height, depth, format, QualitySettings.activeColorSpace == ColorSpace.Linear ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Default);
            rt.wrapMode = TextureWrapMode.Clamp;
            if (QualitySettings.antiAliasing > 0 && usequaAnti)
            {
                rt.antiAliasing = QualitySettings.antiAliasing;
            }
            else
            {
                rt.antiAliasing = 1;
            }

            rt.Create();
            return rt;
        }
    }
}
