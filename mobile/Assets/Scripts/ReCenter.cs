using Unity.XR.XREAL;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

namespace NeuroViz
{
    public class ReCenter : MonoBehaviour
    {
        [SerializeField] private InputActionReference recenterAction;
        [SerializeField] private float distance = 1.5f;
        [SerializeField] private Vector3 offset;

        private void OnEnable()
        {
            DoReCenter();
            recenterAction.action.performed += OnRecenterActionPerformed;
        }

        private void OnDisable()
        {
            recenterAction.action.performed -= OnRecenterActionPerformed;
        }

        private void OnRecenterActionPerformed(InputAction.CallbackContext context)
        {
            if (context.interaction is HoldInteraction)
            {
                DoReCenter();
            }
        }

        private void DoReCenter()
        {
            var camera = XREALUtility.MainCamera.transform;
            transform.position = (camera.position + camera.forward * distance) + offset;
        }
    }
}