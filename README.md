# LostArk Raid CalenderBot
로스트아크 레이드 노션캘린더 연동용 디스코드 봇

# 개요

디스코드에서 !4인일정 또는 !8인일정 명령어를 입력하면 레이드 신청 메세지를 출력하고, Notion Calendar에 해당 정보를 연동합니다.

![image](https://user-images.githubusercontent.com/94309745/142360464-5bd414c8-f920-4963-bffb-d7af79438e3a.png)
![image](https://user-images.githubusercontent.com/94309745/142360536-13d44600-96b6-44b9-b41a-86864b36c0c5.png)


## 명령어

- !4인일정 Title:"[타이틀]" Time:"YY-MM-DD HH시 ss분"
  - 4인 레이드 일정을 입력합니다.
  - ***Title, Time 입력시 큰따옴표를 누락하시면 안됩니다!!!***
  - 예시
    - !4인일정 Title: "쿠크세이튼 노말" Time: "21-11-21 14시 30분"

- !8인일정 Title:"[타이틀]" Time:"YY-MM-DD HH시 ss분"
  - 8인 레이드 일정을 입력합니다.
  - ***Title, Time 입력시 큰따옴표를 누락하시면 안됩니다!!!***
  - 예시
    - !8인일정 Title: "비아키스 하드" Time: "21-11-21 18시 00분"

명령어 입력 후 봇이 보낸 메세지의 리액션 이모티콘을 눌러 참여를 신청합니다.

![image](https://user-images.githubusercontent.com/94309745/142360941-b2953c2b-0f84-4d91-8f51-088ab3146714.png)

## 봇 설정하기

- 디스코드에 봇 등록은 공식 가이드라인을 참고하세요. [디스코드 개발자 포탈](https://discord.com/developers/applications)
  - 봇의 API Token을 [Settings.cs](https://github.com/DippingSauce101/LostArk-Raid-CalendarBot/blob/81c917716e2f3bce482d6b1d40ef84e110b12103/DiscordLostArkBot/Constants/Settings.cs#L5) 파일의 DiscordBotToken에 복붙하세요.
- 노션 API 통합은 다음과 같이 진행합니다.
  - https://developers.notion.com/ 접속
  - 우측 위 My Integrations 클릭
  - ![image](https://user-images.githubusercontent.com/94309745/142361414-a8cab82a-652d-4e41-a7b8-2f85168b1fb8.png)
  - ![image](https://user-images.githubusercontent.com/94309745/142361493-4f334e95-3773-4234-856f-5d275ad81be1.png)
  - Create New Integration 클릭하여 내 워크스페이스 연결
  - ![image](https://user-images.githubusercontent.com/94309745/142361543-a0670199-ad6a-4246-a4e4-ccbf1754e272.png)
  - 위 값을 복사하여 [Settings.cs](https://github.com/DippingSauce101/LostArk-Raid-CalendarBot/blob/81c917716e2f3bce482d6b1d40ef84e110b12103/DiscordLostArkBot/Constants/Settings.cs#L6) 파일의 NotionApiAutoToken에 복붙
    - ![image](https://user-images.githubusercontent.com/94309745/142361641-58401995-32f8-40d5-985a-733d1db13bf3.png)
  - 노션의 캘린더를 열어보면 다음과 같은 형식입니다.
    - https://www.notion.so/[내_아이디]/[캘린더_ID]?v=[캘린더_뷰_ID]
    - 여기서 중간의 [캘린더_ID]를 복사해 [Settings.cs](https://github.com/DippingSauce101/LostArk-Raid-CalendarBot/blob/81c917716e2f3bce482d6b1d40ef84e110b12103/DiscordLostArkBot/Constants/Settings.cs#L7) 파일의 NotionCalendarDbId에 복붙하시면 끝.



