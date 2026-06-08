using System.Text.Json;
using ReviewQuizApp.Models;

namespace ReviewQuizApp.Services;

public sealed class QuizHistoryService
{
    private readonly string _historyPath = Path.Combine(AppContext.BaseDirectory, "Data", "history", "quiz-history.json");
    private readonly string _quizDirectory = Path.Combine(AppContext.BaseDirectory, "Data", "generated-quizzes");

    public void Append(QuizResultRecord record)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_historyPath)!);

        var records = Load();
        records.Add(record);

        SaveRecords(records);
    }

    public List<QuizResultRecord> Load()
    {
        if (!File.Exists(_historyPath))
        {
            return [];
        }

        var json = File.ReadAllText(_historyPath);
        return JsonSerializer.Deserialize<List<QuizResultRecord>>(json, QuizJson.Options) ?? [];
    }

    public void Delete(string resultId)
    {
        var existingRecords = Load();
        var deletedRecords = existingRecords
            .Where(record => string.Equals(record.ResultId, resultId, StringComparison.OrdinalIgnoreCase))
            .ToList();
        var records = existingRecords
            .Where(record => !string.Equals(record.ResultId, resultId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var record in deletedRecords)
        {
            DeleteQuizSnapshot(record.QuizFilePath);
        }

        SaveRecords(records);
    }

    public void DeleteAll()
    {
        if (File.Exists(_historyPath))
        {
            File.Delete(_historyPath);
        }

        if (Directory.Exists(_quizDirectory))
        {
            Directory.Delete(_quizDirectory, recursive: true);
        }
    }

    public string SaveQuizSnapshot(QuizDataFile quiz)
    {
        Directory.CreateDirectory(_quizDirectory);

        if (string.IsNullOrWhiteSpace(quiz.QuizId))
        {
            quiz.QuizId = Guid.NewGuid().ToString("N");
        }

        var fileName = $"{quiz.GeneratedAt:yyyyMMddHHmmss}-{SanitizeFileName(quiz.QuizId)}.json";
        var filePath = Path.Combine(_quizDirectory, fileName);
        var json = JsonSerializer.Serialize(quiz, QuizJson.Options);
        File.WriteAllText(filePath, json);
        return filePath;
    }

    public QuizDataFile LoadQuizSnapshot(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            throw new FileNotFoundException("저장된 퀴즈 파일을 찾을 수 없습니다.", filePath);
        }

        var json = File.ReadAllText(filePath);
        var quiz = JsonSerializer.Deserialize<QuizDataFile>(json, QuizJson.Options)
            ?? throw new InvalidDataException("저장된 퀴즈 데이터를 읽을 수 없습니다.");

        new QuizDataLoader().ValidateAndNormalize(quiz);
        return quiz;
    }

    private void SaveRecords(List<QuizResultRecord> records)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_historyPath)!);
        var json = JsonSerializer.Serialize(records, QuizJson.Options);
        File.WriteAllText(_historyPath, json);
    }

    private void DeleteQuizSnapshot(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return;
        }

        var fullSnapshotPath = Path.GetFullPath(filePath);
        var fullQuizDirectory = Path.GetFullPath(_quizDirectory);
        var relativePath = Path.GetRelativePath(fullQuizDirectory, fullSnapshotPath);
        if (relativePath.StartsWith("..", StringComparison.Ordinal) || Path.IsPathRooted(relativePath))
        {
            return;
        }

        File.Delete(fullSnapshotPath);
    }

    private static string SanitizeFileName(string value)
    {
        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalid, '_');
        }

        return value;
    }
}
