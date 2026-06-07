namespace ReviewQuizApp.Models;

public sealed class QuizDataFile
{
    public string QuizId { get; set; } = Guid.NewGuid().ToString("N");
    public string QuizTitle { get; set; } = "복습 퀴즈";
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public List<QuizQuestion> Questions { get; set; } = [];
}
