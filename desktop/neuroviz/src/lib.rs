use rand::{distr::Alphanumeric, Rng};

pub mod extensions;
pub mod http_server;
pub mod parameters;

/// Generate random secret with 32 characters
pub fn generate_secret() -> String {
    let secret = rand::rng()
        .sample_iter(&Alphanumeric)
        .take(32)
        .map(char::from)
        .collect::<String>();

    secret
}
