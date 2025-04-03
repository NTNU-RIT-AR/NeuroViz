#if XR_ARFOUNDATION
using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.ARSubsystems;

namespace Unity.XR.XREAL
{
    sealed class XREALImageDatabase : RuntimeReferenceImageLibrary
    {
        internal IntPtr Self { get; }

        public unsafe XREALImageDatabase(XRReferenceImageLibrary serializedLibrary)
        {
            if (serializedLibrary != null)
            {
                serializedLibrary.dataStore.TryGetValue(XREALUtility.k_Identifier, out var bytes);
                if (bytes == null || bytes.Length == 0)
                {
                    Debug.LogError($"Failed to load {nameof(XRReferenceImageLibrary)} '{serializedLibrary.name}': library does not contain any XREAL data.");
                    return;
                }

                using var managedReferenceImages = serializedLibrary.ToNativeArray(Allocator.Temp);
                fixed (byte* ptr = bytes)
                {
                    Self = XREALPlugin.InitImageTrackingDatabase(new NativeView
                    {
                        data = ptr,
                        count = bytes.Length
                    }, managedReferenceImages.AsNativeView());
                }
            }
        }

        ~XREALImageDatabase()
        {
            Assert.AreNotEqual(Self, IntPtr.Zero);

            int n = count;
            for (int i = 0; i < n; ++i)
            {
                XREALPlugin.GetReferenceImage(Self, i).Dispose();
            }
            XREALPlugin.ReleaseImageTrackingDatabase(Self);
        }

        protected override XRReferenceImage GetReferenceImageAt(int index)
        {
            Assert.AreNotEqual(Self, IntPtr.Zero);
            return XREALPlugin.GetReferenceImage(Self, index).ToReferenceImage();
        }

        public override int count => XREALPlugin.GetReferenceImageCount(Self);
    }

    public static partial class XREALPlugin
    {
        [DllImport(LibName)]
        internal static extern IntPtr InitImageTrackingDatabase(NativeView database, NativeView managedReferenceImages);

        [DllImport(LibName)]
        internal static extern ManagedReferenceImage GetReferenceImage(IntPtr database, int index);

        [DllImport(LibName)]
        internal static extern int GetReferenceImageCount(IntPtr database);

        [DllImport(LibName)]
        internal static extern void ReleaseImageTrackingDatabase(IntPtr database);
    }
}
#endif
