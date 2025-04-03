#if XR_ARFOUNDATION
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.XR;
using UnityEngine.XR.ARSubsystems;

namespace Unity.XR.XREAL
{
    /// <summary>
    /// Semantic label for meshing vertices
    /// </summary>
    public enum NRMeshingVertexSemanticLabel : byte
    {
        Background = 0,
        Wall = 1,
        Building = 2,
        Floor = 4,
        Ceiling = 5,
        Highway = 6,
        Sidewalk = 7,
        Grass = 8,
        Door = 10,
        Table = 11,
    }

    public static class XREALMeshSubsystemExtensions
    {
        /// <summary>
        /// Get the face classifications for the given mesh ID.
        /// </summary>
        /// <param name="subsystem">The meshing subsystem.</param>
        /// <param name="meshId">The trackable ID representing the mesh.</param>
        /// <param name="allocator">The memory allocator type to use in allocating the native array memory.</param>
        /// <returns>
        /// An array of mesh classifications, one for each face in the mesh.
        /// </returns>
        public static unsafe NativeArray<NRMeshingVertexSemanticLabel> GetFaceClassifications(this XRMeshSubsystem subsystem, TrackableId meshId, Allocator allocator)
        {
            XREALPlugin.GetMeshLabels(meshId, out var classifications, out var numClassifications);

            if (classifications == null)
            {
                numClassifications = 0;
            }

            var meshClassifications = new NativeArray<NRMeshingVertexSemanticLabel>(numClassifications, allocator);
            if (classifications != null)
            {
                NativeArray<NRMeshingVertexSemanticLabel> tmp = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<NRMeshingVertexSemanticLabel>(classifications, numClassifications, Allocator.None);
                meshClassifications.CopyFrom(tmp);
            }

            return meshClassifications;
        }
    }

    public static partial class XREALPlugin
    {
        [DllImport(LibName)]
        internal static unsafe extern bool GetMeshLabels(
            TrackableId trackableId,
            out NRMeshingVertexSemanticLabel* labels,
            out int labelCount);
    }
}
#endif
