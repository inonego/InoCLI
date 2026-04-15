<p align="center">
  <h1 align="center">InoCLI</h1>
  <p align="center">
    CLI Framework — Parser + Command Registry
  </p>
  <p align="center">
    <a href="https://opensource.org/licenses/MIT"><img src="https://img.shields.io/badge/License-MIT-yellow.svg" alt="License: MIT"></a>
    <img src="https://img.shields.io/badge/.NET-8.0%20%7C%20Standard%202.1-purple?logo=dotnet" alt=".NET 8.0 | Standard 2.1">
    <img src="https://img.shields.io/badge/Rust-2021-orange?logo=rust" alt="Rust 2021">
  </p>
  <p align="center">
    <b>English</b> | <a href="README.ko.md">한국어</a>
  </p>
</p>

---

Lightweight CLI framework — argument parser and command registry — available in C# and Rust.

Both implementations share the same feature set: argument parsing, type coercion helpers,
and hierarchical command routing. See [CLAUDE.md](CLAUDE.md) for the canonical feature contract.

## Implementations

| Language | Location | Test |
|----------|----------|------|
| C# (.NET 8 / Standard 2.1) | [`csharp/`](csharp/) | `dotnet test csharp/` |
| Rust (2021 edition) | [`rust/`](rust/) | `cargo test` in `rust/` |

## Repo Structure

```
InoCLI/
├── CLAUDE.md                     Canonical feature contract
├── csharp/
│   ├── src/
│   │   ├── ArgParser.cs          Argument parser
│   │   ├── CommandArgs.cs        Parsed args with typed accessors
│   │   ├── CLICommandAttribute.cs  [CLICommand] attribute
│   │   ├── CommandInfo.cs        Command metadata
│   │   ├── CommandScanner.cs     Reflection-based discovery
│   │   └── CommandRegistry.cs    Scan + resolve + help
│   └── test/
│       ├── TEST_ArgParser.cs
│       └── TEST_CommandRegistry.cs
└── rust/
    └── inocli/
        ├── src/lib.rs            Full library implementation
        └── tests/
            ├── test_arg_parser.rs
            └── test_command_registry.rs
```

## C# — Installation

```bash
git submodule add https://github.com/inonego-ai/InoCLI.git lib/InoCLI
```

```xml
<ItemGroup>
  <ProjectReference Include="../lib/InoCLI/csharp/src/InoCLI.csproj" />
</ItemGroup>
```

## Rust — Installation

Add to your `Cargo.toml`:

```toml
[dependencies]
inocli = { git = "https://github.com/inonego-ai/InoCLI", subdirectory = "rust/inocli" }
```

## Argument Parsing

### Parse

```csharp
var args = new ArgParser().Parse(args);
// myapp build src/file.cs 42 --filter "x > 0" --tag a --tag b --full

args.Positionals  // ["build", "src/file.cs", "42"]
args.Optionals    // {"filter": ["x > 0"], "tag": ["a", "b"], "full": []}
```

### Indexer

```csharp
args[0]              // "build"        (positional by index, null if out of range)
args["filter"]       // "x > 0"        (optional first value, null if missing)
```

### Has / Flag

```csharp
args.Has(0)          // true           (positional exists)
args.Has("full")     // true           (optional exists)
args.Flag("full")    // true           (flag shorthand)
```

### Get (throws on missing/invalid)

```csharp
args.GetInt(1)              // positional as int
args.GetLong(1)             // positional as long
args.GetFloat(1)            // positional as float
args.GetDouble(1)           // positional as double
args.GetBool(1)             // positional as bool

args.GetInt("retries")      // optional as int
args.GetLong("offset")      // optional as long
args.GetFloat("ratio")      // optional as float
args.GetDouble("precision") // optional as double
args.GetBool("verbose")     // optional as bool
```

### Get (fallback on missing/invalid)

```csharp
args.GetInt(1, 0)              // returns 0 if missing/invalid
args.GetInt("retries", 3)      // returns 3 if missing/invalid
args.Get("name", "default")    // string fallback
```

### All (multiple values)

```csharp
args.All("tag")          // ["a", "b"]       (List<string>)
args.AllInt("port")      // [8080, 9090]     (List<int>)
args.AllLong("id")       // [...]            (List<long>)
args.AllFloat("ratio")   // [1.0, 2.5]       (List<float>)
args.AllDouble("coord")  // [...]            (List<double>)
args.AllBool("flag")     // [true, false]    (List<bool>)
```

### From

```csharp
args.From(2)           // ["42"]  — positionals from index onward
```

### Stdin

`-` is replaced with piped stdin content (POSIX convention).

```bash
cat input.txt | myapp deploy -              # positional stdin
cat input.txt | myapp deploy --code -       # optional stdin
```

### Option Formats

Both `-short` and `--long` formats are supported. `-42` (negative number) is treated as a positional.

```bash
myapp -r 3             # short option with value
myapp --retries 3      # long option with value
myapp --full           # flag (no value)
```

## Command Registry

### Define Commands

```csharp
public static class MyCommands
{
   [CLICommand("status", description = "Show status")]
   public static string HandleStatus(CommandArgs args)
   {
      return "ok";
   }

   [CLICommand("run", "fast", description = "Run fast mode")]
   public static string HandleRunFast(CommandArgs args)
   {
      return "fast:" + args[0];
   }
}
```

### Scan + Resolve

```csharp
var registry = new CommandRegistry();
registry.Initialize(typeof(MyCommands).Assembly);

var args = new ArgParser().Parse(new[] { "run", "fast", "input.txt", "--verbose" });
var (info, resolved) = registry.Resolve(args);

// info.Key         → "run.fast"
// info.Description → "Run fast mode"
// resolved[0]      → "input.txt"       (path stripped)
// resolved.Flag("verbose") → true

var result = info.Method.Invoke(null, new object[] { resolved });
```

### Help

```csharp
registry.GetHelp();            // all commands
registry.GetHelp("run");       // commands under "run.*"
registry.GetRoots();           // ["status", "run"]
```

## Rust — Usage

### Argument Parsing

```rust
use inocli::ArgParser;

let args = ArgParser::new().parse(&["build", "src/main.rs", "42", "--filter", "x > 0", "--tag", "a", "--tag", "b", "--full"]).unwrap();

// args.positionals     → ["build", "src/main.rs", "42"]
// args.optionals       → {"filter": ["x > 0"], "tag": ["a", "b"], "full": []}

args.get(0)             // Some("build")
args.get(5)             // None
args.opt("filter")      // Some("x > 0")
args.flag("full")       // true
args.get_int(2)?        // Ok(42)
args.get_int_or(2, 0)   // 42
args.get_f32_opt("ratio")?          // Ok(f32)
args.get_bool_opt_or("enabled", false)
args.all("tag")?        // Ok(["a", "b"])
args.from_index(1)      // &["src/main.rs", "42"]
```

### Command Registry

```rust
use inocli::{ArgParser, CommandArgs, CommandInfo, CommandRegistry};

fn handle_run_fast(args: &CommandArgs) {
    println!("fast: {}", args.get(0).unwrap_or(""));
}

let mut registry = CommandRegistry::new();
registry
    .register(CommandInfo::new(vec!["run".into(), "fast".into()], "Run fast mode", handle_run_fast));

let parsed = ArgParser::new().parse(&["run", "fast", "input.txt"]).unwrap();
let (info, args) = registry.resolve(&parsed).unwrap();

// info.key         → "run.fast"
// info.description → "Run fast mode"
// args.get(0)      → Some("input.txt")  (path segments stripped)

(info.handler)(&args);

// Help
registry.get_help()           // all commands
registry.get_help_for(&["run"])  // commands under "run.*"
registry.get_roots()          // ["run", ...]
```

## Compatibility

| Target | Version |
|--------|---------|
| .NET | 8.0+ |
| .NET Standard | 2.1 (Unity 2021+) |

No external dependencies. Pure `System` namespace only.

## License

[MIT](LICENSE)
