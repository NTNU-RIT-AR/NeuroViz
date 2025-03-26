using System;
using System.Threading.Tasks;
using EvtSource;
using UnityEngine;

namespace NeuroViz.Scenes
{
    public class ConnectedScene : MonoBehaviour
    {
        [SerializeField] public string ip;
        [SerializeField] public int port;
        [SerializeField] public string secret;
        
        private EventSourceReader event_source;
    
        void Start()
        {
            Debug.Log("Starting event source");
            event_source = new EventSourceReader(new Uri($"http://{ip}:{port}/state/subscribe"));
            event_source.Start();
        
            event_source.MessageReceived += (object sender, EventSourceMessageEventArgs e) => Debug.LogWarning($"{e.Event} : {e.Message}");
            event_source.Disconnected += async (object sender, DisconnectEventArgs e) => {
                Debug.Log($"Retry: {e.ReconnectDelay} - Error: {e.Exception}");
                await Task.Delay(e.ReconnectDelay);
                event_source.Start(); // Reconnect to the same URL
            };
        }

        private void OnDestroy()
        {
            event_source.Dispose();
        }
    }
}