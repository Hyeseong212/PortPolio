# 영상링크
<iframe width="560" height="315" src="https://www.youtube.com/embed/-K1Rn2rFDRA" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

# 프로젝트 구조도
![image](https://github.com/user-attachments/assets/f68a4e7b-4e9f-4ddb-b7e9-71e7b5fae3ae)
# Server 프로젝트

## 개요
Server 프로젝트는 웹 서버를 구축하기 위해 설계된 애플리케이션입니다. 이 프로젝트는 Redis 서버와의 통합을 통해 실시간 데이터 처리를 효율적으로 수행하며, 다양한 서비스와 리포지토리 패턴을 활용하여 확장성과 유지보수성을 높였습니다.

## 주요 기능

- **Redis**: 애플리케이션 시작 시 Redis 서버를 자동으로 초기화합니다.
- **다양한 서비스 및 리포지토리 패턴**: 여러 서비스 (`AccountService`, `ShopService`, `InventoryService`, `RankService`, `GuildService`, `SessionService`, `WebSocketChatService`, `WebSocketMatchService`, `WebSocketLoginService`)와 리포지토리를 싱글톤 패턴으로 DI 컨테이너에 등록하여 사용합니다.
- **MySQL**: 영구적으로 보관해야 할 데이터들은 MySQL DBMS에서 정규화하여 저장합니다.
- **TCP 실시간 인게임 통신**: 매칭 시 세션을 생성하고, 게임 종료 시 세션 자원을 해제하여 세션을 관리합니다.
- **WebSocket**: 매칭 시스템과 채팅 시스템을 구현합니다.
- **API 문서화 (Swagger)**: Swagger를 통해 API 문서를 자동으로 생성하고 관리합니다.

## 기술 스택

- **프로그래밍 언어**: C#
- **프레임워크**: .NET Core
- **데이터베이스**: Redis, MySql
- **API 문서화**: Swagger
- **빌드 도구**: Visual Studio

## Server 디렉토리 구조

- `Configuration`: 설정 관련 클래스 및 파일
- `Controllers`: MVC 컨트롤러 클래스
- `Helper`: 헬퍼 클래스 및 유틸리티 함수
- `Model`: 데이터 모델 클래스
- `Repository`: 데이터 저장소 클래스
- `Service`: 서비스 클래스
- `SwaggerOptions`: Swagger 설정 파일
- `Utils`: 유틸리티 클래스 및 함수
- `WebSocket`: WebSocket 관련 파일
- `IOCPSocket/InGame`: 인게임 세션 통신 관련 클래스 및 파일

## 설치 및 실행 방법

1. **Redis 서버 설정**:
   - 프로젝트 루트 폴더의 `Redis` 디렉토리에서 Redis 서버 실행 파일을 확인합니다.
   - Redis를 실행합니다.

2. **프로젝트 빌드 및 실행**:
   - Visual Studio에서 `WebServer.sln` 솔루션 파일을 엽니다.
   - 솔루션을 빌드하고 실행합니다.

3. **API 문서 확인**:
   - Swagger UI를 통해 API 문서를 확인할 수 있습니다. 기본 주소는 `http://localhost:{포트}/swagger`입니다.

## 서버 프로젝트 기능 요약

- Redis로 유저 세션관리
- HTTP 프로토콜로 주요 서비스 및 리포지토리 클래스 작성
- WebSocket으로 채팅 시스템 및 매칭 시스템 구현
- 인게임 TCP 소켓 통신으로 실시간 통신 구현
- Swagger를 통한 API 문서화 설정
- 로그인 및 길드 관리 (HTTP 통신)
- 매칭 및 채팅 (WebSocket 통신)
- 인게임 (TCP 소켓 통신)
- Task를 사용하여 비동기 프로그래밍 및 쓰레드풀로 고성능으로 작동
- lock, semaphore를 이용한 동시성처리

# Client 프로젝트

## 개요
Client 프로젝트는 웹 서버와의 통신을 위한 클라이언트 애플리케이션입니다. 이 프로젝트는 다양한 컨트롤러, 헬퍼, 모델, 뷰를 활용하여 사용자 인터페이스와 데이터를 처리하며, 인게임 세션 통신을 위한 클래스를 포함하고 있습니다.

## 주요 기능

- **컨트롤러**: 사용자 요청을 처리하고 적절한 뷰와 데이터를 반환합니다.
- **헬퍼 및 유틸리티**: 공통 기능을 수행하는 헬퍼 클래스와 유틸리티 함수들을 제공합니다.
- **모델**: 데이터 구조를 정의합니다.
- **뷰**: 사용자 인터페이스를 렌더링합니다.
- **인게임 세션 통신**: 인게임 통신을 위한 TCP 소켓 통신을 구현합니다.

## 기술 스택

- **프로그래밍 언어**: C#
- **프레임워크**: .NET Core
- **통신 프로토콜**: HTTP, WebSocket, TCP

## 프로젝트 구조

- `Controller`: MVC 컨트롤러 클래스
- `Helper`: 헬퍼 클래스 및 유틸리티 함수
- `InGame`: 인게임 세션 통신 관련 클래스 및 파일
- `Model`: 데이터 모델 클래스
- `SharedCodeLibrary`: 공유 코드 라이브러리
- `View`: 사용자 인터페이스 뷰 클래스

# SharedCode

## 개요
Client 프로젝트와 Server 프로젝트에서 서로 공유하는 코드입니다.

## 주요 기능

- **프로토콜**: 서버와 클라이언트 간의 공유하는 프로토콜입니다.
- **HttpCommand**: 서버와 클라이언트 간의 WebRequest 관련 로직 작성 시 사용되는 명령입니다.
- **Packet**: 서버와 클라이언트가 사용하는 패킷 구조 관련 클래스입니다.
- **기타 모델**: 서버와 클라이언트가 공유되는 모델 구조로, 주로 아이템이나 유저 데이터 등을 포함합니다.

## 전체 프로젝트 기능 요약

- Redis 서버 통합 및 초기화 구현
- HTTP 프로토콜로 주요 서비스 및 리포지토리 클래스 작성
- WebSocket으로 채팅 시스템 및 매칭 시스템 구현
- 인게임 TCP 소켓 실시간 통신 구현
- Swagger를 통한 API 문서화 설정
- 로그인 및 길드 관리 (HTTP 통신)
- 매칭 및 채팅 (WebSocket 통신)
- 인게임 (TCP 소켓 통신)

## 설명
롤 아레나를 재밌게 한 기억이 있어 참고하여 기초 틀만 작업해보았습니다.
