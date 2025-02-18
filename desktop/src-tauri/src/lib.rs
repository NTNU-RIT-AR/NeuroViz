// Learn more about Tauri commands at https://tauri.app/develop/calling-rust/
use tokio::{io::AsyncWriteExt, net::TcpListener};
use serde::Serialize;
use serde_json;

use std::{sync::{Arc, Mutex}, time::Duration};
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
        // stream.send(json_string.into()).await.expect("Failed to send");  
        stream.write_all(json_string.as_bytes()).await;
        tokio::time::sleep(Duration::from_millis(1)).await;
    }

}

/// Function called by Tauri UI when a slider value changes
#[tauri::command]
fn update_slider(new_slider1: f32, new_slider2: f32, new_slider3: f32, new_slider4: f32) {
    let mut params = PARAMS.lock().unwrap();
    dbg!("hallaaaaaa");
    params.slider1 = new_slider1;
    params.slider2 = new_slider2;
    params.slider3 = new_slider3;
    params.slider4 = new_slider4;
    println!("Updated slider values {:?}", params);

    // let json_string = serde_json::to_string(&*params).unwrap();
    // match tx.send(json_string) {
    //     Ok(_) => true,
    //     Err(_) => false
    // }
}
