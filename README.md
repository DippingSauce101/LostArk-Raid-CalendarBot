# LostArk Raid CalenderBot
로스트아크 레이드 노션캘린더 연동용 디스코드 봇

# 개요

디스코드에서 !4인일정 또는 !8인일정 명령어를 입력하면 레이드 신청 메세지를 출력하고, Notion Calendar에 해당 정보를 연동합니다.

![image](https://user-images.githubusercontent.com/94309745/145355772-779a4ac1-34eb-4b68-99dd-fe8106413b34.png)
![image](https://user-images.githubusercontent.com/94309745/142360536-13d44600-96b6-44b9-b41a-86864b36c0c5.png)


## 명령어

- !4인일정 레이드이름(레이드시간)
  - 4인 레이드 일정을 입력합니다.
  - 예시
    - !4인일정 쿠크하드(21/12/13 20:00)

- !8인일정 레이드이름(레이드시간)
  - 8인 레이드 일정을 입력합니다.
  - 예시
    - !8인일정 비아노말(12/30 오후 4시)

명령어 입력 후 봇이 보낸 메세지의 리액션 이모티콘을 눌러 참여를 신청합니다.

![image](https://user-images.githubusercontent.com/94309745/142360941-b2953c2b-0f84-4d91-8f51-088ab3146714.png)

## 봇 설정하기

- 디스코드에 봇 등록은 공식 가이드라인을 참고하세요. [디스코드 개발자 포탈](https://discord.com/developers/applications)
- 노션 API 통합은 다음과 같이 진행합니다.
  - https://developers.notion.com/ 접속
  - 우측 위 My Integrations 클릭
  - ![image](https://user-images.githubusercontent.com/94309745/142361414-a8cab82a-652d-4e41-a7b8-2f85168b1fb8.png)
  - ![image](https://user-images.githubusercontent.com/94309745/142361493-4f334e95-3773-4234-856f-5d275ad81be1.png)
  - Create New Integration 클릭하여 내 워크스페이스 연결
  - 노션의 API 접근자를 초대합니다.
    - ![image](https://user-images.githubusercontent.com/94309745/144707565-06f484d0-e33b-49aa-84d3-713e0490ee70.png)
  - ***노션 캘린더에 '레이드명', '참가자', '날짜' 프로퍼티를 추가해야합니다.***
    - ![image](https://user-images.githubusercontent.com/94309745/144707652-4453c68d-e268-46ee-8ad3-3213cea8d287.png)

## Tokens.ini

- 디스코드 및 노션 클라이언트가 접속하기 위해 Token 값들이 필요합니다.
- .exe 파일이 빌드된 경로에 Tokens.ini 파일을 생성한 뒤 값을 입력합니다.
  - DiscordBotToken
    - 디스코드 봇의 Auth Token 값을 입력합니다.
    - ![image](https://user-images.githubusercontent.com/94309745/145357508-6a8b6982-711b-47aa-8ca8-2e9d5b977bba.png)
  - NotionApiAuthToken
    - ![image](https://user-images.githubusercontent.com/94309745/145357658-ed0cd11d-c27f-49be-ab10-efaf0a7a10e7.png)
  - NotionCalendarDBId
    - 노션의 캘린더를 열어보면 다음과 같은 형식입니다.
    - https://www.notion.so/[내_아이디]/[캘린더_ID]?v=[캘린더_뷰_ID]
    - 중간의 [캘린더_ID] 부분을 붙여넣으세요.
  - NotionCalendarUrl
    - 노션 캘린더 링크 값입니다.
