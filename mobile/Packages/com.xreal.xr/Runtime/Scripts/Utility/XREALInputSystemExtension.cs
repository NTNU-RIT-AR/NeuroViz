#if UNITY_INPUT_SYSTEM
using UnityEngine.InputSystem;

namespace Unity.XR.XREAL
{
    public static class XREALInputSystemExtension
    {
        /// <summary>
        /// Finds an InputAction by its action map name and action name from the provided InputActionAsset.
        /// </summary>
        /// <param name="actionAsset">The InputActionAsset that contains the action maps and actions.</param>
        /// <param name="actionMapName">The name of the action map where the action is defined.</param>
        /// <param name="actionName">The name of the action to find within the specified action map.</param>
        /// <returns>The InputAction if found; otherwise, null.</returns>
        public static InputAction FindInputAction(this InputActionAsset actionAsset, string actionMapName, string actionName)
        {
            return actionAsset.FindActionMap(actionMapName)?.FindAction(actionName);
        }
    }
}
#endif
