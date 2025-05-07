use std::{sync::Arc, time::Duration};

use ::neuroviz::{
    generate_secret,
    http_server::{
        ExperimentAnswer, ExperimentPrompt, HttpServer, UnityEvent, UnityExperimentType, UnityState,
    },
    parameters::{ParameterKey, ParameterValues},
};
use anyhow::{Context, anyhow, bail};
use local_ip_address::local_ip;
use pyo3::{prelude::*, types::PyDict};
use strum::IntoEnumIterator;
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
) -> PyResult<()> {
    let http_server = HttpServer {
        state: unity_state_receiver,
        event_sender: unity_event_sender,
        secret: secret.map(Arc::new),
    };

    let app = http_server.app();
    axum::serve(listener, app).await?;
    Ok(())
}

fn dict_to_parameters<'py>(dict: Bound<'py, PyDict>) -> PyResult<ParameterValues> {
    let extract =
        |key: &str| -> PyResult<f32> { dict.get_item(key)?.context("required")?.extract() };

    let transparency = extract("transparency")?;
    let glow = extract("glow")?;
    let smoothness = extract("smoothness")?;
    let emission = extract("emission")?;
    let light_intensity = extract("light_intensity")?;
    let light_temperature = extract("light_temperature")?;

    Ok(ParameterValues {
        transparency,
        glow,
        smoothness,
        emission,
        light_intensity,
        light_temperature,
    })
}

fn parameters_to_dict<'py>(
    parameters: ParameterValues,
    py: Python<'py>,
) -> PyResult<Bound<'py, PyDict>> {
    let dict = PyDict::new(py);

    for parameter_key in ParameterKey::iter() {
        dict.set_item(parameter_key.to_string(), parameters.get(parameter_key))?;
    }

    Ok(dict)
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

#[pymethods]
impl NeuroViz {
    #[new]
    fn new(port: u16, use_secret: bool) -> PyResult<Self> {
        let (unity_state_sender, unity_state_receiver) = watch::channel(UnityState::Idle);
        let (unity_event_sender, unity_event_receiver) = mpsc::channel(100);

        let secret = use_secret.then(generate_secret);

        let ip = local_ip().expect("Get IPv4 address").to_string();

        let secret_str = secret.as_ref().map(|s| s.as_str()).unwrap_or("None");
        let qr_payload = format!(r#"{{ "ip": "{ip}", "port": {port}, "secret": "{secret_str}" }}"#);

        println!("Starting server on port {port}");
        println!("Connect glasses using QR code:");
        qr2term::print_qr(qr_payload).context("Print QR code")?;

        let runtime = Arc::new(Runtime::new().context("Create runtime")?);
        let cancellation_token = CancellationToken::new();

        let listener = runtime.block_on(async {
            let addr = format!("0.0.0.0:{port}");
            let listener = TcpListener::bind(&addr)
                .await
                .context("Bind TCP listener")?;

            PyResult::Ok(listener)
        })?;

        runtime.spawn({
            let secret = secret.clone();
            let cancellation_token = cancellation_token.clone();

            async move {
                // runtime.block_on(async {
                let task = async {
                    let http_server = http_server_task(
                        listener,
                        unity_state_receiver,
                        unity_event_sender,
                        secret,
                    );

                    http_server.await?;

                    PyResult::Ok(())
                };

                select! {
                    _ = task => (),
                    _ = cancellation_token.cancelled() => (),
                }
            }
        });

        Ok(NeuroViz {
            runtime,
            cancellation_token,
            unity_state_sender,
            unity_event_receiver,
            ip,
            port,
            secret,
        })
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
                    UnityEvent::SwapPreset => {
                        is_preset_a = !is_preset_a;

                        show_presets(&unity_state_sender, current_preset(is_preset_a))?;
                    }

                    UnityEvent::Answer(ExperimentAnswer::Choice) => {
                        unity_state_sender.send(UnityState::Idle)?;

                        return Ok(match is_preset_a {
                            true => a,
                            false => b,
                        });
                    }

                    _ => {}
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

    fn prompt_rating<'py>(
        &mut self,
        py: Python<'py>,
        parameters: Bound<'py, PyDict>,
    ) -> PyResult<u8> {
        let runtime = self.runtime.clone();
        let cancellation_token = self.cancellation_token.clone();

        let parsed_parameters = dict_to_parameters(parameters)?;

        let unity_state_sender = self.unity_state_sender.clone();

        unity_state_sender
            .send(UnityState::Experiment {
                prompt: ExperimentPrompt {
                    experiment_type: UnityExperimentType::Rating,
                    parameters: parsed_parameters,
                },
            })
            .context("Broadcast prompt choice")?;

        let task = async move {
            while let Some(event) = self.unity_event_receiver.recv().await {
                match event {
                    UnityEvent::Answer(ExperimentAnswer::Rating { value }) => {
                        unity_state_sender.send(UnityState::Idle)?;

                        return Ok(value);
                    }

                    _ => {}
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

    fn set_idle(&mut self) -> PyResult<()> {
        self.unity_state_sender
            .send(UnityState::Idle)
            .context("Send idle state")?;

        Ok(())
    }
}

impl Drop for NeuroViz {
    fn drop(&mut self) {
        self.cancellation_token.cancel();
    }
}

#[derive(FromPyObject)]
pub struct ParsedParameterDict {
    pub transparency: f32,
    pub glow: f32,
    pub smoothness: f32,
    pub emission: f32,
    pub light_intensity: f32,
    pub light_temperature: f32,
}

#[pyclass]
pub struct ParameterDict {
    pub transparency: f32,
    pub glow: f32,
    pub smoothness: f32,
    pub emission: f32,
    pub light_intensity: f32,
    pub light_temperature: f32,
}

#[pyfunction]
fn default_parameters<'py>(py: Python<'py>) -> PyResult<Bound<'py, PyDict>> {
    let parameters = ParameterValues::default();

    parameters_to_dict(parameters, py)
}

#[pymodule]
fn neuroviz(m: &Bound<'_, PyModule>) -> PyResult<()> {
    m.add_class::<NeuroViz>()?;
    m.add_class::<ParameterDict>()?;
    m.add_function(wrap_pyfunction!(default_parameters, m)?)?;
    Ok(())
}
