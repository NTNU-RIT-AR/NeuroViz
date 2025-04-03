using System;
using System.Runtime.InteropServices;
#if XR_ARFOUNDATION
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;
#endif

namespace Unity.XR.XREAL
{
#if XR_ARFOUNDATION
    /// <summary>
    /// XREAL implementation of the `XRSessionSubsystem`. Do not create this directly. Use the `SubsystemManager` instead.
    /// </summary>
    [Preserve]
    public sealed class XREALSessionSubsystem : XRSessionSubsystem
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            var cInfo = new XRSessionSubsystemDescriptor.Cinfo
            {
                id = "XREAL Session",
                providerType = typeof(XREALProvider),
                subsystemTypeOverride = typeof(XREALSessionSubsystem),
                supportsInstall = false,
                supportsMatchFrameRate = false
            };

#if UNITY_6000_0_OR_NEWER
            XRSessionSubsystemDescriptor.Register(cInfo);
#else
            XRSessionSubsystemDescriptor.RegisterDescriptor(cInfo);
#endif
        }

        class XREALProvider : Provider
        {
            public override Promise<SessionAvailability> GetAvailabilityAsync()
                => Promise<SessionAvailability>.CreateResolvedPromise(SessionAvailability.Supported | SessionAvailability.Installed);

            public override Guid sessionId => XREALPlugin.GetSessionGuid();

            public override TrackingState trackingState => XREALPlugin.GetTrackingState();

            public override NotTrackingReason notTrackingReason => XREALPlugin.GetTrackingReason();
        }
    }
#else
    /// <summary>
    /// Represents the reason tracking was lost.
    /// </summary>
    public enum NotTrackingReason
    {
        /// <summary>
        /// Tracking is working normally.
        /// </summary>
        None,

        /// <summary>
        /// Tracking is being initialized.
        /// </summary>
        Initializing,

        /// <summary>
        /// Tracking is resuming after an interruption.
        /// </summary>
        Relocalizing,

        /// <summary>
        /// Tracking was lost due to poor lighting conditions.
        /// </summary>
        InsufficientLight,

        /// <summary>
        /// Tracking was lost due to insufficient visual features.
        /// </summary>
        InsufficientFeatures,

        /// <summary>
        /// Tracking was lost due to excessive motion.
        /// </summary>
        ExcessiveMotion,

        /// <summary>
        /// Tracking lost reason is not supported.
        /// </summary>
        Unsupported,

        /// <summary>
        /// The camera is in use by another application. Tracking can resume once the app regains access to the camera.
        /// </summary>
        CameraUnavailable
    }

    /// <summary>
    /// Represents pose tracking quality.
    /// Can apply to a device or trackables it is tracking in the environment.
    /// </summary>
    public enum TrackingState
    {
        /// <summary>
        /// Not tracking.
        /// </summary>
        None,

        /// <summary>
        /// Some tracking information is available, but it is limited or of poor quality.
        /// </summary>
        Limited,

        /// <summary>
        /// Tracking is working normally.
        /// </summary>
        Tracking,
    }
#endif

    public static partial class XREALPlugin
    {
        [DllImport(LibName)]
        internal extern static Guid GetSessionGuid();

        [DllImport(LibName)]
        internal extern static NotTrackingReason GetTrackingReason();

        [DllImport(LibName)]
        internal extern static TrackingState GetTrackingState();
    }
}
