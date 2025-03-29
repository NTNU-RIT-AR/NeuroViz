use axum::{
    extract::State,
    response::{sse::Event, Sse},
    routing::get,
    Json, Router,
};
use futures::stream::Stream;
use serde::{Deserialize, Serialize};
use std::{convert::Infallible, time::Duration};
use tokio::{net::TcpListener, sync::{watch, mpsc}};
use tokio_stream::{wrappers::WatchStream, StreamExt};

use crate::{consts::HTTP_SERVER_PORT, structs::{ExperimentPrompt, RenderParamsInner}};

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
#[serde(tag = "kind")]
pub enum UnityState {
    #[serde(rename = "idle")]
    Idle,

    #[serde(rename = "live")]
    Live { parameters: RenderParamsInner },

    #[serde(rename = "experiment")]
    Experiment { prompt: ExperimentPrompt },
}

#[derive(Clone)]
pub struct HttpServer {
    pub state: watch::Receiver<UnityState>,
    pub sender: mpsc::Sender<u8>
}

impl HttpServer {
    pub fn app(self) -> Router {
        let state = self;

        let app = Router::new()
            .route("/state/current", get(get_state))
            .route("/state/subscribe", get(subscribe_state))
            .route("/experiment/swap", get(swap_preset))
            .with_state(state);

        app
    }

    pub async fn run(self) {
        let app = self.app();

        let addr = format!("0.0.0.0:{HTTP_SERVER_PORT}");
        let listener = TcpListener::bind(&addr).await.unwrap();

        println!(
            "HTTP server listening on http://localhost:{HTTP_SERVER_PORT}"
        );

        axum::serve(listener, app).await.unwrap();
    }
}

async fn swap_preset(State(state): State<HttpServer>) {
    println!("Got request to swap preset");
    match state.sender.send(1).await{
        Ok(_) => println!("Sent signal successfully"),
        Err(e) => println!("{}", format!("Failed to send signal: {}", e.to_string()))
    };
}

/// Get the current state
async fn get_state(State(state): State<HttpServer>) -> Json<UnityState> {
    Json(state.state.borrow().clone())
}

/// Subscribe to state updates as an SSE stream
async fn subscribe_state(
    State(state): State<HttpServer>,
) -> Sse<impl Stream<Item = Result<Event, Infallible>>> {
    // Convert the watch channel to a stream
    let stream = WatchStream::new(state.state);

    // Map the stream to SSE events with JSON data
    let stream = stream
        .map(|params| {
            let params = serde_json::to_string(&params).unwrap();

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
    use tokio::net::TcpListener;

    use crate::structs::{Preset, UnityExperimentType};

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
        let (_state_sender, state_receiver) = watch::channel(UnityState::Idle);
        let (swap_signal_sender, _swap_signal_reciever) = mpsc::channel(10);

        let http_server = HttpServer {
            state: state_receiver,
            sender: swap_signal_sender
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
        // A watch MPMC channel for the state
        let (state_sender, state_receiver) = watch::channel(UnityState::Idle);
        let (swap_signal_sender, _swap_signal_reciever) = mpsc::channel(10);

        let http_server = HttpServer {
            state: state_receiver,
            sender: swap_signal_sender
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
            parameters: RenderParamsInner {
                hue: 0.5,
                smoothness: 0.5,
                metallic: 0.5,
                emission: 0.5,
            },
        };
        state_sender.send(live.clone()).unwrap();

        assert_eq!(get_next_state().await, live);

        // Send an experiment state, check if the event stream receives it
        let experiment = UnityState::Experiment {
            prompt: ExperimentPrompt {
                experiment_type: UnityExperimentType::Choice,
                preset: Preset {
                    name: String::from("smoothish"),
                    parameters: RenderParamsInner {
                        hue: 0.5,
                        smoothness: 0.5,
                        metallic: 0.5,
                        emission: 0.5,
                    }
                },
            },
        };
        state_sender.send(experiment.clone()).unwrap();

        assert_eq!(get_next_state().await, experiment);
    }
}
