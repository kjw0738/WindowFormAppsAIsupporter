namespace ReviewQuizApp.Models;

public sealed class QuizQuestion
{
    public string Id { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<string> Options { get; set; } = [];
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
}
