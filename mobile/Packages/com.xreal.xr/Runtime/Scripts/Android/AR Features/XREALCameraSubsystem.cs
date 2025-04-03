#if XR_ARFOUNDATION
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// The XREAL implementation of the
    /// [`XRCameraSubsystem`](xref:UnityEngine.XR.ARSubsystems.XRCameraSubsystem).
    /// Do not create this directly. Use the
    /// [`SubsystemManager`](xref:UnityEngine.SubsystemManager)
    /// instead.
    /// </summary>
    [Preserve]
    public sealed class XREALCameraSubsystem : XRCameraSubsystem
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
#if UNITY_6000_0_OR_NEWER
            var cinfo = new XRCameraSubsystemDescriptor.Cinfo
#else
            var cinfo = new XRCameraSubsystemCinfo
#endif
            {
                id = "XREAL Camera",
                providerType = typeof(XREALCameraProvider),
                subsystemTypeOverride = typeof(XREALCameraSubsystem),
                supportsAverageBrightness = false,
                supportsAverageColorTemperature = false,
                supportsColorCorrection = false,
                supportsDisplayMatrix = false,
                supportsProjectionMatrix = false,
                supportsCameraConfigurations = false,
                supportsAverageIntensityInLumens = false,
                supportsFocusModes = false,
                supportsFaceTrackingAmbientIntensityLightEstimation = false,
                supportsFaceTrackingHDRLightEstimation = false,
                supportsWorldTrackingAmbientIntensityLightEstimation = false,
                supportsWorldTrackingHDRLightEstimation = false,
                supportsCameraGrain = false,
                supportsExifData = false,
                supportsTimestamp = true,
                supportsCameraImage = true,
            };

#if UNITY_6000_0_OR_NEWER
            XRCameraSubsystemDescriptor.Register(cinfo);
#else
            Register(cinfo);
#endif
        }

        /// <summary>
        /// Provides the camera functionality for the XREAL implementation.
        /// </summary>
        class XREALCameraProvider : Provider
        {
            public override XRCpuImage.Api cpuImageApi => XREALCpuImageApi.instance;
            public override bool permissionGranted => true;
            public override Feature currentCamera => Feature.WorldFacingCamera | Feature.AnyTrackingMode;

            public override void Start()
            {
                Debug.Log($"[XREALCameraSubsystem] Start");
                XREALPlugin.StartRGBCameraDataCapture();
            }

            public override void Stop()
            {
                Debug.Log($"[XREALCameraSubsystem] Stop");
                XREALPlugin.StopRGBCameraDataCapture();
            }

            /// <summary>
            /// Get the camera frame for the subsystem.
            /// </summary>
            /// <param name="cameraParams">The current Unity <c>Camera</c> parameters.</param>
            /// <param name="cameraFrame">The current camera frame returned by the method.</param>
            /// <returns>
            /// <see langword="true"/> if the method successfully got a frame. Otherwise, <see langword="false"/>.
            /// </returns>
            public override bool TryGetFrame(XRCameraParams cameraParams, out XRCameraFrame cameraFrame)
            {
                ulong timestamp = 0;
                if (XREALPlugin.TryGetRGBCameraFrame(ref timestamp))
                {
                    XRCameraFrameProperties properties = XRCameraFrameProperties.Timestamp;
                    cameraFrame = new XRCameraFrame(
                        (long)timestamp,
                        0f,
                        0f,
                        Color.black,
                        Matrix4x4.identity,
                        Matrix4x4.identity,
                        TrackingState.None,
                        IntPtr.Zero,
                        properties,
                        0f,
                        0.0,
                        0f,
                        0f,
                        Color.black,
                        Vector3.zero,
                        new SphericalHarmonicsL2(),
                        new XRTextureDescriptor(),
                        0f);
                    return true;
                }

                cameraFrame = default;
                return false;
            }

            /// <summary>
            /// Query for the latest native camera image.
            /// </summary>
            /// <param name="cameraImageCinfo">The metadata required to construct a <see cref="XRCpuImage"/></param>
            /// <returns>
            /// <see langword="true"/> if the camera image is acquired. Otherwise, <see langword="false"/>.
            /// </returns>
            public override bool TryAcquireLatestCpuImage(out XRCpuImage.Cinfo cameraImageCinfo)
            {
                int frameHandle = 0;
                Vector2Int resolution = Vector2Int.zero;
                ulong timeStamp = 0;
                if (XREALPlugin.TryAcquireLatestImage(ref frameHandle, ref resolution, ref timeStamp))
                {
                    cameraImageCinfo = new XRCpuImage.Cinfo(frameHandle, resolution, 3, timeStamp, XRCpuImage.Format.AndroidYuv420_888);
                    return true;
                }

                cameraImageCinfo = default;
                return false;
            }

            /// <summary>
            /// Get the camera intrinsics information.
            /// </summary>
            /// <param name="cameraIntrinsics">The camera intrinsics information returned from the method.</param>
            /// <returns>
            /// <see langword="true"/> if the method successfully gets the camera intrinsics information. Otherwise, <see langword="false"/>.
            /// </returns>
            public override bool TryGetIntrinsics(out XRCameraIntrinsics cameraIntrinsics)
            {
                Vector2Int resolution = Vector2Int.zero;
                Vector2 focalLength = Vector2.zero;
                Vector2 principalPoiont = Vector2.zero;
                if (XREALPlugin.GetDeviceResolution(XREALComponent.XREAL_COMPONENT_RGB_CAMERA, ref resolution) &&
                    XREALPlugin.GetCameraIntrinsic(XREALComponent.XREAL_COMPONENT_RGB_CAMERA, ref focalLength, ref principalPoiont))
                {
                    cameraIntrinsics = new XRCameraIntrinsics(focalLength, principalPoiont, resolution);
                    return true;
                }

                cameraIntrinsics = default;
                return false;
            }
        }

        internal class XREALCpuImageApi : XRCpuImage.Api
        {
            public static XREALCpuImageApi instance { get; } = new XREALCpuImageApi();

            public override bool TryGetPlane(int nativeHandle, int planeIndex, out XRCpuImage.Plane.Cinfo planeCinfo)
            {
                bool result = XREALPlugin.TryGetRGBCameraDataPlane(nativeHandle, planeIndex, out IntPtr dataPtr, out Vector2Int size);
                if (result)
                {
                    planeCinfo = new XRCpuImage.Plane.Cinfo(dataPtr, size.x * size.y, size.x, size.y);
                }
                else
                {
                    planeCinfo = default;
                }
                return result;
            }

            public override bool NativeHandleValid(int nativeHandle)
            {
                return XREALPlugin.IsRGBCameraDataHandleValid(nativeHandle);
            }

            public override void DisposeImage(int nativeHandle)
            {
                XREALPlugin.DisposeRGBCameraDataHandle(nativeHandle);
            }
        }
    }
}
#endif
