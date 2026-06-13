using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace WinFormsAppAIsupporter.Services
{
    public class GroqService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatService;

        public GroqService(string apiKey)
        {
            // Semantic Kernel 빌더 생성
            var builder = Kernel.CreateBuilder();

            // Groq은 OpenAI와 호환되는 API 형식을 사용합니다.
            // OpenAI 커넥터를 사용하되, Groq의 엔드포인트를 지정합니다.
            builder.AddOpenAIChatCompletion(
                modelId: "openai/gpt-oss-20b",
                apiKey: apiKey,
                httpClient: new HttpClient { BaseAddress = new Uri("https://api.groq.com/openai/v1/") }
            );

            _kernel = builder.Build();
            _chatService = _kernel.GetRequiredService<IChatCompletionService>();
        }

        public async Task<string> GetCompletionAsync(string prompt, string systemMessage = "You are a helpful meeting assistant.")
        {
            try
            {
                var history = new ChatHistory(systemMessage);
                history.AddUserMessage(prompt);

                var executionSettings = new OpenAIPromptExecutionSettings
                {
                    Temperature = 0.5,
                    MaxTokens = 2048 // 충분한 응답 길이를 확보하여 잘림 방지
                };

                var result = await _chatService.GetChatMessageContentAsync(history, executionSettings, _kernel);
                return result.Content ?? "";
            }
            catch (HttpRequestException httpEx)
            {
                throw new Exception($"API 요청 중 네트워크 오류가 발생했습니다: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"AI 서비스 오류: {ex.Message}");
            }
        }
    }
}
