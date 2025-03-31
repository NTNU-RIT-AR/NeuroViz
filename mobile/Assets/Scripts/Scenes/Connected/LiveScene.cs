using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NeuroViz.Scenes.Connected
{
    public class LiveScene : MonoBehaviour
    {
        [Required]
        [SerializeField] private ConnectedScene connectedScene;

        [Required]
        [SerializeField] private UpdateParameters updateParameters;

        private UnityState.Live state;

        // Opacity
        // Border
        // Xray

        // Subsurface?

        private void OnEnable()
        {
            if (connectedScene.State is UnityState.Live live)
            {
                state = live;
            }

            connectedScene.OnStateChanged += HandleStateChange;
        }

        private void OnDisable()
        {
            connectedScene.OnStateChanged -= HandleStateChange;
        }

        private void HandleStateChange(UnityState state)
        {
            if (state is UnityState.Live live)
            {
                this.state = live;
            }

            updateParameters.transparency = this.state.Parameters.Hue;
            updateParameters.seeThrough = this.state.Parameters.Smoothness;
            updateParameters.outline = this.state.Parameters.Metallic;
            updateParameters.HandlePropertiesEdited();
        }
    }
}