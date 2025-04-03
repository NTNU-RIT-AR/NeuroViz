#if XR_ARFOUNDATION && UNITY_6000_0_OR_NEWER
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using Guid = System.Guid;
using Pool = UnityEngine.Pool;

namespace Unity.XR.XREAL
{
    public sealed partial class XREALAnchorSubsystem : XRAnchorSubsystem
    {
        partial class XREALAnchorProvider : Provider
        {
            #region API 6.0.2

            static readonly Dictionary<Guid, AwaitableCompletionSource<Result<XRAnchor>>> s_AddAsyncPendingRequests = new();

            static readonly Dictionary<TrackableId, AwaitableCompletionSource<Result<SerializableGuid>>> s_SaveAsyncPendingRequests = new();

            static readonly Dictionary<SerializableGuid, AwaitableCompletionSource<Result<XRAnchor>>> s_LoadAsyncPendingRequests = new();

            static readonly Dictionary<SerializableGuid, AwaitableCompletionSource<XRResultStatus>> s_EraseAsyncPendingRequests = new();

            static readonly Dictionary<Guid, AwaitableCompletionSource<Result<NativeArray<SerializableGuid>>>> s_GetSavedAnchorIdsAsyncPendingRequests = new();

            static readonly Pool.ObjectPool<AwaitableCompletionSource<Result<XRAnchor>>> s_AddAsyncCompletionSources = new(
                createFunc: () => new AwaitableCompletionSource<Result<XRAnchor>>(),
                actionOnGet: null,
                actionOnRelease: null,
                actionOnDestroy: null,
                collectionCheck: false,
                defaultCapacity: 8,
                maxSize: 1024);

            static readonly Pool.ObjectPool<AwaitableCompletionSource<Result<SerializableGuid>>> s_SaveAsyncCompletionSources = new(
                    createFunc: () => new AwaitableCompletionSource<Result<SerializableGuid>>(),
                    actionOnGet: null,
                    actionOnRelease: null,
                    actionOnDestroy: null,
                    collectionCheck: false,
                    defaultCapacity: 8,
                    maxSize: 1024);

            static readonly Pool.ObjectPool<AwaitableCompletionSource<Result<XRAnchor>>> s_LoadAsyncCompletionSources = new(
                createFunc: () => new AwaitableCompletionSource<Result<XRAnchor>>(),
                actionOnGet: null,
                actionOnRelease: null,
                actionOnDestroy: null,
                collectionCheck: false,
                defaultCapacity: 8,
                maxSize: 1024);

            static readonly Pool.ObjectPool<AwaitableCompletionSource<XRResultStatus>> s_EraseAsyncCompletionSources = new(
                createFunc: () => new AwaitableCompletionSource<XRResultStatus>(),
                actionOnGet: null,
                actionOnRelease: null,
                actionOnDestroy: null,
                collectionCheck: false,
                defaultCapacity: 8,
                maxSize: 1024);

            static readonly Pool.ObjectPool<AwaitableCompletionSource<Result<NativeArray<SerializableGuid>>>> s_GetSavedAnchorIdsAsyncCompletionSources = new(
                createFunc: () => new AwaitableCompletionSource<Result<NativeArray<SerializableGuid>>>(),
                actionOnGet: null,
                actionOnRelease: null,
                actionOnDestroy: null,
                collectionCheck: false,
                defaultCapacity: 8,
                maxSize: 1024);

            /// <summary>        
            /// Attempts to persistently save the given anchor so that it can be loaded in a future AR session. Use the
            /// `SerializableGuid` returned by this method as an input parameter to <see cref="TryLoadAnchorAsync"/> or
            /// <see cref="TryEraseAnchorAsync"/>.
            /// </summary>
            /// <param name="anchorId">The anchor to save. You can create an anchor using <see cref="TryAddAnchorAsync"/>.</param>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            public override Awaitable<Result<SerializableGuid>> TrySaveAnchorAsync(TrackableId anchorId, CancellationToken cancellationToken = default)
            {
                var completionSource = s_SaveAsyncCompletionSources.Get();
                var wasAddedToMap = s_SaveAsyncPendingRequests.TryAdd(anchorId, completionSource);

                if (!wasAddedToMap)
                {
                    Debug.LogError($"[AnchorProvider] Cannot save anchor with trackableId [{anchorId}] while saving for it is already in progress!");
                    var resultStatus = new XRResultStatus(XRResultStatus.StatusCode.UnknownError);
                    var _result = new Result<SerializableGuid>(resultStatus, default);
                    return AwaitableUtils<Result<SerializableGuid>>.FromResult(completionSource, _result);
                }
                else
                {
                    Debug.Log($"[AnchorProvider] save anchor with trackableId [{anchorId}]");
                }

                Task.Run(() =>
                {
                    try
                    {
                        SerializableGuid guid = default;
                        bool result = TrySaveAnchor(anchorId, ref guid);

                        if (!s_SaveAsyncPendingRequests.Remove(anchorId, out var completionSource))
                        {
                            Debug.LogError($"[AnchorProvider] An unknown error occurred during a system callback for TrySaveAnchorAsync.");
                            return;
                        }

                        Debug.Log($"[AnchorProvider] trySave Task prepare to finish");
                        completionSource.SetResult(new Result<SerializableGuid>(result, guid));
                        completionSource.Reset();
                        s_SaveAsyncCompletionSources.Release(completionSource);

                        Debug.Log($"[AnchorProvider] trySave Task finish");
                    }
                    catch(System.Exception ex)
                    {
                        Debug.LogError($"[AnchorProvider] exception {ex}");
                        Debug.LogException(ex);
                    }
                });

                return completionSource.Awaitable;

            }

            /// <summary>
            /// Attempts to create an anchor at the given <paramref name="pose"/>.
            /// </summary>
            /// <param name="pose">The pose, in session space, of the anchor.</param>
            /// <returns>The result of the async operation.</returns>
            public override Awaitable<Result<XRAnchor>> TryAddAnchorAsync(Pose pose)
            {
                var requestId = Guid.NewGuid();
                var completionSource = s_AddAsyncCompletionSources.Get();
                var wasAddedToMap = s_AddAsyncPendingRequests.TryAdd(requestId, completionSource);

                if (!wasAddedToMap)
                {
                    var resultStatus = new XRResultStatus(XRResultStatus.StatusCode.UnknownError);
                    var _result = new Result<XRAnchor>(resultStatus, XRAnchor.defaultValue);
                    return AwaitableUtils<Result<XRAnchor>>.FromResult(completionSource, _result);
                }
                Task.Run(() =>
                {
                    bool result = TryAddAnchor(pose, out var anchor);

                    if (!s_AddAsyncPendingRequests.Remove(requestId, out var completionSource))
                    {
                        Debug.LogError($"An unknown error occurred during a system callback for TryAddAnchorAsync.");
                        return;
                    }

                    completionSource.SetResult(new Result<XRAnchor>(result, anchor));
                    completionSource.Reset();
                    s_AddAsyncCompletionSources.Release(completionSource);
                });

                return completionSource.Awaitable;
            }

            /// <summary>
            /// Attempts to erase the persistent saved data associated with an anchor given its persistent anchor GUID.
            /// </summary>
            /// <param name="savedAnchorGuid">A persistent anchor GUID created by <see cref="TrySaveAnchorAsync"/>.</param>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            public override Awaitable<XRResultStatus> TryEraseAnchorAsync(SerializableGuid savedAnchorGuid, CancellationToken cancellationToken = default)
            {
                var completionSource = s_EraseAsyncCompletionSources.Get();
                var wasAddedToMap = s_EraseAsyncPendingRequests.TryAdd(savedAnchorGuid, completionSource);

                if (!wasAddedToMap)
                {
                    Debug.LogError($"Cannot erase persistent anchor GUID [{savedAnchorGuid}] while erasing for it is already in progress!");
                    var resultStatus = new XRResultStatus(XRResultStatus.StatusCode.UnknownError);
                    return AwaitableUtils<XRResultStatus>.FromResult(completionSource, resultStatus);
                }

                Task.Run(() =>
                {
                    TryEraseAnchor(savedAnchorGuid.guid);

                    if (!s_EraseAsyncPendingRequests.Remove(savedAnchorGuid, out var completionSource))
                    {
                        Debug.LogError($"An unknown error occurred during a system callback for TryEraseAnchorAsync.");
                        return;
                    }
                    completionSource.SetResult(true);
                    completionSource.Reset();
                    s_EraseAsyncCompletionSources.Release(completionSource);
                });

                return completionSource.Awaitable;
            }

            /// <summary>
            /// Attempts to get a `NativeArray` containing all saved persistent anchor GUIDs
            /// </summary>
            /// <param name="allocator">The allocation strategy to use for the resulting `NativeArray`.</param>
            /// <param name="cancellationToken"></param>
            /// <returns>The result of the async operation, containing a `NativeArray` of persistent anchor GUIDs
            /// allocated with the given <paramref name="allocator"/> if the operation succeeded.</returns>
            public override Awaitable<Result<NativeArray<SerializableGuid>>> TryGetSavedAnchorIdsAsync(Allocator allocator, CancellationToken cancellationToken = default)
            {
                var requestId = Guid.NewGuid();
                var completionSource = s_GetSavedAnchorIdsAsyncCompletionSources.Get();
                var wasAddedToMap = s_GetSavedAnchorIdsAsyncPendingRequests.TryAdd(requestId, completionSource);

                if (!wasAddedToMap)
                {
                    var resultStatus = new XRResultStatus(XRResultStatus.StatusCode.UnknownError);
                    var _result = new Result<NativeArray<SerializableGuid>>(resultStatus, default);
                    return AwaitableUtils<Result<NativeArray<SerializableGuid>>>.FromResult(completionSource, _result);
                }
                Task.Run(() =>
                {
                    var list = GetSavedAnchorUUIDList();
                    List<SerializableGuid> sguidList = new List<SerializableGuid>();
                    foreach (var item in list)
                    {
                        sguidList.Add(new SerializableGuid(item));
                    }
                    NativeArray<SerializableGuid> array = new NativeArray<SerializableGuid>(sguidList.Count, Allocator.Persistent);
                    array.CopyFrom(sguidList.ToArray());

                    if (!s_GetSavedAnchorIdsAsyncPendingRequests.Remove(requestId, out var completionSource))
                    {
                        Debug.LogError($"An unknown error occurred during a system callback for TryGetSavedAnchorIdsAsync.");
                        return;
                    }
                    Debug.Log($"[AnchorProvider] TryGetSavedAnchorIdsAsync array.length={array.Count()}");
                    completionSource.SetResult(new Result<NativeArray<SerializableGuid>>(true, array));
                    completionSource.Reset();
                    s_GetSavedAnchorIdsAsyncCompletionSources.Release(completionSource);

                });

                return completionSource.Awaitable;
            }

            /// <summary>
            /// Attempts to load an anchor given its persistent anchor GUID.
            /// </summary>
            /// <param name="savedAnchorGuid">A persistent anchor GUID created by <see cref="TrySaveAnchorAsync"/>.</param>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            public override Awaitable<Result<XRAnchor>> TryLoadAnchorAsync(SerializableGuid savedAnchorGuid, CancellationToken cancellationToken = default)
            {
                var completionSource = s_LoadAsyncCompletionSources.Get();
                var wasAddedToMap = s_LoadAsyncPendingRequests.TryAdd(savedAnchorGuid, completionSource);

                if (!wasAddedToMap)
                {
                    Debug.LogError($"Cannot load persistent anchor GUID [{savedAnchorGuid}] while loading for it is already in progress!");
                    var resultStatus = new XRResultStatus(XRResultStatus.StatusCode.UnknownError);
                    var _result = new Result<XRAnchor>(resultStatus, XRAnchor.defaultValue);
                    return AwaitableUtils<Result<XRAnchor>>.FromResult(completionSource, _result);
                }

                Task.Run(async () =>
                {
                    XRAnchor anchor = XRAnchor.defaultValue;
                    bool result = TryLoadAnchor(savedAnchorGuid.guid, out anchor);

                    Debug.Log($"[AnchorProvider] TryLoadAnchorAsync result={result}");
                    if (!s_LoadAsyncPendingRequests.Remove(savedAnchorGuid, out var _completionSource))
                    {
                        Debug.LogError($"An unknown error occurred during a system callback for TryLoadAnchorAsync.");
                        return;
                    }

                    await Awaitable.MainThreadAsync();
                    Debug.Log($"[AnchorProvider] TryLoadAnchorAsync SetResult");
                    _completionSource.SetResult(new Result<XRAnchor>(result, anchor));
                    _completionSource.Reset();
                    s_LoadAsyncCompletionSources.Release(_completionSource);
                });
                return completionSource.Awaitable;
            }
            protected override bool TryInitialize()
            {
                return base.TryInitialize();
            }
            #endregion
        }
    }
}
#endif
