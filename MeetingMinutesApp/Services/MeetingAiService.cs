using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace MeetingMinutesApp.Services;

public sealed class MeetingAiService
{
    private const int DefaultSummaryChunkSize = 5500;
    private const int TranscriptCorrectionChunkSize = 3000;
    private const int TranscriptCorrectionMaxTokens = 1400;
    private const int DefaultPartialSummaryMaxTokens = 500;
    private const int DefaultFinalSummaryMaxTokens = 1200;
    private const int DefaultCacheRetentionDays = 30;
    private const int DefaultCacheMaxFiles = 1000;
    private const int MaxSttRetries = 3;
    private const int MaxChatRetries = 4;
    private const int SummaryCacheVersion = 2;

    private static readonly TimeSpan DefaultChatRequestDelay = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan DefaultMaxAutoRetryDelay = TimeSpan.FromSeconds(120);
    private static readonly Regex RetryAfterPattern = new(@"try again in\s+([0-9.]+)s", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex HorizontalWhitespacePattern = new(@"[^\S\r\n]+", RegexOptions.Compiled);
    private static readonly Regex SentenceBreakPattern = new(@"(?<=[.!?。！？])\s+(?=\S)", RegexOptions.Compiled);
    private static readonly Regex KoreanSentenceBreakPattern = new(@"(?<=[가-힣](?:다|요|죠|까|네|음|임|함|됨|됨니다|습니다|입니다|합니다|같습니다))\s+(?=[가-힣\[])", RegexOptions.Compiled);
    private static readonly Regex ExcessBlankLinePattern = new(@"(?:\r?\n\s*){3,}", RegexOptions.Compiled);
    private static readonly Regex BracketOnlyPattern = new(@"^\[[^\]]{1,30}\]$", RegexOptions.Compiled);
    private static readonly object CacheCleanupLock = new();
    private static bool _cacheCleanupCompleted;
    private static readonly IReadOnlyList<(Regex Pattern, string Replacement)> AiOutputTextCorrections =
    [
        (new Regex("詳細", RegexOptions.Compiled), "상세"),
        (new Regex("详细", RegexOptions.Compiled), "상세"),
        (new Regex("詳しく", RegexOptions.Compiled), "자세히"),
        (new Regex("詳しい", RegexOptions.Compiled), "자세한"),
        (new Regex("より詳しく", RegexOptions.Compiled), "더 자세히")
    ];
    private static readonly IReadOnlyList<(Regex Pattern, string Replacement)> TechnicalTermCorrections =
    [
        (new Regex(@"\bhtb\b", RegexOptions.IgnoreCase | RegexOptions.Compiled), "HTTP"),
        (new Regex(@"\bSMGP\b|\bS&TB\b|\bS-S-T-P\b", RegexOptions.IgnoreCase | RegexOptions.Compiled), "SMTP"),
        (new Regex(@"\bD\.S\.\b|\bD\.S\b", RegexOptions.IgnoreCase | RegexOptions.Compiled), "DNS"),
        (new Regex("클라이터|클라이간", RegexOptions.Compiled), "클라이언트"),
        (new Regex("데이터메이트|데이터베이트", RegexOptions.Compiled), "데이터베이스"),
        (new Regex("리스포스|리스포트", RegexOptions.Compiled), "response"),
        (new Regex("리퀘스트", RegexOptions.Compiled), "request"),
        (new Regex("메세지", RegexOptions.Compiled), "메시지"),
        (new Regex("후키", RegexOptions.Compiled), "쿠키"),
        (new Regex("리플레이어 탭|리플레이어 택", RegexOptions.Compiled), "replay attack"),
        (new Regex("마셋티케이션|마셋티케이션이", RegexOptions.Compiled), "authentication"),
        (new Regex("스프피아|스푸핑", RegexOptions.Compiled), "spoofing"),
        (new Regex("매니저 미드런트|맨 인 더 미들|맨인더미들", RegexOptions.Compiled), "man-in-the-middle"),
        (new Regex("도맹", RegexOptions.Compiled), "도메인"),
        (new Regex("아이비티|아이비다", RegexOptions.Compiled), "IP"),
        (new Regex("유아의", RegexOptions.Compiled), "URL")
    ];
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly AiConfiguration _configuration;
    private readonly AudioChunkingService _audioChunkingService = new();
    private readonly LocalWhisperTranscriptionService _localWhisperService;
    private readonly string _summaryCacheDirectory;
    private readonly string _sttCacheDirectory;
    private string _sttCategory = "컴퓨터 네트워크 강의";
    private string _sttTerms = "HTTP, HTTPS, request, response, message, header, cookie, session, state, transaction, transition, client, server, database, authentication, authorization, replay attack, man-in-the-middle, DNS, DNS spoofing, IP, URL, URI, domain, ICANN, TLD, SMTP, POP3, IMAP, mail server, user agent, TCP, UDP, handshake, port, protocol";

    public MeetingAiService(HttpClient httpClient, AiConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _localWhisperService = new LocalWhisperTranscriptionService(_audioChunkingService);
        _summaryCacheDirectory = Path.Combine(AppContext.BaseDirectory, "Data", "summary-cache");
        _sttCacheDirectory = Path.Combine(AppContext.BaseDirectory, "Data", "stt-cache");
        Directory.CreateDirectory(_summaryCacheDirectory);
        Directory.CreateDirectory(_sttCacheDirectory);
        CleanupCacheDirectories();
    }

    public void SetSttContext(string? category, string? terms)
    {
        _sttCategory = string.IsNullOrWhiteSpace(category) ? "일반 회의" : category.Trim();
        _sttTerms = string.IsNullOrWhiteSpace(terms) ? string.Empty : terms.Trim();
    }

    public async Task<string> TranscribeAsync(string audioPath, CancellationToken cancellationToken, IProgress<string>? progress = null)
    {
        if (TryLoadSttCache(audioPath, out var cachedTranscript))
        {
            progress?.Report("저장된 STT 원문 캐시를 재사용합니다.");
            return cachedTranscript;
        }

        if (!_configuration.IsSttReady)
        {
            progress?.Report("STT API 키가 없어 로컬 Whisper STT로 전환합니다...");
            return SaveAndReturnSttCache(
                audioPath,
                NormalizeTranscript(await TranscribeWithLocalWhisperAsync(audioPath, cancellationToken, progress)));
        }

        var plan = await _audioChunkingService.CreatePlanAsync(audioPath, cancellationToken);
        if (!plan.RequiresChunking)
        {
            progress?.Report($"{_configuration.SttProvider.ToUpperInvariant()} STT 변환 중입니다...");
            try
            {
                return SaveAndReturnSttCache(
                    audioPath,
                    NormalizeTranscript(await TranscribeSingleFileAsync(audioPath, cancellationToken, progress)));
            }
            catch (Exception ex) when (IsRecoverableSttFailure(ex, cancellationToken))
            {
                progress?.Report("API STT 실패. 로컬 Whisper STT로 전환합니다...");
                return SaveAndReturnSttCache(
                    audioPath,
                    NormalizeTranscript(await TranscribeWithLocalWhisperFallbackAsync(audioPath, cancellationToken, progress)));
            }
        }

        progress?.Report("긴 녹음 파일을 설정된 구간 단위로 전처리/분할하는 중입니다...");
        using var chunkSet = await _audioChunkingService.SplitAsync(audioPath, cancellationToken, progress);

        var builder = new StringBuilder();
        for (var i = 0; i < chunkSet.ChunkPaths.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report($"{_configuration.SttProvider.ToUpperInvariant()} STT 변환 중입니다... ({i + 1}/{chunkSet.ChunkPaths.Count})");

            string text;
            if (TryLoadSttChunkCache(audioPath, i, chunkSet.ChunkPaths.Count, out text))
            {
                progress?.Report($"저장된 STT 구간 캐시를 재사용합니다. ({i + 1}/{chunkSet.ChunkPaths.Count})");
            }
            else
            {
                try
                {
                    text = await TranscribeSingleFileAsync(chunkSet.ChunkPaths[i], cancellationToken, progress, prepareAudio: false);
                }
                catch (Exception ex) when (IsRecoverableSttFailure(ex, cancellationToken))
                {
                    progress?.Report($"구간 {i + 1} API STT 실패. 해당 구간을 로컬 Whisper로 처리합니다...");
                    text = await TranscribeChunkWithLocalWhisperFallbackAsync(chunkSet.ChunkPaths[i], i + 1, cancellationToken, progress);
                }

                SaveSttChunkCache(audioPath, i, chunkSet.ChunkPaths.Count, text);
            }

            builder.AppendLine($"[구간 {i + 1}]");
            builder.AppendLine(text);
            builder.AppendLine();
        }

        return SaveAndReturnSttCache(audioPath, NormalizeTranscript(builder.ToString()));
    }
    public async Task<string> SummarizeAsync(string transcript, CancellationToken cancellationToken, IProgress<string>? progress = null)
    {
        EnsureReady();

        if (string.IsNullOrWhiteSpace(transcript))
        {
            throw new InvalidOperationException("회의록을 정리하려면 먼저 STT 텍스트가 필요합니다.");
        }

        var summaryInput = PrepareTranscriptForSummary(transcript);
        if (string.IsNullOrWhiteSpace(summaryInput))
        {
            throw new InvalidOperationException("회의록 정리에 사용할 수 있는 STT 텍스트가 없습니다.");
        }

        if (summaryInput.Length < transcript.Length)
        {
            progress?.Report($"회의록 정리 입력에서 반복/자막성 노이즈를 제외했습니다. ({transcript.Length:N0} -> {summaryInput.Length:N0}자)");
        }

        var summaryChunkSize = GetSummaryChunkSize();
        var chunks = SplitText(summaryInput, summaryChunkSize).ToList();
        var extractedOpinions = new StringBuilder();
        var cache = LoadSummaryCache(summaryInput, chunks.Count, summaryChunkSize);
        DateTimeOffset? lastChatRequestCompletedAt = null;

        for (var i = 0; i < chunks.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report($"회의 의견 키워드를 추출하는 중입니다... ({i + 1}/{chunks.Count})");

            if (!cache.ChunkOpinions.TryGetValue(i, out var chunkOpinion) || string.IsNullOrWhiteSpace(chunkOpinion))
            {
                if (TryLoadChunkOpinionCache(chunks[i], out chunkOpinion))
                {
                    progress?.Report($"구간 해시 캐시에서 의견 추출 결과를 재사용합니다... ({i + 1}/{chunks.Count})");
                    cache.ChunkOpinions[i] = chunkOpinion;
                    SaveSummaryCache(cache);
                }
                else
                {
                    chunkOpinion = await ExtractMeetingOpinionsAsync(chunks[i], cancellationToken, progress);
                    lastChatRequestCompletedAt = DateTimeOffset.UtcNow;
                    cache.ChunkOpinions[i] = chunkOpinion;
                    SaveChunkOpinionCache(chunks[i], chunkOpinion);
                    SaveSummaryCache(cache);
                }
            }
            else
            {
                progress?.Report($"저장된 의견 추출 결과를 재사용합니다... ({i + 1}/{chunks.Count})");
            }

            extractedOpinions.AppendLine($"## 구간 의견 추출 {i + 1}");
            extractedOpinions.AppendLine(chunkOpinion);
            extractedOpinions.AppendLine();

            if (i < chunks.Count - 1 && !HasCachedChunkOpinion(cache, chunks[i + 1], i + 1))
            {
                var delay = GetChatRequestDelay();
                if (lastChatRequestCompletedAt is null)
                {
                    continue;
                }

                var remainingDelay = delay - (DateTimeOffset.UtcNow - lastChatRequestCompletedAt.Value);
                if (remainingDelay <= TimeSpan.Zero)
                {
                    continue;
                }

                delay = remainingDelay;
                progress?.Report($"API 제한 회피를 위해 잠시 대기합니다... ({i + 1}/{chunks.Count})");
                await Task.Delay(delay, cancellationToken);
            }
        }

        progress?.Report("의견 키워드를 주제별 회의록으로 병합하는 중입니다...");
        if (!string.IsNullOrWhiteSpace(cache.FinalMinutes))
        {
            progress?.Report("저장된 최종 회의록 정리 결과를 재사용합니다...");
            return cache.FinalMinutes;
        }

        var finalMergeInput = PrepareExtractedOpinionsForFinalMerge(extractedOpinions.ToString());
        if (finalMergeInput.Length < extractedOpinions.Length)
        {
            progress?.Report($"최종 병합 입력에서 빈 의견 항목을 제외했습니다. ({extractedOpinions.Length:N0} -> {finalMergeInput.Length:N0}자)");
        }

        if (lastChatRequestCompletedAt is not null)
        {
            var remainingDelay = GetChatRequestDelay() - (DateTimeOffset.UtcNow - lastChatRequestCompletedAt.Value);
            if (remainingDelay > TimeSpan.Zero)
            {
                progress?.Report($"API 제한 회피를 위해 잠시 대기합니다... (최종 병합)");
                await Task.Delay(remainingDelay, cancellationToken);
            }
        }

        cache = cache with
        {
            FinalMinutes = await BuildFinalMinutesAsync(finalMergeInput, cancellationToken, progress)
        };
        SaveSummaryCache(cache);
        return cache.FinalMinutes ?? string.Empty;
    }

    public async Task<string> ImproveTranscriptAsync(string transcript, CancellationToken cancellationToken, IProgress<string>? progress = null)
    {
        EnsureReady();

        if (string.IsNullOrWhiteSpace(transcript))
        {
            throw new InvalidOperationException("보정할 STT 원문이 없습니다.");
        }

        var normalized = NormalizeTranscript(transcript);
        var chunks = SplitText(normalized, TranscriptCorrectionChunkSize).ToList();
        var builder = new StringBuilder();

        for (var i = 0; i < chunks.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report($"STT 원문을 문맥 기준으로 보정하는 중입니다... ({i + 1}/{chunks.Count})");

            var corrected = await CorrectTranscriptChunkAsync(chunks[i], cancellationToken, progress);
            builder.AppendLine(corrected);
            builder.AppendLine();

            if (i < chunks.Count - 1)
            {
                await Task.Delay(GetChatRequestDelay(), cancellationToken);
            }
        }

        return NormalizeTranscript(builder.ToString());
    }

    public bool DeleteSummaryCache(string transcript)
    {
        if (string.IsNullOrWhiteSpace(transcript))
        {
            return false;
        }

        var deleted = false;
        var summaryInput = PrepareTranscriptForSummary(transcript);
        foreach (var hashSource in new[] { transcript, summaryInput }.Where(text => !string.IsNullOrWhiteSpace(text)).Distinct())
        {
            var cachePath = GetSummaryCachePath(ComputeHash(hashSource));
            if (File.Exists(cachePath))
            {
                File.Delete(cachePath);
                deleted = true;
            }
        }

        if (!string.IsNullOrWhiteSpace(summaryInput))
        {
            foreach (var chunk in SplitText(summaryInput, GetSummaryChunkSize()))
            {
                var chunkCachePath = GetChunkOpinionCachePath(chunk);
                if (File.Exists(chunkCachePath))
                {
                    File.Delete(chunkCachePath);
                    deleted = true;
                }
            }
        }

        return deleted;
    }

    private async Task<string> TranscribeWithLocalWhisperAsync(string audioPath, CancellationToken cancellationToken, IProgress<string>? progress)
    {
        var plan = await _audioChunkingService.CreatePlanAsync(audioPath, cancellationToken);
        if (!plan.RequiresChunking)
        {
            return NormalizeTranscript(await _localWhisperService.TranscribeAsync(audioPath, cancellationToken, progress));
        }

        progress?.Report("긴 녹음 파일을 10분 단위로 분할한 뒤 로컬 STT를 실행합니다...");
        using var chunkSet = await _audioChunkingService.SplitAsync(audioPath, cancellationToken, progress);

        var builder = new StringBuilder();
        for (var i = 0; i < chunkSet.ChunkPaths.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report($"로컬 Whisper STT 처리 중입니다... ({i + 1}/{chunkSet.ChunkPaths.Count})");

            string text;
            if (TryLoadSttChunkCache(audioPath, i, chunkSet.ChunkPaths.Count, out text))
            {
                progress?.Report($"저장된 STT 구간 캐시를 재사용합니다. ({i + 1}/{chunkSet.ChunkPaths.Count})");
            }
            else
            {
                text = await _localWhisperService.TranscribeAsync(chunkSet.ChunkPaths[i], cancellationToken, progress);
                SaveSttChunkCache(audioPath, i, chunkSet.ChunkPaths.Count, text);
            }

            builder.AppendLine($"[구간 {i + 1}]");
            builder.AppendLine(text);
            builder.AppendLine();
        }

        return NormalizeTranscript(builder.ToString());
    }

    private async Task<string> TranscribeWithLocalWhisperFallbackAsync(string audioPath, CancellationToken cancellationToken, IProgress<string>? progress)
    {
        try
        {
            return await TranscribeWithLocalWhisperAsync(audioPath, cancellationToken, progress);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "API STT 실패 후 로컬 Whisper STT로 전환했지만 로컬 처리도 실패했습니다.\r\n" +
                "무한 재시도하지 않고 처리를 중단합니다.\r\n\r\n" +
                $"로컬 STT 오류: {ex.Message}",
                ex);
        }
    }

    private async Task<string> TranscribeChunkWithLocalWhisperFallbackAsync(string chunkPath, int chunkNumber, CancellationToken cancellationToken, IProgress<string>? progress)
    {
        try
        {
            return await _localWhisperService.TranscribeAsync(chunkPath, cancellationToken, progress);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"구간 {chunkNumber} API STT 실패 후 로컬 Whisper STT로 전환했지만 로컬 처리도 실패했습니다.\r\n" +
                "무한 재시도하지 않고 처리를 중단합니다.\r\n\r\n" +
                $"로컬 STT 오류: {ex.Message}",
                ex);
        }
    }

    private async Task<string> TranscribeSingleFileAsync(string audioPath, CancellationToken cancellationToken, IProgress<string>? progress, bool prepareAudio = true)
    {
        using var preparedAudio = prepareAudio
            ? await _audioChunkingService.PrepareApiAudioAsync(audioPath, cancellationToken, progress)
            : null;
        var uploadPath = preparedAudio?.Path ?? audioPath;

        for (var attempt = 1; attempt <= MaxSttRetries; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, _configuration.GetSttEndpoint());
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.SttApiKey);

            await using var stream = File.OpenRead(uploadPath);
            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(GuessMimeType(uploadPath));

            content.Add(fileContent, "file", Path.GetFileName(uploadPath));
            content.Add(new StringContent(_configuration.TranscriptionModel, Encoding.UTF8), "model");
            content.Add(new StringContent("ko", Encoding.UTF8), "language");
            if (IsSttPromptEnabled())
            {
                content.Add(new StringContent(BuildSttPrompt(), Encoding.UTF8), "prompt");
            }
            content.Add(new StringContent("json", Encoding.UTF8), "response_format");
            request.Content = content;

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            if ((int)response.StatusCode == 429 && attempt < MaxSttRetries)
            {
                var retryDelay = GetRetryDelay(response, responseText);
                var maxAutoRetryDelay = GetMaxAutoRetryDelay();
                if (retryDelay > maxAutoRetryDelay)
                {
                    throw new InvalidOperationException(
                        $"STT 변환 실패: 429 Too Many Requests\r\n" +
                        $"STT API 사용량 제한 대기 시간이 너무 깁니다. 예상 대기 시간: {Math.Ceiling(retryDelay.TotalSeconds)}초, 자동 대기 한도: {Math.Ceiling(maxAutoRetryDelay.TotalSeconds)}초.\r\n" +
                        "해당 구간은 로컬 Whisper STT로 전환합니다.\r\n" +
                        responseText);
                }

                progress?.Report($"STT API 사용량 제한으로 {Math.Ceiling(retryDelay.TotalSeconds)}초 후 재시도합니다... ({attempt}/{MaxSttRetries})");
                await Task.Delay(retryDelay, cancellationToken);
                continue;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"STT 변환 실패: {(int)response.StatusCode} {response.ReasonPhrase}\r\n{responseText}");
            }

            var transcription = JsonSerializer.Deserialize<TranscriptionResponse>(responseText, JsonOptions);
            if (string.IsNullOrWhiteSpace(transcription?.Text))
            {
                throw new InvalidOperationException("STT 응답에서 변환된 텍스트를 찾지 못했습니다.");
            }

            return transcription.Text.Trim();
        }

        throw new InvalidOperationException("STT API 재시도 횟수를 초과했습니다.");
    }

    private async Task<string> ExtractMeetingOpinionsAsync(string transcriptChunk, CancellationToken cancellationToken, IProgress<string>? progress)
    {
        var opinions = await CreateChatCompletionAsync(
            [
                new ChatRequestMessage("system", """
                너는 회의 녹취록에서 의사결정 단서를 추출하는 기록 담당자다.
                반드시 한국어로 작성한다.
                한국어 문장은 한글로 작성하고, 중국어/일본어 한자 표현을 섞지 않는다.
                예: '詳細' 대신 '상세' 또는 '자세히'처럼 한글 표현으로 쓴다.
                HTTP, DNS, API 같은 영어 약어와 기술 용어는 유지해도 된다.
                입력에 실제로 있는 내용만 사용한다.
                추측, 창작, 외부 지식 추가는 금지한다.
                발화가 불명확하면 '확인 필요'로 표시한다.
                긴 문장 요약보다 핵심 키워드와 의견 유형 분류를 우선한다.
                """),
                new ChatRequestMessage("user", $"""
                아래 회의 녹취 구간에서 회의록에 필요한 정보만 추출해.
                문장을 길게 풀어 쓰지 말고, 주제별 키워드와 의견 유형을 분류해.

                출력 형식:
                ## 주제
                - 주제명:
                - 핵심 키워드:
                - 찬성 의견:
                - 반대 의견:
                - 추가할 내용:
                - 삭제/제외할 내용:
                - 수정할 내용:
                - 새로 제시된 주제:
                - 결정 사항:
                - 담당 업무:
                - 확인 필요:

                규칙:
                - 해당 내용이 없으면 '없음'이라고 쓴다.
                - 같은 주제가 여러 번 나오면 같은 주제 아래에 묶는다.
                - 사람 이름, 담당자, 기한이 명확하지 않으면 '확인 필요'로 표시한다.
                - 한국어 표현은 한글로 작성하고, 한자식 표현은 한글 단어로 풀어 쓴다.

                녹취 구간:
                {transcriptChunk}
            """)
            ],
            GetPartialSummaryMaxTokens(),
            cancellationToken,
            progress);

        return NormalizeAiMeetingText(opinions);
    }

    private async Task<string> BuildFinalMinutesAsync(string extractedOpinions, CancellationToken cancellationToken, IProgress<string>? progress)
    {
        var minutes = await CreateChatCompletionAsync(
            [
                new ChatRequestMessage("system", """
                너는 대학교 팀 프로젝트 회의록을 정리하는 기록 담당자다.
                반드시 한국어로 작성한다.
                한국어 문장은 한글로 작성하고, 중국어/일본어 한자 표현을 섞지 않는다.
                예: '詳細' 대신 '상세' 또는 '자세히'처럼 한글 표현으로 쓴다.
                HTTP, DNS, API 같은 영어 약어와 기술 용어는 유지해도 된다.
                입력된 의견 추출 결과에 실제로 포함된 내용만 사용한다.
                추측, 창작, 외부 지식 추가는 금지한다.
                같은 주제의 찬성/반대/추가/삭제/수정 의견을 병합해 의사결정 흐름이 보이게 정리한다.
                불명확한 내용은 '확인 필요'로 표시한다.
                """),
                new ChatRequestMessage("user", $"""
                아래 구간별 의견 추출 결과를 병합해서 최종 회의록으로 작성해.
                단순 요약이 아니라 주제별로 찬성/반대/추가/삭제/수정/새 주제/결정/업무를 정리해.

                출력 형식:
                # 회의록
                ## 회의 개요
                - 일시:
                - 참석자:
                - 전체 주제:

                ## 주제별 논의 정리
                ### 주제명
                - 핵심 키워드:
                - 찬성 의견:
                - 반대 의견:
                - 추가할 내용:
                - 삭제/제외할 내용:
                - 수정할 내용:
                - 결정 사항:
                - 확인 필요:

                ## 새로 제시된 주제
                -

                ## 최종 결정 사항
                -

                ## 담당 업무
                | 담당자 | 업무 | 기한 | 상태 |
                | --- | --- | --- | --- |

                ## 다음 회의 전 확인 필요
                -

                규칙:
                - 중복 의견은 하나로 합친다.
                - 찬성/반대가 모두 있으면 양쪽을 모두 남긴다.
                - 삭제/제외 의견과 추가 의견을 섞지 말고 분리한다.
                - 결정되지 않은 내용은 결정 사항에 넣지 말고 확인 필요에 넣는다.
                - 한국어 표현은 한글로 작성하고, 한자식 표현은 한글 단어로 풀어 쓴다.

                구간별 의견 추출 결과:
                {extractedOpinions}
            """)
            ],
            GetFinalSummaryMaxTokens(),
            cancellationToken,
            progress);

        return NormalizeAiMeetingText(minutes);
    }

    private async Task<string> CorrectTranscriptChunkAsync(string transcriptChunk, CancellationToken cancellationToken, IProgress<string>? progress)
    {
        return await CreateChatCompletionAsync(
            [
                new ChatRequestMessage("system", """
                너는 한국어 회의 STT 원문을 보수적으로 교정하는 편집자다.
                목적은 STT 품질 문제로 뭉개진 단어, 띄어쓰기, 문장 경계를 문맥상 명확한 경우에만 고치는 것이다.
                절대 요약하지 않는다.
                절대 새로운 정보, 사람 이름, 결정 사항, 근거를 추가하지 않는다.
                원문에 있는 순서와 의미를 유지한다.
                [구간 1] 같은 구간 표기는 그대로 유지한다.
                문맥상 확실하지 않은 단어는 억지로 고치지 말고 원문을 유지하거나 '[확인 필요: 원문]'으로 표시한다.
                출력은 교정된 STT 원문만 작성한다.
                """),
                new ChatRequestMessage("user", $"""
                아래 STT 원문을 회의 맥락에 맞게 보수적으로 교정해.

                회의/강의 카테고리:
                {_sttCategory}

                주요 용어:
                {(string.IsNullOrWhiteSpace(_sttTerms) ? "별도 용어 없음" : _sttTerms)}

                교정 가능:
                - 명백한 오탈자 또는 STT 인식 오류
                - 한국어 띄어쓰기
                - 너무 긴 문장의 자연스러운 줄바꿈
                - 문맥상 확실한 전문용어/프로젝트 용어

                교정 금지:
                - 내용 요약
                - 없는 발언 추가
                - 불명확한 내용을 확정적으로 바꾸기
                - 회의록 형식으로 재작성

                STT 원문:
                {transcriptChunk}
                """)
            ],
            TranscriptCorrectionMaxTokens,
            cancellationToken,
            progress);
    }

    private async Task<string> CreateChatCompletionAsync(
        IReadOnlyList<ChatRequestMessage> messages,
        int maxCompletionTokens,
        CancellationToken cancellationToken,
        IProgress<string>? progress)
    {
        var body = new ChatCompletionRequest(
            _configuration.ChatModel,
            messages,
            0.2,
            maxCompletionTokens);

        for (var attempt = 1; attempt <= MaxChatRetries; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.ApiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            if ((int)response.StatusCode == 429 && attempt < MaxChatRetries)
            {
                var retryDelay = GetRetryDelay(response, responseText);
                var maxAutoRetryDelay = GetMaxAutoRetryDelay();
                if (IsDailyTokenLimit(responseText))
                {
                    throw new InvalidOperationException(
                        $"회의록 정리 실패: 429 Too Many Requests\r\n" +
                        "Groq 하루 토큰 한도(TPD)에 도달했거나 거의 도달했습니다.\r\n" +
                        "분당 제한과 달리 짧은 자동 재시도로는 해결되지 않습니다.\r\n" +
                        "이미 처리된 중간 결과는 캐시에 저장되어 있으니 한도가 회복된 뒤 다시 실행하면 이어서 처리할 수 있습니다.\r\n" +
                        "토큰 사용량을 줄이려면 설정에서 GROQ_PARTIAL_MAX_TOKENS 또는 GROQ_FINAL_MAX_TOKENS 값을 낮추세요.\r\n" +
                        responseText);
                }

                if (retryDelay > maxAutoRetryDelay)
                {
                    throw new InvalidOperationException(
                        $"회의록 정리 실패: 429 Too Many Requests\r\n" +
                        $"API 사용량 제한 대기 시간이 너무 깁니다. 예상 대기 시간: {Math.Ceiling(retryDelay.TotalSeconds)}초, 자동 대기 한도: {Math.Ceiling(maxAutoRetryDelay.TotalSeconds)}초.\r\n" +
                        "이미 처리된 중간 결과는 캐시에 저장되어 있습니다. 잠시 후 다시 실행하면 이어서 처리할 수 있습니다.\r\n" +
                        responseText);
                }

                progress?.Report($"API 사용량 제한으로 {Math.Ceiling(retryDelay.TotalSeconds)}초 후 재시도합니다... ({attempt}/{MaxChatRetries})");
                await Task.Delay(retryDelay, cancellationToken);
                continue;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"회의록 정리 실패: {(int)response.StatusCode} {response.ReasonPhrase}\r\n{responseText}");
            }

            var completion = JsonSerializer.Deserialize<ChatCompletionResponse>(responseText, JsonOptions);
            var message = completion?.Choices?.FirstOrDefault()?.Message;
            var summary = message?.Content ?? message?.ReasoningContent ?? message?.Reasoning;
            if (string.IsNullOrWhiteSpace(summary))
            {
                var preview = responseText.Length > 1200 ? responseText[..1200] + "..." : responseText;
                throw new InvalidOperationException($"AI 응답에서 회의록 내용을 찾지 못했습니다.\r\n\r\n응답 미리보기:\r\n{preview}");
            }

            return summary.Trim();
        }

        throw new InvalidOperationException("회의록 정리 API 재시도 횟수를 초과했습니다.");
    }

    private static TimeSpan GetChatRequestDelay()
    {
        var configured =
            Environment.GetEnvironmentVariable("GROQ_CHAT_DELAY_SECONDS", EnvironmentVariableTarget.Process) ??
            Environment.GetEnvironmentVariable("GROQ_CHAT_DELAY_SECONDS", EnvironmentVariableTarget.User) ??
            Environment.GetEnvironmentVariable("GROQ_CHAT_DELAY_SECONDS", EnvironmentVariableTarget.Machine);

        return double.TryParse(configured, out var seconds) && seconds >= 0
            ? TimeSpan.FromSeconds(seconds)
            : DefaultChatRequestDelay;
    }

    private static TimeSpan GetRetryDelay(HttpResponseMessage response, string responseText)
    {
        if (response.Headers.RetryAfter?.Delta is { } delta)
        {
            return delta + TimeSpan.FromSeconds(2);
        }

        var match = RetryAfterPattern.Match(responseText);
        if (match.Success && double.TryParse(match.Groups[1].Value, out var seconds))
        {
            return TimeSpan.FromSeconds(Math.Ceiling(seconds) + 2);
        }

        return TimeSpan.FromSeconds(30);
    }

    private static TimeSpan GetMaxAutoRetryDelay()
    {
        var configured =
            Environment.GetEnvironmentVariable("GROQ_MAX_AUTO_RETRY_SECONDS", EnvironmentVariableTarget.Process) ??
            Environment.GetEnvironmentVariable("GROQ_MAX_AUTO_RETRY_SECONDS", EnvironmentVariableTarget.User) ??
            Environment.GetEnvironmentVariable("GROQ_MAX_AUTO_RETRY_SECONDS", EnvironmentVariableTarget.Machine);

        return double.TryParse(configured, out var seconds) && seconds > 0
            ? TimeSpan.FromSeconds(seconds)
            : DefaultMaxAutoRetryDelay;
    }

    private static int GetPartialSummaryMaxTokens()
    {
        return ReadPositiveIntSetting("GROQ_PARTIAL_MAX_TOKENS", DefaultPartialSummaryMaxTokens, min: 200, max: 1200);
    }

    private static int GetSummaryChunkSize()
    {
        return ReadPositiveIntSetting("GROQ_SUMMARY_CHUNK_SIZE", DefaultSummaryChunkSize, min: 3000, max: 8000);
    }

    private static int GetFinalSummaryMaxTokens()
    {
        return ReadPositiveIntSetting("GROQ_FINAL_MAX_TOKENS", DefaultFinalSummaryMaxTokens, min: 500, max: 2500);
    }

    private static int ReadPositiveIntSetting(string name, int defaultValue, int min, int max)
    {
        var configured =
            Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process) ??
            Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User) ??
            Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine);

        return int.TryParse(configured, out var value)
            ? Math.Clamp(value, min, max)
            : defaultValue;
    }

    private static bool IsDailyTokenLimit(string responseText)
    {
        return responseText.Contains("tokens per day", StringComparison.OrdinalIgnoreCase) ||
            responseText.Contains("TPD", StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> SplitText(string text, int maxLength)
    {
        for (var start = 0; start < text.Length; start += maxLength)
        {
            var length = Math.Min(maxLength, text.Length - start);
            yield return text.Substring(start, length);
        }
    }

    private static string PrepareTranscriptForSummary(string transcript)
    {
        var normalized = NormalizeTranscript(transcript);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        var lines = normalized
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToList();

        var prepared = new List<string>();
        foreach (var line in lines)
        {
            if (IsLowValueSummaryNoise(line))
            {
                continue;
            }

            prepared.Add(line);
        }

        return ExcessBlankLinePattern.Replace(string.Join('\n', prepared).Trim(), "\n\n");
    }

    private static string NormalizeAiMeetingText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalized = text.Normalize(NormalizationForm.FormKC);
        foreach (var (pattern, replacement) in AiOutputTextCorrections)
        {
            normalized = pattern.Replace(normalized, replacement);
        }

        return normalized.Trim();
    }

    private static string PrepareExtractedOpinionsForFinalMerge(string extractedOpinions)
    {
        if (string.IsNullOrWhiteSpace(extractedOpinions))
        {
            return string.Empty;
        }

        var lines = extractedOpinions
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToList();

        var prepared = new List<string>();
        string? previousLine = null;
        foreach (var line in lines)
        {
            if (IsEmptyOpinionLine(line) || string.Equals(line, previousLine, StringComparison.Ordinal))
            {
                continue;
            }

            prepared.Add(line);
            previousLine = line;
        }

        return string.Join('\n', prepared).Trim();
    }

    private static bool IsEmptyOpinionLine(string line)
    {
        return line.StartsWith("- ", StringComparison.Ordinal) &&
            Regex.IsMatch(line, @"^-\s*[^:：]+[:：]\s*없음\s*$");
    }

    private static bool IsLowValueSummaryNoise(string line)
    {
        if (IsSectionMarker(line))
        {
            return false;
        }

        if (line.Length <= 1)
        {
            return true;
        }

        if (line.Length <= 3 && Regex.IsMatch(line, @"^[가-힣ㅋㅎㅠㅜ]+$"))
        {
            return true;
        }

        return false;
    }

    private SummaryCache LoadSummaryCache(string transcript, int chunkCount, int chunkSize)
    {
        var transcriptHash = ComputeHash(transcript);
        var cachePath = GetSummaryCachePath(transcriptHash);
        if (!File.Exists(cachePath))
        {
            return CreateEmptySummaryCache(transcriptHash, chunkCount, chunkSize, cachePath);
        }

        try
        {
            var cache = JsonSerializer.Deserialize<SummaryCache>(
                File.ReadAllText(cachePath, Encoding.UTF8),
                JsonOptions);

            if (cache is null ||
                cache.TranscriptHash != transcriptHash ||
                cache.CacheVersion != SummaryCacheVersion ||
                cache.ChatModel != _configuration.ChatModel ||
                cache.ChunkSize != chunkSize ||
                cache.ChunkCount != chunkCount)
            {
                return CreateEmptySummaryCache(transcriptHash, chunkCount, chunkSize, cachePath);
            }

            return cache with { CachePath = cachePath };
        }
        catch
        {
            return CreateEmptySummaryCache(transcriptHash, chunkCount, chunkSize, cachePath);
        }
    }

    private SummaryCache CreateEmptySummaryCache(string transcriptHash, int chunkCount, int chunkSize, string cachePath)
    {
        return new SummaryCache(
            SummaryCacheVersion,
            _configuration.ChatModel,
            transcriptHash,
            chunkSize,
            chunkCount,
            cachePath,
            [],
            null);
    }

    private void SaveSummaryCache(SummaryCache cache)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(cache.CachePath)!);
        File.WriteAllText(cache.CachePath, JsonSerializer.Serialize(cache, JsonOptions), Encoding.UTF8);
    }

    private bool HasCachedChunkOpinion(SummaryCache cache, string chunk, int index)
    {
        return cache.ChunkOpinions.TryGetValue(index, out var indexedOpinion) &&
            !string.IsNullOrWhiteSpace(indexedOpinion) ||
            TryLoadChunkOpinionCache(chunk, out _);
    }

    private bool TryLoadChunkOpinionCache(string chunk, out string opinion)
    {
        var cachePath = GetChunkOpinionCachePath(chunk);
        if (File.Exists(cachePath))
        {
            opinion = File.ReadAllText(cachePath, Encoding.UTF8).Trim();
            return !string.IsNullOrWhiteSpace(opinion);
        }

        opinion = string.Empty;
        return false;
    }

    private void SaveChunkOpinionCache(string chunk, string opinion)
    {
        if (string.IsNullOrWhiteSpace(opinion))
        {
            return;
        }

        var cachePath = GetChunkOpinionCachePath(chunk);
        Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
        File.WriteAllText(cachePath, opinion.Trim(), Encoding.UTF8);
    }

    private string GetChunkOpinionCachePath(string chunk)
    {
        var cacheKey = ComputeHash($"{SummaryCacheVersion}|{_configuration.ChatModel}|{ComputeHash(chunk)}");
        return Path.Combine(_summaryCacheDirectory, "chunks", $"{cacheKey}.txt");
    }

    private string GetSummaryCachePath(string transcriptHash)
    {
        return Path.Combine(_summaryCacheDirectory, $"{transcriptHash}.json");
    }

    private bool TryLoadSttCache(string audioPath, out string transcript)
    {
        try
        {
            var cachePath = GetSttCachePath(audioPath);
            if (File.Exists(cachePath))
            {
                transcript = File.ReadAllText(cachePath, Encoding.UTF8).Trim();
                return !string.IsNullOrWhiteSpace(transcript);
            }
        }
        catch
        {
            // STT caching is an optimization only. Cache failures must not block transcription.
        }

        transcript = string.Empty;
        return false;
    }

    private string SaveAndReturnSttCache(string audioPath, string transcript)
    {
        if (string.IsNullOrWhiteSpace(transcript))
        {
            return transcript;
        }

        try
        {
            var cachePath = GetSttCachePath(audioPath);
            Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
            File.WriteAllText(cachePath, transcript.Trim(), Encoding.UTF8);
        }
        catch
        {
            // STT caching is an optimization only. Cache failures must not block the result.
        }

        return transcript;
    }

    private bool TryLoadSttChunkCache(string audioPath, int chunkIndex, int chunkCount, out string transcript)
    {
        try
        {
            var cachePath = GetSttChunkCachePath(audioPath, chunkIndex, chunkCount);
            if (File.Exists(cachePath))
            {
                transcript = File.ReadAllText(cachePath, Encoding.UTF8).Trim();
                return !string.IsNullOrWhiteSpace(transcript);
            }
        }
        catch
        {
            // Chunk caching is an optimization only. Cache failures must not block transcription.
        }

        transcript = string.Empty;
        return false;
    }

    private void SaveSttChunkCache(string audioPath, int chunkIndex, int chunkCount, string transcript)
    {
        if (string.IsNullOrWhiteSpace(transcript))
        {
            return;
        }

        try
        {
            var cachePath = GetSttChunkCachePath(audioPath, chunkIndex, chunkCount);
            Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
            File.WriteAllText(cachePath, transcript.Trim(), Encoding.UTF8);
        }
        catch
        {
            // Chunk caching is an optimization only. Cache failures must not block transcription.
        }
    }

    private string GetSttCachePath(string audioPath)
    {
        var file = new FileInfo(audioPath);
        var fullPath = file.Exists ? file.FullName : Path.GetFullPath(audioPath);
        var fileLength = file.Exists ? file.Length : 0;
        var lastWriteTicks = file.Exists ? file.LastWriteTimeUtc.Ticks : 0;
        var cacheKey = ComputeHash(string.Join('\n',
            "stt-cache-v1",
            fullPath,
            fileLength.ToString(),
            lastWriteTicks.ToString(),
            _configuration.SttProvider,
            _configuration.TranscriptionModel,
            _configuration.LocalWhisperModel,
            ReadEnvironmentSetting("STT_AUDIO_PREPROCESSING", "true"),
            ReadEnvironmentSetting("STT_PROMPT_ENABLED", "false"),
            _sttCategory,
            _sttTerms));

        return Path.Combine(_sttCacheDirectory, $"{cacheKey}.txt");
    }

    private string GetSttChunkCachePath(string audioPath, int chunkIndex, int chunkCount)
    {
        var baseKey = Path.GetFileNameWithoutExtension(GetSttCachePath(audioPath));
        var chunkKey = ComputeHash(string.Join('\n',
            "stt-chunk-cache-v1",
            baseKey,
            chunkIndex.ToString(),
            chunkCount.ToString(),
            ReadEnvironmentSetting("STT_CHUNK_MINUTES", "5")));

        return Path.Combine(_sttCacheDirectory, "chunks", $"{chunkKey}.txt");
    }

    private void CleanupCacheDirectories()
    {
        lock (CacheCleanupLock)
        {
            if (_cacheCleanupCompleted)
            {
                return;
            }

            _cacheCleanupCompleted = true;
        }

        var retentionDays = ReadPositiveIntSetting("AI_CACHE_RETENTION_DAYS", DefaultCacheRetentionDays, min: 1, max: 365);
        var maxFiles = ReadPositiveIntSetting("AI_CACHE_MAX_FILES", DefaultCacheMaxFiles, min: 100, max: 10000);

        CleanupCacheDirectory(_summaryCacheDirectory, retentionDays, maxFiles);
        CleanupCacheDirectory(_sttCacheDirectory, retentionDays, maxFiles);
    }

    private static void CleanupCacheDirectory(string directory, int retentionDays, int maxFiles)
    {
        try
        {
            if (!Directory.Exists(directory))
            {
                return;
            }

            var cutoff = DateTime.UtcNow.AddDays(-retentionDays);
            var files = Directory
                .EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                .Select(path => new FileInfo(path))
                .Where(file => file.Exists)
                .ToList();

            foreach (var file in files.Where(file => file.LastWriteTimeUtc < cutoff))
            {
                TryDeleteFile(file);
            }

            files = Directory
                .EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                .Select(path => new FileInfo(path))
                .Where(file => file.Exists)
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .ToList();

            foreach (var file in files.Skip(maxFiles))
            {
                TryDeleteFile(file);
            }
        }
        catch
        {
            // Cache cleanup is a performance maintenance task only.
        }
    }

    private static void TryDeleteFile(FileInfo file)
    {
        try
        {
            file.Delete();
        }
        catch
        {
            // Ignore locked or already removed cache files.
        }
    }

    private static string ReadEnvironmentSetting(string name, string fallback)
    {
        return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process) ??
            Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User) ??
            Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine) ??
            fallback;
    }

    private static string ComputeHash(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private void EnsureReady()
    {
        if (!_configuration.IsReady)
        {
            throw new InvalidOperationException("GROQ_API_KEY 또는 XAI_API_KEY 환경 변수가 없어 AI 회의록 정리 기능을 사용할 수 없습니다.");
        }
    }

    private static string NormalizeTranscript(string transcript)
    {
        if (string.IsNullOrWhiteSpace(transcript))
        {
            return string.Empty;
        }

        var normalized = transcript.Replace("\r\n", "\n").Replace('\r', '\n');
        normalized = ApplyTechnicalTermCorrections(normalized);
        normalized = HorizontalWhitespacePattern.Replace(normalized, " ");
        normalized = SentenceBreakPattern.Replace(normalized, "\n");
        normalized = KoreanSentenceBreakPattern.Replace(normalized, "\n");

        var lines = normalized
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToList();
        lines = RemoveSttHallucinationRepeats(lines);

        var builder = new StringBuilder();
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (IsSectionMarker(line) && builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.AppendLine(line);

            if (IsSectionMarker(line) && i < lines.Count - 1)
            {
                builder.AppendLine();
            }
        }

        return ExcessBlankLinePattern.Replace(builder.ToString().Trim(), "\n\n");
    }

    private static List<string> RemoveSttHallucinationRepeats(IReadOnlyList<string> lines)
    {
        var cleaned = new List<string>();
        var repeatedBracketHallucinations = lines
            .Where(line => BracketOnlyPattern.IsMatch(line) && !IsSectionMarker(line) && !IsPreservedBracketAnnotation(line))
            .GroupBy(line => line, StringComparer.Ordinal)
            .Where(group => group.Count() >= 5)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.Ordinal);

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (repeatedBracketHallucinations.Contains(line))
            {
                continue;
            }

            if (IsLikelySttBracketAnnotation(line))
            {
                continue;
            }

            var runLength = 1;
            while (i + runLength < lines.Count &&
                string.Equals(lines[i + runLength], line, StringComparison.Ordinal))
            {
                runLength++;
            }

            if (IsRepeatedBracketHallucination(line, runLength))
            {
                i += runLength - 1;
                continue;
            }

            if (runLength >= 3 && !IsSectionMarker(line))
            {
                cleaned.Add(line);
                i += runLength - 1;
                continue;
            }

            cleaned.Add(line);
        }

        return cleaned;
    }

    private static bool IsRepeatedBracketHallucination(string line, int runLength)
    {
        return runLength >= 3 &&
            BracketOnlyPattern.IsMatch(line) &&
            !IsSectionMarker(line);
    }

    private static bool IsLikelySttBracketAnnotation(string line)
    {
        return line.Length <= 80 &&
            line.StartsWith("[", StringComparison.Ordinal) &&
            !IsSectionMarker(line) &&
            !IsPreservedBracketAnnotation(line);
    }

    private static bool IsPreservedBracketAnnotation(string line)
    {
        return line.StartsWith("[확인 필요", StringComparison.Ordinal);
    }

    private static string ApplyTechnicalTermCorrections(string text)
    {
        foreach (var (pattern, replacement) in TechnicalTermCorrections)
        {
            text = pattern.Replace(text, replacement);
        }

        return text;
    }

    private string BuildSttPrompt()
    {
        var terms = string.IsNullOrWhiteSpace(_sttTerms)
            ? "별도 용어 없음"
            : _sttTerms;

        return $"""
        한국어 {_sttCategory} 음성입니다.
        다음 카테고리와 주요 용어를 참고해서 STT를 수행하세요.
        회의/강의 카테고리: {_sttCategory}
        주요 용어: {terms}

        한국어 발화 속에 영어 약어, 제품명, 인명, 기술 용어가 섞여 나올 수 있습니다.
        주요 용어와 비슷하게 들리는 발화는 가능하면 정확한 용어로 인식하세요.
        요약하지 말고 들리는 발화를 그대로 전사하세요.
        불명확한 부분은 억지로 새 내용을 만들지 마세요.
        """;
    }

    private static bool IsSttPromptEnabled()
    {
        var configured =
            Environment.GetEnvironmentVariable("STT_PROMPT_ENABLED", EnvironmentVariableTarget.Process) ??
            Environment.GetEnvironmentVariable("STT_PROMPT_ENABLED", EnvironmentVariableTarget.User) ??
            Environment.GetEnvironmentVariable("STT_PROMPT_ENABLED", EnvironmentVariableTarget.Machine);

        return string.Equals(configured?.Trim(), "true", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(configured?.Trim(), "1", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSectionMarker(string line)
    {
        return line.StartsWith("[구간 ", StringComparison.Ordinal) && line.EndsWith(']');
    }

    private static bool IsRecoverableSttFailure(Exception ex, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested || ex is OperationCanceledException)
        {
            return false;
        }

        return ex is HttpRequestException or TaskCanceledException or InvalidOperationException;
    }

    private static string GuessMimeType(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".m4a" => "audio/mp4",
            ".mp4" => "video/mp4",
            ".ogg" => "audio/ogg",
            ".webm" => "audio/webm",
            ".opus" => "audio/ogg",
            _ => "application/octet-stream"
        };
    }

    private sealed record TranscriptionResponse(string? Text);
    private sealed record ChatCompletionRequest(
        string Model,
        IReadOnlyList<ChatRequestMessage> Messages,
        double Temperature,
        [property: JsonPropertyName("max_completion_tokens")]
        int MaxCompletionTokens);
    private sealed record ChatRequestMessage(string Role, string Content);
    private sealed record ChatResponseMessage(
        string? Role,
        string? Content,
        [property: JsonPropertyName("reasoning_content")]
        string? ReasoningContent = null,
        string? Reasoning = null);
    private sealed record ChatCompletionResponse(IReadOnlyList<ChatChoice>? Choices);
    private sealed record ChatChoice(ChatResponseMessage? Message);
    private sealed record SummaryCache(
        int CacheVersion,
        string ChatModel,
        string TranscriptHash,
        int ChunkSize,
        int ChunkCount,
        string CachePath,
        Dictionary<int, string> ChunkOpinions,
        string? FinalMinutes);
}
