using System.Text;
using System.Text.Json;

namespace IntegratedMeetingStudio.Services;

public sealed class MeetingStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _storageDirectory;

    public MeetingStorageService()
    {
        _storageDirectory = Path.Combine(AppContext.BaseDirectory, "Data", "meetings");
        Directory.CreateDirectory(_storageDirectory);
    }

    public IReadOnlyList<MeetingRecord> GetRecords()
    {
        return Directory
            .GetFiles(_storageDirectory, "*.json")
            .Select(ReadRecordOrNull)
            .OfType<MeetingRecord>()
            .OrderByDescending(record => record.CreatedAt)
            .ToList();
    }

    public IReadOnlyList<MeetingRecordListItem> GetRecordList()
    {
        return Directory
            .GetFiles(_storageDirectory, "*.json")
            .Select(ReadRecordListItemOrNull)
            .OfType<MeetingRecordListItem>()
            .OrderByDescending(record => record.CreatedAt)
            .ToList();
    }

    public MeetingRecord? GetRecord(string id)
    {
        var jsonPath = Path.Combine(_storageDirectory, $"{id}.json");
        return File.Exists(jsonPath) ? ReadRecordOrNull(jsonPath) : null;
    }

    public MeetingRecord Save(string audioPath, string transcript, string summary, string title = "")
    {
        var now = DateTime.Now;
        var id = $"{now:yyyyMMdd_HHmmss}_{SanitizeFileName(Path.GetFileNameWithoutExtension(audioPath))}";
        var markdownPath = Path.Combine(_storageDirectory, $"{id}.md");
        var jsonPath = Path.Combine(_storageDirectory, $"{id}.json");

        var record = new MeetingRecord(
            id,
            now,
            audioPath,
            markdownPath,
            jsonPath,
            transcript,
            summary,
            title);

        File.WriteAllText(markdownPath, CreateMarkdown(record), Encoding.UTF8);
        File.WriteAllText(jsonPath, JsonSerializer.Serialize(record, JsonOptions), Encoding.UTF8);
        SaveRecordListItem(CreateListItem(record), GetMetadataPath(jsonPath));

        return record;
    }

    public void Update(MeetingRecord record)
    {
        var jsonPath = record.JsonPath;
        File.WriteAllText(jsonPath, JsonSerializer.Serialize(record, JsonOptions), Encoding.UTF8);
        var listItem = CreateListItem(record);
        SaveRecordListItem(listItem, GetMetadataPath(jsonPath));
    }

    public void Delete(string id)
    {
        var record = GetRecord(id);
        if (record is null)
        {
            return;
        }

        DeleteIfExists(record.MarkdownPath);
        DeleteIfExists(record.JsonPath);
        DeleteIfExists(GetMetadataPath(record.JsonPath));
    }

    public void DeleteAll()
    {
        foreach (var path in Directory.GetFiles(_storageDirectory, "*.json"))
        {
            DeleteIfExists(path);
        }

        foreach (var path in Directory.GetFiles(_storageDirectory, "*.md"))
        {
            DeleteIfExists(path);
        }

        foreach (var path in Directory.GetFiles(_storageDirectory, "*.meta"))
        {
            DeleteIfExists(path);
        }
    }

    private static string CreateMarkdown(MeetingRecord record)
    {
        var summary = string.IsNullOrWhiteSpace(record.Summary)
            ? "AI ?뚯쓽濡앹? ?꾩쭅 ?앹꽦?섏? ?딆븯?듬땲?? STT ?먮Ц留???λ맂 湲곕줉?낅땲??"
            : record.Summary;

        return $"""
        # ?뚯쓽濡??먮룞 ?뺣━ 寃곌낵

        - ?앹꽦 ?쇱떆: {record.CreatedAt:yyyy-MM-dd HH:mm:ss}
        - ?먮낯 ?뚯씪: {record.AudioPath}

        ## AI ?뺣━ ?뚯쓽濡?

        {summary}

        ## STT ?먮Ц

        {record.Transcript}
        """;
    }

    private static MeetingRecord? ReadRecordOrNull(string jsonPath)
    {
        try
        {
            var record = JsonSerializer.Deserialize<MeetingRecord>(File.ReadAllText(jsonPath, Encoding.UTF8), JsonOptions);
            return record is null ? null : record with { JsonPath = jsonPath };
        }
        catch
        {
            return null;
        }
    }

    private static MeetingRecordListItem? ReadRecordListItemOrNull(string jsonPath)
    {
        try
        {
            var metadataPath = GetMetadataPath(jsonPath);
            if (TryReadFreshRecordListItem(metadataPath, jsonPath, out var cachedItem))
            {
                return cachedItem;
            }

            var bytes = File.ReadAllBytes(jsonPath);
            var reader = new Utf8JsonReader(bytes, isFinalBlock: true, state: default);
            string? id = null;
            var createdAt = DateTime.MinValue;
            string? audioPath = null;
            string? markdownPath = null;
            string? summary = null;

            while (reader.Read())
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    continue;
                }

                var propertyName = reader.GetString();
                if (!reader.Read())
                {
                    break;
                }

                switch (propertyName)
                {
                    case "id":
                        id = reader.GetString();
                        break;
                    case "createdAt":
                        createdAt = reader.GetDateTime();
                        break;
                    case "audioPath":
                        audioPath = reader.GetString();
                        break;
                    case "markdownPath":
                        markdownPath = reader.GetString();
                        break;
                    case "summary":
                        summary = reader.GetString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                id = Path.GetFileNameWithoutExtension(jsonPath);
            }

            var item = new MeetingRecordListItem(
                id,
                createdAt,
                audioPath ?? string.Empty,
                markdownPath ?? Path.ChangeExtension(jsonPath, ".md"),
                jsonPath,
                summary ?? string.Empty);
            SaveRecordListItem(item, metadataPath);
            return item;
        }
        catch
        {
            return null;
        }
    }

    private static bool TryReadFreshRecordListItem(string metadataPath, string jsonPath, out MeetingRecordListItem? item)
    {
        item = null;
        if (!File.Exists(metadataPath) ||
            File.GetLastWriteTimeUtc(metadataPath) < File.GetLastWriteTimeUtc(jsonPath))
        {
            return false;
        }

        try
        {
            item = JsonSerializer.Deserialize<MeetingRecordListItem>(File.ReadAllText(metadataPath, Encoding.UTF8), JsonOptions);
            return item is not null;
        }
        catch
        {
            return false;
        }
    }

    private static MeetingRecordListItem CreateListItem(MeetingRecord record)
    {
        return new MeetingRecordListItem(
            record.Id,
            record.CreatedAt,
            record.AudioPath,
            record.MarkdownPath,
            record.JsonPath,
            record.Summary,
            record.Title);
    }

    private static void SaveRecordListItem(MeetingRecordListItem item, string metadataPath)
    {
        try
        {
            File.WriteAllText(metadataPath, JsonSerializer.Serialize(item, JsonOptions), Encoding.UTF8);
        }
        catch
        {
            // Metadata cache is a performance optimization only.
        }
    }

    private static string GetMetadataPath(string jsonPath)
    {
        return Path.ChangeExtension(jsonPath, ".meta");
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(invalid, '_');
        }

        return string.IsNullOrWhiteSpace(fileName) ? "meeting" : fileName;
    }
}

public sealed record MeetingRecord(
    string Id,
    DateTime CreatedAt,
    string AudioPath,
    string MarkdownPath,
    string JsonPath,
    string Transcript,
    string Summary,
    string Title = "")
{
    public string? SwotJson { get; init; }
    public string? RolesJson { get; init; }
    public string? TimelineJson { get; init; }
    public string? SpeakerStatsJson { get; init; }
    public string? NextAgendaJson { get; init; }
}

public sealed record MeetingRecordListItem(
    string Id,
    DateTime CreatedAt,
    string AudioPath,
    string MarkdownPath,
    string JsonPath,
    string Summary,
    string Title = "");
