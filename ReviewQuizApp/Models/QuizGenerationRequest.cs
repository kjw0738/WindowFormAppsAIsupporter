namespace ReviewQuizApp.Models;

public sealed class QuizGenerationRequest
{
    public AssignmentInfo Assignment { get; set; } = new();
    public string Goal { get; set; } = string.Empty;
    public int QuestionCount { get; set; } = 5;
    public string Difficulty { get; set; } = "Normal";
    public List<QuestionType> QuestionTypes { get; set; } = [];
}
