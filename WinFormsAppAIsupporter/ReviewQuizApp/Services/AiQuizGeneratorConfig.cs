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
        var groqKey = Environment.GetEnvironmentVariable("GROQ_API_KEY");
        if (!string.IsNullOrWhiteSpace(groqKey))
        {
            return new AiQuizGeneratorConfig(AiProvider.Groq, groqKey);
        }

        var xaiKey = Environment.GetEnvironmentVariable("XAI_API_KEY");
        if (!string.IsNullOrWhiteSpace(xaiKey))
        {
            if (LooksLikeGroqKey(xaiKey))
            {
                return new AiQuizGeneratorConfig(AiProvider.Groq, xaiKey);
            }

            return new AiQuizGeneratorConfig(AiProvider.Xai, xaiKey);
        }

        var geminiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (!string.IsNullOrWhiteSpace(geminiKey))
        {
            return new AiQuizGeneratorConfig(AiProvider.Gemini, geminiKey);
        }

        var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (!string.IsNullOrWhiteSpace(openAiKey))
        {
            return new AiQuizGeneratorConfig(AiProvider.OpenAI, openAiKey);
        }

        return new AiQuizGeneratorConfig(AiProvider.None, string.Empty);
    }

    private static bool LooksLikeGroqKey(string apiKey)
    {
        return apiKey.TrimStart().StartsWith("gsk_", StringComparison.OrdinalIgnoreCase);
    }
}
