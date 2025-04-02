using UnityEngine;

namespace NeuroViz.Scenes.Connected
{
    public class ExperimentScene : MonoBehaviour
    {
        [SerializeField] private ConnectedScene connectedScene;
        [SerializeField] private UpdateParameters updateParameters;

        private UnityState.Experiment state;

        // Opacity
        // Border
        // Xray

        // Subsurface?

        private void OnEnable()
        {
            if (connectedScene.State is UnityState.Experiment experiment)
            {
                state = experiment;
            }

            connectedScene.OnStateChanged += HandleStateChange;
        }

        private void OnDisable()
        {
            connectedScene.OnStateChanged -= HandleStateChange;
        }

        private void HandleStateChange(UnityState state)
        {
            if (state is UnityState.Experiment experiment)
            {
                this.state = experiment;
            }

            var prompt = this.state.Prompt;

            updateParameters.transparency = prompt.Parameters.Transparency;
            updateParameters.seeThrough = prompt.Parameters.SeeThrough;
            updateParameters.outline = prompt.Parameters.Outline;
            updateParameters.HandlePropertiesEdited();
        }
    }
}