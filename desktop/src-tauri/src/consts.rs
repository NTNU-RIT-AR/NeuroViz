use core::fmt;
pub const HTTP_SERVER_PORT: u16 = 9001;

// pub const FOLDER_PRESETS: &str = "presets";
// pub const FOLDER_EXPERIMENTS: &str = "experiments";
// pub const FOLDER_RESULTS: &str = "results";

pub enum Folder {
    Presets,
    Experiments,
    Results,
}

impl Folder {
    pub fn as_str(&self) -> &'static str {
        match self {
            Folder::Presets => "presets",
            Folder::Experiments => "experiments",
            Folder::Results => "results",
        }
    }
}
impl From<&str> for Folder {
    fn from(s: &str) -> Self {
        match s.to_lowercase().as_str() {
            "presets" => Folder::Presets,
            "experiments" => Folder::Experiments,
            "results" => Folder::Results,
            _ => panic!("Folder enum not defined"),
        }
    }
}

impl fmt::Display for Folder {
    fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {
        write!(f, "{}", self.as_str())
    }
}
