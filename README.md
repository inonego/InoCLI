<p align="center">
  <h1 align="center">InoCLI</h1>
  <p align="center">
    .NET CLI Argument Parser
  </p>
  <p align="center">
    <a href="https://opensource.org/licenses/MIT"><img src="https://img.shields.io/badge/License-MIT-yellow.svg" alt="License: MIT"></a>
    <img src="https://img.shields.io/badge/.NET-8.0-purple?logo=dotnet" alt=".NET 8.0">
  </p>
  <p align="center">
    <b>English</b> | <a href="README.ko.md">한국어</a>
  </p>
</p>

---

Lightweight CLI argument parser for .NET. Splits `string[] args` into **positionals** and **optionals** — nothing more.

## Structure

```
InoCLI/
├── src/InoCLI/
│   ├── Parsing/     ArgParser, ParsedArgs
│   └── Json/        JsonHelper (read/write JSON utilities)
└── tests/InoCLI.Tests/
```

## Installation

```bash
git submodule add https://github.com/inonego/InoCLI.git lib/InoCLI
```

```xml
<ItemGroup>
  <ProjectReference Include="../lib/InoCLI/src/InoCLI/InoCLI.csproj" />
</ItemGroup>
```

## Usage

### Parse

```csharp
var parsed = new ArgParser().Parse(args);
// myapp build src/file.cs 42 --filter "x > 0" --tag a --tag b --full

parsed.Positionals  // ["build", "src/file.cs", "42"]
parsed.Optionals    // {"filter": ["x > 0"], "tag": ["a", "b"], "full": []}
```

### Indexer

```csharp
parsed[0]              // "build"        (positional by index, null if out of range)
parsed["filter"]       // "x > 0"        (optional first value, null if missing)
```

### Has

```csharp
parsed.Has(0)          // true           (positional exists)
parsed.Has("full")     // true           (flag exists)
parsed.Has("missing")  // false
```

### Get (throws on missing/invalid)

```csharp
parsed.GetInt(1)              // positional as int
parsed.GetFloat(1)            // positional as float
parsed.GetBool(1)             // positional as bool

parsed.GetInt("retries")      // optional as int
parsed.GetFloat("ratio")      // optional as float
parsed.GetBool("verbose")     // optional as bool
```

### All (multiple values)

```csharp
parsed.All("tag")          // ["a", "b"]       (List<string>)
parsed.AllInt("port")      // [8080, 9090]     (List<int>)
parsed.AllFloat("ratio")   // [1.0, 2.5]       (List<float>)
parsed.AllBool("flag")     // [true, false]    (List<bool>)
```

### Stdin

`-` is replaced with piped stdin content (POSIX convention). Works for both positionals and optional values.

```bash
cat input.txt | myapp deploy -              # positional stdin
cat input.txt | myapp deploy --code -       # optional stdin
```

### Option Formats

Both `-short` and `--long` formats are supported.

```bash
myapp -r 3             # short option with value
myapp --retries 3      # long option with value
myapp --full           # flag (no value)
myapp -f               # short flag
```

`-42` (negative number) is treated as a positional, not an option.

## JsonHelper

JSON read/write utilities.

### Read (from JsonElement)

```csharp
JsonHelper.GetInt(element, fallback)      // number or "42" → int
JsonHelper.GetFloat(element, fallback)    // number or "1.5" → float
JsonHelper.GetString(element, fallback)   // string extraction
JsonHelper.GetBool(element, fallback)     // true, "true", 1 → bool
```

### Write (to console)

```csharp
JsonHelper.Write(json, pretty);           // stdout
JsonHelper.WriteError(json, pretty);      // stderr
JsonHelper.Prettify(json);                // re-format with indentation
```

## License

[MIT](LICENSE)
