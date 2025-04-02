using UnityEngine;

namespace NeuroViz.Scenes.Connected
{
    public class LiveScene : MonoBehaviour
    {
        [SerializeField] private ConnectedScene connectedScene;
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

            updateParameters.transparency = this.state.Parameters.Transparency;
            updateParameters.seeThrough = this.state.Parameters.SeeThrough;
            updateParameters.outline = this.state.Parameters.Outline;
            updateParameters.HandlePropertiesEdited();
        }
    }
}