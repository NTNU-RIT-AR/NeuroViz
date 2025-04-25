using System.Collections.Generic;
using UnityEngine;

namespace NeuroViz
{
    public class UpdateParameters : MonoBehaviour
    {
        private static readonly int SeeThroughID = Shader.PropertyToID("_SeeThrough");
        private static readonly int SmoothnessID = Shader.PropertyToID("_Smoothness");

        [Header("Properties")]
        [SerializeField] public float transparency;

        [SerializeField] public float outline;
        [SerializeField] public float seeThrough;
        [SerializeField] public float smoothness;
        [SerializeField] public float lightIntensity;
        [SerializeField] public float lightTemperature;


        [Header("Objects")]
        [SerializeField] private List<Outline> outlineObjects;

        [SerializeField] private List<Material> transparencyMaterials;

        [SerializeField] private new Light light;


        public void SetParameters(RenderParameters parameters)
        {
            transparency = parameters.Transparency;
            outline = parameters.Outline;
            smoothness = parameters.Smoothness;
            lightIntensity = parameters.LightIntensity;
            lightTemperature = parameters.LightTemperature;

            HandlePropertiesEdited();
        }

        private void OnEnable()
        {
            HandlePropertiesEdited();
        }

        private static float EaseInQuad(float start, float end, float value)
        {
            end -= start;
            return end * value * value + start;
        }

        private void HandlePropertiesEdited()
        {
            Shader.SetGlobalFloat(SeeThroughID, seeThrough);
            Shader.SetGlobalFloat(SmoothnessID, smoothness);

            foreach (var outlineObject in outlineObjects)
            {
                var color = outlineObject.OutlineColor;
                outlineObject.OutlineColor = new Color(color.r, color.g, color.b, EaseInQuad(0f, 1f, outline));
                outlineObject.OutlineWidth = outline * 10f;
            }

            foreach (var transparencyMaterial in transparencyMaterials)
            {
                transparencyMaterial.SetFloat("_Transparency", transparency);
            }

            light.intensity = lightIntensity;
            light.colorTemperature = lightTemperature;
        }
    }
}