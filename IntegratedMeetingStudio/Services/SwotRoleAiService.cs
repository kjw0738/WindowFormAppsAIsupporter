using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Audio;
using System.ClientModel;
using System.Text.Json;
using IntegratedMeetingStudio.Models;

namespace IntegratedMeetingStudio.Services
{
    public class SwotRoleAiService
    {
        private readonly ChatClient _chatClient;
        private readonly AudioClient _audioClient;

        public SwotRoleAiService()
        {
            var config = AiConfiguration.Load();
            string? apiKey = config.ApiKey;
            
            if (string.IsNullOrEmpty(apiKey) || apiKey == "your_api_key_here")
            {
                throw new InvalidOperationException("API 키를 찾을 수 없습니다. 설정에서 API 키를 입력해주세요.");
            }

            var options = new OpenAIClientOptions
            {
                Endpoint = new Uri("https://api.groq.com/openai/v1")
            };

            var client = new OpenAIClient(new ApiKeyCredential(apiKey), options);
            
            _chatClient = client.GetChatClient("llama-3.3-70b-versatile"); 
            _audioClient = client.GetAudioClient("whisper-large-v3");
        }

        public async Task<string> TranscribeAudioAsync(string filePath)
        {
            AudioTranscription transcription = await _audioClient.TranscribeAudioAsync(filePath);
            return transcription.Text;
        }

        public async Task<SwotAnalysisResult?> AnalyzeSwotAsync(string meetingContent)
        {
            var prompt = $@"
다음 회의 내용을 바탕으로 SWOT 분석을 진행해주세요.
내용(Value)은 반드시 **한국어**로만 작성하되, JSON 키(Key)는 반드시 **영어(Strengths, Weaknesses, Opportunities, Threats)**를 유지하세요.

[가이드라인]
1. 각 항목별로 핵심적인 내용을 2~4개씩 도출하세요.
2. 분석 내용은 간결하고 명확하게 작성하세요.
3. 결과 형식: 반드시 오직 JSON 형식으로만 응답해주세요.

출력 예시 JSON:
{{
    ""Strengths"": [""강점 1"", ""강점 2""],
    ""Weaknesses"": [""약점 1"", ""약점 2""],
    ""Opportunities"": [""기회 1"", ""기회 2""],
    ""Threats"": [""위협 1"", ""위협 2""]
}}

회의 내용:
{meetingContent}
";
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a helpful assistant that acts as a business analyst and outputs strictly in JSON format."),
                new UserChatMessage(prompt)
            };

            var options = new ChatCompletionOptions { Temperature = 0.5f };

            try
            {
                var response = await _chatClient.CompleteChatAsync(messages, options);
                var text = response.Value.Content[0].Text;
                
                int startIndex = text.IndexOf('{');
                int endIndex = text.LastIndexOf('}');
                if (startIndex >= 0 && endIndex > startIndex)
                {
                    string jsonString = text.Substring(startIndex, endIndex - startIndex + 1);
                    var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<SwotAnalysisResult>(jsonString, jsonOptions);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SWOT Error: {ex.Message}");
                return null;
            }
        }

        public async Task<List<RoleDistributionResult>> DistributeRolesAsync(string meetingContent, List<string> participants)
        {
            if (participants == null || participants.Count == 0)
                participants = new List<string> { "참여자 1", "참여자 2", "참여자 3" };

            string participantsListString = string.Join(", ", participants);

            var prompt = $@"
다음 회의 내용을 바탕으로 다음의 참여자 명단에게 업무를 분배해주세요: {participantsListString}
내용(Value)은 반드시 **한국어**로만 작성하되, JSON 키(Key)는 반드시 **영어(Participant, Roles)**를 유지하세요.

[가이드라인]
1. 명시적 할당: 지정된 참여자 명단({participantsListString})에 존재하는 사람에게만 역할을 할당하세요.
2. 기능적 그룹: 정해지지 않은 나머지 업무들은 연관성 있는 기능끼리 그룹화하여 이들에게 적절히 배분하세요.
3. 상세한 업무 내용 작성: 회의 원문을 바탕으로 구체적인 맥락, 목표, 기대 결과가 포함되도록 **상세하고 명확하게 행동 지향적인 문장**으로 업무 내용을 길게 작성하세요. (예: 단순히 ""자료 조사""가 아니라 ""XX 프로젝트 런칭을 위한 타사 레퍼런스 자료 조사 및 초안 작성"" 처럼 작성)
4. 결과 형식: 반드시 다음 JSON 배열(Array) 형식으로만 응답해주세요.

출력 예시 JSON:
[
  {{
    ""Participant"": ""홍길동"",
    ""Roles"": [""업무 내용 1"", ""업무 내용 2""]
  }}
]

회의 내용:
";
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a helpful assistant that outputs strictly in JSON format."),
                new UserChatMessage(prompt)
            };

            var options = new ChatCompletionOptions { Temperature = 0.5f };

            try
            {
                var response = await _chatClient.CompleteChatAsync(messages, options);
                var text = response.Value.Content[0].Text;
                
                int startIndex = text.IndexOf('[');
                int endIndex = text.LastIndexOf(']');
                if (startIndex >= 0 && endIndex > startIndex)
                {
                    string jsonString = text.Substring(startIndex, endIndex - startIndex + 1);
                    Console.WriteLine("AI JSON Response:");
                    Console.WriteLine(jsonString);
                    var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<List<RoleDistributionResult>>(jsonString, jsonOptions) ?? new List<RoleDistributionResult>();
                }
                Console.WriteLine("Could not find JSON array in response.");
                Console.WriteLine("Raw text:");
                Console.WriteLine(text);
                return new List<RoleDistributionResult>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Role Error: {ex.Message}");
                return new List<RoleDistributionResult>();
            }
        }

        public async Task<List<QuizQuestion>> GenerateQuizAsync(string meetingContent)
        {
            var prompt = $@"
아래 회의록 내용을 바탕으로 팀원들의 이해도를 확인할 수 있는 복습 질문 3개를 생성해줘.
응답은 반드시 아래 JSON 형식을 지켜줘:
{{
  ""quizzes"": [
    {{ ""question"": ""질문 내용"", ""answer"": ""정답"", ""explanation"": ""해설"" }}
  ]
}}
*중요: 응답은 반드시 {{로 시작해서 }}로 끝나는 순수 JSON 데이터만 보내주세요.*

회의록 내용:
{meetingContent}";

            ChatCompletion completion = await _chatClient.CompleteChatAsync(prompt);
            string rawResponse = completion.Content[0].Text;
            
            int start = rawResponse.IndexOf('{');
            int end = rawResponse.LastIndexOf('}');
            if (start >= 0 && end > start)
            {
                rawResponse = rawResponse.Substring(start, end - start + 1);
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(rawResponse);
                if (doc.RootElement.TryGetProperty("quizzes", out JsonElement quizzesElement))
                {
                    return JsonSerializer.Deserialize<List<QuizQuestion>>(quizzesElement.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<QuizQuestion>();
                }
            }
            catch {}
            
            return new List<QuizQuestion>();
        }

        public async Task<List<TaskItem>> ExtractTasksAsync(string meetingContent, string rolesJson = null)
        {
            var prompt = $@"
아래 전달된 '분배된 역할(Role Distribution)' 정보만을 바탕으로 칸반 보드에 등록할 업무(To-Do)를 추출해주세요.
원문 내용 없이, 오직 제공된 역할 분배 JSON 데이터를 파싱하여 각 인원별로 할당된 업무를 칸반 보드 태스크로 분리 및 생성해야 합니다.

가이드라인:
1. 담당자(Assignee)는 역할 분배 데이터에 명시된 참여자 이름으로 설정하세요.
2. 과제/일정 내용(Content)은 역할 분배 데이터에 적힌 세부 업무 내용을 바탕으로 작성하세요.
3. 마감일(DueDate)은 역할 분배 데이터에 날짜가 없다면 오늘로부터 3일 뒤로 설정해주세요 (YYYY-MM-DD 형식).
4. 응답은 반드시 아래 JSON 배열 형식으로만 작성하세요. (추가 설명이나 인사말 절대 금지)

응답 예시:
{{
  ""tasks"": [
    {{ ""Assignee"": ""홍길동"", ""Content"": ""UI 화면 겹침 버그 수정"", ""DueDate"": ""2026-06-20"" }}
  ]
}}

분배된 역할 정보 JSON:
{(string.IsNullOrEmpty(rolesJson) ? "없음" : rolesJson)}";

            ChatCompletion completion = await _chatClient.CompleteChatAsync(prompt);
            string rawResponse = completion.Content[0].Text;
            
            int start = rawResponse.IndexOf('{');
            int end = rawResponse.LastIndexOf('}');
            if (start >= 0 && end > start)
            {
                rawResponse = rawResponse.Substring(start, end - start + 1);
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(rawResponse);
                if (doc.RootElement.TryGetProperty("tasks", out JsonElement tasksElement))
                {
                    return JsonSerializer.Deserialize<List<TaskItem>>(tasksElement.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<TaskItem>();
                }
            }
            catch {{}}
            
            return new List<TaskItem>();
        }
    }
}
