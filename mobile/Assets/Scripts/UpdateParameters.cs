using System.Collections.Generic;
using UnityEngine;

namespace NeuroViz
{
    public class UpdateParameters : MonoBehaviour
    {
        private static readonly int SeeThroughID = Shader.PropertyToID("_SeeThrough");
        private static readonly int SmoothnessID = Shader.PropertyToID("_Smoothness");

        [Header("Properties")]
        // [OnValueChanged(nameof(HandlePropertiesEdited))]
        // [PropertyRange(0, 1)]
        [SerializeField] public float transparency;

        // [OnValueChanged(nameof(HandlePropertiesEdited))]
        // [PropertyRange(0, 1)]
        [SerializeField] public float outline;

        // [OnValueChanged(nameof(HandlePropertiesEdited))]
        // [PropertyRange(0, 1)]
        [SerializeField] public float seeThrough;
        [SerializeField] public float smoothness;


        [Header("Objects")]
        [SerializeField] private List<Outline> outlineObjects;

        [SerializeField] private List<Material> transparencyMaterials;

        public void SetParameters(RenderParameters parameters)
        {
            transparency = parameters.Transparency;
            seeThrough = parameters.SeeThrough;
            outline = parameters.Outline;
            smoothness = parameters.Smoothness;

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
        }

        //TODO fix iris material   
    }
}