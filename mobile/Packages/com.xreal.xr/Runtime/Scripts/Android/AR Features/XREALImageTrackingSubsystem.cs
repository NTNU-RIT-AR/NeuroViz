#if XR_ARFOUNDATION
using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// XREAL implementation of the <c>XRImageTrackingSubsystem</c>.
    /// </summary>
    [Preserve]
    public sealed class XREALImageTrackingSubsystem : XRImageTrackingSubsystem
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            XRImageTrackingSubsystemDescriptor.Cinfo cInfo = new XRImageTrackingSubsystemDescriptor.Cinfo
            {
                id = "XREAL ImageTracking",
                providerType = typeof(XREALImageTrackingProvider),
                subsystemTypeOverride = typeof(XREALImageTrackingSubsystem),
                supportsMovingImages = true,
                requiresPhysicalImageDimensions = true,
                supportsMutableLibrary = false,
            };

#if UNITY_6000_0_OR_NEWER
            XRImageTrackingSubsystemDescriptor.Register(cInfo);
#else
            XRImageTrackingSubsystemDescriptor.Create(cInfo);
#endif
        }

        class XREALImageTrackingProvider : Provider
        {
            public override void Start() { }
            public override void Stop() { }

            public override RuntimeReferenceImageLibrary CreateRuntimeLibrary(XRReferenceImageLibrary serializedLibrary)
            {
                return new XREALImageDatabase(serializedLibrary);
            }

            public override RuntimeReferenceImageLibrary imageLibrary
            {
                set
                {
                    if (value == null)
                    {
                        XREALPlugin.SetImageTrackingDatabase(IntPtr.Zero);
                    }
                    else if (value is XREALImageDatabase database)
                    {
                        XREALPlugin.SetImageTrackingDatabase(database.Self);
                    }
                    else
                    {
                        throw new ArgumentException($"The {value.GetType().Name} is not a valid XREAL image library.");
                    }
                }
            }

            public override unsafe TrackableChanges<XRTrackedImage> GetChanges(XRTrackedImage defaultTrackedImage, Allocator allocator)
            {
                XREALPlugin.GetImageTrackingChanges(out var changes);

                return new TrackableChanges<XRTrackedImage>(
                    changes.addedPtr, changes.addedCount,
                    changes.updatedPtr, changes.updatedCount,
                    changes.removedPtr, changes.removedCount,
                    defaultTrackedImage, changes.elementSize,
                    allocator);
            }

            public override void Destroy() => XREALPlugin.SetImageTrackingDatabase(IntPtr.Zero);

            public override int requestedMaxNumberOfMovingImages { get; set; }

            public override int currentMaxNumberOfMovingImages => requestedMaxNumberOfMovingImages;
        }
    }

    public static partial class XREALPlugin
    {
        [DllImport(LibName)]
        internal static extern void SetImageTrackingDatabase(IntPtr database);

        [DllImport(LibName)]
        internal static extern unsafe void GetImageTrackingChanges(out ARSubsystemChanges changes);
    }
}
#endif
