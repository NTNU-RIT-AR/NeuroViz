using Unity.XR.XREAL;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

namespace NeuroViz
{
    public class ReCenter : MonoBehaviour
    {
        [SerializeField] private InputActionReference recenterAction;
        [SerializeField] private float offset = 1.5f;

        void OnEnable()
        {
            DoReCenter();
            recenterAction.action.performed += OnRecenterActionPerformed;
        }

        void OnDisable()
        {
            recenterAction.action.performed -= OnRecenterActionPerformed;
        }

        void OnRecenterActionPerformed(InputAction.CallbackContext context)
        {
            if (context.interaction is HoldInteraction)
            {
                DoReCenter();
            }
        }

        private void DoReCenter()
        {
            var camera = XREALUtility.MainCamera.transform;
            transform.position = camera.position + camera.forward * offset;
        }
    }
}