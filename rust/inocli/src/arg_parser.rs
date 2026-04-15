use std::io::{IsTerminal, Read};

use crate::{CliError, CommandArgs};

pub struct ArgParser {
    stdin_cache: Option<String>,
}

impl ArgParser {
    pub fn new() -> Self {
        ArgParser { stdin_cache: None }
    }

    /// Parses a slice of argument strings into a `CommandArgs`.
    /// Returns `Err` only if `--` (empty key) is encountered.
    pub fn parse(&mut self, args: &[&str]) -> Result<CommandArgs, CliError> {
        // Pre-read stdin if it's redirected (not a terminal).
        if !std::io::stdin().is_terminal() {
            let mut buf = String::new();
            let _ = std::io::stdin().read_to_string(&mut buf);
            self.stdin_cache = Some(buf.trim().to_string());
        }

        let mut result = CommandArgs::new();
        let mut i = 0;

        while i < args.len() {
            let arg = args[i];

            match Self::try_parse_optional(arg)? {
                Some(key) => {
                    let entry = result.optionals.entry(key.to_string()).or_default();
                    // Consume next arg as value if it is not itself an option.
                    if i + 1 < args.len() {
                        match Self::try_parse_optional(args[i + 1]) {
                            Ok(None) => {
                                i += 1;
                                entry.push(Self::resolve(args[i], &self.stdin_cache));
                            }
                            _ => {} // flag: next arg is another option or invalid — no value consumed
                        }
                    }
                }
                None => {
                    result.positionals.push(Self::resolve(arg, &self.stdin_cache));
                }
            }

            i += 1;
        }

        Ok(result)
    }

    fn try_parse_optional(arg: &str) -> Result<Option<&str>, CliError> {
        if let Some(key) = arg.strip_prefix("--") {
            if key.is_empty() {
                return Err(CliError::InvalidOption("-- (empty key)".to_string()));
            }
            return Ok(Some(key));
        }
        if let Some(key) = arg.strip_prefix('-') {
            // Negative numbers (e.g. "-42", "-3.14") stay as positionals.
            if key.is_empty() || key.chars().next().is_some_and(|c| c.is_ascii_digit()) {
                return Ok(None);
            }
            return Ok(Some(key));
        }
        Ok(None)
    }

    fn resolve(arg: &str, stdin_cache: &Option<String>) -> String {
        if arg == "-" {
            stdin_cache.clone().unwrap_or_else(|| "-".to_string())
        } else {
            arg.to_string()
        }
    }
}

impl Default for ArgParser {
    fn default() -> Self {
        Self::new()
    }
}
