# Server 프로젝트

## 개요
Server 프로젝트는 고성능 웹 서버를 구축하기 위해 설계된 애플리케이션입니다. 이 프로젝트는 Redis 서버와의 통합을 통해 실시간 데이터 처리를 효율적으로 수행하며, 다양한 서비스와 리포지토리 패턴을 활용하여 확장성과 유지보수성을 높였습니다.

## 주요 기능

- **Redis 서버 통합**: 애플리케이션 시작 시 Redis 서버를 자동으로 시작하고 초기화합니다.
- **다양한 서비스 및 리포지토리 패턴**: 여러 서비스 (`AccountService`, `ShopService`, `InventoryService`, `RankService`, `GuildService`, `SessionService`, `WebSocketChatService`, `WebSocketMatchService`, `WebSocketLoginService`)와 리포지토리를 싱글톤 패턴으로 DI 컨테이너에 등록하여 사용합니다.
- **API 문서화 (Swagger)**: Swagger를 통해 API 문서를 자동으로 생성하고 관리합니다.
- **미들웨어 구성**: 사용자 정의 미들웨어를 통해 요청 처리 파이프라인을 구성하여 확장성을 높입니다.

## 기술 스택

- **프로그래밍 언어**: C#
- **프레임워크**: .NET Core
- **데이터베이스**: Redis, MySql
- **API 문서화**: Swagger
- **빌드 도구**: Visual Studio

## Server 디렉토리 구조

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


---

# Client 프로젝트

## 개요
Client 프로젝트는 고성능 웹 서버와의 통신을 위한 클라이언트 애플리케이션입니다. 이 프로젝트는 다양한 컨트롤러, 헬퍼, 모델, 뷰를 활용하여 사용자 인터페이스와 데이터를 처리하며, 인게임 세션 통신을 위한 클래스를 포함하고 있습니다.

## 주요 기능

- **컨트롤러**: 사용자 요청을 처리하고 적절한 뷰와 데이터를 반환합니다.
- **헬퍼 및 유틸리티**: 공통 기능을 수행하는 헬퍼 클래스와 유틸리티 함수들을 제공합니다.
- **모델**: 데이터 구조를 정의합니다.
- **뷰**: 사용자 인터페이스를 렌더링합니다.
- **인게임 세션 통신**: 인게임 통신을 위한 TCP 소켓 통신을 구현합니다.

## 기술 스택

- **프로그래밍 언어**: C#
- **프레임워크**: .NET Core
- **API 통신**: HTTP, WebSocket, Socket(TCP)

## 프로젝트 구조

- `Controller` : MVC 컨트롤러 클래스
- `Helper` : 헬퍼 클래스 및 유틸리티 함수
- `InGame` : 인게임 세션통신 관련 클래스 및 파일
- `Model` : 데이터 모델 클래스
- `SharedCodeLibrary` : 공유 코드 라이브러리
- `View` : 사용자 인터페이스 뷰 클래스

# SharedCode

## 개요
Client 프로젝트와 Server 프로젝트의 서로 공유하는 코드입니다.

## 주요기능

- **프로토콜** 서버와 클라이언트간의 공유하는 프로토콜입니다.
- **HttpCommand** 서버와 클라이언트간의 WebRequest관련 로직 작성시 사용되는 명령입니다.
- **Packet** 서버와 클라이언트가 사용하는 패킷구조 관련 클래스입니다.
- **기타Model** 서버와 클라이언트가 공유되는 모델구조입니다. 주로 아이템이나 유저데이터관련등이 있습니다

전체 프로젝트 기능

- Redis 서버 통합 및 초기화 구현
- http프로토콜로 주요 서비스 및 리포지토리 클래스 작성
- 웹소켓으로 채팅시스템, 매칭시스템 구현
- 인게임 TCP 소켓 실시간통신 구현
- Swagger를 통한 API 문서화 설정
- 로그인, 길드(http통신)
- 매칭, 채팅(웹소켓통신)
- 인게임(tcp소켓통신)

설명
 롤 아레나를 재밌게 한 기억이 있어 참고하여 기초틀만 작업해보았습니다.
