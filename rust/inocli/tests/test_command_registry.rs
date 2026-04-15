use inocli::{ArgParser, CliError, CommandArgs, CommandInfo, CommandRegistry};

// ================================================================
// Test handler functions
// ================================================================

fn handle_status(_args: &CommandArgs) {}
fn handle_run_fast(_args: &CommandArgs) {}
fn handle_run_slow(_args: &CommandArgs) {}
fn handle_config_set_value(_args: &CommandArgs) {}

fn make_registry() -> CommandRegistry {
    let mut registry = CommandRegistry::new();
    registry
        .register(CommandInfo::new(
            vec!["status".to_string()],
            "Show status",
            handle_status,
        ))
        .register(CommandInfo::new(
            vec!["run".to_string(), "fast".to_string()],
            "Run fast mode",
            handle_run_fast,
        ))
        .register(CommandInfo::new(
            vec!["run".to_string(), "slow".to_string()],
            "Run slow mode",
            handle_run_slow,
        ))
        .register(CommandInfo::new(
            vec!["config".to_string(), "set".to_string(), "value".to_string()],
            "Set config value",
            handle_config_set_value,
        ));
    registry
}

fn make_args(positionals: &[&str]) -> CommandArgs {
    let mut args = CommandArgs::new();
    args.positionals = positionals.iter().map(|s| s.to_string()).collect();
    args
}

// ================================================================
// CommandRegistry — Resolve
// ================================================================

#[test]
fn resolve_single_segment() {
    let registry = make_registry();
    let parsed = make_args(&["status"]);
    let (info, args) = registry.resolve(&parsed).unwrap();
    assert_eq!(info.key, "status");
    assert!(args.positionals.is_empty());
}

#[test]
fn resolve_two_segments_with_remaining_args() {
    let registry = make_registry();
    let parsed = make_args(&["run", "fast", "input.txt"]);
    let (info, args) = registry.resolve(&parsed).unwrap();
    assert_eq!(info.key, "run.fast");
    assert_eq!(args.positionals.len(), 1);
    assert_eq!(args.get(0), Some("input.txt"));
}

#[test]
fn resolve_three_segments() {
    let registry = make_registry();
    let parsed = make_args(&["config", "set", "value", "42"]);
    let (info, args) = registry.resolve(&parsed).unwrap();
    assert_eq!(info.key, "config.set.value");
    assert_eq!(args.positionals.len(), 1);
    assert_eq!(args.get(0), Some("42"));
}

#[test]
fn resolve_greedy_prefers_longest_path() {
    let registry = make_registry();
    let parsed = make_args(&["run", "slow", "data"]);
    let (info, args) = registry.resolve(&parsed).unwrap();
    assert_eq!(info.key, "run.slow");
    assert_eq!(args.get(0), Some("data"));
}

#[test]
fn resolve_preserves_optionals() {
    let registry = make_registry();
    let mut parsed = make_args(&["status"]);
    parsed.optionals.insert("verbose".to_string(), vec![]);
    parsed.optionals.insert("count".to_string(), vec!["5".to_string()]);

    let (_, args) = registry.resolve(&parsed).unwrap();
    assert!(args.flag("verbose"));
    assert_eq!(args.opt("count"), Some("5"));
}

#[test]
fn resolve_unknown_command_errors() {
    let registry = make_registry();
    let parsed = make_args(&["nonexistent"]);
    assert!(matches!(
        registry.resolve(&parsed),
        Err(CliError::UnknownCommand(_))
    ));
}

#[test]
fn resolve_empty_positionals_errors() {
    let registry = make_registry();
    let parsed = make_args(&[]);
    assert!(matches!(
        registry.resolve(&parsed),
        Err(CliError::UnknownCommand(_))
    ));
}

// ================================================================
// CommandRegistry — Query
// ================================================================

#[test]
fn find_existing_command() {
    let registry = make_registry();
    let info = registry.find(&["run", "fast"]).unwrap();
    assert_eq!(info.description, "Run fast mode");
}

#[test]
fn find_missing_returns_none() {
    let registry = make_registry();
    assert!(registry.find(&["nonexistent"]).is_none());
}

#[test]
fn get_roots_returns_distinct_roots() {
    let registry = make_registry();
    let mut roots = registry.get_roots();
    roots.sort();
    assert_eq!(roots, vec!["config", "run", "status"]);
}

#[test]
fn get_all_returns_all_commands() {
    let registry = make_registry();
    assert_eq!(registry.get_all().len(), 4);
}

// ================================================================
// CommandRegistry — Help
// ================================================================

#[test]
fn get_help_lists_all_commands() {
    let registry = make_registry();
    let help = registry.get_help();
    assert!(help.contains("status"));
    assert!(help.contains("run.fast"));
    assert!(help.contains("run.slow"));
    assert!(help.contains("config.set.value"));
}

#[test]
fn get_help_includes_descriptions() {
    let registry = make_registry();
    let help = registry.get_help();
    assert!(help.contains("Show status"));
    assert!(help.contains("Run fast mode"));
}

#[test]
fn get_help_for_filters_by_prefix() {
    let registry = make_registry();
    let help = registry.get_help_for(&["run"]);
    assert!(help.contains("fast"));
    assert!(help.contains("slow"));
    assert!(!help.contains("status"));
    assert!(!help.contains("config"));
}

// ================================================================
// Integration: parse then resolve
// ================================================================

#[test]
fn parse_then_resolve_two_segments() {
    let registry = make_registry();
    let parsed = ArgParser::new().parse(&["run", "fast", "input.txt"]).unwrap();
    let (info, args) = registry.resolve(&parsed).unwrap();
    assert_eq!(info.key, "run.fast");
    assert_eq!(args.get(0), Some("input.txt"));
}

#[test]
fn parse_then_resolve_with_options() {
    let registry = make_registry();
    let parsed = ArgParser::new()
        .parse(&["status", "--verbose", "--count", "5"])
        .unwrap();
    let (info, args) = registry.resolve(&parsed).unwrap();
    assert_eq!(info.key, "status");
    assert!(args.flag("verbose"));
    assert_eq!(args.get_int_opt_or("count", 0), 5);
}
