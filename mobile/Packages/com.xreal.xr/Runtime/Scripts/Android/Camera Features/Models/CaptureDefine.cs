using System;
using UnityEngine;

namespace Unity.XR.XREAL
{
    public enum CameraType
    {
        RGB = 0,
        Gray = 1,
    }
    /// <summary> The camera mode of capture. </summary>
    public enum CamMode
    {
        /// <summary>
        /// Resource is not in use.
        /// </summary>
        None = 0,

        /// <summary>
        /// Resource is in Photo Mode.
        /// </summary>
        PhotoMode = 1,

        /// <summary>
        /// Resource is in Video Mode.
        /// </summary>
        VideoMode = 2
    }
    public enum CaptureSide
    {
        Single = 0,
        Both = 1,
        Left = 2,
        Right = 3,
    }
    /// <summary> A camera texture frame. </summary>
    public struct CameraTextureFrame
    {
        /// <summary> The time stamp. </summary>
        public UInt64 timeStamp;
        /// <summary> The gain </summary>
        public UInt32 gain;
        /// <summary> The exposureTime </summary>
        public UInt32 exposureTime;
        /// <summary> The texture. </summary>
        public Texture texture;
    }

    /// <summary>
    /// The encoded image or video pixel format to use for PhotoCapture and VideoCapture. </summary>
    public enum CapturePixelFormat
    {
        /// <summary>
        /// 8 bits per channel (blue, green, red, and alpha).
        /// </summary>
        BGRA32 = 0,

        /// <summary>
        /// 8-bit Y plane followed by an interleaved U/V plane with 2x2 subsampling.
        /// </summary>
        NV12 = 1,

        /// <summary>
        /// Encode photo in JPEG format.
        /// </summary>
        JPEG = 2,

        /// <summary>
        /// Portable Network Graphics Format.
        /// </summary>
        PNG = 3
    }
    /// <summary> Image Encoding Format. </summary>
    public enum PhotoCaptureFileOutputFormat
    {
        /// <summary>
        /// PNG Encoding.
        /// </summary>
        PNG = 0,

        /// <summary>
        /// JPEG Encoding.
        /// </summary>
        JPG = 1
    }
}
