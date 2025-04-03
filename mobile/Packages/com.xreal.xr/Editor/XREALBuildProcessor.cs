using UnityEditor.XR.Management;

namespace Unity.XR.XREAL.Editor
{
    public partial class XREALBuildProcessor : XRBuildHelper<XREALSettings>
    {
        public override string BuildSettingsKey => XREALSettings.k_SettingsKey;
    }
}
