#if UNITY_ANDROID && !UNITY_EDITOR
#define XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
#endif

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// Delegate for handling RGB camera data callbacks.
    /// </summary>
    /// <param name="rgbCameraData">The data frame from the RGB camera.</param>
    /// <param name="userData">A user-defined pointer for additional data.</param>
    public delegate void RGBCameraDataCallback(RGBCameraDataFrame rgbCameraData, IntPtr userData);

    /// <summary>
    /// Represents a single frame of RGB camera data.
    /// <param name="timeStamp">The timestamp of frame data.</param>
    /// <param name="resolution">The resolution(width and height) of frame data.</param>
    /// <param name="rawDataSize">The raw data's size of frame data.</param>
    /// <param name="rawData">The raw data of frame data.</param>
    /// </summary>
    public struct RGBCameraDataFrame
    {
        public ulong timeStamp;
        public Vector2Int resolution;
        public ulong rawDataSize;
        public IntPtr rawData;
    }

    /// <summary>
    /// Provides a set of utility functions and events for interacting with the XREAL XR Plugin.
    /// </summary>
    public static partial class XREALPlugin
    {
        /// <summary>
        /// Starts capturing data from the RGB camera.
        /// </summary>
        /// <returns>The capture session ID.</returns>
        public static ulong StartRGBCameraDataCapture()
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            return Internal.StartRGBCameraDataCapture(null, IntPtr.Zero);
#else
            return 0;
#endif
        }

        /// <summary>
        /// Stops the currently active RGB camera data capture.
        /// </summary>
        /// <returns>True if the capture was successfully stopped; otherwise, false.</returns>
        public static bool StopRGBCameraDataCapture()
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            return Internal.StopRGBCameraDataCapture(0);
#else
            return false;
#endif
        }

        /// <summary>
        /// Attempts to get the timestamp of the latest RGB camera frame.
        /// </summary>
        /// <param name="timeStamp">The timestamp of the frame.</param>
        /// <returns>True if the timestamp was successfully retrieved; otherwise, false.</returns>
        public static bool TryGetRGBCameraFrame(ref ulong timeStamp)
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            return Internal.TryGetRGBCameraFrame(ref timeStamp);
#else
            return false;
#endif
        }

        /// <summary>
        /// Attempts to acquire the latest image from the RGB camera.
        /// </summary>
        /// <param name="frameHandle">The handle for the acquired frame.</param>
        /// <param name="resolution">The resolution of the acquired frame.</param>
        /// <param name="timeStamp">The timestamp of the acquired frame.</param>
        /// <returns>True if the image was successfully acquired; otherwise, false.</returns>
        public static bool TryAcquireLatestImage(ref int frameHandle, ref Vector2Int resolution, ref ulong timeStamp)
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            return Internal.TryAcquireLatestImage(ref frameHandle, ref resolution, ref timeStamp);
#else
            return false;
#endif
        }

        /// <summary>
        /// Attempts to get the data plane of a specific frame from the RGB camera.
        /// </summary>
        /// <param name="frameHandle">The handle for the frame.</param>
        /// <param name="planeIndex">The index of the data plane.</param>
        /// <param name="dataPtr">A pointer to the data plane.</param>
        /// <param name="size">The size of the data plane.</param>
        /// <returns>True if the data plane was successfully retrieved; otherwise, false.</returns>
        public static bool TryGetRGBCameraDataPlane(int frameHandle, int planeIndex, out IntPtr dataPtr, out Vector2Int size)
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            return Internal.TryGetRGBCameraDataPlane(frameHandle, planeIndex, out dataPtr, out size);
#else
            dataPtr = IntPtr.Zero;
            size = default;
            return false;
#endif
        }

        /// <summary>
        /// Checks whether a given RGB camera data handle is valid.
        /// </summary>
        /// <param name="frameHandle">The handle to validate.</param>
        /// <returns>True if the handle is valid; otherwise, false.</returns>
        public static bool IsRGBCameraDataHandleValid(int frameHandle)
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            return Internal.IsRGBCameraDataHandleValid(frameHandle);
#else
            return false;
#endif
        }

        /// <summary>
        /// Releases resources associated with a specific RGB camera data handle.
        /// </summary>
        /// <param name="frameHandle">The handle to dispose.</param>
        public static void DisposeRGBCameraDataHandle(int frameHandle)
        {
#if XREALPLUGIN_SUPPORTS_TARGET_PLATFORM
            Internal.DisposeRGBCameraDataHandle(frameHandle);
#endif
        }

        private static partial class Internal
        {
            [DllImport(LibName)]
            internal static extern ulong StartRGBCameraDataCapture(RGBCameraDataCallback callback, IntPtr userData);

            [DllImport(LibName)]
            internal static extern bool StopRGBCameraDataCapture(ulong callbackHandle);

            [DllImport(LibName)]
            internal static extern bool TryGetRGBCameraFrame(ref ulong timeStamp);

            [DllImport(LibName)]
            internal static extern bool TryAcquireLatestImage(ref int frameHandle, ref Vector2Int resolution, ref ulong timeStamp);

            [DllImport(LibName)]
            public static extern bool TryGetRGBCameraDataPlane(int frameHandle, int planeIndex, out IntPtr dataPtr, out Vector2Int size);

            [DllImport(LibName)]
            internal static extern bool IsRGBCameraDataHandleValid(int frameHandle);

            [DllImport(LibName)]
            internal static extern void DisposeRGBCameraDataHandle(int frameHandle);
        }
    }
}
