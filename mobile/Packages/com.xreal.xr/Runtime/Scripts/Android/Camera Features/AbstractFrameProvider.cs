using UnityEngine;

namespace Unity.XR.XREAL
{
    public enum TextureType
    {
        RGB,
        YUV,
        R8
    }

    public struct UniversalTextureFrame
    {
        public TextureType textureType;
        public ulong timeStamp;
        public uint gain;
        public uint exposureTime;
        public Texture[] textures;
    }

    public abstract class AbstractFrameProvider
    {
        public delegate void UpdateImageFrame(UniversalTextureFrame frame);

        public UpdateImageFrame OnUpdate;

        protected bool m_IsFrameReady = false;

        public virtual Resolution GetFrameInfo() { return new Resolution(); }

        public virtual bool IsFrameReady() { return m_IsFrameReady; }

        public virtual void Play() { }

        public virtual void Stop() { }

        public virtual void Release() { }
    }
}
