use std::collections::HashMap;

use crate::CliError;

#[derive(Debug, Clone, Default)]
pub struct CommandArgs {
    pub positionals: Vec<String>,
    pub optionals: HashMap<String, Vec<String>>,
}

impl CommandArgs {
    pub fn new() -> Self {
        Self::default()
    }

    // ----------------------------------------------------------------
    // Positional helpers
    // ----------------------------------------------------------------

    pub fn count(&self) -> usize {
        self.positionals.len()
    }

    pub fn has(&self, index: usize) -> bool {
        index < self.positionals.len()
    }

    pub fn get(&self, index: usize) -> Option<&str> {
        self.positionals.get(index).map(|s| s.as_str())
    }

    pub fn get_or<'a>(&'a self, index: usize, fallback: &'a str) -> &'a str {
        self.get(index).unwrap_or(fallback)
    }

    pub fn get_int(&self, index: usize) -> Result<i64, CliError> {
        let s = self.get(index).ok_or(CliError::MissingPositional(index))?;
        s.parse::<i64>().map_err(|_| CliError::ParseError {
            key: index.to_string(),
            value: s.to_string(),
            expected: "int",
        })
    }

    pub fn get_int_or(&self, index: usize, fallback: i64) -> i64 {
        self.get(index)
            .and_then(|s| s.parse::<i64>().ok())
            .unwrap_or(fallback)
    }

    pub fn get_f32(&self, index: usize) -> Result<f32, CliError> {
        let s = self.get(index).ok_or(CliError::MissingPositional(index))?;
        s.parse::<f32>().map_err(|_| CliError::ParseError {
            key: index.to_string(),
            value: s.to_string(),
            expected: "f32",
        })
    }

    pub fn get_f32_or(&self, index: usize, fallback: f32) -> f32 {
        self.get(index)
            .and_then(|s| s.parse::<f32>().ok())
            .unwrap_or(fallback)
    }

    pub fn get_f64(&self, index: usize) -> Result<f64, CliError> {
        let s = self.get(index).ok_or(CliError::MissingPositional(index))?;
        s.parse::<f64>().map_err(|_| CliError::ParseError {
            key: index.to_string(),
            value: s.to_string(),
            expected: "f64",
        })
    }

    pub fn get_f64_or(&self, index: usize, fallback: f64) -> f64 {
        self.get(index)
            .and_then(|s| s.parse::<f64>().ok())
            .unwrap_or(fallback)
    }

    pub fn get_bool(&self, index: usize) -> Result<bool, CliError> {
        let s = self.get(index).ok_or(CliError::MissingPositional(index))?;
        parse_bool(s).ok_or_else(|| CliError::ParseError {
            key: index.to_string(),
            value: s.to_string(),
            expected: "bool",
        })
    }

    pub fn get_bool_or(&self, index: usize, fallback: bool) -> bool {
        self.get(index).and_then(parse_bool).unwrap_or(fallback)
    }

    /// Returns positionals from `index` onward. Empty slice if out of range.
    pub fn from_index(&self, index: usize) -> &[String] {
        if index >= self.positionals.len() {
            &[]
        } else {
            &self.positionals[index..]
        }
    }

    // ----------------------------------------------------------------
    // Optional helpers
    // ----------------------------------------------------------------

    pub fn has_opt(&self, key: &str) -> bool {
        self.optionals.contains_key(key)
    }

    /// Returns true if the optional key exists (flag or valued).
    pub fn flag(&self, key: &str) -> bool {
        self.optionals.contains_key(key)
    }

    pub fn opt(&self, key: &str) -> Option<&str> {
        self.optionals
            .get(key)
            .and_then(|v| v.first())
            .map(|s| s.as_str())
    }

    pub fn opt_or<'a>(&'a self, key: &str, fallback: &'a str) -> &'a str {
        self.opt(key).unwrap_or(fallback)
    }

    pub fn get_int_opt(&self, key: &str) -> Result<i64, CliError> {
        let s = self
            .opt(key)
            .ok_or_else(|| CliError::MissingOption(key.to_string()))?;
        s.parse::<i64>().map_err(|_| CliError::ParseError {
            key: key.to_string(),
            value: s.to_string(),
            expected: "int",
        })
    }

    pub fn get_int_opt_or(&self, key: &str, fallback: i64) -> i64 {
        self.opt(key)
            .and_then(|s| s.parse::<i64>().ok())
            .unwrap_or(fallback)
    }

    pub fn get_f32_opt(&self, key: &str) -> Result<f32, CliError> {
        let s = self
            .opt(key)
            .ok_or_else(|| CliError::MissingOption(key.to_string()))?;
        s.parse::<f32>().map_err(|_| CliError::ParseError {
            key: key.to_string(),
            value: s.to_string(),
            expected: "f32",
        })
    }

    pub fn get_f32_opt_or(&self, key: &str, fallback: f32) -> f32 {
        self.opt(key)
            .and_then(|s| s.parse::<f32>().ok())
            .unwrap_or(fallback)
    }

    pub fn get_f64_opt(&self, key: &str) -> Result<f64, CliError> {
        let s = self
            .opt(key)
            .ok_or_else(|| CliError::MissingOption(key.to_string()))?;
        s.parse::<f64>().map_err(|_| CliError::ParseError {
            key: key.to_string(),
            value: s.to_string(),
            expected: "f64",
        })
    }

    pub fn get_f64_opt_or(&self, key: &str, fallback: f64) -> f64 {
        self.opt(key)
            .and_then(|s| s.parse::<f64>().ok())
            .unwrap_or(fallback)
    }

    pub fn get_bool_opt(&self, key: &str) -> Result<bool, CliError> {
        let s = self
            .opt(key)
            .ok_or_else(|| CliError::MissingOption(key.to_string()))?;
        parse_bool(s).ok_or_else(|| CliError::ParseError {
            key: key.to_string(),
            value: s.to_string(),
            expected: "bool",
        })
    }

    pub fn get_bool_opt_or(&self, key: &str, fallback: bool) -> bool {
        self.opt(key).and_then(parse_bool).unwrap_or(fallback)
    }

    pub fn all(&self, key: &str) -> Result<&[String], CliError> {
        self.optionals
            .get(key)
            .map(|v| v.as_slice())
            .ok_or_else(|| CliError::MissingOption(key.to_string()))
    }

    pub fn all_or<'a>(&'a self, key: &str, fallback: &'a [String]) -> &'a [String] {
        self.optionals
            .get(key)
            .map(|v| v.as_slice())
            .unwrap_or(fallback)
    }
}

// ----------------------------------------------------------------
// Internal helpers
// ----------------------------------------------------------------

/// Case-insensitive bool parse matching C#'s bool.TryParse behaviour.
pub(crate) fn parse_bool(s: &str) -> Option<bool> {
    match s.to_ascii_lowercase().as_str() {
        "true" => Some(true),
        "false" => Some(false),
        _ => None,
    }
}
