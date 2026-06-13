using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIMeetingMinutes
{
    public class ClaudeApiService
    {
        private readonly HttpClient _http;
        private const string Endpoint = "https://api.groq.com/openai/v1/chat/completions";
        private const string Model = "meta-llama/llama-4-scout-17b-16e-instruct";

        public ClaudeApiService(string apiKey)
        {
            _http = new HttpClient();
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        private async Task<string> CallAsync(string system, string user)
        {
            var body = new
            {
                model = Model,
                messages = new[]
                {
                    new { role = "system", content = system },
                    new { role = "user",   content = user }
                },
                max_tokens = 2048
            };
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(Endpoint, content);
            var responseJson = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new System.Net.Http.HttpRequestException($"API 오류 ({response.StatusCode}): {responseJson}");
            using var doc = JsonDocument.Parse(responseJson);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
        }

        public Task<string> SuggestNextAgendaAsync(string transcript) =>
            CallAsync(
                "당신은 회의록을 분석하는 전문 비서입니다. 반드시 한국어로만 답변하고 한자는 절대 쓰지 마세요.",
                $@"아래 회의 내용을 분석해서 다음 회의 추천 안건을 제안해주세요.

## 📅 다음 회의 추천 안건
(번호를 붙여 5개 내외로 제안, 각 안건마다 필요한 이유 한 줄 포함)

---
회의 내용:
{transcript}");

        public Task<string> SummarizeParticipantAsync(string speaker, List<string> messages) =>
            CallAsync(
                "당신은 회의록을 분석하는 전문 비서입니다. 반드시 한국어로만 답변하고 한자는 절대 쓰지 마세요.",
                $@"아래는 '{speaker}'의 발언 목록입니다. 이 사람의 주요 의견과 기여 내용을 3~5문장으로 요약해주세요.

발언 목록:
{string.Join("\n", messages.Select((m, i) => $"{i + 1}. {m}"))}");
    }
}
