using System;
using System.Collections.Generic;
using UnityEngine;
#if !UNITY_EDITOR
using AOT;
using System.Runtime.InteropServices;
#endif

namespace Unity.XR.XREAL
{
    /// <summary> A video encoder. </summary>
    public class AudioEncoder : IEncoderBase
    {
        private NativeEncoder mNativeEncoder;
        public NativeEncodeConfig EncodeConfig;
        private IntPtr androidMediaProjection { get; set; }

        private bool m_IsStarted = false;
        private AudioDataCallBack mDataCallBack = null;
        private static List<AudioEncoder> gInactiveAudioEncoders = new List<AudioEncoder>();

#if !UNITY_EDITOR
        private const int STARTENCODEEVENT = 0x2001;
        private const int STOPENCODEEVENT = 0x2002;
        private delegate void RenderEventDelegate(int eventID);
        private static RenderEventDelegate RenderThreadHandle = new RenderEventDelegate(RunOnRenderThread);
        private static IntPtr RenderThreadHandlePtr = Marshal.GetFunctionPointerForDelegate(RenderThreadHandle);
#endif

        /// <summary> Default constructor. </summary>
        public AudioEncoder()
        {
#if !UNITY_EDITOR
            mNativeEncoder = NativeEncoder.GetInstance();
            mNativeEncoder.Register(this);
#endif
        }

#if !UNITY_EDITOR
        [MonoPInvokeCallback(typeof(RenderEventDelegate))]
        private static void RunOnRenderThread(int eventID)
        {
            if (eventID == STARTENCODEEVENT)
            {
                lock (gInactiveAudioEncoders)
                {
                    for (int i = 0; i < gInactiveAudioEncoders.Count; i++)
                    {
                        NativeEncoder.GetInstance().StartAudioRecorder(gInactiveAudioEncoders[i].mDataCallBack);
                    }
                    gInactiveAudioEncoders.Clear();
                }
                
            }
        }
#endif

        /// <summary> Configurations the given parameter. </summary>
        /// <param name="param"> The parameter.</param>
        public void Config(CameraParameters param)
        {
            EncodeConfig = new NativeEncodeConfig(param);
            androidMediaProjection = (param.mediaProjection != null) ? param.mediaProjection.GetRawObject() : IntPtr.Zero;
        }

        /// <summary> Adjust the volume of encoder.</summary>
        /// <param name="recordIdx"> Recorder index.</param>
        /// <param name="factor"> The factor of volume.</param>
        public void AdjustVolume(RecorderIndex recordIdx, float factor)
        {
#if !UNITY_EDITOR
            mNativeEncoder.AdjustVolume(recordIdx, factor);
#endif
        }

        /// <summary> Starts this object. </summary>
        public void Start(AudioDataCallBack onAudioDataCallback = null)
        {
            if (m_IsStarted)
            {
                return;
            }
            Debug.Log("[AudioEncoder] Start");
            Debug.Log($"[AudioEncoder] Config {EncodeConfig.ToString()}");
            mDataCallBack = onAudioDataCallback;
            lock (gInactiveAudioEncoders)
            {
                gInactiveAudioEncoders.Add(this);
            }
#if !UNITY_EDITOR
            mNativeEncoder.SetConfigration(EncodeConfig, androidMediaProjection);
            //mNativeEncoder.StartAudioRecorder(mDataCallBack);
            GL.IssuePluginEvent(RenderThreadHandlePtr, STARTENCODEEVENT);
#endif
            m_IsStarted = true;
        }

        /// <summary> Stops this object. </summary>
        public void Stop()
        {
            if (!m_IsStarted)
            {
                return;
            }

            Debug.Log("[AudioEncoder] Stop");
            lock (gInactiveAudioEncoders)
            {
                gInactiveAudioEncoders.Remove(this);
            }
#if !UNITY_EDITOR
            mNativeEncoder.StopAudioRecorder(mDataCallBack);
#endif
            m_IsStarted = false;
        }

        /// <summary> Releases this object. </summary>
        public void Release()
        {
            Debug.Log("[AudioEncoder] Release...");
#if !UNITY_EDITOR
            mNativeEncoder.UnRegister(this);
            mNativeEncoder.Destroy();
#endif
        }
    }
}
