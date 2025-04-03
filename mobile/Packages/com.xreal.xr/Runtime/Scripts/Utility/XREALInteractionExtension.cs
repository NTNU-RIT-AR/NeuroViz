#if XR_INTERACTION
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace Unity.XR.XREAL
{
    public static class XREALInteractionExtension
    {
        static InputActionManager s_ActionManager;

        /// <summary>
        /// Finds an InputAction by its action map name and action name across all input action assets.
        /// </summary>
        /// <param name="actionMapName">The name of the action map where the action is defined.</param>
        /// <param name="actionName">The name of the action to find within the specified action map.</param>
        /// <returns>The InputAction if found; otherwise, null.</returns>
        public static InputAction FindInputAction(string actionMapName, string actionName)
        {
            if (s_ActionManager == null)
            {
                s_ActionManager = XREALUtility.FindAnyObjectByType<InputActionManager>();
            }

            if (s_ActionManager != null)
            {
                foreach (var actionAsset in s_ActionManager.actionAssets)
                {
                    var action = actionAsset.FindInputAction(actionMapName, actionName);
                    if (action != null)
                        return action;
                }
            }

            return null;
        }
    }
}
#endif
