using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary> An editor frame provider. </summary>
    public class EditorFrameProvider : NullDataFrameProvider
    {
        public EditorFrameProvider() : base(NativeConstants.RECORD_FPS_DEFAULT)
        {
            Texture temp = Resources.Load<Texture2D>("Textures/captureDefault");
            var mat = new Material(Resources.Load<Shader>("Shaders/CaptureBackground"));
            RenderTexture rt = new RenderTexture(temp.width, temp.height, 24, RenderTextureFormat.ARGB32, QualitySettings.activeColorSpace == ColorSpace.Linear ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Default);
            rt.wrapMode = TextureWrapMode.Clamp;
            rt.antiAliasing = 1;
            rt.Create();

            Graphics.Blit(temp, rt, mat);

            m_DefaultFrame.textures = new Texture[3];
            m_DefaultFrame.textureType = TextureType.RGB;
            m_DefaultFrame.textures[0] = rt;
        }
    }
}
