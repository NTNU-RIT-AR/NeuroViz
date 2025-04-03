#if XR_ARFOUNDATION
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// The XREAL implementation of the <c>XRPlaneSubsystem</c>. Do not create this directly. Use the <c>SubsystemManager</c> instead.
    /// </summary>
    [Preserve]
    public sealed class XREALPlaneSubsystem : XRPlaneSubsystem
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            var cInfo = new XRPlaneSubsystemDescriptor.Cinfo
            {
                id = "XREAL Plane Detection",
                providerType = typeof(XREALPlaneProvider),
                subsystemTypeOverride = typeof(XREALPlaneSubsystem),
                supportsHorizontalPlaneDetection = true,
                supportsVerticalPlaneDetection = true,
                supportsArbitraryPlaneDetection = false,
                supportsBoundaryVertices = true
            };
#if UNITY_6000_0_OR_NEWER
            XRPlaneSubsystemDescriptor.Register(cInfo);
#else
            XRPlaneSubsystemDescriptor.Create(cInfo);
#endif
        }

        class XREALPlaneProvider : Provider
        {
            public override void Start()
            {
            }

            public override void Stop()
            {
            }

            public override void Destroy()
            {
            }

            public override PlaneDetectionMode requestedPlaneDetectionMode
            {
                get => XREALPlugin.GetPlaneDetectionMode();
                set => XREALPlugin.SetPlaneDetectionMode(value);
            }

            public override PlaneDetectionMode currentPlaneDetectionMode => requestedPlaneDetectionMode;

            public override unsafe TrackableChanges<BoundedPlane> GetChanges(BoundedPlane defaultPlane, Allocator allocator)
            {
                XREALPlugin.GetPlaneDetectionChanges(out var changes);

                return new TrackableChanges<BoundedPlane>(
                    changes.addedPtr, changes.addedCount,
                    changes.updatedPtr, changes.updatedCount,
                    changes.removedPtr, changes.removedCount,
                    defaultPlane, changes.elementSize,
                    allocator);
            }

            public override unsafe void GetBoundary(
                TrackableId trackableId,
                Allocator allocator,
                ref NativeArray<Vector2> boundary)
            {
                int numPoints = XREALPlugin.GetPlaneBoundaryVertexCount(trackableId);
                CreateOrResizeNativeArrayIfNecessary(numPoints, allocator, ref boundary);
                XREALPlugin.GetPlaneBoundaryVertexData(trackableId, boundary.GetUnsafePtr());
            }
        }
    }

    public static partial class XREALPlugin
    {
        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct ARSubsystemChanges
        {
            public void* addedPtr;
            public int addedCount;
            public void* updatedPtr;
            public int updatedCount;
            public void* removedPtr;
            public int removedCount;
            public int elementSize;
        }

        [DllImport(LibName)]
        internal static extern PlaneDetectionMode GetPlaneDetectionMode();

        [DllImport(LibName)]
        internal static extern bool SetPlaneDetectionMode(PlaneDetectionMode value);

        [DllImport(LibName)]
        internal static extern unsafe void GetPlaneDetectionChanges(out ARSubsystemChanges changes);

        [DllImport(LibName)]
        internal static extern int GetPlaneBoundaryVertexCount(TrackableId trackableId);

        [DllImport(LibName)]
        internal static extern unsafe void GetPlaneBoundaryVertexData(TrackableId trackableId, void* boundary);
    }
}
#endif
