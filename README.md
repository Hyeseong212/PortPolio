
# 프로젝트 아키텍쳐
![image](https://github.com/user-attachments/assets/717af687-0026-48aa-b857-8d4884dc3f2e)


# 📃프로젝트 정보

League Of Legends에서 아레나 방식 게임방식식을 착안하여
C# ASP.NET와 MySQL, Unity 로 유저인벤토리, 레이팅 시스템, 채팅, 길드시스템, 인게임등을 REST API 소켓통신으로 구현하였습니다.

Rest API 메인서버와 TCP 인게임 서버를 별도의 프로세스를 두어 인게임의 내부로직이 메인서버의 진행을 막지않아 안정성을 강화 및 의존성을 떨어트려 유지보수를 쉽게 하였습니다.

#📹 영상링크

[![유튜브 영상](https://img.youtube.com/vi/-K1Rn2rFDRA/0.jpg)](https://youtu.be/-K1Rn2rFDRA)

## 제작 기간
2024-04-15 ~ 2024-07-15

# RESTful API 메인서버


## RESTful API 메인서버 구조도
![image](https://github.com/user-attachments/assets/bf87c150-bcae-4c41-b954-26f29554113f)

## 주요 기능

- **Redis**: 클라이언트에서 접속시 유저 로그인세션을 보관하고 3분마다 체크하여 응답이없을시 세션키를 expire합니다.
- **다양한 서비스 및 리포지토리 패턴**: 여러 서비스 (`AccountService`, `ShopService`, `InventoryService`, `RankService`, `GuildService`, `SessionService`, `WebSocketChatService`, `WebSocketMatchService`, `WebSocketLoginService`)와 리포지토리를 싱글톤 패턴으로 DI 컨테이너에 등록하여 사용합니다.
- **Account Repository(MySQL)**: 영구적으로 보관해야 할 데이터들은 MySQL DBMS에서 정규화하여 저장합니다.
- **API 문서화 (Swagger)**: Swagger를 통해 API 문서를 자동으로 생성하고 관리합니다.

## 기술 스택

- **프로그래밍 언어**: C#
- **프레임워크**: .NET Core
- **데이터베이스**: Redis, MySql
- **API 문서화**: Swagger
- **빌드 도구**: Visual Studio

## RESTful API 메인서버 기능 요약

- Redis로 유저 세션관리
- HTTP 프로토콜로 주요 서비스 및 리포지토리 클래스 작성
- Swagger를 통한 API 문서화 설정
- 로그인 및 길드 관리 (HTTP 통신)
- Task를 사용하여 비동기 프로그래밍 및 쓰레드풀로 고성능으로 작동
- lock, semaphore를 이용한 동시성처리

# WebSocket 메인서버

## WebSocket 메인서버 구조도
![image](https://github.com/user-attachments/assets/91d5ffee-2aa3-49f6-a119-d3d5a15d7972)

## 주요 기능

- **채팅시스템**: 길드가입유저들끼리 소통할수있는 길드채팅, 로비에있는 유저들과 대화할수있는 일반채팅, 친구에게 귓속말을 보낼수있는 귓속말 채팅을 구현하였습니다.
- **매칭시스템**: 일반게임과 랭크게임을 매칭하여 클라이언트에게 응답을 줍니다. 랭크게임의 경우 유저의 레이팅을 MySQL에서 조회하여 레이팅에 맞게끔 범위를 잡아 구현하였습니다
(추후에 유저의레이팅같은 경우는 Redis로 캐싱하여 쓸수있도록 리팩토링 예정입니다.)
- **Account Repository(MySQL)**: 유저들의 정보를 조회합니다.

## 기술 스택

- **프로그래밍 언어**: C#
- **프레임워크**: .NET Core
- **데이터베이스**: Redis, MySql
- **빌드 도구**: Visual Studio

## 서버 프로젝트 기능 요약

- Redis로 유저 세션관리
- WebSocket으로 채팅 시스템 및 매칭 시스템 구현
- 매칭 및 채팅
- Task를 사용하여 비동기 프로그래밍 및 쓰레드풀로 고성능으로 작동

# In Game 세션 서버

## In Game 세션 서버 구조도
![image](https://github.com/user-attachments/assets/21cbc522-abdc-47ea-974c-1a9e0514208b)


## 주요 기능

- **인게임 동기화**: 인게임에서 환경요소 동기화, 유저 캐릭터들의 상태 동기화를 담당합니다
- **인게임 유저 데이터 동기화**: 세션에 접속한 유저들의 데이터를 보관하고 결과처리를 담당합니다

## 기술 스택

- **프로그래밍 언어**: C#
- **프레임워크**: .NET Core
- **빌드 도구**: Visual Studio

## 서버 프로젝트 기능 요약

- 인게임 환경 동기화 및 유저 게임결과처리
- Task를 사용하여 비동기 프로그래밍 및 쓰레드풀로 고성능으로 작동
- lock, semaphore를 이용한 동시성처리

# Client 프로젝트

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

## Client 프로젝트 구조

- `Controller`: MVC 컨트롤러 클래스
- `Helper`: 헬퍼 클래스 및 유틸리티 함수
- `InGame`: 인게임 세션 통신 관련 클래스 및 파일
- `Model`: 데이터 모델 클래스
- `SharedCodeLibrary`: 공유 코드 라이브러리
- `View`: 사용자 인터페이스 뷰 클래스

# SharedCode

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
