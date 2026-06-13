using System.Text;
using Whisper.net;
using Whisper.net.Ggml;

namespace MeetingMinutesApp.Services;

public sealed class LocalWhisperTranscriptionService
{
    private readonly AudioChunkingService _audioChunkingService;
    private readonly string _modelPath;
    private readonly GgmlType _modelType;

    public LocalWhisperTranscriptionService(AudioChunkingService audioChunkingService)
    {
        _audioChunkingService = audioChunkingService;
        _modelType = ResolveModelType();
        _modelPath = Path.Combine(GetModelDirectory(), GetModelFileName(_modelType));
    }

    public async Task<string> TranscribeAsync(string audioPath, CancellationToken cancellationToken, IProgress<string>? progress = null)
    {
        await EnsureModelAsync(cancellationToken, progress);

        progress?.Report("로컬 Whisper용 오디오를 준비하는 중입니다...");
        using var convertedAudio = await _audioChunkingService.ConvertToWhisperWavAsync(audioPath, cancellationToken, progress);

        progress?.Report("로컬 Whisper STT 처리 중입니다. 파일 길이에 따라 시간이 걸릴 수 있습니다...");
        using var whisperFactory = WhisperFactory.FromPath(_modelPath);
        using var processor = whisperFactory
            .CreateBuilder()
            .WithLanguage("ko")
            .Build();

        await using var fileStream = File.OpenRead(convertedAudio.Path);
        var builder = new StringBuilder();

        await foreach (var result in processor.ProcessAsync(fileStream, cancellationToken))
        {
            if (!string.IsNullOrWhiteSpace(result.Text))
            {
                builder.AppendLine(result.Text.Trim());
            }
        }

        var transcript = builder.ToString().Trim();
        if (string.IsNullOrWhiteSpace(transcript))
        {
            throw new InvalidOperationException("로컬 Whisper STT 결과가 비어 있습니다.");
        }

        return transcript;
    }

    private async Task EnsureModelAsync(CancellationToken cancellationToken, IProgress<string>? progress)
    {
        if (File.Exists(_modelPath))
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(_modelPath)!);

        progress?.Report($"로컬 Whisper 모델을 다운로드하는 중입니다. 처음 1회만 필요합니다. ({_modelType})");
        await using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(_modelType, cancellationToken: cancellationToken);
        await using var fileStream = File.Create(_modelPath);
        await modelStream.CopyToAsync(fileStream, cancellationToken);
    }

    private static GgmlType ResolveModelType()
    {
        var configured =
            Environment.GetEnvironmentVariable("LOCAL_WHISPER_MODEL", EnvironmentVariableTarget.Process) ??
            Environment.GetEnvironmentVariable("LOCAL_WHISPER_MODEL", EnvironmentVariableTarget.User) ??
            Environment.GetEnvironmentVariable("LOCAL_WHISPER_MODEL", EnvironmentVariableTarget.Machine);

        return configured?.Trim().ToLowerInvariant() switch
        {
            "tiny" => GgmlType.Tiny,
            "base" => GgmlType.Base,
            "small" => GgmlType.Small,
            "medium" => GgmlType.Medium,
            "large-v3" => GgmlType.LargeV3,
            "largev3" => GgmlType.LargeV3,
            "large-v3-turbo" => GgmlType.LargeV3Turbo,
            "largev3turbo" => GgmlType.LargeV3Turbo,
            _ => GgmlType.Base
        };
    }

    private static string GetModelFileName(GgmlType modelType)
    {
        return modelType switch
        {
            GgmlType.Tiny => "ggml-tiny.bin",
            GgmlType.Base => "ggml-base.bin",
            GgmlType.Small => "ggml-small.bin",
            GgmlType.Medium => "ggml-medium.bin",
            GgmlType.LargeV3 => "ggml-large-v3.bin",
            GgmlType.LargeV3Turbo => "ggml-large-v3-turbo.bin",
            _ => "ggml-base.bin"
        };
    }

    private static string GetModelDirectory()
    {
        return Path.Combine(AppContext.BaseDirectory, "AppData", "Models");
    }
}
