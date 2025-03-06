pub mod commands {
    use local_ip_address::local_ip;
    use super::super::super::PARAMS;

    #[tauri::command]
    pub fn get_ip_address() -> String {
        match local_ip() {
            Ok(ip) => {
                println!("Local IP address: {}", ip);
                return format!("{}", ip);
            },
            Err(e) => {
                eprintln!("Error retrieving local IP: {}", e);
                return String::from("");
            },
        }
    }

    #[tauri::command]
    pub fn update_slider(slider_number: &str, slider_value: f32) {
        let mut params = PARAMS.lock().unwrap();
        match slider_number {
            "1" => params.slider1 = slider_value,
            "2" => params.slider2 = slider_value,
            "3" => params.slider3 = slider_value,
            "4" => params.slider4 = slider_value,
            _ => {}
        }
        println!("Updated slider values {:?}", params);
    }
}
