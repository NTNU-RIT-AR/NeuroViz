pub mod tcpservice {
    use super::super::super::PARAMS;
    use crate::api::events::events;

    use tauri::AppHandle;
    use tokio::{io::AsyncWriteExt, net::TcpListener};
    use std::time::Duration;

    pub async fn tcp_listener(app_handle: AppHandle) {
        let addr = "0.0.0.0:9001"; // Listen on all interfaces
        let listener = TcpListener::bind(&addr).await.expect("Failed to bind");
    
        println!("WebSocket server listening on ws://{}", addr);
    
        while let Ok((stream, addr)) = listener.accept().await {
            println!("New connection: {}", addr);
            let app_handle_clone = app_handle.clone();
            tokio::spawn(handle_connection(stream, addr, app_handle_clone));
        }
    }

    pub async fn handle_connection(mut stream: tokio::net::TcpStream, addr: std::net::SocketAddr, app_handle: AppHandle) {
    
        //Emit to frontend that device has connected
        events::emit_connection_event(&app_handle);
    
        loop {
            let params = PARAMS.lock().unwrap().clone();
            let json_string = serde_json::to_string(&params).unwrap();
    
            // Breaks out of the loop when the Tcp client has disconnected and can not recieve more writes
            if let Err(_) = stream.write_all((json_string + "\n").as_bytes()).await {
                println!("Client disconnected: {}", addr);
                events::emit_disconnection_event(app_handle);
                break;
            }
            tokio::time::sleep(Duration::from_millis(1)).await;
        }
    }
}