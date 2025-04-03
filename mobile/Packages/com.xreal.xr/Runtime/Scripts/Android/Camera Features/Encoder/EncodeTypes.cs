using System;
using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary> Values that represent codec types. </summary>
    public enum CodecType
    {
        /// <summary> An enum constant representing the local option. </summary>
        Local = 0,
        /// <summary> An enum constant representing the rtmp option. </summary>
        Rtmp = 1,
        /// <summary> An enum constant representing the rtp option. </summary>
        Rtp = 2,
    }

    /// <summary> Values that represent record type index. </summary>
    public enum RecorderIndex
    {
        /// <summary> Recorder index of mic. </summary>
        REC_MIC = 0,
        /// <summary> Recorder index of application. </summary>
        REC_APP = 1,
    }

    /// <summary> Callback, called when the capture task. </summary>
    /// <param name="task"> The task.</param>
    /// <param name="data"> The data.</param>
    public delegate void CaptureTaskCallback(CaptureTask task, byte[] data);

    /// <summary> A capture task. </summary>
    public struct CaptureTask
    {
        /// <summary> The width of capture image task. </summary>
        public int Width;
        /// <summary> The height of capture image task. </summary>
        public int Height;
        /// <summary> The capture format. </summary>
        public PhotoCaptureFileOutputFormat CaptureFormat;
        /// <summary> The on receive callback. </summary>
        public CaptureTaskCallback OnReceive;

        /// <summary> Constructor. </summary>
        /// <param name="w">        The width.</param>
        /// <param name="h">        The height.</param>
        /// <param name="format">   Describes the format to use.</param>
        /// <param name="callback"> The callback.</param>
        public CaptureTask(int w, int h, PhotoCaptureFileOutputFormat format, CaptureTaskCallback callback)
        {
            this.Width = w;
            this.Height = h;
            this.CaptureFormat = format;
            this.OnReceive = callback;
        }
    }

    /// <summary> A native encode configuration. </summary>
    [Serializable]
    public class NativeEncodeConfig
    {
        /// <summary> Gets or sets the width. </summary>
        /// <value> The width. </value>
        public int width { get; private set; }
        /// <summary> Gets or sets the height. </summary>
        /// <value> The height. </value>
        public int height { get; private set; }
        /// <summary> Gets or sets the bit rate. </summary>
        /// <value> The bit rate. </value>
        public int bitRate { get; private set; }
        /// <summary> Gets or sets the FPS. </summary>
        /// <value> The FPS. </value>
        public int fps { get; private set; }
        /// <summary> Gets or sets the type of the codec. </summary>
        /// <value> The type of the codec. </value>
        public int codecType { get; private set; }    // 0 local; 1 rtmp ; 2 rtp
        /// <summary> Gets or sets the full pathname of the out put file. </summary>
        /// <value> The full pathname of the out put file. </value>
        public string outPutPath { get; private set; }
        /// <summary> Gets or sets the use step time. </summary>
        /// <value> The use step time. </value>
        public int useStepTime { get; private set; }
        /// <summary> Gets or sets a value indicating whether this object use alpha. </summary>
        /// <value> True if use alpha, false if not. </value>
        public bool useAlpha { get; private set; }
        /// <summary>
        /// Gets or sets a value indicating whether this object use linner texture. </summary>
        /// <value> True if use linner texture, false if not. </value>
        public bool useLinnerTexture { get; private set; }

        public bool addMicphoneAudio { get; private set; }

        public bool addInternalAudio { get; private set; }

        public int audioSampleRate { get; private set; }

        public int audioBitRate { get; private set; }

        public int audioChannels { get; private set; }

        /// <summary> Constructor. </summary>
        /// <param name="cameraparam"> The cameraparam.</param>
        /// <param name="path">        (Optional) Full pathname of the file.</param>
        public NativeEncodeConfig(CameraParameters cameraparam, string path = "")
        {
            this.width = cameraparam.cameraResolutionWidth;
            this.height = cameraparam.captureSide == CaptureSide.Both ? (int)(0.5 * cameraparam.cameraResolutionHeight) : cameraparam.cameraResolutionHeight;
            this.bitRate = NativeConstants.RECORD_VIDEO_BITRATE_DEFAULT;
            this.fps = cameraparam.frameRate;
            this.codecType = GetCodecTypeByPath(path);
            this.outPutPath = path;
            this.useStepTime = 0;
            this.addMicphoneAudio = cameraparam.CaptureAudioMic;
            this.addInternalAudio = cameraparam.CaptureAudioApplication;
            this.useAlpha = cameraparam.hologramOpacity < float.Epsilon;
            this.useLinnerTexture = QualitySettings.activeColorSpace == ColorSpace.Linear;
            this.audioBitRate = NativeConstants.RECORD_AUDIO_BITRATE_DEFAULT;
            this.audioSampleRate = NativeConstants.RECORD_AUDIO_SAMPLERATE_DEFAULT;
            this.audioChannels = NativeConstants.RECORD_AUDIO_CHANNEL;
        }

        /// <summary> Sets out put path. </summary>
        /// <param name="path"> Full pathname of the file.</param>
        public void SetOutPutPath(string path)
        {
            this.codecType = GetCodecTypeByPath(path);
            this.outPutPath = path;
        }

        /// <summary> Gets codec type by path. </summary>
        /// <param name="path"> Full pathname of the file.</param>
        /// <returns> The codec type by path. </returns>
        private static int GetCodecTypeByPath(string path)
        {
            if (path.StartsWith("rtmp://"))
            {
                return 1;
            }
            else if (path.StartsWith("rtp://"))
            {
                return 2;
            }
            else
            {
                return 0;
            }
        }

        /// <summary> Convert this object into a string representation. </summary>
        /// <returns> A string that represents this object. </returns>
        ///format: { "width":1280,"height":720,"bitRate":10240000,"fps":30,"codecType":0,"outPutPath":"/storage/emulated/0/Android/data/com.DefaultCompany.NRSDKXRPlugin/files/Xreal_Record_6437926.mp4","useStepTime":0,"useAlpha":false,"useLinnerTexture":true,"addMicphoneAudio":true,"addInternalAudio":true,"audioSampleRate":16000,"audioBitRate":128000}
        public override string ToString()
        {
            return "{" +
               $"\"width\":{this.width}," +
               $"\"height\":{this.height}," +
               $"\"bitRate\":{this.bitRate}," +
               $"\"fps\":{this.fps}," +
               $"\"codecType\":{this.codecType}," +
               $"\"outPutPath\":\"{this.outPutPath}\"," +
               $"\"useStepTime\":{this.useStepTime}," +
               $"\"useAlpha\":{this.useAlpha.ToString().ToLower()}," +
               $"\"useLinnerTexture\":{this.useLinnerTexture.ToString().ToLower()}," +
               $"\"addMicphoneAudio\":{this.addMicphoneAudio.ToString().ToLower()}," +
               $"\"addInternalAudio\":{this.addInternalAudio.ToString().ToLower()}," +
               $"\"audioSampleRate\":{this.audioSampleRate}," +
               $"\"audioBitRate\":{this.audioBitRate}," +
                $"\"audioChannels\":{this.audioChannels}" +
                "}";
        }
    }
}
