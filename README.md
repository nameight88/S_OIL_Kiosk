# 요소수 자동 판매함 키오스크 솔루션


## 사용 기술
	- C# (WinForms) 12.0 version
	- .NET Framework 4.8
	- MS SQL Server
	- Target Platform : X86


### 구조
	- Model : DB에 있는 데이터 인터페이스
	- Resource : 키오스크에 사용한 이미지
	- Services : DB 연결 및 쿼리 처리(DBConnector), forms 전환 처리 (Navigator), 각종 연동 및 체크 ApplicationContext
	- Forms : 실제 키오스크 UI  처음에 모든 forms 로드 후 ApplicationContext에서 관리
	- Device: 하드웨어 연동 및 제어 (BU/CU 함제어 - PLC, 영수증 프린터, 결재기 처리)
		- 결재기 : 스마트로 V-Cat 라이브러리 연동
			- 시리얼 포트 통신으로 연동
			- Axlnterop.SmtSndRcvVCATLib
			- interop.SmtSndRcvVCATLib
			- interop.WinHttp
			- Newonsoft.Json
			- PayControl
			- SmtSndRcvVCAT.ocx 를 등록을 진행을 해야함 (regsvr32 SmtSndRcvVCAT.ocx)
		-프린터 : HMK-072 프린터 사용
		    - 시리얼 포트 통신으로 연동
			- Printer.cs 에서 제어 및 영수증 출력 템플릿 사용
	- Utils : 각종 유틸리티 클래스
			- Funcs.cs : 공통 함수
			- Logger.cs : 로그 기록
			- SClient.cs : BU/CU 통신을 위한 클래스
			- StateObject.cs : 소켓 연동을 위한 클래스
			- IniParser.cs : ini 파일 파싱 클래스
			- ImageButton.cs : 이미지 버튼 커스텀 컨트롤
			- LockerButton : 해당 함 클릭 및 처리에 대한 커스텀 클래스
			- SoundManager.cs : 음성 안내(TTS) 및 효과음 재생 관리 클래스
				- System.Speech 라이브러리 사용
				- 싱글톤 패턴으로 전역 접근 가능
				- 한국어 음성 지원 (설치된 경우)
				- 비동기 재생 지원

### 아키텍처 패턴
	-	싱글톤 패턴 - DBConnector, 장치 관리 클래스
	-	서비스 레이어 - ReceiptService, DBConnector
	-	컨텍스트 패턴 - ApplicationContext로 전역 상태 관리
	-	이벤트 기반 - Windows Forms 이벤트 모델

	s-oil 프로젝트
	├── UI Layer (Windows Forms)
	│   ├── Customer Forms (고객용 화면)
	│   └── Admin Forms (관리자 화면)
	├──  Services Layer
	│   ├── DBConnector (데이터베이스 연동)
	│   ├── ReceiptService (영수증 출력)
	│   └── ApplicationContext (앱 상태 관리)
	├──  Device Layer
	│   ├── Printer (시리얼 통신 영수증 프린터)
	│   ├── BU (PLC 사물함 제어)
	│   └── PayControl (결제 단말기)
	└──  Utils Layer
		├── Logger (파일 로깅)
		├── IniParser (설정 파일 파싱)
		├── LockerButton (커스텀 UI 컨트롤)
		└── SoundManager (음성 안내 TTS)
