using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary> Callback, called when audio data is sampled. </summary>
    /// <param name="data">                The sampled audio data.</param>
    /// <param name="size">                The size of the audio data.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void AudioDataCallBack(IntPtr data, UInt32 size);

    /// <summary> A native encoder. </summary>
    public class NativeEncoder
    {
        public const string NRNativeEncodeLibrary = "libmedia_codec";
        public UInt64 EncodeHandle;
        private event AudioDataCallBack m_OnAudioDataCallback;
        private bool mVideoEncoderWorking = false;
        private bool mAudioEncoderWorking = false;
        private List<IEncoderBase> mActiveEncoders = new List<IEncoderBase>();

        //NativeEncoder can only exist one instance
        private static NativeEncoder gInstance = null;

        public static NativeEncoder GetInstance()
        {
            if (gInstance == null)
            {
                gInstance = new NativeEncoder();
            }
            return gInstance;
        }

        private NativeEncoder()
        {
            var result = NativeApi.HWEncoderCreate(ref EncodeHandle);
        }

        public bool Start()
        {
            if (mVideoEncoderWorking)
            {
                Debug.LogWarning("[NativeEncoder] video encoder is already working");
                return false;
            }
            var result = NativeApi.HWEncoderStart(EncodeHandle);
            bool success = (result == XREALErrorCode.Success);
            if (success)
            {
                mVideoEncoderWorking = true;
            }
            return success;
        }

        public bool StartAudioRecorder(AudioDataCallBack onAudioDataCallback)
        {
            if (onAudioDataCallback != null)
                m_OnAudioDataCallback += onAudioDataCallback;
            if (!mAudioEncoderWorking)
            {
                var result = NativeApi.HWEncoderStartOnlyAudioRecorder(EncodeHandle, OnAudioDataCallback);
                bool success = (result == XREALErrorCode.Success);
                if (success)
                {
                    mAudioEncoderWorking = true;
                }
                return success;
            }
            return true;
        }

        public bool StopAudioRecorder(AudioDataCallBack onAudioDataCallback)
        {
            if (onAudioDataCallback != null)
                m_OnAudioDataCallback -= onAudioDataCallback;
            var delegateArray = m_OnAudioDataCallback?.GetInvocationList();
            if ((delegateArray == null || delegateArray.Length == 0) && mAudioEncoderWorking)
            {
                var result = NativeApi.HWEncoderStopOnlyAudioRecorder(EncodeHandle);
                bool success = (result == XREALErrorCode.Success);
                if (success)
                {
                    mAudioEncoderWorking = false;
                }
                return success;
            }
            return true;
        }

        [MonoPInvokeCallback(typeof(AudioDataCallBack))]
        private static void OnAudioDataCallback(IntPtr data, UInt32 size)
        {
            if (gInstance != null)
                gInstance.m_OnAudioDataCallback?.Invoke(data, size);
        }

        public void SetConfigration(NativeEncodeConfig config, IntPtr androidMediaProjection)
        {
            var result = NativeApi.HWEncoderSetConfigration(EncodeHandle, config.ToString());
            NativeApi.HWEncoderSetMediaProjection(EncodeHandle, androidMediaProjection);
        }

        /// <summary> Adjust the volume of encoder.</summary>
        /// <param name="recordIdx"> Recorder index.</param>
        /// <param name="factor"> The factor of volume.</param>
        public void AdjustVolume(RecorderIndex recordIdx, float factor)
        {
            //Debug.Log("[NativeEncoder] AdjustVolume: recordIdx={0}, factor={1}", (int)recordIdx, factor);
            var result = NativeApi.HWEncoderAdjustVolume(EncodeHandle, (int)recordIdx, factor);
        }

        /// <summary> Updates the surface. </summary>
        /// <param name="texture_id"> Identifier for the texture.</param>
        /// <param name="time_stamp"> The time stamp.</param>
        public void UpdateSurface(IntPtr texture_id, UInt64 time_stamp)
        {
            NativeApi.HWEncoderUpdateSurface(EncodeHandle, texture_id, time_stamp);
        }

        public void UpdateAudioData(byte[] audioData, int samplerate, int bytePerSample, int channel)
        {
            //Debug.Log("[NativeEncode] UpdateAudioData, audioData len:{0} samplerate:{1} bytePerSample:{2} channel:{3}", audioData.Length, samplerate, bytePerSample, channel);
            NativeApi.HWEncoderNotifyAudioData(EncodeHandle, audioData, audioData.Length / bytePerSample, bytePerSample, channel, samplerate, 1);
        }

        public void Register(IEncoderBase encoder)
        {
            if (!mActiveEncoders.Contains(encoder))
            {
                mActiveEncoders.Add(encoder);
            }
        }

        public void UnRegister(IEncoderBase encoder)
        {
            if (mActiveEncoders.Contains(encoder))
            {
                mActiveEncoders.Remove(encoder);
            }
        }

        public bool Stop()
        {
            if (mVideoEncoderWorking)
            {
                var result = NativeApi.HWEncoderStop(EncodeHandle);
                bool success = (result == XREALErrorCode.Success);
                if (success)
                {
                    mVideoEncoderWorking = false;
                }
                return success;
            }
            return true;
        }

        public void Destroy()
        {
            if (mActiveEncoders.Count == 0)
            {
                var result = NativeApi.HWEncoderDestroy(EncodeHandle);
                gInstance = null;
            }
        }

        private struct NativeApi
        {
            /// <summary> Hardware encoder create. </summary>
            /// <param name="out_encoder_handle"> [in,out] Handle of the out encoder.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NRNativeEncodeLibrary)]
            public static extern XREALErrorCode HWEncoderCreate(ref UInt64 out_encoder_handle);

            /// <summary> Hardware encoder start. </summary>
            /// <param name="encoder_handle"> Handle of the encoder.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NRNativeEncodeLibrary)]
            public static extern XREALErrorCode HWEncoderStart(UInt64 encoder_handle);

            /// <summary> Hardware encoder start, only for audio and no video.</summary>
            /// <param name="encoder_handle"> Handle of the encoder.</param>
            /// <param name="onAudioDataCallback"> Callback for sampled audio data.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NRNativeEncodeLibrary)]
            public static extern XREALErrorCode HWEncoderStartOnlyAudioRecorder(UInt64 encoder_handle, AudioDataCallBack onAudioDataCallback);

            /// <summary> Hardware encoder stop, only for audio and no video.</summary>
            /// <param name="encoder_handle"> Handle of the encoder.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NRNativeEncodeLibrary)]
            public static extern XREALErrorCode HWEncoderStopOnlyAudioRecorder(UInt64 encoder_handle);

            /// <summary> Hardware encoder set configration. </summary>
            /// <param name="encoder_handle"> Handle of the encoder.</param>
            /// <param name="config">         The configuration.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NRNativeEncodeLibrary)]
            public static extern XREALErrorCode HWEncoderSetConfigration(UInt64 encoder_handle, string config);

            /// <summary> Hardware encoder set configration. </summary>
            /// <param name="encoder_handle"> Handle of the encoder.</param>
            /// <param name="which_recorder"> Recorder index.</param>
            /// <param name="scale_factor"> Scale factor.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NRNativeEncodeLibrary)]
            public static extern XREALErrorCode HWEncoderAdjustVolume(UInt64 encoder_handle, int which_recorder, float scale_factor);

            /// <summary> Hardware encoder set android MediaProjection object. </summary>
            /// <param name="encoder_handle">   Handle of the encoder.</param>
            /// <param name="mediaProjection">  The android MediaProjection object.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NRNativeEncodeLibrary)]
            public static extern XREALErrorCode HWEncoderSetMediaProjection(UInt64 encoder_handle, IntPtr mediaProjection);

            /// <summary> Hardware encoder update surface. </summary>
            /// <param name="encoder_handle"> Handle of the encoder.</param>
            /// <param name="texture_id">     Identifier for the texture.</param>
            /// <param name="time_stamp">     The time stamp.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NRNativeEncodeLibrary)]
            public static extern XREALErrorCode HWEncoderUpdateSurface(UInt64 encoder_handle, IntPtr texture_id, UInt64 time_stamp);

            /// <summary> Push sampled audio data. </summary>
            /// <param name="encoder_handle">   Handle of the encoder.</param>
            /// <param name="audioSamples">     Sampled audio data.</param>
            /// <param name="nSamples">         Count of samples.</param>
            /// <param name="nBytesPerSample">  Bytes per sample.</param>
            /// <param name="nChannels">        Count of channels.</param>
            /// <param name="samples_per_sec">  Count of samples per second.</param>
            /// <param name="sample_fmt">       Sample format.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NRNativeEncodeLibrary)]
            public static extern XREALErrorCode HWEncoderNotifyAudioData(UInt64 encoder_handle, byte[] audioSamples, int nSamples,
                             int nBytesPerSample, int nChannels, int samples_per_sec, int sample_fmt); //sample_fmt :0:s16, 8 float

            /// <summary> Hardware encoder stop. </summary>
            /// <param name="encoder_handle"> Handle of the encoder.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NRNativeEncodeLibrary)]
            public static extern XREALErrorCode HWEncoderStop(UInt64 encoder_handle);

            /// <summary> Hardware encoder destroy. </summary>
            /// <param name="encoder_handle"> Handle of the encoder.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NRNativeEncodeLibrary)]
            public static extern XREALErrorCode HWEncoderDestroy(UInt64 encoder_handle);
        }
    }
}
