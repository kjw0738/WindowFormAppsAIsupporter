namespace ReviewQuizApp.Services;

public sealed class AiQuizGeneratorConfig
{
    public AiProvider Provider { get; }
    public string ApiKey { get; }
    public bool IsConfigured => Provider != AiProvider.None && !string.IsNullOrWhiteSpace(ApiKey);

    private AiQuizGeneratorConfig(AiProvider provider, string apiKey)
    {
        Provider = provider;
        ApiKey = apiKey;
    }

    public static AiQuizGeneratorConfig Load()
    {
        var groqKey = GetEnvironmentVariable("GROQ_API_KEY");
        if (!string.IsNullOrWhiteSpace(groqKey))
        {
            return new AiQuizGeneratorConfig(AiProvider.Groq, groqKey);
        }

        var xaiKey = GetEnvironmentVariable("XAI_API_KEY");
        if (!string.IsNullOrWhiteSpace(xaiKey))
        {
            if (LooksLikeGroqKey(xaiKey))
            {
                return new AiQuizGeneratorConfig(AiProvider.Groq, xaiKey);
            }

            return new AiQuizGeneratorConfig(AiProvider.Xai, xaiKey);
        }

        var geminiKey = GetEnvironmentVariable("GEMINI_API_KEY");
        if (!string.IsNullOrWhiteSpace(geminiKey))
        {
            return new AiQuizGeneratorConfig(AiProvider.Gemini, geminiKey);
        }

        var openAiKey = GetEnvironmentVariable("OPENAI_API_KEY");
        if (!string.IsNullOrWhiteSpace(openAiKey))
        {
            return new AiQuizGeneratorConfig(AiProvider.OpenAI, openAiKey);
        }

        return new AiQuizGeneratorConfig(AiProvider.None, string.Empty);
    }

    private static string? GetEnvironmentVariable(string name)
    {
        return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process)
            ?? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User)
            ?? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine);
    }

    private static bool LooksLikeGroqKey(string apiKey)
    {
        return apiKey.TrimStart().StartsWith("gsk_", StringComparison.OrdinalIgnoreCase);
    }
}
