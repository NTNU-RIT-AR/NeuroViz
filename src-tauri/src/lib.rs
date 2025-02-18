// Learn more about Tauri commands at https://tauri.app/develop/calling-rust/
use tokio::net::TcpListener;
use tokio_tungstenite::accept_async;
use futures_util::{StreamExt, SinkExt};
use serde::Serialize;
use serde_json;

use std::sync::{Arc, Mutex};
use tokio::sync::broadcast;
use serde::Deserialize;
use lazy_static::lazy_static;


#[tauri::command]
fn greet(name: &str) -> String {
    println!("Hii");
    format!("Hi, {}! You've been greeted from Rust!", name)
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {

    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![greet, update_slider])
        // .setup(|_app| {
        //     tauri::async_runtime::spawn(thingy());
        //     Ok(())
        // })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}

lazy_static! {
    static ref PARAMS: Arc<Mutex<Parameters>> = Arc::new(Mutex::new(Parameters {
        slider1: 1.0,
        slider2: 2.5,
        slider3: 3.75,
        slider4: 4.9,
    }));
}

#[derive(Serialize, Deserialize, Clone, Debug)]
struct Parameters {
    slider1: f32,
    slider2: f32,
    slider3: f32,
    slider4: f32,
}

async fn thingy() {
    let addr = "0.0.0.0:9001"; // Listen on all interfaces
    let listener = TcpListener::bind(&addr).await.expect("Failed to bind");

    println!("WebSocket server listening on ws://{}", addr);

    let (tx, _) = broadcast::channel::<String>(10); // Message broadcaster

    while let Ok((stream, addr)) = listener.accept().await {
        let tx = tx.clone();
        tokio::spawn(handle_connection(stream, addr, tx));
    }
}

async fn handle_connection(stream: tokio::net::TcpStream, addr: std::net::SocketAddr, tx: broadcast::Sender<String>) {
    println!("New WebSocket connection: {}", addr);

    let ws_stream = accept_async(stream).await.expect("Error accepting WebSocket");
    let (mut write, mut read) = ws_stream.split();

    let params = PARAMS.lock().unwrap().clone();
    let json_string = serde_json::to_string(&params).unwrap();
    write.send(json_string.into()).await.expect("Failed to send");

    let mut rx = tx.subscribe(); // Get a receiver for this client

    tokio::spawn(async move {
        while let Ok(updated_message) = rx.recv().await {
            if write.send(updated_message.clone().into()).await.is_err() {
                println!("Client {} disconnected", addr);
                break;
            }
        }
    });

    while let Some(Ok(_msg)) = read.next().await {
        // You can add logic here to handle incoming messages from the client if needed
    }

    println!("Connection closed: {}", addr);
}

/// Function called by Tauri UI when a slider value changes
#[tauri::command]
fn update_slider(new_slider1: f32, new_slider2: f32, new_slider3: f32, new_slider4: f32, tx: tauri::State<broadcast::Sender<String>>) -> bool {
    let mut params = PARAMS.lock().unwrap();
    params.slider1 = new_slider1;
    params.slider2 = new_slider2;
    params.slider3 = new_slider3;
    params.slider4 = new_slider4;
    println!("Updated slider values {:?}", params);

    let json_string = serde_json::to_string(&*params).unwrap();
    match tx.send(json_string) {
        Ok(_) => true,
        Err(_) => false
    }
}
