﻿using System;
using NeuroViz.Scenes;
using Unity.XR.XREAL;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NeuroViz
{
    public class ExperimentUI : MonoBehaviour
    {
        [SerializeField] private GameObject choiceUI;
        [SerializeField] private GameObject ratingUI;

        [SerializeField] private Button swapButton;
        [SerializeField] private Slider ratingSlider;
        [SerializeField] private Button confirmButton;
        [SerializeField] private TMPro.TMP_Text ratingText;

        private CanvasGroup canvasGroup;
        private ConnectedScene connectedScene;

        private UnityState.Experiment state;
        private int ratingValue = 1;

        private void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();

            var scene = SceneManager.GetActiveScene();

            foreach (var rootGameObject in scene.GetRootGameObjects())
            {
                connectedScene = rootGameObject.GetComponentInChildren<ConnectedScene>(true);
                if (connectedScene != null) break;
            }

            HandleStateChange(connectedScene.State);
            connectedScene.OnStateChanged += HandleStateChange;
        }

        private void OnEnable()
        {
            Handheld.Vibrate();

            swapButton.onClick.AddListener(HandleSwapButtonClick);
            confirmButton.onClick.AddListener(HandleConfirmButtonClick);
            ratingSlider.onValueChanged.AddListener(HandleRatingSliderValueChanged);
        }

        private void OnDisable()
        {
            connectedScene.OnStateChanged -= HandleStateChange;
            swapButton.onClick.RemoveListener(HandleSwapButtonClick);
            confirmButton.onClick.RemoveListener(HandleConfirmButtonClick);
            ratingSlider.onValueChanged.RemoveListener(HandleRatingSliderValueChanged);
        }

        void HandleStateChange(UnityState state)
        {
            var isEnabled = false;

            if (state is UnityState.Experiment experiment)
            {
                isEnabled = true;
                this.state = experiment;
            }

            canvasGroup.interactable = isEnabled;
            canvasGroup.alpha = isEnabled ? 1 : 0;
            canvasGroup.blocksRaycasts = isEnabled;

            if (this.state == null) return;
            var prompt = this.state.Prompt;
            var parameters = prompt.Parameters;
            var experimentType = prompt.ExperimentType;

            switch (experimentType)
            {
                case ExperimentType.Choice:
                    ratingUI.SetActive(false);
                    choiceUI.SetActive(true);
                    break;
                case ExperimentType.Rating:
                    ratingUI.SetActive(true);
                    choiceUI.SetActive(false);
                    break;
            }
        }

        private void HandleSwapButtonClick()
        {
            if (XREALVirtualController.Singleton != null)
                XREALVirtualController.Singleton.Controller.SendHapticImpulse(0, 0.25f, 0.15f);

            StartCoroutine(connectedScene.Swap());
        }

        private void HandleConfirmButtonClick()
        {
            ExperimentAnswer answer;

            switch (state.Prompt.ExperimentType)
            {
                case ExperimentType.Choice:
                    answer = new ExperimentAnswer.Choice();
                    break;
                case ExperimentType.Rating:
                    answer = new ExperimentAnswer.Rating()
                    {
                        value = ratingValue
                    };
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (XREALVirtualController.Singleton != null)
                XREALVirtualController.Singleton.Controller.SendHapticImpulse(0, 0.4f, 0.25f);

            StartCoroutine(connectedScene.Answer(answer));
        }

        private void HandleRatingSliderValueChanged(float value)
        {
            if (XREALVirtualController.Singleton != null)
                XREALVirtualController.Singleton.Controller.SendHapticImpulse(0, 0.02f, 0.05f);

            ratingValue = (int)Math.Round(value);
            ratingText.text = ratingValue.ToString();
            Debug.Log($"Rating value changed: {ratingValue}");
        }
    }
}