using System.Collections.Generic;
using UnityEngine;

namespace NeuroViz
{
    public class UpdateParameters : MonoBehaviour
    {
        private static readonly int SeeThrough = Shader.PropertyToID("_SeeThrough");

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


        [Header("Objects")]
        [SerializeField] private List<Outline> outlineObjects;

        [SerializeField] private List<Material> transparencyMaterials;

        private void OnEnable()
        {
            HandlePropertiesEdited();
        }

        public void HandlePropertiesEdited()
        {
            Shader.SetGlobalFloat(SeeThrough, seeThrough);

            foreach (var outlineObject in outlineObjects)
            {
                var color = outlineObject.OutlineColor;
                outlineObject.OutlineColor = new Color(color.r, color.g, color.b, outline);
            }

            foreach (var transparencyMaterial in transparencyMaterials)
            {
                transparencyMaterial.SetFloat("_Transparency", transparency);
            }
        }

        //TODO fix iris material   
    }
}