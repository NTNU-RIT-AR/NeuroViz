// Learn more about Tauri commands at https://tauri.app/develop/calling-rust/
pub mod api;
pub mod appdata;
pub mod consts;
pub mod extensions;
pub mod structs;

use api::commands;
use api::http_server::{HttpServer, UnityEvent};
use appdata::{AppData, AppState};
use consts::HTTP_SERVER_PORT;

use extensions::MpscReceiverExt;
use futures::StreamExt;
use futures_signals::signal::{Mutable, ReadOnlyMutable, SignalExt};
use tauri::{AppHandle, Emitter, Manager};
use tokio::join;
use tokio::net::TcpListener;
use tokio::sync::{mpsc, watch};

/// Runs the HTTP server, and also transforms the app state into a Unity state
pub async fn http_server_task(
    listener: TcpListener,
    app_state: ReadOnlyMutable<AppState>,
    unity_event_sender: mpsc::Sender<UnityEvent>,
) {
    // Channel for unity state
    let (unity_state_sender, unity_state_receiver) = watch::channel(app_state.get_cloned().into());

    let http_server = HttpServer {
        state: unity_state_receiver,
        event_sender: unity_event_sender,
    };

    // Task to update the unity state based on app state changes
    let update_unity_state = async move {
        let mut app_state_stream = app_state.signal_cloned().to_stream();

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
    app_state: Mutable<AppState>,
    unity_event_receiver: mpsc::Receiver<UnityEvent>,
) {
    let mut stream = unity_event_receiver.into_stream();

    while let Some(event) = stream.next().await {
        let mut app_state = app_state.lock_mut();
        let Some(experiment) = app_state.try_as_experiment_mut() else {
            // Not in experiment mode, ignore
            continue;
        };

        match event {
            UnityEvent::SwapPreset => experiment.swap_current_preset(),
            UnityEvent::Answer(_experiment_answer) => todo!(),
        }
    }
}

async fn setup(app: AppHandle) {
    // Initialize app state
    // TODO: Maybe starting as idle would be better?
    let app_data = AppData::new(AppState::LiveView(Default::default()));
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
        app_data.state.read_only(),
        unity_event_sender.clone(),
    );

    // Task to update the app state based on Unity events
    let handle_unity_events =
        handle_unity_events_task(app_data.state.clone(), unity_event_receiver);

    // Tawsk to emit app state changes to the tauri frontend
    let emit_app_state = async move {
        let mut app_state_stream = app_data.state.signal_cloned().to_stream();

        while let Some(new_state) = app_state_stream.next().await {
            println!("Emitting new state: {:?}", new_state);
            app.emit("state", new_state).unwrap();
        }
    };

    // Run all tasks concurrently
    join!(http_server, handle_unity_events, emit_app_state);

    // println!(
    //     "{:?}",
    //     commands::start_experiment(
    //         app,
    //         String::from("example-experiment-1"),
    //         0,
    //         String::from("my note hihi")
    //     )
    // );
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![
            commands::set_param,
            commands::get_param,
            commands::get_ip_address,
            commands::list_presets,
            commands::retrieve_preset,
            commands::list_experiments,
            commands::retrieve_experiment,
            commands::create_experiment,
            commands::start_experiment
        ])
        .setup(|app| {
            tauri::async_runtime::spawn(setup(app.handle().clone()));
            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
