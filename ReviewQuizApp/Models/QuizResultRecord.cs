namespace ReviewQuizApp.Models;

public sealed class QuizResultRecord
{
    public string ResultId { get; set; } = Guid.NewGuid().ToString("N");
    public string QuizId { get; set; } = string.Empty;
    public string QuizTitle { get; set; } = string.Empty;
    public string QuizFilePath { get; set; } = string.Empty;
    public string AssignmentId { get; set; } = string.Empty;
    public string AssignmentTitle { get; set; } = string.Empty;
    public DateTime SolvedAt { get; set; } = DateTime.Now;
    public int CorrectCount { get; set; }
    public int TotalCount { get; set; }
    public List<QuizAnswerRecord> Answers { get; set; } = [];
    public int Score => TotalCount == 0 ? 0 : CorrectCount * 100 / TotalCount;
}
