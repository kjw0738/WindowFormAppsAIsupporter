namespace MeetingMinutesApp.Services;

public sealed class AiConfiguration
{
    private static readonly string[] KeyNames =
    [
        "GROQ_API_KEY",
        "XAI_API_KEY"
    ];

    public string? ApiKey { get; private init; }
    public string ApiKeyStatus { get; private init; } = "등록되지 않음";
    public string SttProvider { get; private init; } = "groq";
    public string SttProviderSource { get; private init; } = "기본값";
    public string? SttApiKey { get; private init; }
    public string SttApiKeyStatus { get; private init; } = "등록되지 않음";
    public string TranscriptionModel { get; private init; } = "whisper-large-v3-turbo";
    public string TranscriptionModelSource { get; private init; } = "기본값";
    public string ChatModel { get; private init; } = "llama-3.3-70b-versatile";
    public string ChatModelSource { get; private init; } = "기본값";
    public string ChatDelaySeconds { get; private init; } = "20";
    public string ChatDelaySource { get; private init; } = "기본값";
    public string MaxAutoRetrySeconds { get; private init; } = "120";
    public string MaxAutoRetrySource { get; private init; } = "기본값";
    public string LocalWhisperModel { get; private init; } = "base";
    public string LocalWhisperModelSource { get; private init; } = "기본값";
    public bool IsReady => !string.IsNullOrWhiteSpace(ApiKey);
    public bool IsSttReady => !string.IsNullOrWhiteSpace(SttApiKey);

    public static AiConfiguration Load()
    {
        string? apiKey = null;
        var apiKeyStatus = "등록되지 않음";

        foreach (var name in KeyNames)
        {
            var setting = ReadSettingWithSource(name, null);
            apiKey = setting.Value;

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                apiKeyStatus = $"{name} ({setting.Source})";
                break;
            }
        }

        var sttProvider = ReadSettingWithSource("STT_PROVIDER", "groq");
        var normalizedSttProvider = NormalizeSttProvider(sttProvider.Value);
        var sttApiKey = ResolveSttApiKey(normalizedSttProvider, apiKey);
        var sttModel = ReadSettingWithSource("STT_MODEL", GetDefaultSttModel(normalizedSttProvider));
        var chatModel = ReadSettingWithSource("GROQ_CHAT_MODEL", "llama-3.3-70b-versatile");
        var chatDelay = ReadSettingWithSource("GROQ_CHAT_DELAY_SECONDS", "20");
        var maxAutoRetry = ReadSettingWithSource("GROQ_MAX_AUTO_RETRY_SECONDS", "120");
        var localWhisperModel = ReadSettingWithSource("LOCAL_WHISPER_MODEL", "base");

        return new AiConfiguration
        {
            ApiKey = apiKey,
            ApiKeyStatus = apiKeyStatus,
            SttProvider = normalizedSttProvider,
            SttProviderSource = sttProvider.Source,
            SttApiKey = sttApiKey.Value,
            SttApiKeyStatus = sttApiKey.Source,
            TranscriptionModel = sttModel.Value ?? GetDefaultSttModel(normalizedSttProvider),
            TranscriptionModelSource = sttModel.Source,
            ChatModel = chatModel.Value ?? "llama-3.3-70b-versatile",
            ChatModelSource = chatModel.Source,
            ChatDelaySeconds = chatDelay.Value ?? "20",
            ChatDelaySource = chatDelay.Source,
            MaxAutoRetrySeconds = maxAutoRetry.Value ?? "120",
            MaxAutoRetrySource = maxAutoRetry.Source,
            LocalWhisperModel = localWhisperModel.Value ?? "base",
            LocalWhisperModelSource = localWhisperModel.Source
        };
    }

    public string GetSttEndpoint()
    {
        return SttProvider switch
        {
            "openai" => "https://api.openai.com/v1/audio/transcriptions",
            _ => "https://api.groq.com/openai/v1/audio/transcriptions"
        };
    }

    private static SettingValue ResolveSttApiKey(string provider, string? groqApiKey)
    {
        if (provider == "openai")
        {
            return ReadSettingWithSource("OPENAI_API_KEY", null);
        }

        if (!string.IsNullOrWhiteSpace(groqApiKey))
        {
            return new SettingValue(groqApiKey, "GROQ_API_KEY 또는 XAI_API_KEY");
        }

        return ReadSettingWithSource("GROQ_API_KEY", null);
    }

    private static string NormalizeSttProvider(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "openai" => "openai",
            _ => "groq"
        };
    }

    private static string GetDefaultSttModel(string provider)
    {
        return provider == "openai" ? "gpt-4o-mini-transcribe" : "whisper-large-v3-turbo";
    }

    private static SettingValue ReadSettingWithSource(string name, string? fallback)
    {
        var process = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        if (!string.IsNullOrWhiteSpace(process))
        {
            return new SettingValue(process, "프로세스 환경 변수");
        }

        var user = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User);
        if (!string.IsNullOrWhiteSpace(user))
        {
            return new SettingValue(user, "사용자 환경 변수");
        }

        var machine = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine);
        if (!string.IsNullOrWhiteSpace(machine))
        {
            return new SettingValue(machine, "시스템 환경 변수");
        }

        return new SettingValue(fallback, fallback is null ? "등록되지 않음" : "기본값");
    }

    private sealed record SettingValue(string? Value, string Source);
}
