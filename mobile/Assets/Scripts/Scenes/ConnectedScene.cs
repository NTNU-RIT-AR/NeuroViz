using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EvtSource;
using JetBrains.Annotations;
using NeuroViz;
using NeuroViz.Scenes.Connected;
using UnityEngine;
using UnityEngine.Networking;

public class RenderParameters
{
    public float Transparency { get; set; }
    public float Glow { get; set; }
    public float Smoothness { get; set; }
    public float Emission { get; set; }
    public float LightIntensity { get; set; }
    public float LightTemperature { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExperimentType
{
    [JsonStringEnumMemberName("choice")] Choice,
    [JsonStringEnumMemberName("rating")] Rating
}

public struct ExperimentPrompt
{
    public ExperimentType ExperimentType { get; set; }
    public RenderParameters Parameters { get; set; }
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

public static class ExperimentAnswerKind
{
    public const string Choice = "choice";
    public const string Rating = "rating";
}

[UnionTag(nameof(ExperimentType))]
[UnionCase(typeof(Choice), ExperimentAnswerKind.Choice)]
[UnionCase(typeof(Rating), ExperimentAnswerKind.Rating)]
[JsonConverter(typeof(UnionConverter<ExperimentAnswer>))]
public abstract class ExperimentAnswer
{
    public abstract string ExperimentType { get; }

    public sealed class Choice : ExperimentAnswer
    {
        public override string ExperimentType => ExperimentAnswerKind.Choice;
    }

    public sealed class Rating : ExperimentAnswer
    {
        public override string ExperimentType => ExperimentAnswerKind.Rating;
        public int value { get; set; }
    }
}


namespace NeuroViz.Scenes
{
    public class ConnectedScene : MonoBehaviour
    {
        [SerializeField] public string ip;
        [SerializeField] public int port;
        [SerializeField] public string secret;

        [SerializeField] private ScanScene scanScene;

        [Header("Scenes")]
        [SerializeField] private GameObject idleScene;

        [SerializeField] private GameObject liveScene;

        [SerializeField] private GameObject experimentScene;

        private EventSourceReader eventSource;
        private UnityState state = new UnityState.Idle();
        private bool isDisconnected = false;

        public UnityState State => state;
        public event Action<UnityState> OnStateChanged;

        private void OnEnable()
        {
            // Subscribe to updates from the controller application
            var url = $"http://{ip}:{port}/state/subscribe?secret={secret}";
            Debug.Log($"Starting event source at: {url}");
            eventSource = new EventSourceReader(new Uri(url));
            eventSource.Start();

            eventSource.MessageReceived += (sender, e) => HandleEvent(e);
            eventSource.Disconnected += async (sender, e) =>
            {
                Debug.LogError("Failed to connect.");
                isDisconnected = true;
            };
        }

        private void Update()
        {
            if (isDisconnected)
            {
                isDisconnected = false;
                gameObject.SetActive(false);
                scanScene.gameObject.SetActive(true);
            }
        }

        private void OnDisable()
        {
            eventSource.Dispose();
        }

        private void HandleEvent(EventSourceMessageEventArgs e)
        {
            try
            {
                // Debug.LogWarning($"{e.Event} : {e.Message}");

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, };

                state = JsonSerializer.Deserialize<UnityState>(e.Message, options);

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

        public IEnumerator Swap()
        {
            var url = $"http://{ip}:{port}/experiment/swap?secret={secret}";

            using var www = UnityWebRequest.PostWwwForm(url, "");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }
        }

        public IEnumerator Answer(ExperimentAnswer answer)
        {
            var url = $"http://{ip}:{port}/experiment/answer?secret={secret}";
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, };
            var json = JsonSerializer.Serialize(answer, options);


            using var www = UnityWebRequest.Post(url, json, "application/json");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }
        }
    }
}