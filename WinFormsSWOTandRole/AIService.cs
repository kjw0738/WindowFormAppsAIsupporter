using OpenAI;
using OpenAI.Chat;
using OpenAI.Audio;
using DotNetEnv;
using System.ClientModel;

namespace WinFormsAppAIsupporter
{
    public class AIService
    {
        private readonly ChatClient _chatClient;
        private readonly AudioClient _audioClient;

        public AIService()
        {
            // .env 파일이 실행 파일과 같은 폴더에 있는지 확인하고 로드
            string envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
            if (File.Exists(envPath))
            {
                Env.Load(envPath);
            }
            else
            {
                // 현재 작업 디렉토리에서도 시도
                Env.Load();
            }
            
            string? apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            
            if (string.IsNullOrEmpty(apiKey) || apiKey == "your_api_key_here")
            {
                throw new InvalidOperationException("API 키를 찾을 수 없습니다. .env 파일을 확인해주세요.");
            }

            // Groq API 설정을 위한 옵션
            // Groq은 OpenAI와 호환되므로 엔드포인트를 Groq으로 설정합니다.
            var options = new OpenAIClientOptions
            {
                Endpoint = new Uri("https://api.groq.com/openai/v1")
            };

            // OpenAI 클라이언트 초기화 (Groq 엔드포인트 사용)
            var client = new OpenAIClient(new ApiKeyCredential(apiKey), options);
            
            // Groq의 고성능 모델 사용 (llama-3.3-70b-versatile)
            _chatClient = client.GetChatClient("llama-3.3-70b-versatile"); 
            
            // Groq의 음성 인식 모델 사용 (whisper-large-v3)
            _audioClient = client.GetAudioClient("whisper-large-v3");
        }

        /// <summary>
        /// 음성 파일을 텍스트로 변환(STT)합니다.
        /// </summary>
        public async Task<string> TranscribeAudioAsync(string filePath)
        {
            // Groq 음성 인식 호출
            AudioTranscription transcription = await _audioClient.TranscribeAudioAsync(filePath);
            return transcription.Text;
        }

        /// <summary>
        /// 회의록 내용을 바탕으로 SWOT 분석을 수행합니다.
        /// </summary>
        public async Task<string> AnalyzeSwotAsync(string meetingContent)
        {
            var prompt = $@"
다음 회의 내용을 바탕으로 SWOT 분석을 수행해줘.
반드시 모든 답변을 **한국어**로만 작성해. 영어, 아랍어, 한자 등 다른 언어는 절대 사용하지 마.

불필요한 설명이나 인사말 없이 오직 아래의 태그 형식으로만 답변해줘.
각 항목은 반드시 줄바꿈을 포함한 불렛 포인트(*) 형식으로 작성해줘.

[S]
* (강점 내용 1)
* (강점 내용 2)
[W]
* (약점 내용 1)
* (약점 내용 2)
[O]
* (기회 내용 1)
* (기회 내용 2)
[T]
* (위협 내용 1)
* (위협 내용 2)

회의 내용:
{meetingContent}
";

            ChatCompletion completion = await _chatClient.CompleteChatAsync(prompt);
            return completion.Content[0].Text;
        }

        /// <summary>
        /// 회의록 내용과 참여 인원수를 바탕으로 지능형 역할 분배를 수행합니다.
        /// </summary>
        public async Task<string> DistributeRolesAsync(string meetingContent, int participantCount)
        {
            var prompt = $@"
다음 회의 내용을 바탕으로 프로젝트 참여 인원 {participantCount}명에게 업무를 분배해줘.
반드시 모든 답변을 **한국어**로만 작성해. 불필요한 서론/결론은 생략해.

[가이드라인]
1. 명시적 할당: 회의 중 특정 인물에게 배정된 업무는 그대로 반영해.
2. 지능적 그룹화: 정해지지 않은 나머지 업무들은 연관성 있는 기능끼리 그룹화해줘.
3. 결과 형식: 각 업무는 반드시 '-' 기호로 시작하고, 한 줄에 한 업무씩 줄바꿈을 사용하여 작성해줘.
반드시 참여자 번호에 맞춰 아래의 태그 형식을 사용해줘.

[P1]
- 업무 내용 1
- 업무 내용 2
[P2]
- 업무 내용 1
- 업무 내용 2
... (인원수 {participantCount}까지 반복)

회의 내용:
{meetingContent}
";

            ChatCompletion completion = await _chatClient.CompleteChatAsync(prompt);
            return completion.Content[0].Text;
        }
    }
}
