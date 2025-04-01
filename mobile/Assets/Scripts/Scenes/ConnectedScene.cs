using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EvtSource;
using JetBrains.Annotations;
using NeuroViz;
using NeuroViz.Scenes.Connected;
using UnityEngine;

public class RenderParameters
{
    public float Hue { get; set; }
    public float Smoothness { get; set; }
    public float Metallic { get; set; }
    public float Emission { get; set; }
}

public struct Preset
{
    public string Name { get; set; }
    public RenderParameters Parameters { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExperimentType
{
    [JsonStringEnumMemberName("choice")] Choice,
    [JsonStringEnumMemberName("rating")] Rating
}

// public static class ExperimentTypeKind
// {
//     public const string Rating = "rating";
//     public const string Choice = "choice";
// }
//
// public struct Choice
// {
//     public string A { get; set; }
//     public string B { get; set; }
// }

// [UnionTag(nameof(Kind))]
// [UnionCase(typeof(Rating), ExperimentTypeKind.Rating)]
// [UnionCase(typeof(Choice), ExperimentTypeKind.Choice)]
// public abstract class ExperimentType
// {
//     public abstract string Kind { get; }
//
//     public sealed class Rating : UnityState
//     {
//         public override string Kind => ExperimentTypeKind.Rating;
//
//         public List<string> Order { get; set; }
//     }
//
//     public sealed class Choice : UnityState
//     {
//         public override string Kind => ExperimentTypeKind.Choice;
//
//         public List<Choice> Choices { get; set; }
//     }
// }


public struct ExperimentPrompt
{
    public ExperimentType ExperimentType { get; set; }
    public Preset Preset { get; set; }
}

public static class UnityStateKind
{
    public const string Idle = "idle";
    public const string Live = "live";
    public const string Experiment = "experiment";
}

[UnionTag(nameof(Kind))]
[UnionCase(typeof(Idle), UnityStateKind.Idle)]
[UnionCase(typeof(Live), UnityStateKind.Live)]
[UnionCase(typeof(Experiment), UnityStateKind.Experiment)]
[JsonConverter(typeof(UnionConverter<UnityState>))]
public abstract class UnityState
{
    public abstract string Kind { get; }

    public sealed class Idle : UnityState
    {
        public override string Kind => UnityStateKind.Idle;
    }

    public sealed class Live : UnityState
    {
        public override string Kind => UnityStateKind.Live;
        public RenderParameters Parameters { get; set; }
    }

    public sealed class Experiment : UnityState
    {
        public override string Kind => UnityStateKind.Experiment;
        public ExperimentPrompt Prompt { get; set; }
    }
}


namespace NeuroViz.Scenes
{
    public class ConnectedScene : MonoBehaviour
    {
        [SerializeField] public string ip;
        [SerializeField] public int port;
        [SerializeField] public string secret;

        [Header("Scenes")]
        [SerializeField] private GameObject idleScene;

        [SerializeField] private GameObject liveScene;

        [SerializeField] private GameObject experimentScene;

        private EventSourceReader eventSource;
        private UnityState state = new UnityState.Idle();

        public UnityState State => state;
        public event Action<UnityState> OnStateChanged;

        private void Start()
        {
            Debug.Log("Starting event source");
            eventSource = new EventSourceReader(new Uri($"http://{ip}:{port}/state/subscribe"));
            eventSource.Start();

            eventSource.MessageReceived += (sender, e) => HandleEvent(e);
            eventSource.Disconnected += async (sender, e) =>
            {
                Debug.Log($"Retry: {e.ReconnectDelay} - Error: {e.Exception}");
                await Task.Delay(e.ReconnectDelay);
                eventSource.Start(); // Reconnect to the same URL
            };
        }

        private void OnDestroy()
        {
            eventSource.Dispose();
        }

        private void HandleEvent(EventSourceMessageEventArgs e)
        {
            try
            {
                Debug.LogWarning($"{e.Event} : {e.Message}");

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, };

                state = JsonSerializer.Deserialize<UnityState>(e.Message, options);

                Debug.Log(ObjectDumper.Dump(state));
                SetActiveSubScene(state);
                OnStateChanged?.Invoke(state);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
        }

        private void SetActiveSubScene(UnityState state)
        {
            idleScene.SetActive(false);
            liveScene.SetActive(false);
            experimentScene.SetActive(false);

            switch (state)
            {
                case UnityState.Idle idle:
                    idleScene.SetActive(true);
                    break;
                case UnityState.Live live:
                    liveScene.SetActive(true);
                    break;
                case UnityState.Experiment experiment:
                    experimentScene.SetActive(true);
                    break;
            }
        }
    }
}