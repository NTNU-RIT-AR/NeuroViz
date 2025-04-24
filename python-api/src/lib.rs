use std::{sync::Arc, time::Duration};

use anyhow::{Context, anyhow, bail};
use local_ip_address::linux::local_ip;
use neuroviz_lib::{
    api::http_server::{ExperimentPrompt, HttpServer, UnityEvent, UnityExperimentType, UnityState},
    data::parameters::ParameterValues,
    generate_secret,
};
use pyo3::{prelude::*, types::PyDict};
use tokio::{
    net::TcpListener,
    runtime::Runtime,
    select,
    sync::{mpsc, watch},
    time::sleep,
};
use tokio_util::sync::CancellationToken;

/// Runs the HTTP server, and also transforms the app state into a Unity state
pub async fn http_server_task(
    listener: TcpListener,
    unity_state_receiver: watch::Receiver<UnityState>,
    unity_event_sender: mpsc::Sender<UnityEvent>,
    secret: Option<String>,
) {
    let http_server = HttpServer {
        state: unity_state_receiver,
        event_sender: unity_event_sender,
        secret: secret.map(Arc::new),
    };

    let app = http_server.app();
    axum::serve(listener, app).await.unwrap();
}

#[pyclass]
struct NeuroViz {
    runtime: Arc<Runtime>,
    cancellation_token: CancellationToken,
    unity_state_sender: watch::Sender<UnityState>,
    unity_event_receiver: mpsc::Receiver<UnityEvent>,

    #[pyo3(get)]
    ip: String,
    #[pyo3(get)]
    port: u16,
    #[pyo3(get)]
    secret: Option<String>,
}

fn dict_to_parameters<'py>(dict: Bound<'py, PyDict>) -> PyResult<ParameterValues> {
    let transparency: f32 = dict
        .get_item("transparency")?
        .context("required")?
        .extract()?;

    let see_through: f32 = dict
        .get_item("see_through")?
        .context("required")?
        .extract()?;

    let outline: f32 = dict.get_item("outline")?.context("required")?.extract()?;

    let smoothness: f32 = dict
        .get_item("smoothness")?
        .context("required")?
        .extract()?;

    Ok(ParameterValues {
        transparency,
        see_through,
        outline,
        smoothness,
    })
}

#[pymethods]
impl NeuroViz {
    #[new]
    fn new(port: u16, use_secret: bool) -> Self {
        let (unity_state_sender, unity_state_receiver) = watch::channel(UnityState::Idle);
        let (unity_event_sender, unity_event_receiver) = mpsc::channel(100);

        let secret = use_secret.then(generate_secret);

        let ip = local_ip().expect("Get IPv4 address").to_string();

        let secret_str = secret.as_ref().map(|s| s.as_str()).unwrap_or("None");
        let qr_payload = format!(r#"{{ "ip": "{ip}", port: {port}, secret: "{secret_str}" }}"#);

        println!("Starting server on port {port}");
        println!("Connect glasses using QR code:");
        qr2term::print_qr(qr_payload).unwrap();

        let runtime = Arc::new(Runtime::new().unwrap());
        let cancellation_token = CancellationToken::new();
        runtime.spawn({
            let secret = secret.clone();
            let cancellation_token = cancellation_token.clone();

            async move {
                // runtime.block_on(async {
                let task = async {
                    let addr = format!("0.0.0.0:{port}");
                    let listener = TcpListener::bind(&addr).await.unwrap();

                    let http_server = http_server_task(
                        listener,
                        unity_state_receiver,
                        unity_event_sender,
                        secret,
                    );

                    http_server.await;
                };

                select! {
                    _ = task => (),
                    _ = cancellation_token.cancelled() => (),
                }
            }
        });

        NeuroViz {
            runtime,
            cancellation_token,
            unity_state_sender,
            unity_event_receiver,
            ip,
            port,
            secret,
        }
    }

    fn set_live_parameters<'py>(&mut self, parameters: Bound<'py, PyDict>) -> PyResult<()> {
        let parameters = dict_to_parameters(parameters)?;

        self.unity_state_sender
            .send(UnityState::Live { parameters })
            .context("Broadcast live parameters")?;

        Ok(())
    }

    fn prompt_choice<'py>(
        &mut self,
        py: Python<'py>,
        a: Bound<'py, PyDict>,
        b: Bound<'py, PyDict>,
    ) -> PyResult<Bound<'py, PyDict>> {
        let runtime = self.runtime.clone();
        let cancellation_token = self.cancellation_token.clone();

        let parsed_a = dict_to_parameters(a.clone())?;
        let parsed_b = dict_to_parameters(b.clone())?;

        let unity_state_sender = self.unity_state_sender.clone();
        let mut is_preset_a = true;

        let current_preset = move |is_preset_a: bool| match is_preset_a {
            true => parsed_a,
            false => parsed_b,
        };

        let show_presets = |unity_state_sender: &watch::Sender<UnityState>,
                            parameters: ParameterValues|
         -> PyResult<()> {
            unity_state_sender
                .send(UnityState::Experiment {
                    prompt: ExperimentPrompt {
                        experiment_type: UnityExperimentType::Choice,
                        parameters,
                    },
                })
                .context("Broadcast prompt choice")?;

            Ok(())
        };

        show_presets(&unity_state_sender, current_preset(is_preset_a))?;

        let task = async move {
            while let Some(event) = self.unity_event_receiver.recv().await {
                match event {
                    UnityEvent::Connection { .. } => {}
                    UnityEvent::SwapPreset => {
                        is_preset_a = !is_preset_a;

                        show_presets(&unity_state_sender, current_preset(is_preset_a))?;
                    }
                    UnityEvent::Answer { .. } => {
                        unity_state_sender.send(UnityState::Idle)?;

                        return Ok(match is_preset_a {
                            true => a,
                            false => b,
                        });
                    }
                }
            }

            bail!("Unity event receiver closed unexpectedly");
        };

        let signal = async {
            loop {
                if let Err(error) = py.check_signals() {
                    return error;
                }

                sleep(Duration::from_millis(100)).await;
            }
        };

        let chosen = runtime.block_on(async {
            select! {
                result = task => result.map_err(|e| e.into()),
                err = signal => Err(err),
                _ = cancellation_token.cancelled() => Err(anyhow!("Cancelled").into()),
            }
        })?;

        Ok(chosen)
    }
}

impl Drop for NeuroViz {
    fn drop(&mut self) {
        self.cancellation_token.cancel();
    }
}

// #[pyclass]
#[derive(FromPyObject)]
pub struct ParsedParameterDict {
    pub transparency: f32,
    pub see_through: f32,
    pub outline: f32,
    pub smoothness: f32,
}

#[pyclass]
pub struct ParameterDict {
    pub transparency: f32,
    pub see_through: f32,
    pub outline: f32,
    pub smoothness: f32,
}

impl From<ParsedParameterDict> for ParameterValues {
    fn from(value: ParsedParameterDict) -> Self {
        ParameterValues {
            transparency: value.transparency,
            see_through: value.see_through,
            outline: value.outline,
            smoothness: value.smoothness,
        }
    }
}

#[pymodule]
fn neuroviz(m: &Bound<'_, PyModule>) -> PyResult<()> {
    m.add_class::<NeuroViz>()?;
    m.add_class::<ParameterDict>()?;
    Ok(())
}
