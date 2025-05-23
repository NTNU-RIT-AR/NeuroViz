﻿using System;
using UnityEngine;

namespace NeuroViz.Scenes
{
    [Serializable]
    public struct QrPayload
    {
        public string ip;
        public int port;
        public string secret;
    }

    public class ScanScene : MonoBehaviour
    {
        [SerializeField] private ConnectedScene connectedScene;

        private WebCamTexture camTexture;
        private Rect screenRect;
        private QrReader qrReader;

        private Color32[] pixels = null;
        private bool refreshPixels = true;

        private Nullable<QrPayload> foundQrPayload = null;

        void OnGUI()
        {
            var before = GUI.matrix;
            GUIUtility.RotateAroundPivot(camTexture.videoRotationAngle,
                new Vector2(screenRect.width / 2, screenRect.height / 2));
            GUI.DrawTexture(screenRect, camTexture, ScaleMode.ScaleToFit);
            GUI.matrix = before;

            var textRect = new Rect(0, 0, Screen.width, Screen.height * 0.80f);

            GUI.Label(textRect,
                "Scan QR code to connect", new GUIStyle
                {
                    fontSize = 60,
                    alignment = TextAnchor.LowerCenter,
                    normal = new GUIStyleState { textColor = Color.white }
                });
        }

        void OnEnable()
        {
            screenRect = new Rect(0, 0, Screen.width, Screen.height);
            camTexture = new WebCamTexture();

            camTexture.Play();

            Debug.Log("Starting QR reader");
            qrReader = new QrReader(() =>
            {
                refreshPixels = true;

                return (pixels, camTexture.width, camTexture.height);
            });
            qrReader.OnQrCodeFound += OnQrCodeFound;

            Update();
        }

        private void OnDisable()
        {
            Debug.Log("Stopping QR reader");
            qrReader.Dispose();
            camTexture.Stop();
        }

        private void Update()
        {
            if (refreshPixels)
            {
                pixels = camTexture.GetPixels32();
                refreshPixels = false;
            }

            if (foundQrPayload != null)
            {
                gameObject.SetActive(false);
                connectedScene.ip = foundQrPayload.Value.ip;
                connectedScene.port = foundQrPayload.Value.port;
                connectedScene.secret = foundQrPayload.Value.secret;
                foundQrPayload = null;
                connectedScene.gameObject.SetActive(true);
            }
        }

        private void OnQrCodeFound(string text)
        {
            try
            {
                var payload = JsonUtility.FromJson<QrPayload>(text);

                Debug.LogWarning($"IP: {payload.ip}, Port: {payload.port}, Secret: {payload.secret}");

                foundQrPayload = payload;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }
}