using ReviewQuizApp.Models;

namespace ReviewQuizApp.Services;

public sealed class AssignmentQuizGenerator
{
    private readonly AiQuizGeneratorConfig _config = AiQuizGeneratorConfig.Load();
    private readonly InternalAiAssignmentQuizGenerator _aiGenerator = new();

    public bool IsConfigured => _config.IsConfigured;
    public bool UsesAi => _config.IsConfigured;
    public AiProvider Provider => _config.Provider;

    public async Task<QuizDataFile> GenerateAsync(QuizGenerationRequest request, CancellationToken cancellationToken = default)
    {
        if (!_config.IsConfigured)
        {
            throw new InvalidOperationException("AI API key was not found. Set GROQ_API_KEY, XAI_API_KEY, GEMINI_API_KEY, or OPENAI_API_KEY before using the quiz module.");
        }

        return await _aiGenerator.GenerateAsync(request, _config, cancellationToken);
    }
}
