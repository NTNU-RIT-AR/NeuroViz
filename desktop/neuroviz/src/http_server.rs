use axum::{
    extract::{Json, Query, Request, State},
    http::{header::AUTHORIZATION, StatusCode},
    middleware::{self, Next},
    response::{
        sse::{Event, KeepAlive},
        Response, Sse,
    },
    routing::{get, post},
    Router,
};
use futures::{Stream, StreamExt};
use serde::{Deserialize, Serialize};
use specta::Type;
use std::{convert::Infallible, sync::Arc, time::Duration};
use tokio::sync::{mpsc, watch};

use crate::{extensions::WatchReceiverExt, parameters::ParameterValues};

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub enum UnityExperimentType {
    #[serde(rename = "choice")]
    Choice,
    #[serde(rename = "rating")]
    Rating,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct ExperimentPrompt {
    pub experiment_type: UnityExperimentType,
    pub parameters: ParameterValues,
}

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

#[derive(Debug, Clone, Serialize, Deserialize, Type, PartialEq)]
#[serde(tag = "experiment_type")]
pub enum ExperimentAnswer {
    #[serde(rename = "choice")]
    Choice,

    #[serde(rename = "rating")]
    Rating { value: u8 },
}

#[derive(Debug)]
pub enum UnityEvent {
    Connection { is_connected: bool },
    SwapPreset,
    Answer(ExperimentAnswer),
}

#[derive(Clone)]
pub struct HttpServer {
    pub state: watch::Receiver<UnityState>,
    pub event_sender: mpsc::Sender<UnityEvent>,
    /// Secret key for authentication, use None to disable authentication
    pub secret: Option<Arc<String>>,
}

impl HttpServer {
    pub fn app(self) -> Router {
        let state = self;

        let app = Router::new()
            .route("/state/current", get(current_state))
            .route("/state/subscribe", get(subscribe_state))
            .route("/experiment/swap", post(swap_preset))
            .route("/experiment/answer", post(answer_choice_experiment))
            .route_layer(middleware::from_fn_with_state(state.clone(), auth))
            .with_state(state);

        app
    }
}

#[derive(Deserialize)]
struct AuthQuery {
    secret: Option<String>,
}

/// Authentication middleware using secret
async fn auth(
    State(http_server): State<HttpServer>,
    Query(auth_query): Query<AuthQuery>,
    req: Request,
    next: Next,
) -> Result<Response, StatusCode> {
    let Some(secret) = http_server.secret else {
        return Ok(next.run(req).await);
    };

    let auth_header = req
        .headers()
        .get(AUTHORIZATION)
        .and_then(|header| header.to_str().ok());

    // let Some(auth_header) = auth_header else {
    //     return Err(StatusCode::UNAUTHORIZED);
    // };

    let is_valid_token = match (auth_header, auth_query.secret) {
        (Some(header_secret), _) => header_secret == *secret,
        (None, Some(query_secret)) => query_secret == *secret,
        (None, None) => false,
    };

    match is_valid_token {
        true => Ok(next.run(req).await),
        false => Err(StatusCode::UNAUTHORIZED),
    }
}

// Answer experiment
async fn answer_choice_experiment(
    State(http_server): State<HttpServer>,
    Json(payload): Json<ExperimentAnswer>,
) {
    http_server
        .event_sender
        .send(UnityEvent::Answer(payload))
        .await
        .unwrap();
}

// Swap preset
async fn swap_preset(State(http_server): State<HttpServer>) {
    http_server
        .event_sender
        .send(UnityEvent::SwapPreset)
        .await
        .unwrap();
}

// Get current state
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
    let _ = http_server
        .event_sender
        .try_send(UnityEvent::Connection { is_connected: true });

    // Guard to ensure we notify when the stream is closed
    struct Guard {
        event_sender: mpsc::Sender<UnityEvent>,
    }

    impl Drop for Guard {
        fn drop(&mut self) {
            let event_sender = self.event_sender.clone();

            tokio::spawn(async move {
                event_sender
                    .send(UnityEvent::Connection {
                        is_connected: false,
                    })
                    .await
                    .unwrap();
            });
        }
    }

    let mut state_stream = http_server.state.into_stream();

    let stream = async_stream::stream! {
        let _guard = Guard {
            event_sender: http_server.event_sender.clone(),
        };

        while let Some(state) = state_stream.next().await {
            // Send the state as an SSE event
            yield Ok(Event::default()
                .json_data(state)
                .unwrap());
        }
    };

    // Create an SSE stream with a keep-alive interval of 1 second
    Sse::new(stream).keep_alive(
        KeepAlive::new()
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

        let secret = Arc::new("secret".to_owned());

        let http_server = HttpServer {
            state: unity_state_receiver,
            event_sender: unity_event_sender,
            secret: Some(secret.clone()),
        };

        let listening_url = spawn_app("127.0.0.1", http_server.app()).await;

        let app_state = reqwest::Client::new()
            .get(format!("{}/state/current", listening_url))
            .header(AUTHORIZATION, (*secret).clone())
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

        let secret = Arc::new("secret".to_owned());

        let http_server = HttpServer {
            state: unity_state_receiver,
            event_sender: unity_event_sender,
            secret: Some(secret.clone()),
        };

        let listening_url = spawn_app("127.0.0.1", http_server.app()).await;

        let mut event_stream = reqwest::Client::new()
            .get(format!("{}/state/subscribe", listening_url))
            .header(AUTHORIZATION, (*secret).clone())
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
            parameters: ParameterValues::default(),
        };
        unity_state_sender.send(live.clone()).unwrap();
        assert_eq!(get_next_state().await, live);

        // Send an experiment state, check if the event stream receives it
        let experiment = UnityState::Experiment {
            prompt: ExperimentPrompt {
                experiment_type: UnityExperimentType::Choice,
                parameters: ParameterValues::default(),
            },
        };

        unity_state_sender.send(experiment.clone()).unwrap();
        assert_eq!(get_next_state().await, experiment);
    }
}
