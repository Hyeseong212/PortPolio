# Server 프로젝트

## 개요
Server 프로젝트는 고성능 웹 서버를 구축하기 위해 설계된 애플리케이션입니다. 이 프로젝트는 Redis 서버와의 통합을 통해 실시간 데이터 처리를 효율적으로 수행하며, 다양한 서비스와 리포지토리 패턴을 활용하여 확장성과 유지보수성을 높였습니다.

## 주요 기능

- **Redis 서버 통합**: 애플리케이션 시작 시 Redis 서버를 자동으로 시작하고 초기화합니다.
- **다양한 서비스 및 리포지토리 패턴**: 여러 서비스 (`AccountService`, `ShopService`, `InventoryService`, `RankService`, `GuildService`, `SessionService`, `WS_ChatService`, `WS_MatchService`, `WS_LoginService`)와 리포지토리를 싱글톤 패턴으로 DI 컨테이너에 등록하여 사용합니다.
- **API 문서화 (Swagger)**: Swagger를 통해 API 문서를 자동으로 생성하고 관리합니다.
- **미들웨어 구성**: 사용자 정의 미들웨어를 통해 요청 처리 파이프라인을 구성하여 확장성을 높입니다.

## 기술 스택

- **프로그래밍 언어**: C#
- **프레임워크**: .NET Core
- **데이터베이스**: Redis, MySql
- **API 문서화**: Swagger
- **빌드 도구**: Visual Studio

## 프로젝트 구조

- `.git` : Git 버전 관리 폴더
- `.gitattributes` : Git 특성 파일
- `.github` : GitHub 관련 워크플로우 및 설정 파일
- `.gitignore` : Git이 무시할 파일 및 디렉토리 목록
- `.vs` : Visual Studio 관련 설정 폴더
- `DB` : 데이터베이스 관련 파일 및 스크립트
- `Redis` : Redis 관련 파일 및 설정
- `WebServer.sln` : Visual Studio 솔루션 파일
- `WebServer` : WebServer 애플리케이션 소스 코드 폴더

### Server 디렉토리 구조

- `appsettings.Development.json` : 개발 환경 설정 파일
- `appsettings.json` : 기본 설정 파일
- `Configuration` : 설정 관련 클래스 및 파일
- `Controllers` : MVC 컨트롤러 클래스
- `Helper` : 헬퍼 클래스 및 유틸리티 함수
- `Middleware` : 미들웨어 클래스
- `Model` : 데이터 모델 클래스
- `Repository` : 데이터 저장소 클래스
- `Service` : 서비스 클래스
- `SwaggerOptions` : Swagger 설정 파일
- `Utils` : 유틸리티 클래스 및 함수
- `WebSocket` : WebSocket 관련 파일
- `IOCPSocket/InGame` : 인게임 세션통신 관련 클래스 및 파일

## 설치 및 실행 방법

1. **Redis 서버 설정**:
   - 프로젝트 루트 폴더의 `Redis` 디렉토리에서 Redis 서버 실행 파일을 확인합니다.
   - Redis를 실행합니다

2. **프로젝트 빌드 및 실행**:
   - Visual Studio에서 `WebServer.sln` 솔루션 파일을 엽니다.
   - 솔루션을 빌드하고 실행합니다.

3. **API 문서 확인**:
   - Swagger UI를 통해 API 문서를 확인할 수 있습니다. 기본 주소는 `http://localhost:{포트}/swagger`입니다.

주요 프로젝트 기능

- Redis 서버 통합 및 초기화 구현
- http프로토콜로 주요 서비스 및 리포지토리 클래스 작성
- 웹소켓으로 채팅시스템, 매칭시스템
- 인게임 TCP 소켓 통신으로 구현
- Swagger를 통한 API 문서화 설정
- 사용자 정의 미들웨어 개발

# Client 프로젝트
