use std::collections::{HashMap, HashSet};

use crate::{CliError, CommandArgs, CommandInfo};

pub struct CommandRegistry {
    commands: HashMap<String, CommandInfo>,
    max_depth: usize,
}

impl CommandRegistry {
    pub fn new() -> Self {
        CommandRegistry {
            commands: HashMap::new(),
            max_depth: 0,
        }
    }

    pub fn register(&mut self, info: CommandInfo) -> &mut Self {
        if info.path.len() > self.max_depth {
            self.max_depth = info.path.len();
        }
        self.commands.insert(info.key.clone(), info);
        self
    }

    /// Greedy longest-path match. Strips matched path segments from positionals.
    pub fn resolve(&self, args: &CommandArgs) -> Result<(&CommandInfo, CommandArgs), CliError> {
        let depth = args.positionals.len().min(self.max_depth);

        for d in (1..=depth).rev() {
            let key = args.positionals[..d].join(".");
            if let Some(info) = self.commands.get(&key) {
                let remaining = CommandArgs {
                    positionals: args.positionals[d..].to_vec(),
                    optionals: args.optionals.clone(),
                };
                return Ok((info, remaining));
            }
        }

        let attempted = args.positionals.first().map_or("(empty)", |s| s.as_str());
        Err(CliError::UnknownCommand(attempted.to_string()))
    }

    pub fn find(&self, path: &[&str]) -> Option<&CommandInfo> {
        self.commands.get(&path.join("."))
    }

    pub fn get_all(&self) -> Vec<&CommandInfo> {
        self.commands.values().collect()
    }

    pub fn get_roots(&self) -> Vec<&str> {
        let mut roots: HashSet<&str> = HashSet::new();
        for info in self.commands.values() {
            if let Some(first) = info.path.first() {
                roots.insert(first.as_str());
            }
        }
        roots.into_iter().collect()
    }

    pub fn get_help(&self) -> String {
        let mut sorted: Vec<&CommandInfo> = self.commands.values().collect();
        sorted.sort_by(|a, b| a.key.cmp(&b.key));

        let mut help = String::new();
        for info in sorted {
            help.push_str(&format!("  {:<30}", info.key));
            if !info.description.is_empty() {
                help.push_str(&info.description);
            }
            help.push('\n');
        }
        help
    }

    pub fn get_help_for(&self, prefix: &[&str]) -> String {
        let prefix_key = prefix.join(".");
        let mut matched: Vec<&CommandInfo> = self
            .commands
            .values()
            .filter(|c| c.key.starts_with(&prefix_key))
            .collect();
        matched.sort_by(|a, b| a.key.cmp(&b.key));

        let mut help = String::new();
        for info in matched {
            let relative = if info.key.len() > prefix_key.len() {
                &info.key[prefix_key.len() + 1..]
            } else {
                &info.key
            };
            help.push_str(&format!("  {:<30}", relative));
            if !info.description.is_empty() {
                help.push_str(&info.description);
            }
            help.push('\n');
        }
        help
    }
}

impl Default for CommandRegistry {
    fn default() -> Self {
        Self::new()
    }
}
