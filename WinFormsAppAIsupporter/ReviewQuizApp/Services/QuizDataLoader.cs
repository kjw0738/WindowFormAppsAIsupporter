using ReviewQuizApp.Models;

namespace ReviewQuizApp.Services;

public sealed class QuizDataLoader
{
    public QuizDataFile Load(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Quiz file path is empty.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Quiz file was not found.", filePath);
        }

        var json = File.ReadAllText(filePath);
        var quiz = System.Text.Json.JsonSerializer.Deserialize<QuizDataFile>(json, QuizJson.Options)
            ?? throw new InvalidDataException("Quiz JSON could not be read.");

        ValidateAndNormalize(quiz);
        return quiz;
    }

    public void ValidateAndNormalize(QuizDataFile quiz)
    {
        if (quiz.Questions.Count == 0)
        {
            throw new InvalidDataException("Quiz has no questions.");
        }

        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < quiz.Questions.Count; index++)
        {
            var question = quiz.Questions[index];
            if (string.IsNullOrWhiteSpace(question.Id))
            {
                question.Id = $"q{index + 1}";
            }

            var label = question.Id;

            if (!ids.Add(question.Id))
            {
                throw new InvalidDataException($"{label}: Question ID is duplicated.");
            }

            if (string.IsNullOrWhiteSpace(question.Title))
            {
                throw new InvalidDataException($"{label}: Question title is empty.");
            }

            if (string.IsNullOrWhiteSpace(question.CorrectAnswer))
            {
                throw new InvalidDataException($"{label}: Correct answer is empty.");
            }

            if (question.Type == QuestionType.TrueFalse)
            {
                question.Options = ["O", "X"];
            }

            if (question.Type == QuestionType.MultipleChoice || question.Type == QuestionType.TrueFalse)
            {
                if (question.Options.Count < 2)
                {
                    throw new InvalidDataException($"{label}: Choice questions need at least two options.");
                }

                if (!question.Options.Any(option => SameAnswer(option, question.CorrectAnswer)))
                {
                    throw new InvalidDataException($"{label}: Correct answer must be included in options.");
                }
            }
        }
    }

    public static bool SameAnswer(string left, string right)
    {
        return string.Equals(Normalize(left), Normalize(right), StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string value)
    {
        return value.Trim().Replace(" ", string.Empty);
    }
}
