<p align="center">
  <h1 align="center">InoCLI</h1>
  <p align="center">
    .NET CLI 프레임워크 — 파서 + 커맨드 레지스트리
  </p>
  <p align="center">
    <a href="https://opensource.org/licenses/MIT"><img src="https://img.shields.io/badge/License-MIT-yellow.svg" alt="License: MIT"></a>
    <img src="https://img.shields.io/badge/.NET-8.0%20%7C%20Standard%202.1-purple?logo=dotnet" alt=".NET 8.0 | Standard 2.1">
  </p>
  <p align="center">
    <a href="README.md">English</a> | <b>한국어</b>
  </p>
</p>

---

.NET용 경량 CLI 프레임워크. 인자 파서 + 어트리뷰트 기반 커맨드 레지스트리.

## 구조

```
InoCLI/
├── src/
│   ├── ArgParser.cs              인자 파서
│   ├── CommandArgs.cs            타입 접근자 포함 파싱 결과
│   ├── CLICommandAttribute.cs    [CLICommand] 어트리뷰트
│   ├── CommandInfo.cs            커맨드 메타데이터
│   ├── CommandScanner.cs         리플렉션 기반 탐색
│   └── CommandRegistry.cs        스캔 + 리졸브 + 도움말
└── test/
    ├── TEST_ArgParser.cs
    └── TEST_CommandRegistry.cs
```

## 설치

```bash
git submodule add https://github.com/inonego/InoCLI.git lib/InoCLI
```

```xml
<ItemGroup>
  <ProjectReference Include="../lib/InoCLI/src/InoCLI.csproj" />
</ItemGroup>
```

## 인자 파싱

### 파싱

```csharp
var args = new ArgParser().Parse(args);
// myapp build src/file.cs 42 --filter "x > 0" --tag a --tag b --full

args.Positionals  // ["build", "src/file.cs", "42"]
args.Optionals    // {"filter": ["x > 0"], "tag": ["a", "b"], "full": []}
```

### 인덱서

```csharp
args[0]              // "build"        (위치 인자, 범위 초과 시 null)
args["filter"]       // "x > 0"        (옵션 첫 번째 값, 없으면 null)
```

### Has / Flag

```csharp
args.Has(0)          // true           (위치 인자 존재)
args.Has("full")     // true           (옵션 존재)
args.Flag("full")    // true           (플래그 축약)
```

### Get (없거나 잘못된 경우 throw)

```csharp
args.GetInt(1)              // 위치 인자 → int
args.GetLong(1)             // 위치 인자 → long
args.GetFloat(1)            // 위치 인자 → float
args.GetDouble(1)           // 위치 인자 → double
args.GetBool(1)             // 위치 인자 → bool

args.GetInt("retries")      // 옵션 → int
args.GetLong("offset")      // 옵션 → long
args.GetFloat("ratio")      // 옵션 → float
args.GetDouble("precision") // 옵션 → double
args.GetBool("verbose")     // 옵션 → bool
```

### Get (없으면 기본값)

```csharp
args.GetInt(1, 0)              // 없으면 0
args.GetInt("retries", 3)      // 없으면 3
args.Get("name", "default")    // 문자열 기본값
```

### All (복수 값)

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
args.From(2)           // ["42"]  — 해당 인덱스부터 나머지 위치 인자
```

### Stdin

`-`는 파이프된 stdin 내용으로 치환됩니다 (POSIX 관례).

```bash
cat input.txt | myapp deploy -              # 위치 인자로 stdin
cat input.txt | myapp deploy --code -       # 옵션 값으로 stdin
```

### 옵션 형식

`-단축`과 `--전체` 형식 모두 지원. `-42` (음수)는 위치 인자로 처리.

```bash
myapp -r 3             # 단축 옵션 + 값
myapp --retries 3      # 전체 옵션 + 값
myapp --full           # 플래그 (값 없음)
```

## 커맨드 레지스트리

### 커맨드 정의

```csharp
public static class MyCommands
{
   [CLICommand("status", description = "상태 확인")]
   public static string HandleStatus(CommandArgs args)
   {
      return "ok";
   }

   [CLICommand("run", "fast", description = "빠른 모드 실행")]
   public static string HandleRunFast(CommandArgs args)
   {
      return "fast:" + args[0];
   }
}
```

### 스캔 + 리졸브

```csharp
var registry = new CommandRegistry();
registry.Initialize(typeof(MyCommands).Assembly);

var args = new ArgParser().Parse(new[] { "run", "fast", "input.txt", "--verbose" });
var (info, resolved) = registry.Resolve(args);

// info.Key         → "run.fast"
// info.Description → "빠른 모드 실행"
// resolved[0]      → "input.txt"       (경로 제거됨)
// resolved.Flag("verbose") → true

var result = info.Method.Invoke(null, new object[] { resolved });
```

### 도움말

```csharp
registry.GetHelp();            // 전체 커맨드 목록
registry.GetHelp("run");       // "run.*" 하위 커맨드
registry.GetRoots();           // ["status", "run"]
```

## 호환성

| 타겟 | 버전 |
|------|------|
| .NET | 8.0+ |
| .NET Standard | 2.1 (Unity 2021+) |

외부 의존성 없음. 순수 `System` 네임스페이스만 사용.

## 라이선스

[MIT](LICENSE)
