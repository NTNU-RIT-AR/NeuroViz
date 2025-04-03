using System.Runtime.InteropServices;
using UnityEngine.XR;

namespace Unity.XR.XREAL
{
    public static class XREALDisplaySubsystemExtensions
    {
        /// <summary>
        /// Enables or disables the teared frame count feature in the XR display subsystem.
        /// </summary>
        /// <param name="subsystem">The XRDisplaySubsystem instance to which this extension is applied.</param>
        /// <param name="enabled">A boolean value indicating whether to enable or disable teared frame count.</param>
        /// <returns>Returns true if the operation is successful, otherwise false.</returns>
        public static bool EnableTearedFrameCount(this XRDisplaySubsystem subsystem, bool enabled)
        {
            return XREALPlugin.EnableTearedFrameCount(enabled);
        }

        /// <summary>
        /// Enables or disables the render background color feature in the XR display subsystem.
        /// </summary>
        /// <param name="subsystem">The XRDisplaySubsystem instance to which this extension is applied.</param>
        /// <param name="enabled">A boolean value indicating whether to enable or disable render background color.</param>
        /// <returns>Returns true if the operation is successful, otherwise false.</returns>
        public static bool EnableRenderBackColor(this XRDisplaySubsystem subsystem, bool enabled)
        {
            return XREALPlugin.EnableRenderBackColor(enabled);
        }
    }

    public static partial class XREALPlugin
    {
        [DllImport(LibName)]
        internal static extern bool EnableTearedFrameCount(bool enabled);

        [DllImport(LibName)]
        internal static extern bool EnableRenderBackColor(bool enabled);
    }
}
