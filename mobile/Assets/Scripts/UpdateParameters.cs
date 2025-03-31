using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace NeuroViz
{
    public class UpdateParameters : MonoBehaviour
    {
        private static readonly int SeeThrough = Shader.PropertyToID("_SeeThrough");

        [TitleGroup("Properties")]
        [OnValueChanged(nameof(HandlePropertiesEdited))]
        [PropertyRange(0, 1)]
        [SerializeField] public float transparency;

        [OnValueChanged(nameof(HandlePropertiesEdited))]
        [PropertyRange(0, 1)]
        [SerializeField] public float outline;

        [OnValueChanged(nameof(HandlePropertiesEdited))]
        [PropertyRange(0, 1)]
        [SerializeField] public float seeThrough;


        [TitleGroup("Objects")]
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
                outlineObject.OutlineColor = outlineObject.OutlineColor.WithAlpha(outline);
            }

            foreach (var transparencyMaterial in transparencyMaterials)
            {
                transparencyMaterial.SetFloat("_Transparency", transparency);
            }
        }

        //TODO fix iris material   
    }
}