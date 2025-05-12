using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace NeuroViz
{
    [ExecuteInEditMode]
    public class UpdateParameters : MonoBehaviour
    {
        private static readonly int SmoothnessID = Shader.PropertyToID("_Smoothness");
        private static readonly int TransparencyID = Shader.PropertyToID("_Transparency");
        private static readonly int EmissionID = Shader.PropertyToID("_Emission");

        [Header("Properties")]
        [SerializeField] public float transparency;

        [SerializeField] public float glow;
        [SerializeField] public float smoothness;
        [SerializeField] private float emission;
        [SerializeField] public float lightIntensity;
        [SerializeField] public float lightTemperature;


        [FormerlySerializedAs("outlineObjects")]
        [Header("Objects")]
        [SerializeField] private List<Outline> glowObjects;

        [SerializeField] private new Light light;


        public void SetParameters(RenderParameters parameters)
        {
            transparency = parameters.Transparency;
            glow = parameters.Glow;
            smoothness = parameters.Smoothness;
            emission = parameters.Emission;
            lightIntensity = parameters.LightIntensity;
            lightTemperature = parameters.LightTemperature;

            HandlePropertiesEdited();
        }

        private void OnEnable()
        {
            HandlePropertiesEdited();
        }

        // Update parameters while in editor mode
#if UNITY_EDITOR
        private void Update()
        {
            HandlePropertiesEdited();
        }
#endif

        private static float EaseInQuad(float start, float end, float value)
        {
            end -= start;
            return end * value * value + start;
        }

        private void HandlePropertiesEdited()
        {
            // Set the shader properties globally
            Shader.SetGlobalFloat(SmoothnessID, smoothness);
            Shader.SetGlobalFloat(TransparencyID, transparency);
            Shader.SetGlobalFloat(EmissionID, emission);

            // Set the shader properties for each outline object
            foreach (var outlineObject in glowObjects)
            {
                var color = outlineObject.OutlineColor;
                outlineObject.OutlineColor = new Color(5.99215746f, 5.99215746f, 5.99215746f, EaseInQuad(0f, 1f, glow));
            }

            // Set the light properties
            light.intensity = lightIntensity;
            light.colorTemperature = lightTemperature;
        }
    }
}