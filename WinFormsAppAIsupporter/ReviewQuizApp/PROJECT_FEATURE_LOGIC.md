# 6번 파트: 과제 복습 퀴즈 모듈 기능 및 통합 가이드

## 1. 문서 범위

이 문서는 팀 프로젝트의 6번 역할인 `복습 퀴즈 모듈 & QA 엔지니어` 파트만 설명한다.

방금 진행한 메인 앱 연결 작업은 통합 가능성을 확인하기 위한 테스트였고, 이 문서에서는 내가 개발한 퀴즈 파트의 기능, 내부 로직, 데이터 흐름, 메인 앱에서 접근하는 방법만 정리한다.

## 2. 파트 개요

과제 복습 퀴즈 모듈은 과제 JSON 데이터를 기반으로 AI가 복습 퀴즈를 생성하고, 사용자가 퀴즈를 풀고, 결과와 풀이 기록을 조회할 수 있게 하는 WinForms 모듈이다.

이 모듈의 핵심 전제는 다음과 같다.

- 퀴즈는 과제 JSON 데이터가 있어야 생성된다.
- AI API 키가 없으면 퀴즈 생성 기능은 비활성화된다.
- 생성된 문제는 과제 JSON 내용에 근거해야 한다.
- 생성 목적에서 다른 언어를 요청하지 않으면 문제는 기본적으로 한국어로 생성된다.

## 3. 사용자 기능

### 3.1 과제 선택

프로그램은 `Data/assignments` 폴더에 있는 JSON 파일을 과제 데이터로 읽는다.

과제 JSON의 내부 구조는 고정하지 않았다. AI가 JSON 전체 내용을 보고 과제 정보를 해석한 뒤 퀴즈를 만들도록 설계했다.

### 3.2 생성 목적 선택

사용자는 퀴즈 생성 목적을 직접 입력하거나, 미리 제공되는 예시 형식 중 하나를 선택할 수 있다.

현재 제공되는 예시 형식은 다음과 같다.

- 보고서/레포트 핵심 이해
- 발표/프레젠테이션 점검
- 코딩/실습 과제 검토
- 중간/기말 시험 대비
- 토론/논술 과제 준비

예시 형식을 선택하면 생성 목적 입력칸에 문장이 자동으로 들어가며, 사용자가 직접 수정할 수도 있다.

### 3.3 문제 조건 설정

사용자는 다음 조건을 설정할 수 있다.

- 문제 유형: O/X, 객관식, 주관식
- 문항 수: 1개부터 15개까지
- 난이도: 쉬움, 보통, 어려움

문제 유형은 여러 개를 동시에 선택할 수 있다.

### 3.4 AI 퀴즈 생성

`퀴즈 생성` 버튼을 누르면 선택한 과제 JSON, 생성 목적, 문제 유형, 문항 수, 난이도를 AI API에 전달한다.

AI 프롬프트에는 다음 제한이 포함된다.

- 생성 목적에서 다른 언어를 명시하지 않으면 한국어로 생성한다.
- 모든 문제는 과제 JSON의 제목과 내용에만 근거해야 한다.
- 과제 JSON에 없는 일반 지식, 추정, 외부 사실은 문제에 사용할 수 없다.
- 문제, 정답, 선택지, 해설, 태그는 과제 JSON에서 뒷받침되어야 한다.
- O/X 문제는 선택지를 `O`, `X`로 구성한다.
- 객관식 문제는 가능한 경우 4개 이상의 선택지를 제공한다.
- 주관식 문제는 채점 가능한 짧은 정답을 제공한다.

### 3.5 퀴즈 풀이

생성된 퀴즈는 한 문항씩 풀이할 수 있다.

- O/X와 객관식은 선택지 버튼으로 답을 선택한다.
- 주관식은 텍스트 입력칸에 답안을 입력한다.
- `정답 확인` 버튼을 누르면 결과, 정답, 해설이 표시된다.
- `이전`, `다음` 버튼으로 문항을 이동한다.

### 3.6 제출 및 결과

사용자가 `제출` 버튼을 누르면 전체 문항을 채점하고 결과창을 표시한다.

미응답 문항이 있으면 제출 전 확인 메시지를 표시한다. 결과창을 닫으면 자동으로 설정 화면으로 돌아간다.

### 3.7 기록 조회 및 다시 풀기

퀴즈 제출 결과는 기록으로 저장된다.

기록 조회 화면에서는 다음 정보를 볼 수 있다.

- 풀이일
- 퀴즈명
- 과제명
- 점수
- 문항별 사용자 답안
- 문항별 정답
- 정답/오답 여부

저장된 퀴즈 스냅샷이 있으면 같은 퀴즈를 다시 풀 수 있다.

### 3.8 기록 삭제

기록 조회 화면에서는 기록을 삭제할 수 있다.

- `선택 삭제`: 선택한 기록 1개와 연결된 퀴즈 스냅샷을 삭제한다.
- `전체 삭제`: 모든 기록과 퀴즈 스냅샷을 삭제한다.

## 4. 개발 구조

주요 파일과 역할은 다음과 같다.

- `Form1.cs`: 퀴즈 설정, 생성, 풀이, 기록 조회 화면을 구성하고 전체 흐름을 제어한다.
- `Controls/ReviewQuizModuleControl.cs`: 팀 메인 앱에 퀴즈 모듈을 붙이기 위한 UserControl 진입점이다.
- `Forms/QuizResultForm.cs`: 퀴즈 제출 후 결과창을 표시한다.
- `Models/AssignmentInfo.cs`: 과제 JSON 파일 정보를 담는다.
- `Models/QuizGenerationRequest.cs`: AI 퀴즈 생성 요청 조건을 담는다.
- `Models/QuizDataFile.cs`: 생성된 퀴즈 전체 데이터를 담는다.
- `Models/QuizQuestion.cs`: 개별 문항 정보를 담는다.
- `Models/QuizResultRecord.cs`: 풀이 결과 기록을 담는다.
- `Models/QuizAnswerRecord.cs`: 문항별 사용자 답안 기록을 담는다.
- `Services/AssignmentRepository.cs`: `Data/assignments`의 과제 JSON 파일을 읽는다.
- `Services/AiQuizGeneratorConfig.cs`: API 키와 AI 제공자를 판단한다.
- `Services/AssignmentQuizGenerator.cs`: API 키 설정 여부를 확인하고 AI 생성기로 요청을 넘긴다.
- `Services/InternalAiAssignmentQuizGenerator.cs`: AI 프롬프트 구성, API 호출, JSON 응답 파싱을 담당한다.
- `Services/QuizDataLoader.cs`: 생성된 퀴즈 JSON을 검증하고 정규화한다.
- `Services/QuizHistoryService.cs`: 퀴즈 결과 기록과 퀴즈 스냅샷을 저장, 조회, 삭제한다.
- `Services/QuizJson.cs`: JSON 직렬화 옵션을 관리한다.

## 5. 데이터 흐름

기본 흐름은 다음과 같다.

1. 프로그램 시작
2. API 키 환경 변수 확인
3. `Data/assignments` 폴더에서 과제 JSON 로드
4. 사용자가 과제와 퀴즈 조건 선택
5. `QuizGenerationRequest` 생성
6. AI API 호출
7. AI 응답 JSON을 `QuizDataFile`로 변환
8. `QuizDataLoader`로 퀴즈 데이터 검증
9. 퀴즈 풀이 화면 표시
10. 사용자 답안 저장
11. 제출 시 점수 계산
12. 결과 기록과 퀴즈 스냅샷 저장

## 6. API 키 처리

API 키는 소스 코드에 직접 저장하지 않는다.

다음 환경 변수를 순서대로 확인한다.

1. `GROQ_API_KEY`
2. `XAI_API_KEY`
3. `GEMINI_API_KEY`
4. `OPENAI_API_KEY`

환경 변수 범위는 다음 순서로 확인한다.

1. Process
2. User
3. Machine

`XAI_API_KEY` 값이 `gsk_`로 시작하면 xAI가 아니라 GroqCloud 키로 판단해 Groq API로 처리한다.

API 키가 없으면 퀴즈 모듈은 기능을 비활성화하고 경고 메시지를 표시한다.

## 7. 저장 구조

저장 위치는 실행 파일 기준 `Data` 폴더 아래에 구성된다.

- 과제 입력: `Data/assignments/*.json`
- 최신 생성 퀴즈: `Data/generated-quiz-latest.json`
- 풀이 기록: `Data/history/quiz-history.json`
- 다시 풀기용 퀴즈 스냅샷: `Data/generated-quizzes/*.json`

개별 기록 삭제 시에는 기록 JSON에서 해당 항목을 제거하고, 연결된 퀴즈 스냅샷도 함께 삭제한다.

## 8. 메인 앱에서 접근하는 방법

팀 메인 앱에는 사이드바와 콘텐츠 패널이 있고, `복습 퀴즈` 버튼을 눌렀을 때 퀴즈 파트가 표시되는 구조가 가장 자연스럽다.

메인 앱에서 우리 파트에 접근하는 권장 방식은 `ReviewQuizModuleControl`을 `panelQuizView`에 붙이는 것이다.

예시 코드는 다음과 같다.

```csharp
using ReviewQuizApp.Controls;

private ReviewQuizModuleControl? _quizModule;

private void btnQuiz_Click(object? sender, EventArgs e)
{
    ResetMenuButtons();
    btnQuiz.BackColor = Color.Khaki;
    EnsureQuizModuleLoaded();
    ShowView(panelQuizView);
}

private void EnsureQuizModuleLoaded()
{
    if (_quizModule != null)
    {
        return;
    }

    _quizModule = new ReviewQuizModuleControl
    {
        Dock = DockStyle.Fill
    };

    panelQuizView.Controls.Add(_quizModule);
}
```

이 방식은 기존 퀴즈 폼을 통합용 UserControl 안에 넣어 메인 앱의 패널 구조에 맞게 표시한다.

통합 시 주의할 점은 다음과 같다.

- 메인 앱의 진입점은 하나만 있어야 하므로 퀴즈 파트의 `Program.cs`는 직접 병합 대상에서 제외하거나 조정해야 한다.
- 메인 앱이 `ReviewQuizApp` 하위 소스를 직접 컴파일하지 않도록 `ProjectReference` 방식으로 연결하는 것이 안전하다.
- 메인 앱의 콘텐츠 패널 안에 들어갈 때 스크롤이나 잘림이 생기지 않도록 `ReviewQuizModuleControl`은 내부 폼의 `MinimumSize`를 해제한다.
- 과제 JSON 샘플이 메인 앱 실행 폴더의 `Data/assignments`로 복사되도록 설정해야 한다.

## 9. 현재 적용된 통합 보조 처리

우리 파트에는 메인 앱 통합을 위해 다음 보조 처리가 들어가 있다.

- `ReviewQuizModuleControl` 추가
- 내부 퀴즈 폼을 border 없는 embedded form으로 표시
- embedded form의 최소 크기 해제
- 팀 프로젝트와 맞춘 `net10.0-windows` 타겟
- 메인 앱 콘텐츠 패널 안에서 잘리지 않도록 여백과 고정 높이 축소

## 10. 유지보수 원칙

우리 퀴즈 파트의 기능, 로직, 저장 구조, API 처리, UI 흐름이 변경될 경우 이 문서도 함께 업데이트한다.

문서 갱신 대상 예시는 다음과 같다.

- 사용자 화면 흐름 변경
- AI 프롬프트 규칙 변경
- API 키 또는 제공자 처리 방식 변경
- 과제 JSON 또는 퀴즈 JSON 저장 구조 변경
- 기록 저장, 조회, 삭제 로직 변경
- 메인 앱 접근 방식 변경
- 예외 처리 및 경고 메시지 변경

