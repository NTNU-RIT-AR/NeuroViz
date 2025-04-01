using System;
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
        
        // void OnGUI()
        // {
        //     GUI.DrawTexture(screenRect, camTexture, ScaleMode.ScaleToFit);
        // }
        
        void OnEnable()
        {
            screenRect = new Rect(0, 0, Screen.width, Screen.height);
            camTexture = new WebCamTexture();

            camTexture.Play();
            
            qrReader = new QrReader(() =>
            {
                refreshPixels = true;
                
                return (pixels, camTexture.width, camTexture.height);
            });
            qrReader.OnQrCodeFound += OnQrCodeFound;

            Update();
        }
        
        void OnDisable()
        {
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