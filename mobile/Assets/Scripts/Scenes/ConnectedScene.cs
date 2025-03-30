using System;
using System.Threading.Tasks;
using EvtSource;
using UnityEngine;
using System.Text.Json;
using NeuroViz;

public struct RenderParameters
{
    public float Hue;
    public float Smoothness;
    public float Metallic;
    public float Emission;
}

public static class UnityStateKind
{
    public const string Idle = "idle";
    public const string Live = "live";
}

[UnionTag(nameof(Kind))]
[UnionCase(typeof(IdleUnityState), UnityStateKind.Idle)]
[UnionCase(typeof(LiveUnityState), UnityStateKind.Live)]
public abstract class UnityState
{
    public abstract string Kind { get; }
}

public sealed class IdleUnityState : UnityState
{
    public override string Kind => UnityStateKind.Idle;
}

public sealed class LiveUnityState : UnityState
{
    public override string Kind => UnityStateKind.Idle;

    public RenderParameters Parameters;
}


namespace NeuroViz.Scenes
{
    public class ConnectedScene : MonoBehaviour
    {
        [SerializeField] public string ip;
        [SerializeField] public int port;
        [SerializeField] public string secret;
        
        private EventSourceReader eventSource;
    
        void Start()
        {
            Debug.Log("Starting event source");
            eventSource = new EventSourceReader(new Uri($"http://{ip}:{port}/state/subscribe"));
            eventSource.Start();
        
            eventSource.MessageReceived += (object sender, EventSourceMessageEventArgs e) => HandleEvent(e);
            eventSource.Disconnected += async (object sender, DisconnectEventArgs e) => {
                Debug.Log($"Retry: {e.ReconnectDelay} - Error: {e.Exception}");
                await Task.Delay(e.ReconnectDelay);
                eventSource.Start(); // Reconnect to the same URL
            };
        }

        private void HandleEvent(EventSourceMessageEventArgs e)
        {
            Debug.LogWarning($"{e.Event} : {e.Message}");
            
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                Converters = { new UnionConverterFactory() }
            };

            var state = JsonSerializer.Deserialize<UnityState>(e.Message, options);
            Debug.Log(state);
        }

        private void OnDestroy()
        {
            eventSource.Dispose();
        }
    }
}