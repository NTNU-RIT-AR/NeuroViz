#if XR_ARFOUNDATION
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// Extension methods for ARAnchorManager to interact with XREALAnchorSubsystem.
    /// </summary>
    public static class XREALAnchorManagerExtension
    {
#if !UNITY_6000_0_OR_NEWER
        /// <summary>
        /// Try to get saved anchor IDs from the XREALAnchorSubsystem.
        /// </summary>
        /// <param name="anchorManager"></param>
        /// <returns></returns>
        public static IEnumerable<SerializableGuid> TryGetSavedAnchorIds(this ARAnchorManager anchorManager)
        {
            if (anchorManager.subsystem is XREALAnchorSubsystem xrealSubsystem)
            {
                return xrealSubsystem.TryGetSavedAnchorIds();
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// Try to load an anchor by its saved GUID (persistent identifier).
        /// </summary>
        /// <param name="anchorManager"></param>
        /// <param name="persistantGuid"></param>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public static bool TryLoadAnchor(this ARAnchorManager anchorManager, SerializableGuid persistantGuid, out XRAnchor anchor)
        {
            if (anchorManager.subsystem is XREALAnchorSubsystem xrealSubsystem)
            {
                return xrealSubsystem.TryLoadAnchor(persistantGuid, out anchor);
            }
            else
            {
                anchor = default;
                return false;
            }
        }

        /// <summary>
        /// Asynchronously try to save an anchor in the XREALAnchorSubsystem.
        /// </summary>
        /// <param name="anchorManager"></param>
        /// <param name="anchor"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<XREALAnchorSubsystem.Result<SerializableGuid>> TrySaveAnchorAsync(this ARAnchorManager anchorManager, ARAnchor anchor, CancellationToken cancellationToken = default)
        {
            if (anchorManager.subsystem is XREALAnchorSubsystem xrealSubsystem)
            {
                return xrealSubsystem.TrySaveAnchorAsync(anchor.trackableId, cancellationToken);
            }
            else
            {
                return Task.FromResult(new XREALAnchorSubsystem.Result<SerializableGuid>(false, default));
            }
        }

        /// <summary>
        /// Asynchronously try to erase a saved anchor by its GUID.
        /// </summary>
        /// <param name="anchorManager"></param>
        /// <param name="savedAnchorGuid"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<bool> TryEraseAnchorAsync(this ARAnchorManager anchorManager, SerializableGuid savedAnchorGuid, CancellationToken cancellationToken = default)
        {
            if (anchorManager.subsystem is XREALAnchorSubsystem xrealSubsystem)
            {
                return xrealSubsystem.TryEraseAnchorAsync(savedAnchorGuid, cancellationToken);
            }
            else
            {
                return Task.FromResult(false);
            }
        }
#endif

        /// <summary>
        /// Get the quality of the anchor by providing its ID and an estimated pose.
        /// </summary>
        /// <param name="anchorManager"></param>
        /// <param name="anchorId"></param>
        /// <param name="estimatePose"></param>
        /// <returns></returns>
        public static XREALAnchorEstimateQuality GetAnchorQuality(this ARAnchorManager anchorManager, TrackableId anchorId, Pose estimatePose)
        {
            if (anchorManager.subsystem is XREALAnchorSubsystem xrealSubsystem)
            {
                return xrealSubsystem.GetAnchorQuality(anchorId, estimatePose);
            }
            else
            {
                return XREALAnchorEstimateQuality.XREAL_ANCHOR_ESTIMATE_QUALITY_INSUFFICIENT;
            }
        }

        /// <summary>
        /// Try to remap an anchor by its ID.
        /// </summary>
        /// <param name="anchorManager"></param>
        /// <param name="anchorId"></param>
        /// <returns></returns>
        public static bool TryRemap(this ARAnchorManager anchorManager, TrackableId anchorId)
        {
            if (anchorManager.subsystem is XREALAnchorSubsystem xrealSubsystem)
            {
                return xrealSubsystem.TryRemap(anchorId);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Set and create a directory for anchor mappings, typically used for saving/loading anchor data.
        /// </summary>
        /// <param name="anchorManager"></param>
        /// <param name="mapFolder"></param>
        public static void SetAndCreateAnchorMappingDirectory(this ARAnchorManager anchorManager, string mapFolder)
        {
            if (anchorManager.subsystem is XREALAnchorSubsystem xrealSubsystem)
            {
                xrealSubsystem.SetAndCreateAnchorMappingDirectory(mapFolder);
            }
        }
    }
}
#endif
