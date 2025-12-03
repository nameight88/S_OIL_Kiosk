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

sequenceDiagram
    participant U as 사용자
    participant K as 키오스크<br/>제어 시스템
    participant D as DB<br/>데이터베이스
    participant PLC as PLC/함제어<br/>모터/센서 제어
    participant P as 결제기<br/>카드 리더
    participant PR as 프린터<br/>영수증 출력
    
    Note over U,PR:  시스템 초기화 및 장치 연결
    K->>K: 설정 파일 로드 (IniParser)
    K->>D: DB 연결 요청
    D-->>K: 연결 완료
    K->>PLC: PLC 연결 요청
    PLC-->>K: 준비 완료
    K->>P: 결제기 연결 요청
    P-->>K: 준비 완료
    K->>PR: 프린터 연결 요청
    PR-->>K: 준비 완료
    
    Note over U,PR:  거래 대기 및 시작
    K->>U: 메인 화면 표시
    U->>K: 화면 터치<br/>거래 시작
    K->>K: 음성 안내 (TTS) 시작
    K->>U: 상품 선택 화면 표시
    
    Note over U,PR:  결제 처리
    K->>U: 결제 화면 표시
    U->>K: 카드 삽입
    K->>P: 결제 요청<br/>시리얼 통신, 금액 포함
    P->>P: 카드 승인 처리
    
    alt 결제 성공
        P-->>K: 승인 완료
        K->>D: 거래 내역 저장
        D-->>K: 저장 완료
    else 결제 실패
        P-->>K: 승인 거부
        K->>D: 함 예약 해제
        D-->>K: 해제 완료
        K->>U: 결제 실패 안내
        K->>U: 메인 화면 복귀
        Note over U,PR: ✋ 프로세스 종료
    end
    
    Note over U,PR:  영수증 출력
    K->>K: 영수증 데이터 생성
    K->>PR: 출력 요청<br/>시리얼 통신
    PR->>PR: 영수증 인쇄
    PR-->>K: 출력 완료
    K->>U: 영수증 수령 안내
    
    Note over U,PR:  상품 출고/함 개방
    K->>PLC: 함 개방 명령<br/>소켓 통신
    PLC->>PLC: 모터 제어 신호
    PLC-->>K: 개방 완료
    K->>K: 음성 안내<br/>상품 수령
    K->>U: 수령 안내 화면
    
    Note over U,PR:  상품 수령 확인 및 문 닫힘 대기
    loop 함 문이 닫힐 때까지<br/>센서 상태 체크
        PLC->>K: 센서 상태 전송<br/>문 열림 감지
        K->>K: 문 닫힘 대기
    end
    
    PLC-->>K: 문 닫힘 감지
    K->>D: 함 상태 업데이트<br/>사용완료
    D-->>K: 업데이트 완료
    K->>K: 음성 안내<br/>감사 인사
    K->>U: 완료 화면 표시
    
    Note over U,PR:  거래 완료 및 복귀
    K->>K: 3초 대기
    K->>U: 메인 화면 복귀
  

