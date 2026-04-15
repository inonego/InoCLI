use std::fmt;

#[derive(Debug, Clone, PartialEq)]
pub enum CliError {
    UnknownCommand(String),
    MissingPositional(usize),
    MissingOption(String),
    ParseError {
        key: String,
        value: String,
        expected: &'static str,
    },
    InvalidOption(String),
}

impl fmt::Display for CliError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            CliError::UnknownCommand(cmd) => write!(f, "Unknown command: {}", cmd),
            CliError::MissingPositional(idx) => write!(f, "Missing positional at index {}", idx),
            CliError::MissingOption(key) => write!(f, "Missing option: --{}", key),
            CliError::ParseError { key, value, expected } => {
                write!(f, "Invalid {} for '{}': {}", expected, key, value)
            }
            CliError::InvalidOption(opt) => write!(f, "Invalid option: {}", opt),
        }
    }
}

impl std::error::Error for CliError {}
