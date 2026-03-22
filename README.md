<p align="center">
  <h1 align="center">InoCLI</h1>
  <p align="center">
    .NET CLI Framework
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

InoCLI is a .NET framework for building CLI tools. Argument parsing, schema validation, help generation, and output formatting are all handled — just add your commands. Includes an optional transport layer for client-server CLIs.

## Architecture

```
CLI Binary (your app)
 └── InoCLI (this library)
      ├── ArgParser       — parse args into group/command/options
      ├── CliSchema       — optional schema validation + help generation
      ├── HelpFormatter   — auto-generate help text from schema
      ├── JsonOutput      — JSON output with optional pretty-print
      ├── StdinReader     — pipe stdin via "-" (POSIX convention)
      └── Transport       — optional: TCP, Unix Domain Socket
```

## Repository Structure

```
InoCLI/
├── src/InoCLI/
│   ├── Parsing/        ← ArgParser, ParsedArgs
│   ├── Schema/         ← CliSchema, GroupSchema, CommandSchema, ...
│   ├── Output/         ← JsonOutput, HelpFormatter
│   ├── Models/         ← CliRequest, CliResponse
│   ├── Utils/          ← StdinReader, JsonHelper
│   ├── Client/         ← CliClient (send with retry)
│   ├── Transport/      ← ITransport, TcpTransport, UnixSocketTransport, MemoryTransport
│   └── Protocol/       ← FrameProtocol (length-prefixed frames)
└── tests/InoCLI.Tests/
```

## Installation

Add as a git submodule to your project:

```bash
git submodule add https://github.com/inonego/InoCLI.git lib/InoCLI
```

Reference in your `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="../lib/InoCLI/src/InoCLI/InoCLI.csproj" />
</ItemGroup>
```

## Quick Start

### With Schema

When commands are known at compile time, use a `schema.json` for validation and help generation.

```json
{
  "globalOptions": {
    "timeout": { "type": "int", "description": "Timeout in seconds" }
  },
  "groups": {
    "flow": {
      "description": "Execution flow control",
      "commands": {
        "continue": { "description": "Continue execution" },
        "step":     { "description": "Step over", "options": {
          "count": { "type": "int", "description": "Number of steps" }
        }}
      }
    }
  }
}
```

```csharp
var schema = CliSchema.Load("schema.json");
var parser = new ArgParser(schema);
var parsed = parser.Parse(args);

if (parsed.HelpRequested)
{
    Console.WriteLine(HelpFormatter.ForAll(schema, "myapp"));
    return 0;
}

var request = CliRequest.FromParsedArgs(parsed);
// ... handle request
```

### Without Schema

When commands are defined elsewhere (e.g. server-side), skip schema validation entirely.

```csharp
var parser = new ArgParser(new[] { "port", "timeout", "pretty" });
var parsed = parser.Parse(args);

var request = CliRequest.FromParsedArgs(parsed);
// ... send to server, print response
```

## Components

### ArgParser

Parses `<group> [command] [args...] [--options]` into a `ParsedArgs` object.

- **With schema**: validates group/command, distinguishes commands from positional args
- **Without schema**: treats second non-option token as command, no validation

```bash
myapp flow step --count 3
#     ↑     ↑       ↑
#   group command  option
```

Supports repeated options:

```bash
myapp eval cs code --using System --using UnityEngine
# options["using"] = ["System", "UnityEngine"]
```

### CliSchema

Loaded from `schema.json`. Defines groups, commands, args, and options. Used by `ArgParser` for validation and `HelpFormatter` for help generation.

### HelpFormatter

Generates help text from schema at three levels:

- `ForAll(schema)` — list all groups
- `ForGroup(schema, group)` — list commands in a group
- `ForCommand(schema, group, command)` — show args and options

### CliResponse

Parses server responses and builds client-side JSON responses.

```csharp
// Parse server response
var response = CliResponse.Parse(serverJson);

// Build responses
CliResponse.Ok("Connected");
// → {"success":true,"message":"Connected"}

CliResponse.Result("version", "0.1.0");
// → {"success":true,"version":"0.1.0"}

CliResponse.Error("TIMEOUT", "Timed out");
// → {"success":false,"error":{"code":"TIMEOUT","message":"Timed out"}}

CliResponse.Error("TIMEOUT", "Timed out", new Dictionary<string, object> { ["elapsed"] = 5000 });
// → {"success":false,"error":{"code":"TIMEOUT","message":"Timed out","elapsed":5000}}
```

### JsonOutput

- `Write(json, pretty)` — stdout with optional pretty-print
- `WriteError(json, pretty)` — stderr with optional pretty-print
- `Write(CliResponse, pretty)` — auto-routes: success → stdout, error → stderr
- `Prettify(json)` — re-format JSON with indentation

### JsonHelper

Extracts typed values from `JsonElement`, handling both string and number representations.

- `GetInt(element, fallback)` — `42` or `"42"` → `int`
- `GetLong(element, fallback)` — `42` or `"42"` → `long`
- `GetString(element, fallback)` — string extraction with fallback
- `GetBool(element, fallback)` — `true`, `"true"`, `1` → `bool`

### StdinReader

Replaces `-` in positional args with piped stdin content (POSIX convention).

```bash
cat script.cs | myapp eval cs -
```

### Transport (Optional)

For client-server CLIs that communicate over length-prefixed JSON frames.

| Class | Protocol | Use Case |
|-------|----------|----------|
| `TcpTransport` | TCP (127.0.0.1) | Remote server connection |
| `UnixSocketTransport` | Unix Domain Socket | Local daemon processes |
| `MemoryTransport` | In-memory buffer | Testing |

Frame format: `[4-byte BE uint32 length][UTF-8 body]`

`CliClient` provides `Send()` and `SendWithRetry()` over any `ITransport`.

## Output Format

All responses follow the same JSON contract:

```json
{"success":true,"result":...}
{"success":false,"error":{"code":"...","message":"..."}}
```

## License

[MIT](LICENSE)
