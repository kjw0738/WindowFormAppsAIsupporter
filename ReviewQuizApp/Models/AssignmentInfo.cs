namespace ReviewQuizApp.Models;

public sealed class AssignmentInfo
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public override string ToString()
    {
        return Title;
    }
}
