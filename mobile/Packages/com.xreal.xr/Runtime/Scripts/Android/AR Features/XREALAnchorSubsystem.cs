#if XR_ARFOUNDATION
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace Unity.XR.XREAL
{
    public enum XREALAnchorEstimateQuality
    {
        XREAL_ANCHOR_ESTIMATE_QUALITY_INSUFFICIENT = 0,
        XREAL_ANCHOR_ESTIMATE_QUALITY_SUFFICIENT = 1,
        XREAL_ANCHOR_ESTIMATE_QUALITY_GOOD = 2,
    }

    /// <summary>
    /// The XREAL implementation of the
    /// [XRAnchorSubsystem](xref:UnityEngine.XR.ARSubsystems.XRAnchorSubsystem).
    /// Do not create this directly. Use the
    /// [SubsystemManager](xref:UnityEngine.SubsystemManager)
    /// instead.
    /// </summary>
    [Preserve]
    public sealed partial class XREALAnchorSubsystem : XRAnchorSubsystem
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            var cinfo = new XRAnchorSubsystemDescriptor.Cinfo
            {
                id = "XREAL Anchor",
                providerType = typeof(XREALAnchorProvider),
                subsystemTypeOverride = typeof(XREALAnchorSubsystem),
                supportsTrackableAttachments = false,
#if UNITY_6000_0_OR_NEWER
                supportsSynchronousAdd = true
#endif
            };

#if UNITY_6000_0_OR_NEWER
            XRAnchorSubsystemDescriptor.Register(cinfo);
#else
            XRAnchorSubsystemDescriptor.Create(cinfo);
#endif
        }

        /// <summary>
        /// Try to remap the anchor with the given trackableId.
        /// </summary>
        /// <param name="trackableId">The trackableId of the Anchor.</param>
        /// <returns></returns>
        public bool TryRemap(TrackableId trackableId)
        {
            bool succecss = XREALPlugin.RemapTrackableAnchor(trackableId);
            Debug.Log($"[XREALAnchorSubsystem] TryRemap {trackableId} result={succecss}");
            return succecss;
        }

        /// <summary>
        /// Get the quality of the anchor with the given trackableId and HMD pose.
        /// </summary>
        /// <param name="trackableId">The trackableId of the Anchor.</param>
        /// <param name="pose">The pose of HMD.</param>
        /// <returns></returns>
        public XREALAnchorEstimateQuality GetAnchorQuality(TrackableId trackableId, Pose pose)
        {
            XREALAnchorEstimateQuality quality = XREALAnchorEstimateQuality.XREAL_ANCHOR_ESTIMATE_QUALITY_INSUFFICIENT;
            XREALPlugin.EstimateTrackableAnchorQuality(trackableId, pose, ref quality);
            Debug.Log($"[XREALAnchorSubsystem] GetQuality {trackableId} quality={quality}");
            return quality;
        }

        /// <summary>
        /// Set the anchor mapping directory. The anchor mapping file will be saved in this directory.
        /// </summary>
        /// <param name="mapFolder">The path to the directory.</param>
        public void SetAndCreateAnchorMappingDirectory(string mapFolder)
        {
            if (provider is XREALAnchorProvider xrealAnchorProvider)
            {
                xrealAnchorProvider.MapPath = mapFolder;
            }
        }

#if !UNITY_6000_0_OR_NEWER
        public struct Result<T>
        {
            public bool success;
            public T value;
            public Result(bool success, T value)
            {
                this.success = success;
                this.value = value;
            }
        }

        /// <summary>
        /// Try to erase the anchor with the given savedAnchorGuid.
        /// </summary>
        /// <param name="savedAnchorGuid">The saved anchor guid to be erased.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<bool> TryEraseAnchorAsync(SerializableGuid savedAnchorGuid, CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>(cancellationToken);

            Task.Run(() =>
            {
                try
                {
                    (provider as XREALAnchorProvider).TryEraseAnchor(savedAnchorGuid.guid);
                    Debug.Log($"[XREALAnchorSubsystem] TryEraseAnchorAsync [{savedAnchorGuid.guid}]");
                    completionSource.SetResult(true);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            });
            return completionSource.Task;
        }

        /// <summary>
        /// Try to Get all saved anchor guids.
        /// </summary>
        /// <returns>All the saved anchor guids.</returns>
        public IEnumerable<SerializableGuid> TryGetSavedAnchorIds()
        {
            var allUUIDList = (provider as XREALAnchorProvider).GetSavedAnchorUUIDList();
            foreach (var uuid in allUUIDList)
            {
                yield return uuid.ToSerialiableGuid();
            }
        }

        /// <summary>
        /// Try to load the anchor with the given anchorGuid.
        /// </summary>
        /// <param name="anchorGuid">The saved anchor guid.</param>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public bool TryLoadAnchor(SerializableGuid anchorGuid, out XRAnchor anchor)
        {
            return (provider as XREALAnchorProvider).TryLoadAnchor(anchorGuid.guid, out anchor);
        }

        /// <summary>
        /// Try to save the anchor with the given anchorId.
        /// </summary>
        /// <param name="anchorId">The trackableId of the anchor.</param>
        /// <param name="guid">The saved anchor guid.</param>
        /// <returns></returns>
        public bool TrySaveAnchor(TrackableId anchorId, ref SerializableGuid guid)
        {
            return (provider as XREALAnchorProvider).TrySaveAnchor(anchorId, ref guid);
        }

        /// <summary>
        /// Try to save the anchor with the given anchorId asynchronously.
        /// </summary>
        /// <param name="anchorId">The trackableId of the anchor.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<Result<SerializableGuid>> TrySaveAnchorAsync(TrackableId anchorId, CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<Result<SerializableGuid>> completionSource = new TaskCompletionSource<Result<SerializableGuid>>(cancellationToken);
            Task.Run(() =>
            {
                try
                {
                    SerializableGuid guid = default;
                    bool result = TrySaveAnchor(anchorId, ref guid);
                    Debug.Log($"[XREALAnchorSubsystem] trySave [{anchorId}] {guid}");
                    completionSource.SetResult(new Result<SerializableGuid>(result, guid));
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            });
            return completionSource.Task;
        }
#endif

        partial class XREALAnchorProvider : Provider
        {
            private string m_MapPath;

            public string MapPath
            {
                get
                {
                    return m_MapPath;
                }
                set
                {
                    m_MapPath = value;
                    XREALPlugin.SetAnchorMappingFileDirectory(m_MapPath);

                    if (!Directory.Exists(m_MapPath))
                        Directory.CreateDirectory(m_MapPath);
                }
            }

            public override void Start()
            {
                Debug.Log($"[XREALAnchorProvider] Start");
                XREALPlugin.SetTrackableAnchorEnabled(true);

                MapPath =
#if UNITY_EDITOR
                    Directory.GetCurrentDirectory();
#else
                    Application.persistentDataPath;
#endif
                Debug.Log($"[XREALAnchorProvider] Start Finish");
            }

            public override void Stop()
            {
                XREALPlugin.SetTrackableAnchorEnabled(false);
                Debug.Log($"[XREALAnchorProvider] Stop");
            }

            public override void Destroy()
            {
                Debug.Log($"[XREALAnchorProvider] Destroy");
            }

            public override unsafe TrackableChanges<XRAnchor> GetChanges(XRAnchor defaultAnchor, Allocator allocator)
            {
                XREALPlugin.GetTrackableAnchorChanges(out var changes);

                return new TrackableChanges<XRAnchor>(
                    changes.addedPtr, changes.addedCount,
                    changes.updatedPtr, changes.updatedCount,
                    changes.removedPtr, changes.removedCount,
                    defaultAnchor, changes.elementSize,
                    allocator);
            }

            /// <summary>
            /// Attempts to create an anchor at the given <paramref name="pose"/>
            /// </summary>
            /// <param name="pose">The pose, in session space, of the anchor.</param>
            /// <param name="anchor"></param>
            /// <returns></returns>
            public override bool TryAddAnchor(Pose pose, out XRAnchor anchor)
            {
                bool success = XREALPlugin.AcquireNewTrackableAnchor(pose, out anchor);
                Debug.Log($"[XREALAnchorProvider] TryAddAnchor pose={{ pos {pose.position}, rot {pose.rotation.eulerAngles}}} result={success} trackableId={anchor.trackableId} sessionId={anchor.sessionId} trackingState={anchor.trackingState}");
                return success;
            }

            /// <summary>
            /// Attempts to erase the persistent saved data associated with an anchor given its persistent anchor GUID.
            /// </summary>
            /// <param name="anchorGuid">A persistent anchor GUID created by <see cref="TrySaveAnchorAsync"/>.</param>
            public void TryEraseAnchor(Guid anchorGuid)
            {
                string path = $"{MapPath}/{anchorGuid}";
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Debug.Log($"[XREALAnchorProvider] TryEraseAnchor path={path}");
                }
                else
                {
                    Debug.LogWarning($"[XREALAnchorProvider] TryEraseAnchor path not exists! {path}");
                }
            }

            /// <summary>
            /// Get guid list of all saved anchors.
            /// </summary>
            /// <returns></returns>
            public IEnumerable<Guid> GetSavedAnchorUUIDList()
            {
                foreach (var file in Directory.EnumerateFiles(MapPath))
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    if (Guid.TryParse(fileName, out var guid))
                    {
                        Debug.Log($"[XREALAnchorProvider] GetSavedAnchorUUIDList found {guid}");
                        yield return guid;
                    }
                }
            }

            /// <summary>
            /// Attempts to load an anchor given its persistent anchor GUID.
            /// </summary>
            /// <param name="anchorGuid">A persistent anchor GUID created by <see cref="TrySaveAnchorAsync"/>.</param>
            /// <param name="anchor"></param>
            /// <returns></returns>
            public bool TryLoadAnchor(Guid anchorGuid, out XRAnchor anchor)
            {
                bool success = XREALPlugin.LoadTrackableAnchor(anchorGuid, out anchor);
                Debug.Log($"[XREALAnchorProvider] TryLoadAnchor {anchorGuid} result={success}");
                return success;
            }

            /// <summary>
            /// Attempts to remove an anchor given its trackableId.
            /// </summary>
            /// <param name="anchorId">The TrackableId of the anchor to remove.</param>
            /// <returns></returns>
            public override bool TryRemoveAnchor(TrackableId anchorId)
            {
                bool success = XREALPlugin.RemoveTrackableAnchor(anchorId);
                Debug.Log($"[XREALAnchorProvider] TryRemoveAnchor {anchorId} result={success}");
                return success;
            }

            /// <summary>
            /// Attempts to persistently save the given anchor so that it can be loaded in a future AR session.Use the
            /// `SerializableGuid` returned by this method as an input parameter to <see cref="TryLoadAnchor"/> or <see cref="TryEraseAnchor"/>.
            /// </summary>
            /// <param name="anchorId">The TrackableId of the anchor to save.</param>
            /// <param name="serializableGuid">A new persistant anchor GUID if saved</param>
            /// <returns></returns>
            public bool TrySaveAnchor(TrackableId anchorId, ref SerializableGuid serializableGuid)
            {
                Guid guid = default;
                bool success = XREALPlugin.SaveTrackableAnchor(anchorId, ref guid);
                Debug.Log($"[XREALAnchorProvider] TrySaveAnchor {anchorId} result={success} guid={guid}");
                serializableGuid = guid.ToSerialiableGuid();
                return success;
            }
        }
    }

    public static class GuidUtils
    {
        /// <summary>
        /// Convert a Guid to a SerializableGuid.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static unsafe SerializableGuid ToSerialiableGuid(this Guid guid)
        {
            byte* ptr = (byte*)(&guid);
            ulong low = *((ulong*)ptr);
            ulong high = *((ulong*)(ptr + 8));
            return new SerializableGuid(low, high);
        }
    }

    public static partial class XREALPlugin
    {
        [DllImport(LibName)]
        internal static extern void SetAnchorMappingFileDirectory(string path);

        [DllImport(LibName)]
        internal static extern void SetTrackableAnchorEnabled(bool enable);

        [DllImport(LibName)]
        internal static extern bool LoadTrackableAnchor(Guid anchorGuid, out XRAnchor anchor);

        [DllImport(LibName)]
        internal static extern bool SaveTrackableAnchor(TrackableId anchorId, ref Guid achorGuid);

        [DllImport(LibName)]
        internal static extern unsafe void GetTrackableAnchorChanges(out ARSubsystemChanges changes);

        [DllImport(LibName)]
        internal static extern bool AcquireNewTrackableAnchor(Pose pose, out XRAnchor anchor);

        [DllImport(LibName)]
        internal static extern bool RemapTrackableAnchor(TrackableId trackableId);

        [DllImport(LibName)]
        internal static extern bool EstimateTrackableAnchorQuality(TrackableId trackableId, Pose pose, ref XREALAnchorEstimateQuality quality);

        [DllImport(LibName)]
        internal static extern bool RemoveTrackableAnchor(TrackableId anchorId);
    }
}
#endif
