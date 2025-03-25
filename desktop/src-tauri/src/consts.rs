use core::fmt;
pub const HTTP_SERVER_PORT: u16 = 9001;

// pub const FOLDER_PRESETS: &str = "presets";
// pub const FOLDER_EXPERIMENTS: &str = "experiments";
// pub const FOLDER_RESULTS: &str = "results";

pub enum Folder {
    Presets,
    Experiments,
    Results
}

impl Folder {
    pub fn as_str(&self) -> &'static str {
        match self {
            Folder::Presets => "presets",
            Folder::Experiments => "experiments",
            Folder::Results => "results"
        }
    }
}

impl fmt::Display for Folder {
    fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {
        write!(f, "{}", self.as_str())
    }
}
