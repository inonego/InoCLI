<p align="center">
  <h1 align="center">InoCLI</h1>
  <p align="center">
    CLI 프레임워크 — 파서 + 커맨드 레지스트리
  </p>
  <p align="center">
    <a href="https://opensource.org/licenses/MIT"><img src="https://img.shields.io/badge/License-MIT-yellow.svg" alt="License: MIT"></a>
    <img src="https://img.shields.io/badge/.NET-8.0%20%7C%20Standard%202.1-purple?logo=dotnet" alt=".NET 8.0 | Standard 2.1">
    <img src="https://img.shields.io/badge/Rust-2024-orange?logo=rust" alt="Rust 2024">
  </p>
  <p align="center">
    <a href="README.md">English</a> | <b>한국어</b>
  </p>
</p>

---

경량 CLI 프레임워크 — C#과 Rust 두 가지 구현체를 제공.
인자 파서 + 타입 변환 헬퍼 + 계층형 커맨드 라우팅.
두 구현체가 지원해야 할 기능 명세는 [CLAUDE.md](CLAUDE.md)를 참고.

## 구현체

| 언어 | 위치 | 테스트 |
|------|------|--------|
| C# (.NET 8 / Standard 2.1) | [`csharp/`](csharp/) | `dotnet test csharp/` |
| Rust (2024 에디션) | [`rust/`](rust/) | `cargo test` (`rust/` 에서) |

## 레포 구조

```
InoCLI/
├── CLAUDE.md                       기능 명세 (정식 계약서)
├── csharp/
│   ├── src/
│   │   ├── ArgParser.cs            인자 파서
│   │   ├── CommandArgs.cs          타입 접근자 포함 파싱 결과
│   │   ├── CLICommandAttribute.cs  [CLICommand] 어트리뷰트
│   │   ├── CommandInfo.cs          커맨드 메타데이터
│   │   ├── CommandScanner.cs       리플렉션 기반 탐색
│   │   └── CommandRegistry.cs      스캔 + 리졸브 + 도움말
│   └── test/
│       ├── TEST_ArgParser.cs
│       └── TEST_CommandRegistry.cs
└── rust/
    └── inocli/
        ├── src/
        │   ├── lib.rs              모듈 선언 + re-export
        │   ├── error.rs            CliError
        │   ├── command_args.rs     CommandArgs
        │   ├── arg_parser.rs       ArgParser
        │   ├── command_info.rs     CommandInfo
        │   └── command_registry.rs CommandRegistry
        └── tests/
            ├── test_arg_parser.rs
            └── test_command_registry.rs
```

## C# — 설치

```bash
git submodule add https://github.com/inonego-ai/InoCLI.git lib/InoCLI
```

```xml
<ItemGroup>
  <ProjectReference Include="../lib/InoCLI/csharp/src/InoCLI.csproj" />
</ItemGroup>
```

## Rust — 설치

`Cargo.toml`에 추가:

```toml
[dependencies]
inocli = { git = "https://github.com/inonego-ai/InoCLI", subdirectory = "rust/inocli" }
```

## 인자 파싱

### 파싱

```csharp
// C#
var args = new ArgParser().Parse(args);
// myapp build src/file.cs 42 --filter "x > 0" --tag a --tag b --full

args.Positionals  // ["build", "src/file.cs", "42"]
args.Optionals    // {"filter": ["x > 0"], "tag": ["a", "b"], "full": []}
```

```rust
// Rust
let args = ArgParser::new().parse(&["build", "src/main.rs", "42", "--filter", "x > 0", "--tag", "a", "--tag", "b", "--full"]).unwrap();

// args.positionals  → ["build", "src/main.rs", "42"]
// args.optionals    → {"filter": ["x > 0"], "tag": ["a", "b"], "full": []}
```

### 인덱서 / 접근자

```csharp
// C#
args[0]              // "build"   (위치 인자, 범위 초과 시 null)
args["filter"]       // "x > 0"  (옵션 첫 번째 값, 없으면 null)
```

```rust
// Rust
args.get(0)          // Some("build")
args.opt("filter")   // Some("x > 0")
```

### Has / Flag

```csharp
// C#
args.Has(0)          // true
args.Has("full")     // true
args.Flag("full")    // true
```

```rust
// Rust
args.has(0)          // true
args.has_opt("full") // true
args.flag("full")    // true
```

### Get (없거나 잘못된 경우 오류)

```csharp
// C#
args.GetInt(1)           args.GetFloat(1)          args.GetDouble(1)
args.GetBool(1)          args.GetLong(1)

args.GetInt("retries")   args.GetFloat("ratio")    args.GetDouble("precision")
args.GetBool("verbose")  args.GetLong("offset")
```

```rust
// Rust  (모두 Result<T, CliError> 반환)
args.get_int(1)?         args.get_f32(1)?           args.get_f64(1)?
args.get_bool(1)?

args.get_int_opt("retries")?     args.get_f32_opt("ratio")?
args.get_f64_opt("precision")?   args.get_bool_opt("verbose")?
```

### Get (기본값)

```csharp
// C#
args.GetInt(1, 0)              // 없으면 0
args.GetInt("retries", 3)      // 없으면 3
args.Get("name", "default")
```

```rust
// Rust
args.get_int_or(1, 0)
args.get_int_opt_or("retries", 3)
args.get_or(1, "default")
```

### All (복수 값)

```csharp
// C#
args.All("tag")          // ["a", "b"]   (List<string>)
args.AllInt("port")      // [8080, 9090] (List<int>)
```

```rust
// Rust
args.all("tag")?         // Ok(&["a", "b"])
args.all_or("tag", &[])  // fallback 지원
```

### From

```csharp
args.From(2)           // ["42"]  — 해당 인덱스부터 나머지
```

```rust
args.from_index(2)     // &["42", ...]
```

### Stdin

`-`는 파이프된 stdin 내용으로 치환 (POSIX 관례).

```bash
cat input.txt | myapp deploy -              # 위치 인자로 stdin
cat input.txt | myapp deploy --code -       # 옵션 값으로 stdin
```

### 옵션 형식

`-단축`과 `--전체` 형식 모두 지원. `-42` (음수)는 위치 인자로 처리.

## 커맨드 레지스트리

### 커맨드 정의

```csharp
// C#
public static class MyCommands
{
   [CLICommand("status", description = "상태 확인")]
   public static string HandleStatus(CommandArgs args) => "ok";

   [CLICommand("run", "fast", description = "빠른 모드 실행")]
   public static string HandleRunFast(CommandArgs args) => "fast:" + args[0];
}
```

```rust
// Rust
fn handle_run_fast(args: &CommandArgs) {
    println!("fast: {}", args.get(0).unwrap_or(""));
}
```

### 스캔 + 리졸브

```csharp
// C#
var registry = new CommandRegistry();
registry.Initialize(typeof(MyCommands).Assembly);

var args = new ArgParser().Parse(new[] { "run", "fast", "input.txt", "--verbose" });
var (info, resolved) = registry.Resolve(args);

// info.Key         → "run.fast"
// resolved[0]      → "input.txt"   (경로 제거됨)
// resolved.Flag("verbose") → true

info.Method.Invoke(null, new object[] { resolved });
```

```rust
// Rust
let mut registry = CommandRegistry::new();
registry.register(CommandInfo::new(vec!["run".into(), "fast".into()], "빠른 모드 실행", handle_run_fast));

let parsed = ArgParser::new().parse(&["run", "fast", "input.txt", "--verbose"]).unwrap();
let (info, args) = registry.resolve(&parsed).unwrap();

// info.key     → "run.fast"
// args.get(0)  → Some("input.txt")
(info.handler)(&args);
```

### 도움말

```csharp
// C#
registry.GetHelp();          // 전체 커맨드
registry.GetHelp("run");     // "run.*" 하위 커맨드
registry.GetRoots();         // ["status", "run"]
```

```rust
// Rust
registry.get_help()
registry.get_help_for(&["run"])
registry.get_roots()
```

## 호환성

| 항목 | 버전 |
|------|------|
| .NET | 8.0+ |
| .NET Standard | 2.1 (Unity 2021+) |
| Rust edition | 2024 (rustc 1.85+) |

외부 의존성 없음.

## 라이선스

[MIT](LICENSE)
