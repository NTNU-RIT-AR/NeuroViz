use axum::{
    extract::State,
    response::{sse::Event, Sse},
    routing::get,
    Json, Router,
};
use futures::{Stream, StreamExt};
use serde::{Deserialize, Serialize};
use std::{convert::Infallible, time::Duration};
use tokio::sync::{mpsc, watch};

use crate::{
    appdata::AppState,
    extensions::WatchReceiverExt,
    structs::{ExperimentPrompt, ExperimentType, ParameterValues, UnityExperimentType},
};

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
#[serde(tag = "kind")]
pub enum UnityState {
    #[serde(rename = "idle")]
    Idle,

    #[serde(rename = "live")]
    Live { parameters: ParameterValues },

    #[serde(rename = "experiment")]
    Experiment { prompt: ExperimentPrompt },
}

impl From<AppState> for UnityState {
    fn from(app_state: AppState) -> Self {
        match app_state {
            AppState::LiveView(parameters) => UnityState::Live { parameters },
            AppState::Experiment(experiment_state) => UnityState::Experiment {
                prompt: ExperimentPrompt {
                    experiment_type: match experiment_state.experiment.experiment_type {
                        ExperimentType::Choice { .. } => UnityExperimentType::Choice,
                        ExperimentType::Rating { .. } => UnityExperimentType::Rating,
                    },
                    parameters: experiment_state.get_current_preset().parameters,
                },
            },
        }
    }
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
#[serde(tag = "experiment_type")]
pub enum ExperimentAnswer {
    #[serde(rename = "choice")]
    Choice,

    #[serde(rename = "rating")]
    Rating { value: u8 },
}

pub enum UnityEvent {
    SwapPreset,
    Answer(ExperimentAnswer),
}

#[derive(Clone)]
pub struct HttpServer {
    pub state: watch::Receiver<UnityState>,
    pub event_sender: mpsc::Sender<UnityEvent>,
}

impl HttpServer {
    pub fn app(self) -> Router {
        let state = self;

        let app = Router::new()
            .route("/state/current", get(current_state))
            .route("/state/subscribe", get(subscribe_state))
            .route("/experiment/swap", get(swap_preset))
            .with_state(state);

        app
    }
}

async fn swap_preset(State(http_server): State<HttpServer>) {
    println!("Got request to swap preset");

    http_server
        .event_sender
        .send(UnityEvent::SwapPreset)
        .await
        .unwrap();
}

async fn current_state(State(http_server): State<HttpServer>) -> Json<UnityState> {
    // Get the current state
    let current_state = http_server.state.borrow().clone();

    // Return the current state as JSON
    Json(current_state)
}

/// Subscribe to state updates as an SSE stream
async fn subscribe_state(
    State(http_server): State<HttpServer>,
) -> Sse<impl Stream<Item = Result<Event, Infallible>>> {
    // Map the stream to SSE events with JSON data
    let stream = http_server
        .state
        .into_stream()
        .map(|unity_state| {
            // Convert the UnityState to a JSON string
            let params = serde_json::to_string(&unity_state).unwrap();

            Event::default().data(params)
        })
        .map(Ok);

    // Create an SSE stream with a keep-alive interval of 1 second
    Sse::new(stream).keep_alive(
        axum::response::sse::KeepAlive::new()
            .interval(Duration::from_secs(1))
            .text("keep-alive-text"),
    )
}

#[cfg(test)]
mod tests {
    use eventsource_stream::Eventsource;
    use tokio::{
        net::TcpListener,
        sync::{mpsc, watch},
    };

    use crate::structs::UnityExperimentType;

    use super::*;

    // A helper function that spawns our axum application in the background
    async fn spawn_app(host: impl Into<String>, app: Router) -> String {
        let host = host.into();
        // Bind to localhost at the port 0, which will let the OS assign an available port to us
        let listener = TcpListener::bind(format!("{}:0", host)).await.unwrap();
        // Retrieve the port assigned to us by the OS
        let port = listener.local_addr().unwrap().port();

        tokio::spawn(async {
            axum::serve(listener, app).await.unwrap();
        });
        // Returns address (e.g. http://127.0.0.1{random_port})
        format!("http://{}:{}", host, port)
    }

    /// Test the `/state/current` endpoint, which should return the current state
    #[tokio::test]
    async fn test_current_state() {
        let (_, unity_state_receiver) = watch::channel(UnityState::Idle);
        let (unity_event_sender, _) = mpsc::channel(100);

        let http_server = HttpServer {
            state: unity_state_receiver,
            event_sender: unity_event_sender,
        };

        let listening_url = spawn_app("127.0.0.1", http_server.app()).await;

        let app_state = reqwest::Client::new()
            .get(format!("{}/state/current", listening_url))
            .send()
            .await
            .unwrap()
            .json::<UnityState>()
            .await
            .unwrap();

        assert_eq!(app_state, UnityState::Idle);
    }

    /// Test the `/state/subscribe` endpoint, which should return a stream of state updates
    #[tokio::test]
    async fn test_subscribe_state() {
        let (unity_state_sender, unity_state_receiver) = watch::channel(UnityState::Idle);
        let (unity_event_sender, _unity_event_reciever) = mpsc::channel(100);

        let http_server = HttpServer {
            state: unity_state_receiver,
            event_sender: unity_event_sender,
        };

        let listening_url = spawn_app("127.0.0.1", http_server.app()).await;

        let mut event_stream = reqwest::Client::new()
            .get(format!("{}/state/subscribe", listening_url))
            .send()
            .await
            .unwrap()
            .bytes_stream()
            .eventsource();

        let mut get_next_state = async || {
            let event = event_stream.next().await.unwrap().unwrap();
            let data = serde_json::from_str::<UnityState>(&event.data).unwrap();

            data
        };

        assert_eq!(get_next_state().await, UnityState::Idle);

        // Send a live state, check if the event stream receives it
        let live = UnityState::Live {
            parameters: ParameterValues {
                hue: 0.5,
                smoothness: 0.5,
                metallic: 0.5,
                emission: 0.5,
            },
        };
        unity_state_sender.send(live.clone()).unwrap();
        assert_eq!(get_next_state().await, live);

        // Send an experiment state, check if the event stream receives it
        let experiment = UnityState::Experiment {
            prompt: ExperimentPrompt {
                experiment_type: UnityExperimentType::Choice,
                parameters: ParameterValues {
                    hue: 0.5,
                    smoothness: 0.5,
                    metallic: 0.5,
                    emission: 0.5,
                },
            },
        };

        unity_state_sender.send(experiment.clone()).unwrap();
        assert_eq!(get_next_state().await, experiment);
    }
}
