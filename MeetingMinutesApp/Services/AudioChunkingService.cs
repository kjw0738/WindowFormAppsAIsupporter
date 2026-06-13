using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;

namespace MeetingMinutesApp.Services;

public sealed class AudioChunkingService
{
    private const long MaxDirectUploadBytes = 20L * 1024L * 1024L;
    private const string FfmpegDownloadUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip";
    private const string SttAudioBitrate = "96k";
    private const string VoicePreprocessFilter = "highpass=f=80,lowpass=f=7800,loudnorm=I=-16:TP=-1.5:LRA=11";
    private static readonly TimeSpan MaxDirectDuration = TimeSpan.FromMinutes(25);
    private static readonly TimeSpan DefaultChunkDuration = TimeSpan.FromMinutes(5);
    private static readonly object ToolPathCacheLock = new();
    private static readonly Dictionary<string, string> ToolPathCache = new(StringComparer.OrdinalIgnoreCase);

    public async Task<AudioChunkingPlan> CreatePlanAsync(string audioPath, CancellationToken cancellationToken)
    {
        var file = new FileInfo(audioPath);
        if (file.Length > MaxDirectUploadBytes)
        {
            return new AudioChunkingPlan(true, null, file.Length);
        }

        var duration = await TryGetDurationAsync(audioPath, cancellationToken);
        var requiresChunking = duration > MaxDirectDuration;

        return new AudioChunkingPlan(requiresChunking, duration, file.Length);
    }

    public async Task<AudioChunkSet> SplitAsync(string audioPath, CancellationToken cancellationToken, IProgress<string>? progress = null)
    {
        var ffmpegPath = await FindOrPrepareToolAsync("ffmpeg", "FFMPEG_PATH", cancellationToken, progress);
        if (ffmpegPath is null)
        {
            throw new InvalidOperationException(
                "긴 녹음 파일을 자동 분할하려면 ffmpeg가 필요하지만 자동 준비에 실패했습니다.\r\n\r\n" +
                "네트워크 연결을 확인하거나 ffmpeg.exe를 PATH에 등록하세요.");
        }

        var workDirectory = Path.Combine(Path.GetTempPath(), "MeetingMinutesApp", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDirectory);

        var outputPattern = Path.Combine(workDirectory, "chunk_%03d.mp3");
        var useVoicePreprocessing = IsAudioPreprocessingEnabled();
        progress?.Report("음성 노이즈와 음량을 정리한 뒤 구간을 분할합니다...");
        var result = await RunProcessAsync(
            ffmpegPath,
            BuildSplitArguments(audioPath, outputPattern, useVoicePreprocessing),
            cancellationToken);

        if (result.ExitCode != 0 && useVoicePreprocessing)
        {
            progress?.Report("음성 전처리 필터 적용에 실패해 기본 분할 방식으로 재시도합니다...");
            result = await RunProcessAsync(
                ffmpegPath,
                BuildSplitArguments(audioPath, outputPattern, useVoicePreprocessing: false),
                cancellationToken);
        }

        if (result.ExitCode != 0)
        {
            Directory.Delete(workDirectory, true);
            throw new InvalidOperationException($"녹음 파일 분할에 실패했습니다.\r\n\r\n{result.Error}");
        }

        var chunks = Directory
            .GetFiles(workDirectory, "chunk_*.mp3")
            .OrderBy(path => path)
            .ToList();

        if (chunks.Count == 0)
        {
            Directory.Delete(workDirectory, true);
            throw new InvalidOperationException("녹음 파일 분할 결과를 찾지 못했습니다.");
        }

        return new AudioChunkSet(workDirectory, chunks);
    }

    public async Task<ConvertedAudioFile> PrepareApiAudioAsync(string audioPath, CancellationToken cancellationToken, IProgress<string>? progress = null)
    {
        var ffmpegPath = await FindOrPrepareToolAsync("ffmpeg", "FFMPEG_PATH", cancellationToken, progress);
        if (ffmpegPath is null)
        {
            throw new InvalidOperationException(
                "STT용 음성 전처리를 실행하려면 ffmpeg가 필요하지만 자동 준비에 실패했습니다.\r\n\r\n" +
                "네트워크 연결을 확인하거나 ffmpeg.exe를 PATH에 등록하세요.");
        }

        var workDirectory = Path.Combine(Path.GetTempPath(), "MeetingMinutesApp", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDirectory);

        var outputPath = Path.Combine(workDirectory, "input-for-stt.mp3");
        var useVoicePreprocessing = IsAudioPreprocessingEnabled();
        progress?.Report("STT 요청 전 음성 노이즈와 음량을 정리합니다...");
        var result = await RunProcessAsync(
            ffmpegPath,
            BuildApiAudioArguments(audioPath, outputPath, useVoicePreprocessing),
            cancellationToken);

        if (result.ExitCode != 0 && useVoicePreprocessing)
        {
            progress?.Report("음성 전처리 필터 적용에 실패해 기본 음성 변환으로 재시도합니다...");
            result = await RunProcessAsync(
                ffmpegPath,
                BuildApiAudioArguments(audioPath, outputPath, useVoicePreprocessing: false),
                cancellationToken);
        }

        if (result.ExitCode != 0 || !File.Exists(outputPath))
        {
            Directory.Delete(workDirectory, true);
            throw new InvalidOperationException($"STT용 음성 전처리에 실패했습니다.\r\n\r\n{result.Error}");
        }

        return new ConvertedAudioFile(workDirectory, outputPath);
    }

    private static TimeSpan GetChunkDuration()
    {
        var configured =
            Environment.GetEnvironmentVariable("STT_CHUNK_MINUTES", EnvironmentVariableTarget.Process) ??
            Environment.GetEnvironmentVariable("STT_CHUNK_MINUTES", EnvironmentVariableTarget.User) ??
            Environment.GetEnvironmentVariable("STT_CHUNK_MINUTES", EnvironmentVariableTarget.Machine);

        return double.TryParse(configured, out var minutes) && minutes > 0
            ? TimeSpan.FromMinutes(minutes)
            : DefaultChunkDuration;
    }

    public async Task<ConvertedAudioFile> ConvertToWhisperWavAsync(string audioPath, CancellationToken cancellationToken, IProgress<string>? progress = null)
    {
        var ffmpegPath = await FindOrPrepareToolAsync("ffmpeg", "FFMPEG_PATH", cancellationToken, progress);
        if (ffmpegPath is null)
        {
            throw new InvalidOperationException(
                "로컬 STT를 실행하려면 ffmpeg가 필요하지만 자동 준비에 실패했습니다.\r\n\r\n" +
                "네트워크 연결을 확인하거나 ffmpeg.exe를 PATH에 등록하세요.");
        }

        var workDirectory = Path.Combine(Path.GetTempPath(), "MeetingMinutesApp", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDirectory);

        var outputPath = Path.Combine(workDirectory, "input.wav");
        var useVoicePreprocessing = IsAudioPreprocessingEnabled();
        progress?.Report("로컬 STT 전 음성 노이즈와 음량을 정리합니다...");
        var result = await RunProcessAsync(
            ffmpegPath,
            BuildWhisperWavArguments(audioPath, outputPath, useVoicePreprocessing),
            cancellationToken);

        if (result.ExitCode != 0 && useVoicePreprocessing)
        {
            progress?.Report("음성 전처리 필터 적용에 실패해 기본 WAV 변환으로 재시도합니다...");
            result = await RunProcessAsync(
                ffmpegPath,
                BuildWhisperWavArguments(audioPath, outputPath, useVoicePreprocessing: false),
                cancellationToken);
        }

        if (result.ExitCode != 0 || !File.Exists(outputPath))
        {
            Directory.Delete(workDirectory, true);
            throw new InvalidOperationException($"로컬 STT용 WAV 변환에 실패했습니다.\r\n\r\n{result.Error}");
        }

        return new ConvertedAudioFile(workDirectory, outputPath);
    }

    private static string[] BuildSplitArguments(string audioPath, string outputPattern, bool useVoicePreprocessing)
    {
        var arguments = new List<string>
        {
            "-y",
            "-i", audioPath,
            "-vn",
            "-ac", "1",
            "-ar", "16000"
        };

        AddVoicePreprocessingArguments(arguments, useVoicePreprocessing);
        arguments.AddRange(
        [
            "-c:a", "libmp3lame",
            "-b:a", SttAudioBitrate,
            "-f", "segment",
            "-segment_time", ((int)GetChunkDuration().TotalSeconds).ToString(CultureInfo.InvariantCulture),
            "-reset_timestamps", "1",
            outputPattern
        ]);

        return arguments.ToArray();
    }

    private static string[] BuildApiAudioArguments(string audioPath, string outputPath, bool useVoicePreprocessing)
    {
        var arguments = new List<string>
        {
            "-y",
            "-i", audioPath,
            "-vn",
            "-ac", "1",
            "-ar", "16000"
        };

        AddVoicePreprocessingArguments(arguments, useVoicePreprocessing);
        arguments.AddRange(["-c:a", "libmp3lame", "-b:a", SttAudioBitrate, outputPath]);

        return arguments.ToArray();
    }

    private static string[] BuildWhisperWavArguments(string audioPath, string outputPath, bool useVoicePreprocessing)
    {
        var arguments = new List<string>
        {
            "-y",
            "-i", audioPath,
            "-vn",
            "-ac", "1",
            "-ar", "16000"
        };

        AddVoicePreprocessingArguments(arguments, useVoicePreprocessing);
        arguments.AddRange(["-c:a", "pcm_s16le", outputPath]);

        return arguments.ToArray();
    }

    private static void AddVoicePreprocessingArguments(List<string> arguments, bool useVoicePreprocessing)
    {
        if (useVoicePreprocessing)
        {
            arguments.AddRange(["-af", VoicePreprocessFilter]);
        }
    }

    private static bool IsAudioPreprocessingEnabled()
    {
        var configured =
            Environment.GetEnvironmentVariable("STT_AUDIO_PREPROCESSING", EnvironmentVariableTarget.Process) ??
            Environment.GetEnvironmentVariable("STT_AUDIO_PREPROCESSING", EnvironmentVariableTarget.User) ??
            Environment.GetEnvironmentVariable("STT_AUDIO_PREPROCESSING", EnvironmentVariableTarget.Machine);

        return !string.Equals(configured?.Trim(), "false", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(configured?.Trim(), "0", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<TimeSpan?> TryGetDurationAsync(string audioPath, CancellationToken cancellationToken)
    {
        var ffprobePath = FindTool("ffprobe", "FFPROBE_PATH");
        if (ffprobePath is null)
        {
            return null;
        }

        var result = await RunProcessAsync(
            ffprobePath,
            [
                "-v", "error",
                "-show_entries", "format=duration",
                "-of", "default=noprint_wrappers=1:nokey=1",
                audioPath
            ],
            cancellationToken);

        if (result.ExitCode != 0)
        {
            return null;
        }

        return double.TryParse(result.Output.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds)
            ? TimeSpan.FromSeconds(seconds)
            : null;
    }

    private async Task<string?> FindOrPrepareToolAsync(
        string executableName,
        string environmentVariableName,
        CancellationToken cancellationToken,
        IProgress<string>? progress)
    {
        var existingPath = FindTool(executableName, environmentVariableName);
        if (existingPath is not null)
        {
            return existingPath;
        }

        progress?.Report("ffmpeg가 없어 앱 전용 폴더에 자동 준비 중입니다...");
        await EnsurePortableFfmpegAsync(cancellationToken, progress);
        return FindTool(executableName, environmentVariableName);
    }

    private static async Task EnsurePortableFfmpegAsync(CancellationToken cancellationToken, IProgress<string>? progress)
    {
        var toolsDirectory = GetPortableToolsDirectory();
        var markerPath = Path.Combine(toolsDirectory, ".ffmpeg-ready");
        if (File.Exists(markerPath) && FindTool("ffmpeg", "FFMPEG_PATH") is not null)
        {
            return;
        }

        Directory.CreateDirectory(toolsDirectory);

        var zipPath = Path.Combine(toolsDirectory, "ffmpeg-release-essentials.zip");
        var extractDirectory = Path.Combine(toolsDirectory, "ffmpeg");

        if (!File.Exists(zipPath))
        {
            progress?.Report("ffmpeg 다운로드 중입니다. 처음 1회만 시간이 걸릴 수 있습니다...");
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
            await using var downloadStream = await httpClient.GetStreamAsync(FfmpegDownloadUrl, cancellationToken);
            await using var fileStream = File.Create(zipPath);
            await downloadStream.CopyToAsync(fileStream, cancellationToken);

            await VerifyChecksumIfAvailableAsync(httpClient, zipPath, cancellationToken);
        }

        if (Directory.Exists(extractDirectory))
        {
            Directory.Delete(extractDirectory, true);
        }

        progress?.Report("ffmpeg 압축 해제 중입니다...");
        ZipFile.ExtractToDirectory(zipPath, extractDirectory);

        var ffmpeg = Directory.GetFiles(extractDirectory, "ffmpeg.exe", SearchOption.AllDirectories).FirstOrDefault();
        var ffprobe = Directory.GetFiles(extractDirectory, "ffprobe.exe", SearchOption.AllDirectories).FirstOrDefault();
        if (ffmpeg is null || ffprobe is null)
        {
            throw new InvalidOperationException("다운로드한 ffmpeg 압축 파일에서 ffmpeg.exe 또는 ffprobe.exe를 찾지 못했습니다.");
        }

        File.WriteAllText(markerPath, DateTime.Now.ToString("O", CultureInfo.InvariantCulture));
    }

    private static async Task VerifyChecksumIfAvailableAsync(HttpClient httpClient, string zipPath, CancellationToken cancellationToken)
    {
        try
        {
            var checksumText = await httpClient.GetStringAsync($"{FfmpegDownloadUrl}.sha256", cancellationToken);
            var expectedHash = checksumText
                .Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(value => value.Length == 64);

            if (expectedHash is null)
            {
                return;
            }

            await using var stream = File.OpenRead(zipPath);
            var actualHashBytes = await SHA256.HashDataAsync(stream, cancellationToken);
            var actualHash = Convert.ToHexString(actualHashBytes).ToLowerInvariant();
            if (!string.Equals(expectedHash, actualHash, StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(zipPath);
                throw new InvalidOperationException("ffmpeg 다운로드 파일의 무결성 검증에 실패했습니다. 다시 시도하세요.");
            }
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch
        {
            // Checksum availability can vary. The downloaded archive is still extracted from the pinned source URL.
        }
    }

    private static string? FindTool(string executableName, string environmentVariableName)
    {
        var cacheKey = $"{environmentVariableName}:{executableName}";
        lock (ToolPathCacheLock)
        {
            if (ToolPathCache.TryGetValue(cacheKey, out var cachedPath) && File.Exists(cachedPath))
            {
                return cachedPath;
            }

            ToolPathCache.Remove(cacheKey);
        }

        var configuredPath =
            Environment.GetEnvironmentVariable(environmentVariableName, EnvironmentVariableTarget.Process) ??
            Environment.GetEnvironmentVariable(environmentVariableName, EnvironmentVariableTarget.User) ??
            Environment.GetEnvironmentVariable(environmentVariableName, EnvironmentVariableTarget.Machine);

        if (File.Exists(configuredPath))
        {
            CacheToolPath(cacheKey, configuredPath);
            return configuredPath;
        }

        foreach (var localPath in GetLocalToolCandidates(executableName))
        {
            if (File.Exists(localPath))
            {
                CacheToolPath(cacheKey, localPath);
                return localPath;
            }
        }

        var pathVariable = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var directory in pathVariable.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(directory.Trim(), $"{executableName}.exe");
            if (File.Exists(candidate))
            {
                CacheToolPath(cacheKey, candidate);
                return candidate;
            }
        }

        return null;
    }

    private static void CacheToolPath(string cacheKey, string path)
    {
        lock (ToolPathCacheLock)
        {
            ToolPathCache[cacheKey] = path;
        }
    }

    private static IEnumerable<string> GetLocalToolCandidates(string executableName)
    {
        yield return Path.Combine(AppContext.BaseDirectory, $"{executableName}.exe");

        var toolsDirectory = GetPortableToolsDirectory();
        if (!Directory.Exists(toolsDirectory))
        {
            yield break;
        }

        foreach (var path in Directory.GetFiles(toolsDirectory, $"{executableName}.exe", SearchOption.AllDirectories))
        {
            yield return path;
        }
    }

    private static string GetPortableToolsDirectory()
    {
        return Path.Combine(AppContext.BaseDirectory, "AppData", "Tools");
    }

    private static async Task<ProcessResult> RunProcessAsync(string fileName, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        using var process = new Process();
        process.StartInfo.FileName = fileName;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return new ProcessResult(process.ExitCode, await outputTask, await errorTask);
    }

    private sealed record ProcessResult(int ExitCode, string Output, string Error);
}

public sealed record AudioChunkingPlan(bool RequiresChunking, TimeSpan? Duration, long FileSizeBytes);

public sealed class AudioChunkSet : IDisposable
{
    public AudioChunkSet(string workDirectory, IReadOnlyList<string> chunkPaths)
    {
        WorkDirectory = workDirectory;
        ChunkPaths = chunkPaths;
    }

    public string WorkDirectory { get; }
    public IReadOnlyList<string> ChunkPaths { get; }

    public void Dispose()
    {
        if (Directory.Exists(WorkDirectory))
        {
            Directory.Delete(WorkDirectory, true);
        }
    }
}

public sealed class ConvertedAudioFile : IDisposable
{
    public ConvertedAudioFile(string workDirectory, string path)
    {
        WorkDirectory = workDirectory;
        Path = path;
    }

    public string WorkDirectory { get; }
    public string Path { get; }

    public void Dispose()
    {
        if (Directory.Exists(WorkDirectory))
        {
            Directory.Delete(WorkDirectory, true);
        }
    }
}
