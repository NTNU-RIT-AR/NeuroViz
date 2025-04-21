use min_tauri_app_lib::{
    data::{experiment::Experiment, experiment_result::ExperimentResult, preset::Preset},
    storage::{self, Folder},
};

/// Checks that all the files in the data folder for development is valid
#[tokio::test]
pub async fn test_data() {
    let presets = storage::read_files::<Preset>(Folder::Presets)
        .await
        .unwrap();
    assert!(!presets.is_empty());

    let experiments = storage::read_files::<Experiment>(Folder::Experiments)
        .await
        .unwrap();
    assert!(!experiments.is_empty());

    let results = storage::read_files::<ExperimentResult>(Folder::Results {
        experiment_key: "example-1".to_owned(),
    })
    .await
    .unwrap();
    assert!(!results.is_empty());
}
