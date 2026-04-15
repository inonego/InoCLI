# InoCLI — Canonical Feature Contract

This file is the source of truth for both language implementations.
When working on either `csharp/` or `rust/`, this contract defines
what each implementation must support.

## Feature Set

Both implementations must support the following capabilities:

### 1. Argument Parsing

- **Positionals** — bare values that are not prefixed with `-` or `--`
  - Exception: negative numbers (e.g. `-1`, `-3.14`) stay as positionals
- **Optionals (short)** — `-key` format; value follows as next arg if the next arg is not itself an option
- **Optionals (long)** — `--key` format; same value rule
- **Flags** — optional keys with no value (empty list); `--verbose` with no next arg is a flag
- **Multi-value** — the same key can appear multiple times; all values accumulate
- **Stdin shorthand** — bare `-` as a value reads from stdin (only when stdin is redirected)
- **Empty `--`** — throws/errors; `--` with no key name is invalid

### 2. Type Coercion on `CommandArgs`

For both positional (by index) and optional (by key) access:

| Method | Return | On missing | On invalid |
|--------|--------|------------|------------|
| `Get(idx/key)` | `string?` / `Option<&str>` | `null` / `None` | — |
| `Get(idx/key, fallback)` | `string` | `fallback` | — |
| `GetInt` / `get_int` | `int` / `i64` | throw / `Err` | throw / `Err` |
| `GetInt(fallback)` / `get_int_or` | `int` / `i64` | `fallback` | `fallback` |
| `GetLong` / (covered by `get_int` in Rust using `i64`) | `long` | throw | throw |
| `GetFloat` / `get_float` | `float` / `f32` | throw / `Err` | throw / `Err` |
| `GetDouble` / `get_f64` | `double` / `f64` | throw / `Err` | throw / `Err` |
| `GetBool` / `get_bool` | `bool` | throw / `Err` | throw / `Err` |
| `Flag(key)` / `flag(key)` | `bool` | `false` | — |
| `All(key)` / `all(key)` | `List<string>` / `&[String]` | throw / `Err` | — |
| `From(idx)` / `from(idx)` | `string[]` / `&[String]` | empty | — |

### 3. Command Registration

- Commands are registered via **attribute/macro** on **public static** functions/fns
- Each command has a **path** — one or more string segments (e.g. `["scene", "load"]`)
- Each command has an optional **description** string
- Commands are identified by their dot-joined key (e.g. `"scene.load"`)

### 4. Command Routing

- **Greedy longest-path match** — tries longest prefix first, shortens until a match is found
- If no match: error with the first positional as the unknown command name
- On match: returns the `CommandInfo` and a new `CommandArgs` with path segments stripped

### 5. Help Generation

- `GetHelp()` / `get_help()` — lists all commands, sorted by key, padded for alignment
- `GetHelp(prefix)` / `get_help_for(prefix)` — lists commands under a path prefix, relative keys

---

## Language-Specific Notes

### C# (`csharp/`)

- **Toolchain:** `dotnet test csharp/`
- **Targets:** `netstandard2.1`, `net8.0`
- **Error handling:** throws `ArgumentException` on invalid/missing values
- **Command discovery:** reflection-based via `CommandScanner` scanning assemblies at runtime
- **Package:** NuGet (`InoCLI`)

### Rust (`rust/`)

- **Toolchain:** `cargo test` from `rust/`
- **Error handling:** returns `Result<T, CliError>` — never panics on user input
- **Command registration:** attribute proc macro `#[cli_command("path", "segments")]` via `inocli-macros` crate; collected at startup using the `inventory` crate
- **Numeric types:** `i64` covers both `int` and `long`; `f32` = float, `f64` = double
- **Package:** crates.io (`inocli` + `inocli-macros`)

---

## Adding Features

When you add a feature to one implementation:
1. Update this file's Feature Set table
2. Add the equivalent to the other implementation
3. Add tests in both `csharp/test/` and `rust/inocli/tests/`
