use inocli::{ArgParser, CliError};

fn parse(args: &[&str]) -> inocli::CommandArgs {
    ArgParser::new().parse(args).unwrap()
}

// ================================================================
// Positionals
// ================================================================

#[test]
fn parse_single_positional() {
    let result = parse(&["ping"]);
    assert_eq!(result.positionals.len(), 1);
    assert_eq!(result.get(0), Some("ping"));
}

#[test]
fn parse_multiple_positionals() {
    let result = parse(&["build", "src/main.rs", "42"]);
    assert_eq!(result.count(), 3);
    assert_eq!(result.get(0), Some("build"));
    assert_eq!(result.get(1), Some("src/main.rs"));
    assert_eq!(result.get(2), Some("42"));
}

#[test]
fn parse_negative_number_is_positional() {
    let result = parse(&["calc", "-42"]);
    assert_eq!(result.count(), 2);
    assert_eq!(result.get(1), Some("-42"));
}

// ================================================================
// Long options
// ================================================================

#[test]
fn parse_long_option_with_value() {
    let result = parse(&["deploy", "--retries", "3"]);
    assert!(result.has_opt("retries"));
    assert_eq!(result.opt("retries"), Some("3"));
}

#[test]
fn parse_long_option_flag() {
    let result = parse(&["status", "--verbose"]);
    assert!(result.has_opt("verbose"));
    assert!(result.optionals["verbose"].is_empty());
}

#[test]
fn parse_long_option_repeated() {
    let result = parse(&["build", "src/main.rs", "--tag", "release", "--tag", "latest"]);
    let tags = &result.optionals["tag"];
    assert_eq!(tags.len(), 2);
    assert_eq!(tags[0], "release");
    assert_eq!(tags[1], "latest");
}

// ================================================================
// Short options
// ================================================================

#[test]
fn parse_short_option_with_value() {
    let result = parse(&["deploy", "-r", "3"]);
    assert!(result.has_opt("r"));
    assert_eq!(result.opt("r"), Some("3"));
}

#[test]
fn parse_short_option_flag() {
    let result = parse(&["status", "-v"]);
    assert!(result.has_opt("v"));
    assert!(result.optionals["v"].is_empty());
}

// ================================================================
// Mixed
// ================================================================

#[test]
fn parse_options_between_positionals() {
    let result = parse(&["build", "src/main.rs", "--filter", "x > 0", "42"]);
    assert_eq!(result.get(0), Some("build"));
    assert_eq!(result.get(1), Some("src/main.rs"));
    assert_eq!(result.get(2), Some("42"));
    assert_eq!(result.opt("filter"), Some("x > 0"));
}

#[test]
fn parse_empty() {
    let result = parse(&[]);
    assert!(result.positionals.is_empty());
    assert!(result.optionals.is_empty());
}

// ================================================================
// Error
// ================================================================

#[test]
fn parse_double_dash_errors() {
    let err = ArgParser::new().parse(&["--"]).unwrap_err();
    assert!(matches!(err, CliError::InvalidOption(_)));
}

// ================================================================
// Indexer / get
// ================================================================

#[test]
fn get_positional_returns_value() {
    let result = parse(&["ping"]);
    assert_eq!(result.get(0), Some("ping"));
}

#[test]
fn get_positional_out_of_range_returns_none() {
    let result = parse(&["ping"]);
    assert_eq!(result.get(5), None);
}

#[test]
fn opt_returns_first_value() {
    let result = parse(&["--retries", "3"]);
    assert_eq!(result.opt("retries"), Some("3"));
}

#[test]
fn opt_missing_returns_none() {
    let result = parse(&["ping"]);
    assert_eq!(result.opt("missing"), None);
}

// ================================================================
// Has
// ================================================================

#[test]
fn has_positional_exists() {
    let result = parse(&["ping"]);
    assert!(result.has(0));
}

#[test]
fn has_positional_missing() {
    let result = parse(&["ping"]);
    assert!(!result.has(1));
}

#[test]
fn has_opt_exists() {
    let result = parse(&["--verbose"]);
    assert!(result.has_opt("verbose"));
}

#[test]
fn has_opt_missing() {
    let result = parse(&["ping"]);
    assert!(!result.has_opt("missing"));
}

// ================================================================
// get_int
// ================================================================

#[test]
fn get_int_positional() {
    let result = parse(&["connect", "8080"]);
    assert_eq!(result.get_int(1).unwrap(), 8080);
}

#[test]
fn get_int_positional_missing_errors() {
    let result = parse(&["ping"]);
    assert!(matches!(result.get_int(5), Err(CliError::MissingPositional(5))));
}

#[test]
fn get_int_optional() {
    let result = parse(&["--retries", "3"]);
    assert_eq!(result.get_int_opt("retries").unwrap(), 3);
}

#[test]
fn get_int_optional_missing_errors() {
    let result = parse(&["ping"]);
    assert!(matches!(result.get_int_opt("retries"), Err(CliError::MissingOption(_))));
}

#[test]
fn get_int_optional_invalid_errors() {
    let result = parse(&["--retries", "abc"]);
    assert!(matches!(result.get_int_opt("retries"), Err(CliError::ParseError { .. })));
}

// ================================================================
// get_f32
// ================================================================

#[test]
fn get_f32_positional() {
    let result = parse(&["resize", "1.5"]);
    assert!((result.get_f32(1).unwrap() - 1.5f32).abs() < f32::EPSILON);
}

#[test]
fn get_f32_optional() {
    let result = parse(&["--ratio", "1.5"]);
    assert!((result.get_f32_opt("ratio").unwrap() - 1.5f32).abs() < f32::EPSILON);
}

// ================================================================
// get_bool
// ================================================================

#[test]
fn get_bool_positional() {
    let result = parse(&["toggle", "true"]);
    assert!(result.get_bool(1).unwrap());
}

#[test]
fn get_bool_optional() {
    let result = parse(&["--verbose", "true"]);
    assert!(result.get_bool_opt("verbose").unwrap());
}

#[test]
fn get_bool_case_insensitive() {
    let result = parse(&["toggle", "True"]);
    assert!(result.get_bool(1).unwrap());
}

// ================================================================
// all
// ================================================================

#[test]
fn all_returns_values() {
    let result = parse(&["--tag", "release", "--tag", "latest"]);
    let values = result.all("tag").unwrap();
    assert_eq!(values.len(), 2);
    assert_eq!(values[0], "release");
    assert_eq!(values[1], "latest");
}

#[test]
fn all_missing_errors() {
    let result = parse(&["ping"]);
    assert!(matches!(result.all("missing"), Err(CliError::MissingOption(_))));
}

// ================================================================
// CommandArgs helpers (mirrors TEST_CommandRegistry helpers section)
// ================================================================

#[test]
fn flag_returns_correctly() {
    let mut args = inocli::CommandArgs::new();
    args.optionals.insert("verbose".to_string(), vec![]);
    assert!(args.flag("verbose"));
    assert!(!args.flag("missing"));
}

#[test]
fn get_int_throws_and_fallback() {
    let mut args = inocli::CommandArgs::new();
    args.positionals = vec!["42".to_string(), "abc".to_string()];
    args.optionals.insert("count".to_string(), vec!["10".to_string()]);

    assert_eq!(args.get_int(0).unwrap(), 42);
    assert!(args.get_int(1).is_err());

    assert_eq!(args.get_int_or(0, 0), 42);
    assert_eq!(args.get_int_or(1, 99), 99);
    assert_eq!(args.get_int_or(5, 99), 99);

    assert_eq!(args.get_int_opt_or("count", 0), 10);
    assert_eq!(args.get_int_opt_or("missing", 99), 99);
}

#[test]
fn get_f32_throws_and_fallback() {
    let mut args = inocli::CommandArgs::new();
    args.positionals = vec!["3.14".to_string()];
    args.optionals.insert("ratio".to_string(), vec!["0.5".to_string()]);

    assert!((args.get_f32(0).unwrap() - 3.14f32).abs() < 0.001);
    assert!((args.get_f32_opt("ratio").unwrap() - 0.5f32).abs() < f32::EPSILON);
    assert!((args.get_f32_or(5, 1.0f32) - 1.0f32).abs() < f32::EPSILON);
    assert!((args.get_f32_opt_or("missing", 1.0f32) - 1.0f32).abs() < f32::EPSILON);
}

#[test]
fn get_f64_throws_and_fallback() {
    let mut args = inocli::CommandArgs::new();
    args.positionals = vec!["3.141592653589793".to_string()];
    args.optionals.insert("precision".to_string(), vec!["0.001".to_string()]);

    assert!((args.get_f64(0).unwrap() - std::f64::consts::PI).abs() < 1e-10);
    assert!((args.get_f64_opt("precision").unwrap() - 0.001).abs() < 1e-10);
    assert_eq!(args.get_f64_or(5, 1.0), 1.0);
    assert_eq!(args.get_f64_opt_or("missing", 1.0), 1.0);
}

#[test]
fn get_bool_throws_and_fallback() {
    let mut args = inocli::CommandArgs::new();
    args.positionals = vec!["true".to_string()];
    args.optionals.insert("enabled".to_string(), vec!["false".to_string()]);

    assert!(args.get_bool(0).unwrap());
    assert!(!args.get_bool_opt("enabled").unwrap());
    assert!(args.get_bool_or(5, true));
    assert!(args.get_bool_opt_or("missing", true));
}

#[test]
fn from_index_returns_slice() {
    let mut args = inocli::CommandArgs::new();
    args.positionals = vec!["a".to_string(), "b".to_string(), "c".to_string(), "d".to_string()];

    assert_eq!(args.from_index(2), &["c".to_string(), "d".to_string()]);
    assert_eq!(args.from_index(0), &["a".to_string(), "b".to_string(), "c".to_string(), "d".to_string()]);
    assert!(args.from_index(10).is_empty());
}

#[test]
fn all_throws_and_fallback() {
    let mut args = inocli::CommandArgs::new();
    args.optionals.insert("tag".to_string(), vec!["a".to_string(), "b".to_string(), "c".to_string()]);

    assert_eq!(args.all("tag").unwrap(), &["a".to_string(), "b".to_string(), "c".to_string()]);
    assert!(matches!(args.all("missing"), Err(CliError::MissingOption(_))));

    let fallback = vec!["default".to_string()];
    assert_eq!(args.all_or("missing", &fallback), fallback.as_slice());
}
