using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;

namespace IntegratedMeetingStudio.Services;

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
                "湲??뱀쓬 ?뚯씪???먮룞 遺꾪븷?섎젮硫?ffmpeg媛 ?꾩슂?섏?留??먮룞 以鍮꾩뿉 ?ㅽ뙣?덉뒿?덈떎.\r\n\r\n" +
                "?ㅽ듃?뚰겕 ?곌껐???뺤씤?섍굅??ffmpeg.exe瑜?PATH???깅줉?섏꽭??");
        }

        var workDirectory = Path.Combine(Path.GetTempPath(), "MeetingMinutesApp", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDirectory);

        var outputPattern = Path.Combine(workDirectory, "chunk_%03d.mp3");
        var useVoicePreprocessing = IsAudioPreprocessingEnabled();
        progress?.Report("?뚯꽦 ?몄씠利덉? ?뚮웾???뺣━????援ш컙??遺꾪븷?⑸땲??..");
        var result = await RunProcessAsync(
            ffmpegPath,
            BuildSplitArguments(audioPath, outputPattern, useVoicePreprocessing),
            cancellationToken);

        if (result.ExitCode != 0 && useVoicePreprocessing)
        {
            progress?.Report("?뚯꽦 ?꾩쿂由??꾪꽣 ?곸슜???ㅽ뙣??湲곕낯 遺꾪븷 諛⑹떇?쇰줈 ?ъ떆?꾪빀?덈떎...");
            result = await RunProcessAsync(
                ffmpegPath,
                BuildSplitArguments(audioPath, outputPattern, useVoicePreprocessing: false),
                cancellationToken);
        }

        if (result.ExitCode != 0)
        {
            Directory.Delete(workDirectory, true);
            throw new InvalidOperationException($"?뱀쓬 ?뚯씪 遺꾪븷???ㅽ뙣?덉뒿?덈떎.\r\n\r\n{result.Error}");
        }

        var chunks = Directory
            .GetFiles(workDirectory, "chunk_*.mp3")
            .OrderBy(path => path)
            .ToList();

        if (chunks.Count == 0)
        {
            Directory.Delete(workDirectory, true);
            throw new InvalidOperationException("?뱀쓬 ?뚯씪 遺꾪븷 寃곌낵瑜?李얠? 紐삵뻽?듬땲??");
        }

        return new AudioChunkSet(workDirectory, chunks);
    }

    public async Task<ConvertedAudioFile> PrepareApiAudioAsync(string audioPath, CancellationToken cancellationToken, IProgress<string>? progress = null)
    {
        var ffmpegPath = await FindOrPrepareToolAsync("ffmpeg", "FFMPEG_PATH", cancellationToken, progress);
        if (ffmpegPath is null)
        {
            throw new InvalidOperationException(
                "STT???뚯꽦 ?꾩쿂由щ? ?ㅽ뻾?섎젮硫?ffmpeg媛 ?꾩슂?섏?留??먮룞 以鍮꾩뿉 ?ㅽ뙣?덉뒿?덈떎.\r\n\r\n" +
                "?ㅽ듃?뚰겕 ?곌껐???뺤씤?섍굅??ffmpeg.exe瑜?PATH???깅줉?섏꽭??");
        }

        var workDirectory = Path.Combine(Path.GetTempPath(), "MeetingMinutesApp", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDirectory);

        var outputPath = Path.Combine(workDirectory, "input-for-stt.mp3");
        var useVoicePreprocessing = IsAudioPreprocessingEnabled();
        progress?.Report("STT ?붿껌 ???뚯꽦 ?몄씠利덉? ?뚮웾???뺣━?⑸땲??..");
        var result = await RunProcessAsync(
            ffmpegPath,
            BuildApiAudioArguments(audioPath, outputPath, useVoicePreprocessing),
            cancellationToken);

        if (result.ExitCode != 0 && useVoicePreprocessing)
        {
            progress?.Report("?뚯꽦 ?꾩쿂由??꾪꽣 ?곸슜???ㅽ뙣??湲곕낯 ?뚯꽦 蹂?섏쑝濡??ъ떆?꾪빀?덈떎...");
            result = await RunProcessAsync(
                ffmpegPath,
                BuildApiAudioArguments(audioPath, outputPath, useVoicePreprocessing: false),
                cancellationToken);
        }

        if (result.ExitCode != 0 || !File.Exists(outputPath))
        {
            Directory.Delete(workDirectory, true);
            throw new InvalidOperationException($"STT???뚯꽦 ?꾩쿂由ъ뿉 ?ㅽ뙣?덉뒿?덈떎.\r\n\r\n{result.Error}");
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
                "濡쒖뺄 STT瑜??ㅽ뻾?섎젮硫?ffmpeg媛 ?꾩슂?섏?留??먮룞 以鍮꾩뿉 ?ㅽ뙣?덉뒿?덈떎.\r\n\r\n" +
                "?ㅽ듃?뚰겕 ?곌껐???뺤씤?섍굅??ffmpeg.exe瑜?PATH???깅줉?섏꽭??");
        }

        var workDirectory = Path.Combine(Path.GetTempPath(), "MeetingMinutesApp", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDirectory);

        var outputPath = Path.Combine(workDirectory, "input.wav");
        var useVoicePreprocessing = IsAudioPreprocessingEnabled();
        progress?.Report("濡쒖뺄 STT ???뚯꽦 ?몄씠利덉? ?뚮웾???뺣━?⑸땲??..");
        var result = await RunProcessAsync(
            ffmpegPath,
            BuildWhisperWavArguments(audioPath, outputPath, useVoicePreprocessing),
            cancellationToken);

        if (result.ExitCode != 0 && useVoicePreprocessing)
        {
            progress?.Report("?뚯꽦 ?꾩쿂由??꾪꽣 ?곸슜???ㅽ뙣??湲곕낯 WAV 蹂?섏쑝濡??ъ떆?꾪빀?덈떎...");
            result = await RunProcessAsync(
                ffmpegPath,
                BuildWhisperWavArguments(audioPath, outputPath, useVoicePreprocessing: false),
                cancellationToken);
        }

        if (result.ExitCode != 0 || !File.Exists(outputPath))
        {
            Directory.Delete(workDirectory, true);
            throw new InvalidOperationException($"濡쒖뺄 STT??WAV 蹂?섏뿉 ?ㅽ뙣?덉뒿?덈떎.\r\n\r\n{result.Error}");
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

        progress?.Report("ffmpeg媛 ?놁뼱 ???꾩슜 ?대뜑???먮룞 以鍮?以묒엯?덈떎...");
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
            progress?.Report("ffmpeg ?ㅼ슫濡쒕뱶 以묒엯?덈떎. 泥섏쓬 1?뚮쭔 ?쒓컙??嫄몃┫ ???덉뒿?덈떎...");
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

        progress?.Report("ffmpeg ?뺤텞 ?댁젣 以묒엯?덈떎...");
        ZipFile.ExtractToDirectory(zipPath, extractDirectory);

        var ffmpeg = Directory.GetFiles(extractDirectory, "ffmpeg.exe", SearchOption.AllDirectories).FirstOrDefault();
        var ffprobe = Directory.GetFiles(extractDirectory, "ffprobe.exe", SearchOption.AllDirectories).FirstOrDefault();
        if (ffmpeg is null || ffprobe is null)
        {
            throw new InvalidOperationException("?ㅼ슫濡쒕뱶??ffmpeg ?뺤텞 ?뚯씪?먯꽌 ffmpeg.exe ?먮뒗 ffprobe.exe瑜?李얠? 紐삵뻽?듬땲??");
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
                throw new InvalidOperationException("ffmpeg ?ㅼ슫濡쒕뱶 ?뚯씪??臾닿껐??寃利앹뿉 ?ㅽ뙣?덉뒿?덈떎. ?ㅼ떆 ?쒕룄?섏꽭??");
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
