use axum::{
    extract::State,
    response::{sse::Event, Sse},
    routing::get,
    Json, Router,
};
use futures::stream::Stream;
use futures_signals::signal::{Broadcaster, Flatten, Signal, SignalExt};
use serde::{Deserialize, Serialize};
use std::{convert::Infallible, pin::Pin, sync::Arc, time::Duration};
use tokio::{
    net::TcpListener,
    sync::{mpsc, Mutex},
};
use tokio_stream::StreamExt;

use crate::{
    appdata::AppState,
    consts::HTTP_SERVER_PORT,
    structs::{ExperimentPrompt, ExperimentType, RenderParameters, UnityExperimentType},
};

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
#[serde(tag = "kind")]
pub enum UnityState {
    #[serde(rename = "idle")]
    Idle,

    #[serde(rename = "live")]
    Live { parameters: RenderParameters },

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

pub type BoxSignal<'a, T> = Pin<Box<dyn Signal<Item = T> + Send + Sync + 'a>>;

#[derive(Clone)]
pub struct HttpServer {
    current_state: Arc<Mutex<UnityState>>,
    state_signal: Broadcaster<BoxSignal<'static, UnityState>>,
    event_sender: mpsc::UnboundedSender<UnityEvent>,
}

impl HttpServer {
    pub fn new(
        current_state: UnityState,
        state_signal: impl Signal<Item = UnityState> + Send + Sync + 'static,
        event_sender: mpsc::UnboundedSender<UnityEvent>,
    ) -> Self {
        let state_signal =
            Box::pin(state_signal) as Pin<Box<dyn Signal<Item = UnityState> + Send + Sync>>;
        let state_signal = state_signal.broadcast();

        Self {
            state_signal,
            current_state: Arc::new(Mutex::new(current_state)),
            event_sender,
        }
    }

    pub fn app(self) -> Router {
        let state = self;

        let app = Router::new()
            .route("/state/current", get(current_state))
            .route("/state/subscribe", get(subscribe_state))
            .route("/experiment/swap", get(swap_preset))
            .with_state(state);

        app
    }

    pub async fn run(self) {
        let state = self.state_signal.signal_cloned();
        let current_state = self.current_state.clone();

        let app = self.app();

        let addr = format!("0.0.0.0:{HTTP_SERVER_PORT}");
        let listener = TcpListener::bind(&addr).await.unwrap();

        println!("HTTP server listening on http://localhost:{HTTP_SERVER_PORT}");

        let update_current_state = state.for_each(|state| {
            let current_state = current_state.clone();

            async {
                let mut current_state = current_state.lock_owned().await;
                *current_state = state;
            }
        });

        let (axum_result, _) = tokio::join!(axum::serve(listener, app), update_current_state);
        axum_result.unwrap();
    }
}

async fn swap_preset(State(state): State<HttpServer>) {
    println!("Got request to swap preset");

    state.event_sender.send(UnityEvent::SwapPreset).unwrap();
}

async fn current_state(State(state): State<HttpServer>) -> Json<UnityState> {
    // Get the current state
    let current_state = state.current_state.lock().await.clone();

    // Return the current state as JSON
    Json(current_state)
}

/// Subscribe to state updates as an SSE stream
async fn subscribe_state(
    State(state): State<HttpServer>,
) -> Sse<impl Stream<Item = Result<Event, Infallible>>> {
    let state = state.state_signal.signal_cloned();

    // Map the stream to SSE events with JSON data
    let stream = state
        .to_stream()
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
    use futures_signals::signal::Mutable;
    use tokio::net::TcpListener;

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
        // A watch MPMC channel for the state
        let unity_state = Mutable::new(UnityState::Idle);
        let (unity_event_sender, _unity_event_reciever) = mpsc::unbounded_channel();

        let http_server = HttpServer::new(
            unity_state.get_cloned(),
            unity_state.signal_cloned(),
            unity_event_sender,
        );

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
        let unity_state = Mutable::new(UnityState::Idle);
        let (unity_event_sender, _unity_event_reciever) = mpsc::unbounded_channel();

        let http_server = HttpServer::new(
            unity_state.get_cloned(),
            unity_state.signal_cloned(),
            unity_event_sender,
        );

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
            parameters: RenderParameters {
                hue: 0.5,
                smoothness: 0.5,
                metallic: 0.5,
                emission: 0.5,
            },
        };
        unity_state.set(live.clone());
        assert_eq!(get_next_state().await, live);

        // Send an experiment state, check if the event stream receives it
        let experiment = UnityState::Experiment {
            prompt: ExperimentPrompt {
                experiment_type: UnityExperimentType::Choice,
                parameters: RenderParameters {
                    hue: 0.5,
                    smoothness: 0.5,
                    metallic: 0.5,
                    emission: 0.5,
                },
            },
        };

        unity_state.set(experiment.clone());
        assert_eq!(get_next_state().await, experiment);
    }
}
