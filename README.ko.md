<p align="center">
  <h1 align="center">InoCLI</h1>
  <p align="center">
    .NET CLI 프레임워크
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

CLI 도구를 만들기 위한 .NET 프레임워크입니다. 인자 파싱, 스키마 검증, 헬프 생성, 출력 포맷팅을 제공합니다 — 커맨드만 추가하면 됩니다. 클라이언트-서버 CLI를 위한 트랜스포트 레이어도 선택적으로 포함되어 있습니다.

## 아키텍처

```
CLI 바이너리 (사용자 앱)
 └── InoCLI (이 라이브러리)
      ├── ArgParser       — args를 group/command/options로 파싱
      ├── CliSchema       — 선택적 스키마 검증 + 헬프 생성
      ├── HelpFormatter   — 스키마에서 헬프 텍스트 자동 생성
      ├── JsonOutput      — JSON 출력 (선택적 포맷팅)
      ├── StdinReader     — "-"로 stdin 파이프 (POSIX 관례)
      └── Transport       — 선택: TCP, Unix Domain Socket
```

## 저장소 구조

```
InoCLI/
├── src/InoCLI/
│   ├── Parsing/        ← ArgParser, ParsedArgs
│   ├── Schema/         ← CliSchema, GroupSchema, CommandSchema, ...
│   ├── Output/         ← JsonOutput, HelpFormatter
│   ├── Models/         ← CliRequest, CliResponse
│   ├── Utils/          ← StdinReader
│   ├── Client/         ← CliClient (재시도 포함 전송)
│   ├── Transport/      ← ITransport, TcpTransport, UnixSocketTransport
│   └── Protocol/       ← FrameProtocol (길이 접두사 프레임)
└── tests/InoCLI.Tests/
```

## 설치

프로젝트에 git submodule로 추가:

```bash
git submodule add https://github.com/inonego/InoCLI.git lib/InoCLI
```

`.csproj`에서 참조:

```xml
<ItemGroup>
  <ProjectReference Include="../lib/InoCLI/src/InoCLI/InoCLI.csproj" />
</ItemGroup>
```

## 빠른 시작

### 스키마 사용

커맨드가 컴파일 타임에 정해져 있을 때, `schema.json`으로 검증과 헬프 생성을 자동화합니다.

```json
{
  "globalOptions": {
    "timeout": { "type": "int", "description": "타임아웃 (초)" }
  },
  "groups": {
    "flow": {
      "description": "실행 흐름 제어",
      "commands": {
        "continue": { "description": "실행 계속" },
        "step":     { "description": "스텝 오버", "options": {
          "count": { "type": "int", "description": "스텝 횟수" }
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
// ... request 처리
```

### 스키마 없이

커맨드가 다른 곳(예: 서버 측)에서 정의될 때, 스키마 검증 없이 사용합니다.

```csharp
var parser = new ArgParser(new[] { "port", "timeout", "pretty" });
var parsed = parser.Parse(args);

var request = CliRequest.FromParsedArgs(parsed);
// ... 서버로 전송, 응답 출력
```

## 구성 요소

### ArgParser

`<group> [command] [args...] [--options]`를 `ParsedArgs` 객체로 파싱합니다.

- **스키마 있을 때**: group/command 검증, 커맨드와 positional 인자 구분
- **스키마 없을 때**: 두 번째 비옵션 토큰을 커맨드로 처리, 검증 없음

```bash
myapp flow step --count 3
#     ↑     ↑       ↑
#   그룹  커맨드   옵션
```

반복 옵션 지원:

```bash
myapp eval cs code --using System --using UnityEngine
# options["using"] = ["System", "UnityEngine"]
```

### CliSchema

`schema.json`에서 로드. 그룹, 커맨드, 인자, 옵션을 정의합니다. `ArgParser`의 검증과 `HelpFormatter`의 헬프 생성에 사용됩니다.

### HelpFormatter

스키마에서 세 단계의 헬프 텍스트를 생성합니다:

- `ForAll(schema)` — 전체 그룹 목록
- `ForGroup(schema, group)` — 그룹 내 커맨드 목록
- `ForCommand(schema, group, command)` — 인자와 옵션 상세

### JsonOutput

- `Write(json, pretty)` — stdout 출력 (선택적 포맷팅)
- `Prettify(json)` — JSON 들여쓰기 재포맷

### StdinReader

positional 인자에서 `-`를 파이프된 stdin 내용으로 교체합니다 (POSIX 관례).

```bash
cat script.cs | myapp eval cs -
```

### Transport (선택)

길이 접두사 JSON 프레임으로 통신하는 클라이언트-서버 CLI용.

| 클래스 | 프로토콜 | 용도 |
|--------|----------|------|
| `TcpTransport` | TCP (127.0.0.1) | 원격 서버 연결 |
| `UnixSocketTransport` | Unix Domain Socket | 로컬 데몬 프로세스 |

프레임 포맷: `[4바이트 BE uint32 길이][UTF-8 바디]`

`CliClient`가 `ITransport` 위에서 `Send()`와 `SendWithRetry()`를 제공합니다.

## 출력 형식

모든 응답은 동일한 JSON 계약을 따릅니다:

```json
{"success":true,"result":...}
{"success":false,"error":{"code":"...","message":"..."}}
```

## 라이선스

[MIT](LICENSE)
