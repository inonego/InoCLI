<p align="center">
  <h1 align="center">InoCLI</h1>
  <p align="center">
    .NET CLI 인자 파서
  </p>
  <p align="center">
    <a href="https://opensource.org/licenses/MIT"><img src="https://img.shields.io/badge/License-MIT-yellow.svg" alt="License: MIT"></a>
    <img src="https://img.shields.io/badge/.NET-8.0-purple?logo=dotnet" alt=".NET 8.0">
  </p>
  <p align="center">
    <a href="README.md">English</a> | <b>한국어</b>
  </p>
</p>

---

.NET용 경량 CLI 인자 파서. `string[] args`를 **positionals**과 **optionals**로 분리합니다.

## 구조

```
InoCLI/
├── src/InoCLI/
│   ├── Parsing/     ArgParser, ParsedArgs
│   └── Json/        JsonHelper (JSON 읽기/쓰기 유틸)
└── tests/InoCLI.Tests/
```

## 설치

```bash
git submodule add https://github.com/inonego/InoCLI.git lib/InoCLI
```

```xml
<ItemGroup>
  <ProjectReference Include="../lib/InoCLI/src/InoCLI/InoCLI.csproj" />
</ItemGroup>
```

## 사용법

### 파싱

```csharp
var parsed = new ArgParser().Parse(args);
// myapp build src/file.cs 42 --filter "x > 0" --tag a --tag b --full

parsed.Positionals  // ["build", "src/file.cs", "42"]
parsed.Optionals    // {"filter": ["x > 0"], "tag": ["a", "b"], "full": []}
```

### 인덱서

```csharp
parsed[0]              // "build"        (위치 인자, 범위 초과 시 null)
parsed["filter"]       // "x > 0"        (옵션 첫 번째 값, 없으면 null)
```

### Has

```csharp
parsed.Has(0)          // true           (위치 인자 존재)
parsed.Has("full")     // true           (플래그 존재)
parsed.Has("missing")  // false
```

### Get (없거나 잘못된 경우 throw)

```csharp
parsed.GetInt(1)              // 위치 인자 → int
parsed.GetFloat(1)            // 위치 인자 → float
parsed.GetBool(1)             // 위치 인자 → bool

parsed.GetInt("retries")      // 옵션 → int
parsed.GetFloat("ratio")      // 옵션 → float
parsed.GetBool("verbose")     // 옵션 → bool
```

### All (복수 값)

```csharp
parsed.All("tag")          // ["a", "b"]       (List<string>)
parsed.AllInt("port")      // [8080, 9090]     (List<int>)
parsed.AllFloat("ratio")   // [1.0, 2.5]       (List<float>)
parsed.AllBool("flag")     // [true, false]    (List<bool>)
```

### Stdin

`-`는 파이프된 stdin 내용으로 치환됩니다 (POSIX 관례). 위치 인자와 옵션 값 모두 지원.

```bash
cat input.txt | myapp deploy -              # 위치 인자로 stdin
cat input.txt | myapp deploy --code -       # 옵션 값으로 stdin
```

### 옵션 형식

`-단축`과 `--전체` 형식 모두 지원.

```bash
myapp -r 3             # 단축 옵션 + 값
myapp --retries 3      # 전체 옵션 + 값
myapp --full           # 플래그 (값 없음)
myapp -f               # 단축 플래그
```

`-42` (음수)는 옵션이 아닌 위치 인자로 처리됩니다.

## JsonHelper

JSON 읽기/쓰기 유틸리티.

### 읽기 (JsonElement에서)

```csharp
JsonHelper.GetInt(element, fallback)      // 숫자 또는 "42" → int
JsonHelper.GetFloat(element, fallback)    // 숫자 또는 "1.5" → float
JsonHelper.GetString(element, fallback)   // 문자열 추출
JsonHelper.GetBool(element, fallback)     // true, "true", 1 → bool
```

### 쓰기 (콘솔에)

```csharp
JsonHelper.Write(json, pretty);           // stdout
JsonHelper.WriteError(json, pretty);      // stderr
JsonHelper.Prettify(json);                // 들여쓰기 포맷팅
```

## 라이선스

[MIT](LICENSE)
