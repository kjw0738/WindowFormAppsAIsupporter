using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ReviewQuizApp.Models;

namespace ReviewQuizApp.Services;

public sealed class InternalAiAssignmentQuizGenerator
{
    private const string OpenAiEndpoint = "https://api.openai.com/v1/responses";
    private const string GroqEndpoint = "https://api.groq.com/openai/v1/chat/completions";
    private const string XaiEndpoint = "https://api.x.ai/v1/chat/completions";
    private const string GeminiModel = "gemini-2.5-flash";
    private const string GroqModel = "openai/gpt-oss-20b";
    private const string DefaultXaiModel = "grok-4.3";
    private static readonly HttpClient HttpClient = new();

    public async Task<QuizDataFile> GenerateAsync(
        QuizGenerationRequest request,
        AiQuizGeneratorConfig config,
        CancellationToken cancellationToken)
    {
        var prompt = BuildPrompt(request);
        var json = config.Provider switch
        {
            AiProvider.OpenAI => await GenerateWithOpenAiAsync(prompt, config.ApiKey, cancellationToken),
            AiProvider.Gemini => await GenerateWithGeminiAsync(prompt, config.ApiKey, cancellationToken),
            AiProvider.Groq => await GenerateWithGroqAsync(prompt, config.ApiKey, cancellationToken),
            AiProvider.Xai => await GenerateWithXaiAsync(prompt, config.ApiKey, cancellationToken),
            _ => throw new InvalidOperationException("AI API key is not configured.")
        };

        var quiz = JsonSerializer.Deserialize<QuizDataFile>(json, QuizJson.Options)
            ?? throw new InvalidDataException("AI response could not be converted to quiz data.");

        new QuizDataLoader().ValidateAndNormalize(quiz);
        return quiz;
    }

    private static string BuildPrompt(QuizGenerationRequest request)
    {
        var types = string.Join(", ", request.QuestionTypes);
        return $"""
            Create an assignment review quiz from the assignment data below.

            Rules:
            - Quiz goal: {request.Goal}
            - Difficulty: {request.Difficulty}
            - Question count: {request.QuestionCount}
            - Allowed question types: {types}
            - Unless the quiz goal explicitly asks for another language, write every quiz title, question, option, correctAnswer, explanation, and tag in Korean.
            - If the quiz goal explicitly asks for another language, follow that requested language only for user-facing quiz text.
            - Every question must be grounded only in the provided assignment JSON title and content.
            - Do not create questions from general background knowledge, assumptions, external facts, or content that is not present in the assignment JSON.
            - If a concept is not clearly supported by the assignment JSON, do not use it as a question, answer, option, explanation, or tag.
            - Explanations must reference the assignment content directly and must not add new facts beyond it.
            - TrueFalse questions must use options ["O", "X"] and correctAnswer "O" or "X".
            - MultipleChoice questions must include at least 4 options when possible.
            - ShortAnswer questions must have a short, gradable correctAnswer.
            - Return only JSON matching the provided schema.

            Assignment title:
            {request.Assignment.Title}

            Assignment content:
            {request.Assignment.Content}
            """;
    }

    private static string XaiModel => Environment.GetEnvironmentVariable("XAI_MODEL") ?? DefaultXaiModel;

    private static object BuildQuizSchema()
    {
        return new
        {
            type = "object",
            additionalProperties = false,
            required = new[] { "quizTitle", "generatedAt", "questions" },
            properties = new
            {
                quizTitle = new { type = "string" },
                generatedAt = new { type = "string", format = "date-time" },
                questions = new
                {
                    type = "array",
                    minItems = 1,
                    items = new
                    {
                        type = "object",
                        additionalProperties = false,
                        required = new[] { "id", "type", "title", "options", "correctAnswer", "explanation", "tags" },
                        properties = new
                        {
                            id = new { type = "string" },
                            type = new { type = "string", @enum = new[] { "TrueFalse", "MultipleChoice", "ShortAnswer" } },
                            title = new { type = "string" },
                            options = new { type = "array", items = new { type = "string" } },
                            correctAnswer = new { type = "string" },
                            explanation = new { type = "string" },
                            tags = new { type = "array", items = new { type = "string" } }
                        }
                    }
                }
            }
        };
    }

    private static object BuildXaiQuizSchema()
    {
        return new
        {
            type = "object",
            additionalProperties = false,
            required = new[] { "quizTitle", "generatedAt", "questions" },
            properties = new
            {
                quizTitle = new { type = "string" },
                generatedAt = new { type = "string" },
                questions = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        additionalProperties = false,
                        required = new[] { "id", "type", "title", "options", "correctAnswer", "explanation", "tags" },
                        properties = new
                        {
                            id = new { type = "string" },
                            type = new { type = "string", @enum = new[] { "TrueFalse", "MultipleChoice", "ShortAnswer" } },
                            title = new { type = "string" },
                            options = new { type = "array", items = new { type = "string" } },
                            correctAnswer = new { type = "string" },
                            explanation = new { type = "string" },
                            tags = new { type = "array", items = new { type = "string" } }
                        }
                    }
                }
            }
        };
    }

    private static string BuildJsonOnlyPrompt(string prompt)
    {
        var schema = JsonSerializer.Serialize(BuildQuizSchema());
        return $"""
            {prompt}

            Output requirements:
            - Return only valid JSON.
            - Do not wrap the JSON in markdown.
            - Follow this JSON schema shape:
            {schema}
            """;
    }

    private static async Task<string> GenerateWithOpenAiAsync(string prompt, string apiKey, CancellationToken cancellationToken)
    {
        var body = new
        {
            model = "gpt-4o-mini",
            input = prompt,
            text = new
            {
                format = new
                {
                    type = "json_schema",
                    name = "quiz_data",
                    strict = true,
                    schema = BuildQuizSchema()
                }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, OpenAiEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        using var response = await HttpClient.SendAsync(request, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"OpenAI quiz generation failed: {(int)response.StatusCode} {response.ReasonPhrase}");
        }

        return ExtractOpenAiText(responseText);
    }

    private static async Task<string> GenerateWithGeminiAsync(string prompt, string apiKey, CancellationToken cancellationToken)
    {
        var body = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                responseJsonSchema = BuildQuizSchema()
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"https://generativelanguage.googleapis.com/v1beta/models/{GeminiModel}:generateContent");
        request.Headers.Add("x-goog-api-key", apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        using var response = await HttpClient.SendAsync(request, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Gemini quiz generation failed: {(int)response.StatusCode} {response.ReasonPhrase}");
        }

        return ExtractGeminiText(responseText);
    }

    private static async Task<string> GenerateWithGroqAsync(string prompt, string apiKey, CancellationToken cancellationToken)
    {
        var body = new
        {
            model = GroqModel,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = "You generate strict JSON quiz data for a WinForms assignment review app."
                },
                new
                {
                    role = "user",
                    content = BuildJsonOnlyPrompt(prompt)
                }
            },
            temperature = 0.2,
            response_format = new { type = "json_object" }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, GroqEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        using var response = await HttpClient.SendAsync(request, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Groq quiz generation failed: {(int)response.StatusCode} {response.ReasonPhrase}");
        }

        return ExtractChatCompletionText(responseText);
    }

    private static async Task<string> GenerateWithXaiAsync(string prompt, string apiKey, CancellationToken cancellationToken)
    {
        var body = new
        {
            model = XaiModel,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = "You generate strict JSON quiz data for a WinForms assignment review app."
                },
                new
                {
                    role = "user",
                    content = BuildJsonOnlyPrompt(prompt)
                }
            },
            temperature = 0.2,
            response_format = new
            {
                type = "json_schema",
                json_schema = new
                {
                    name = "quiz_data",
                    schema = BuildXaiQuizSchema(),
                    strict = true
                }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, XaiEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        using var response = await HttpClient.SendAsync(request, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"xAI quiz generation failed: {(int)response.StatusCode} {response.ReasonPhrase}\n{TrimForError(responseText)}");
        }

        return ExtractChatCompletionText(responseText);
    }

    private static string ExtractOpenAiText(string responseText)
    {
        using var document = JsonDocument.Parse(responseText);
        var root = document.RootElement;
        if (root.TryGetProperty("output_text", out var outputText))
        {
            return outputText.GetString() ?? string.Empty;
        }

        return FindOpenAiOutputText(root) ?? throw new InvalidDataException("OpenAI response did not include output text.");
    }

    private static string ExtractGeminiText(string responseText)
    {
        using var document = JsonDocument.Parse(responseText);
        var parts = document.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts");

        return parts[0].GetProperty("text").GetString() ?? string.Empty;
    }

    private static string ExtractChatCompletionText(string responseText)
    {
        using var document = JsonDocument.Parse(responseText);
        return document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString()
            ?? string.Empty;
    }

    private static string? FindOpenAiOutputText(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("type", out var type)
                && type.GetString() == "output_text"
                && element.TryGetProperty("text", out var text))
            {
                return text.GetString();
            }

            foreach (var property in element.EnumerateObject())
            {
                var found = FindOpenAiOutputText(property.Value);
                if (!string.IsNullOrWhiteSpace(found))
                {
                    return found;
                }
            }
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var found = FindOpenAiOutputText(item);
                if (!string.IsNullOrWhiteSpace(found))
                {
                    return found;
                }
            }
        }

        return null;
    }

    private static string TrimForError(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Length <= 800 ? value : value[..800];
    }
}
