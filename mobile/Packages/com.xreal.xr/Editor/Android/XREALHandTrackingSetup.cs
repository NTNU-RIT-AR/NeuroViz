#if UNITY_INPUT_SYSTEM
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.XR.XREAL.Editor
{
    public static class XREALHandTrackingSetup
    {
        /// <summary>
        /// Configures the Input Action Asset for XREAL hand tracking by updating input bindings for both hands.
        /// </summary>
        /// <remarks>
        /// This method allows users to set up hand tracking for XREAL devices by binding specific actions 
        /// (such as position, rotation, selection, and UI interaction) to the corresponding inputs from XREAL's hand tracking system.
        /// It supports both "XRI Left/RightHand" and "XRI Left/Right" action maps, enabling compatibility with different configurations (e.g., XRI3).
        /// The updated Input Action Asset is saved back to its original file and re-imported into Unity.
        /// </remarks>
        [MenuItem("XREAL/Setup Hand Tracking")]
        public static void SetupHandTracking()
        {
            if (Selection.activeObject is InputActionAsset actionAsset)
            {
                actionAsset.ModifyInputAction("XRI LeftHand", "Aim Position", "<XREALHandTracking>{LeftHand}/pointerPosition");
                actionAsset.ModifyInputAction("XRI LeftHand", "Aim Rotation", "<XREALHandTracking>{LeftHand}/pointerRotation");
                actionAsset.ModifyInputAction("XRI LeftHand Interaction", "Select", "<XREALHandTracking>{LeftHand}/indexPressed");
                actionAsset.ModifyInputAction("XRI LeftHand Interaction", "Select Value", "<XREALHandTracking>{LeftHand}/pinchStrengthIndex");
                actionAsset.ModifyInputAction("XRI LeftHand Interaction", "UI Press", "<XREALHandTracking>{LeftHand}/indexPressed");
                actionAsset.ModifyInputAction("XRI LeftHand Interaction", "UI Press Value", "<XREALHandTracking>{LeftHand}/pinchStrengthIndex");

                actionAsset.ModifyInputAction("XRI RightHand", "Aim Position", "<XREALHandTracking>{RightHand}/pointerPosition");
                actionAsset.ModifyInputAction("XRI RightHand", "Aim Rotation", "<XREALHandTracking>{RightHand}/pointerRotation");
                actionAsset.ModifyInputAction("XRI RightHand Interaction", "Select", "<XREALHandTracking>{RightHand}/indexPressed");
                actionAsset.ModifyInputAction("XRI RightHand Interaction", "Select Value", "<XREALHandTracking>{RightHand}/pinchStrengthIndex");
                actionAsset.ModifyInputAction("XRI RightHand Interaction", "UI Press", "<XREALHandTracking>{RightHand}/indexPressed");
                actionAsset.ModifyInputAction("XRI RightHand Interaction", "UI Press Value", "<XREALHandTracking>{RightHand}/pinchStrengthIndex");

                // XRI3 setup
                actionAsset.ModifyInputAction("XRI Left", "Aim Position", "<XREALHandTracking>{LeftHand}/pointerPosition");
                actionAsset.ModifyInputAction("XRI Left", "Aim Rotation", "<XREALHandTracking>{LeftHand}/pointerRotation");
                actionAsset.ModifyInputAction("XRI Left Interaction", "Select", "<XREALHandTracking>{LeftHand}/indexPressed");
                actionAsset.ModifyInputAction("XRI Left Interaction", "Select Value", "<XREALHandTracking>{LeftHand}/pinchStrengthIndex");
                actionAsset.ModifyInputAction("XRI Left Interaction", "UI Press", "<XREALHandTracking>{LeftHand}/indexPressed");
                actionAsset.ModifyInputAction("XRI Left Interaction", "UI Press Value", "<XREALHandTracking>{LeftHand}/pinchStrengthIndex");

                actionAsset.ModifyInputAction("XRI Right", "Aim Position", "<XREALHandTracking>{RightHand}/pointerPosition");
                actionAsset.ModifyInputAction("XRI Right", "Aim Rotation", "<XREALHandTracking>{RightHand}/pointerRotation");
                actionAsset.ModifyInputAction("XRI Right Interaction", "Select", "<XREALHandTracking>{RightHand}/indexPressed");
                actionAsset.ModifyInputAction("XRI Right Interaction", "Select Value", "<XREALHandTracking>{RightHand}/pinchStrengthIndex");
                actionAsset.ModifyInputAction("XRI Right Interaction", "UI Press", "<XREALHandTracking>{RightHand}/indexPressed");
                actionAsset.ModifyInputAction("XRI Right Interaction", "UI Press Value", "<XREALHandTracking>{RightHand}/pinchStrengthIndex");

                string assetPath = AssetDatabase.GetAssetPath(actionAsset);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    File.WriteAllText(assetPath, actionAsset.ToJson());
                    AssetDatabase.ImportAsset(assetPath);
                }
            }
            else
            {
                Debug.LogError("Please select the current Input Action asset before setting up hand tracking.");
            }
        }

        static void ModifyInputAction(this InputActionAsset actionAsset, string actionMapName, string actionName, string binding)
        {
            var action = actionAsset.FindInputAction(actionMapName, actionName);
            if (action != null && !action.bindings.Any(b => b.path == binding))
            {
                action.AddBinding(path: binding, groups: "Generic XR Controller");
            }
        }
    }
}
#endif
