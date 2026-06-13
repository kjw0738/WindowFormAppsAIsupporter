# WinFormsAppAIsupporter

AI 기반의 효율적인 회의 지원 및 분석 도구입니다. 이 어플리케이션은 회의 녹음 파일을 분석하여 자동으로 텍스트로 변환(STT)하고, 회의 내용을 바탕으로 SWOT 분석 및 참여자별 역할 분배를 수행합니다.

## 🚀 주요 기능

- **음성 인식 (STT):** Groq의 `whisper-large-v3` 모델을 사용하여 회의 녹음 파일을 정확한 텍스트로 변환합니다.
- **SWOT 분석:** 회의의 핵심 내용을 분석하여 강점(Strengths), 약점(Weaknesses), 기회(Opportunities), 위협(Threats) 요소를 도출합니다.
- **지능형 역할 분배:** 참여 인원수에 맞춰 회의에서 논의된 업무를 지능적으로 그룹화하고 각 참여자에게 할당합니다.
- **결과 관리:** 생성된 역할 분배 결과를 직접 수정하고 저장할 수 있는 동적 UI를 제공합니다.
- **멀티미디어 지원:** `mp3`, `m4a`, `wav`, `mp4` 형식의 오디오 파일을 지원합니다.

## 🛠 기술 스택

- **Framework:** .NET 10.0 (Windows Forms)
- **AI Service:** Groq API (OpenAI Compatible SDK)
  - **LLM:** `llama-3.3-70b-versatile`
  - **STT:** `whisper-large-v3`
- **Libraries:**
  - `OpenAI` (v2.11.0)
  - `DotNetEnv` (v3.2.0)

## ⚙️ 설정 방법

### 1. 사전 요구 사항
- .NET 10.0 SDK 이상 설치
- Groq API 키 (OpenAI 호환 API 사용)

### 2. 환경 변수 설정
프로젝트 루트 폴더 또는 실행 파일 경로에 `.env` 파일을 생성하고 다음과 같이 API 키를 설정합니다.

```env
OPENAI_API_KEY=your_groq_api_key_here
```

### 3. 프로젝트 빌드 및 실행
```bash
# 리포지토리 복제
git clone [repository-url]

# 프로젝트 폴더 이동
cd WinFormsAppAIsupporter

# 프로젝트 빌드 및 실행
dotnet run
```

## 📖 사용 방법

1. **파일 첨부:** '회의 음성 파일 첨부' 버튼을 클릭하여 분석할 녹음 파일을 선택합니다.
2. **인원 설정:** 참여 인원수(Number of Participants)를 설정합니다.
3. **분석 시작:** '분석 시작' 버튼을 누르면 STT -> SWOT 분석 -> 역할 분배 과정이 진행됩니다.
4. **결과 확인:**
   - **SWOT 분석** 탭에서 회의의 전략적 분석 결과를 확인합니다.
   - **역할 분배** 탭에서 각 참여자에게 할당된 업무를 확인하고 필요시 수정합니다.

## 📁 프로젝트 구조

```
WinFormsAppAIsupporter/
├── AIService.cs          # Groq API 연동 및 AI 로직 처리
├── Form1.cs              # 메인 UI 및 이벤트 핸들링
├── Form1.Designer.cs     # UI 디자인 코드
├── Program.cs            # 어플리케이션 진입점
└── WinFormsAppAIsupporter.csproj  # 프로젝트 설정 및 종속성 관리
```

## 📝 라이선스
이 프로젝트는 교육 및 개인 프로젝트 용도로 작성되었습니다.
