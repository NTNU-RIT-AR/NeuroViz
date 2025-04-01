use chrono::{DateTime, Local};

// TODO: vurdere om det er vits å ha egen module for "utils"?

// Function to retrieve duration in seconds, i.e 2.1 seconds
pub fn get_duration_since(then: DateTime<Local>) -> f64 {
    Local::now().signed_duration_since(then).num_nanoseconds().unwrap() as f64 / 1_000_000_000.0
}