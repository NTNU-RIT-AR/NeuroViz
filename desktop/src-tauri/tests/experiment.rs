use std::{collections::HashMap, sync::Arc};

use chrono::Local;
use eventsource_stream::Eventsource;
use futures::StreamExt;
use neuroviz_lib::{
    api::http_server::{UnityEvent, UnityState},
    data::{
        experiment::{Choice, ChoiceExperiment, CurrentPreset},
        experiment_result::ChoiceExperimentResult,
        parameters::ParameterValues,
        preset::Preset,
    },
    extensions::MpscReceiverExt,
    http_server_task,
    state::{experiment_state::ExperimentState, AppData, AppState},
};
use reqwest::header::AUTHORIZATION;
use tokio::{
    join,
    net::TcpListener,
    sync::{mpsc, watch},
};

/// Helper function to create a TCP listener on a random port
async fn listener_random_port() -> (TcpListener, String) {
    // Bind to localhost at the port 0, which will let the OS assign an available port to us
    let listener = TcpListener::bind("127.0.0.1:0").await.unwrap();

    // Retrieve the port assigned to us by the OS
    let port = listener.local_addr().unwrap().port();

    // Format the IP address with the assigned port
    let listening_url = format!("http://127.0.0.1:{port}");

    (listener, listening_url)
}

/// Task to handle Unity events, will receive events from Unity and update the app state accordingly
pub async fn handle_unity_events_task(
    app_state_sender: watch::Sender<AppState>,
    unity_event_receiver: mpsc::Receiver<UnityEvent>,
) {
    let mut stream = unity_event_receiver.into_stream();

    while let Some(event) = stream.next().await {
        app_state_sender.send_modify(|state| match event {
            UnityEvent::SwapPreset => state
                .try_as_experiment_mut()
                .unwrap()
                .try_as_choice_mut()
                .unwrap()
                .swap_current_preset(),
            UnityEvent::Answer(experiment_answer) => {
                let is_done = state.answer_experiment(experiment_answer).unwrap();
                if is_done {
                    app_state_sender
                        .send(AppState::LiveView(Default::default()))
                        .unwrap();
                }
            }
            UnityEvent::Connection { .. } => {}
        });
    }
}

/// Integration test for the experiment functionality, tests the AppData and HTTP server integrated
#[tokio::test]
async fn experiment_integration_test() {
    let secret = Arc::new("secret".to_owned());

    let app_data = AppData::new(AppState::LiveView(Default::default()), secret.clone());
    let (unity_event_sender, unity_event_receiver) = mpsc::channel(100);

    let (listener, listening_url) = listener_random_port().await;

    let http_server = http_server_task(
        listener,
        app_data.state.subscribe(),
        unity_event_sender.clone(),
        secret.clone(),
    );
    let handle_unity_events =
        handle_unity_events_task(app_data.state.clone(), unity_event_receiver);

    // Spawn tasks in background
    tokio::spawn(async { join!(http_server, handle_unity_events) });

    let mut event_stream = reqwest::Client::new()
        .get(format!("{listening_url}/state/subscribe"))
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

    // Check if the initial state is sent
    assert_eq!(
        get_next_state().await,
        app_data.state.borrow().clone().into()
    );

    let parameters_1 = ParameterValues {
        transparency: 0.5,
        see_through: 0.5,
        outline: 0.5,
        smoothness: 0.5,
    };

    let parameters_2 = ParameterValues {
        transparency: 0.7,
        see_through: 0.7,
        outline: 0.7,
        smoothness: 0.7,
    };

    // Start an experiment
    let experiment = ChoiceExperiment::new(
        "Experiment 1".to_owned(),
        HashMap::from_iter([
            (
                "preset-1".to_owned(),
                Preset {
                    name: "Preset 1".to_owned(),
                    parameters: parameters_1,
                },
            ),
            (
                "preset-2".to_owned(),
                Preset {
                    name: "Preset 2".to_owned(),
                    parameters: parameters_2,
                },
            ),
        ]),
        vec![Choice {
            a: "preset-1".to_owned(),
            b: "preset-2".to_owned(),
        }],
    );

    let experiment_result = ChoiceExperimentResult::new(
        "result-1".to_owned(),
        Local::now(),
        0,
        "my note".to_owned(),
        &experiment,
    );

    app_data
        .state
        .send(AppState::Experiment(ExperimentState::new_choice(
            "experiment-1".to_owned(),
            "result-1".to_owned(),
            experiment,
            experiment_result,
        )))
        .unwrap();

    // Check if the experiment state is sent
    let _unity_state = get_next_state().await;
    let unity_state = get_next_state().await;
    assert_eq!(unity_state, app_data.state.borrow().clone().into());

    let get_current_preset = || {
        app_data
            .state
            .borrow()
            .try_as_experiment_ref()
            .unwrap()
            .try_as_choice_ref()
            .unwrap()
            .current_preset
    };

    assert_eq!(get_current_preset(), CurrentPreset::A);

    // Send a swap event
    unity_event_sender
        .send(UnityEvent::SwapPreset)
        .await
        .unwrap();

    // Check state is in sync
    let unity_state = get_next_state().await;
    assert_eq!(unity_state, app_data.state.borrow().clone().into());

    // Check if the current preset is swapped
    assert_eq!(get_current_preset(), CurrentPreset::B);
}
