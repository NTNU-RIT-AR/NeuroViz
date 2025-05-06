// Learn more about Tauri commands at https://tauri.app/develop/calling-rust/
pub mod api;
pub mod consts;
pub mod data;
pub mod extensions;
pub mod state;
pub mod storage;

use std::path::PathBuf;
use std::sync::Arc;
use std::time::Duration;

use api::events::{ConnectionEvent, ResultSavedEvent, StateEvent};
use api::{commands, events};
use consts::HTTP_SERVER_PORT;

use extensions::{MpscReceiverExt, WatchReceiverExt, WatchSenderExt};
use futures::StreamExt;
use neuroviz::http_server::{HttpServer, UnityEvent};
use neuroviz::parameters::ParameterValues;
use rand::distr::Alphanumeric;
use rand::Rng;
use specta_typescript::formatter::prettier;
use specta_typescript::Typescript;
use state::{AppData, AppState};
use tauri::{AppHandle, Manager};
use tauri_specta::{collect_commands, collect_events, ErrorHandlingMode, Event};
use tokio::join;
use tokio::net::TcpListener;
use tokio::sync::{mpsc, watch};
use tokio::time::sleep;

/// Runs the HTTP server, and also transforms the app state into a Unity state
pub async fn http_server_task(
    listener: TcpListener,
    app_state_receiver: watch::Receiver<AppState>,
    unity_event_sender: mpsc::Sender<UnityEvent>,
    secret: Option<String>,
) {
    // Channel for unity state
    let (unity_state_sender, unity_state_receiver) =
        watch::channel(app_state_receiver.borrow().clone().into());

    let http_server = HttpServer {
        state: unity_state_receiver,
        event_sender: unity_event_sender,
        secret: secret.map(Arc::new),
    };

    // Task to update the unity state based on app state changes
    let update_unity_state = async move {
        let mut app_state_stream = app_state_receiver.into_stream();

        // Whenever app_state changes, convert it to UnityState and send it the channel
        while let Some(state) = app_state_stream.next().await {
            unity_state_sender.send(state.into()).unwrap();
        }
    };

    let app = http_server.app();
    let http_server = axum::serve(listener, app);

    let (axum_result, _) = join!(http_server, update_unity_state);
    axum_result.unwrap();
}

/// Task to handle Unity events, will receive events from Unity and update the app state accordingly
pub async fn handle_unity_events_task(
    app_handle: AppHandle,
    app_state_sender: watch::Sender<AppState>,
    unity_event_receiver: mpsc::Receiver<UnityEvent>,
) {
    let mut stream = unity_event_receiver.into_stream();

    while let Some(event) = stream.next().await {
        // let mut app_state = app_state.lock_mut();

        match event {
            UnityEvent::SwapPreset => {
                app_state_sender.send_modify(|state| {
                    let choice_experiment = state
                        .try_as_experiment_mut()
                        .and_then(|experiment| experiment.try_as_choice_mut());

                    if let Some(choice) = choice_experiment {
                        choice.swap_current_preset();
                    }
                });
            }

            UnityEvent::Answer(experiment_answer) => {
                let is_done = app_state_sender.send_modify_with(|state| {
                    let is_done = match state.answer_experiment(experiment_answer) {
                        Ok(is_done) => is_done,
                        Err(error) => {
                            eprintln!("Error answering experiment: {}", error);
                            false
                        }
                    };

                    if let Some(experiment_state) = state.try_as_experiment_mut() {
                        experiment_state.set_is_idle(true);
                    }

                    is_done
                });

                // Sleep for a second while idle
                if !is_done {
                    sleep(Duration::from_secs(1)).await;

                    app_state_sender.send_modify(|state| {
                        if let Some(experiment_state) = state.try_as_experiment_mut() {
                            experiment_state.set_is_idle(false);
                        }
                    });
                }

                // If the experiment is done, finish it and emit the result saved event
                if is_done {
                    if let Some(experiment_state) = app_state_sender
                        .send_replace(AppState::Idle)
                        .try_as_experiment()
                    {
                        if let Ok(result_file_path) = experiment_state.finish_experiment().await {
                            let _ = ResultSavedEvent { result_file_path }.emit(&app_handle);
                        }
                    }
                }
            }

            UnityEvent::Connection { is_connected } => {
                dbg!("Received connection event: {}", is_connected);
                ConnectionEvent { is_connected }.emit(&app_handle).unwrap()
            }
        };
    }
}

/// Generate random secret with 32 characters
pub fn generate_secret() -> String {
    let secret = rand::rng()
        .sample_iter(&Alphanumeric)
        .take(32)
        .map(char::from)
        .collect::<String>();

    secret
}

async fn setup(app: AppHandle) {
    // Initialize app state
    let secret = generate_secret();
    println!("Secret: {}", secret);

    let app_data = AppData::new(
        AppState::LiveView(ParameterValues::default()),
        secret.clone(),
    );
    app.manage(app_data.clone());

    // Channel for events from Unity
    let (unity_event_sender, unity_event_receiver) = mpsc::channel(100);

    // Create a TCP listener for the HTTP server
    let addr = format!("0.0.0.0:{HTTP_SERVER_PORT}");
    let listener = TcpListener::bind(&addr).await.unwrap();
    println!("HTTP server listening on http://localhost:{HTTP_SERVER_PORT}");

    // Task that runs http server
    let http_server = http_server_task(
        listener,
        app_data.state.subscribe(),
        unity_event_sender.clone(),
        Some(secret),
    );

    // Task to update the app state based on Unity events
    let handle_unity_events =
        handle_unity_events_task(app.clone(), app_data.state.clone(), unity_event_receiver);

    // Tawsk to emit app state changes to the tauri frontend
    let emit_app_state = async move {
        let mut app_state_stream = app_data.state.subscribe().into_stream();

        while let Some(new_state) = app_state_stream.next().await {
            println!("Emitting new state: {:?}", new_state);
            StateEvent {
                state: new_state.clone(),
            }
            .emit(&app)
            .unwrap();
        }
    };

    // Run all tasks concurrently
    join!(http_server, handle_unity_events, emit_app_state);
}

pub fn tauri_commands() -> tauri_specta::Builder {
    tauri_specta::Builder::<tauri::Wry>::new()
        .error_handling(ErrorHandlingMode::Throw)
        .commands(collect_commands![
            // App data
            commands::current_state,
            commands::show_folder,
            commands::get_ip_address,
            commands::get_secret,
            commands::get_parameters,
            commands::get_default_parameters,
            // CRUD presets
            commands::get_presets,
            commands::create_preset,
            commands::delete_preset,
            // CRUD experiments
            commands::get_experiments,
            commands::create_experiment,
            commands::delete_experiment,
            // Live view
            commands::set_idle_mode,
            commands::set_live_mode,
            // Actvie experiment
            commands::start_experiment,
            commands::exit_experiment,
            commands::answer_experiment,
            commands::swap_preset
        ])
        .events(collect_events![
            events::ConnectionEvent,
            events::StateEvent,
            events::ResultSavedEvent
        ])
}

pub fn generate_typescript_types(builder: &tauri_specta::Builder) {
    let path = PathBuf::from("../src/bindings.gen.ts");

    builder
        .export(Typescript::default().formatter(prettier), &path)
        .expect("Failed to export typescript bindings");

    println!("Generated TypeScript types at {:?}", path);
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    let builder = tauri_commands();

    #[cfg(debug_assertions)]
    generate_typescript_types(&builder);

    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(builder.invoke_handler())
        .setup(move |app| {
            builder.mount_events(app);

            tauri::async_runtime::spawn(setup(app.handle().clone()));
            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
