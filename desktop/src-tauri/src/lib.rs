// Learn more about Tauri commands at https://tauri.app/develop/calling-rust/
use tokio::{io::AsyncWriteExt, net::TcpListener};
use serde::Serialize;
use serde_json;

use std::{sync::{Arc, Mutex}, time::Duration};
use serde::Deserialize;
use lazy_static::lazy_static;


#[tauri::command]
fn greet(name: &str) -> String {
    format!("Hi, {}! You've been greeted from Rust!", name)
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {

    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![greet, update_slider])
        .setup(|_app| {
            tauri::async_runtime::spawn(thingy());
            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}

lazy_static! {
    static ref PARAMS: Arc<Mutex<Parameters>> = Arc::new(Mutex::new(Parameters {
        slider1: 1.0,
        slider2: 1.0,
        slider3: 1.0,
        slider4: 1.0,
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

    // let (tx, _) = broadcast::channel::<String>(10); // Message broadcaster

    while let Ok((stream, addr)) = listener.accept().await {
        
        tokio::spawn(handle_connection(stream, addr));
    }
}

async fn handle_connection(mut stream: tokio::net::TcpStream, addr: std::net::SocketAddr) {
    println!("New connection: {}", addr);

    loop {
        let params = PARAMS.lock().unwrap().clone();
        let json_string = serde_json::to_string(&params).unwrap();
        
        // Breaks out of the loop when the Tcp client has disconnected and can not recieve more writes
        if let Err(_) = stream.write_all((json_string + "\n").as_bytes()).await {
            println!("Client disconnected: {}", addr);
            break;
        }
        tokio::time::sleep(Duration::from_millis(1)).await;
    }

}

/// Function called by Tauri UI when a slider value changes
#[tauri::command]
fn update_slider(slider_number: &str, slider_value: f32) {
    let mut params = PARAMS.lock().unwrap();
    match slider_number {
        "1" => params.slider1 = slider_value,
        "2" => params.slider2 = slider_value,
        "3" => params.slider3 = slider_value,
        "4" => params.slider4 = slider_value,
        _ => {},
    }
    println!("Updated slider values {:?}", params);
}
