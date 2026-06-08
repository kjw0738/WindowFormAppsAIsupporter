using ReviewQuizApp.Models;

namespace ReviewQuizApp.Services;

public sealed class AssignmentRepository
{
    private readonly string _assignmentDirectory;

    public AssignmentRepository()
    {
        _assignmentDirectory = Path.Combine(AppContext.BaseDirectory, "Data", "assignments");
    }

    public List<AssignmentInfo> LoadAssignments()
    {
        if (!Directory.Exists(_assignmentDirectory))
        {
            Directory.CreateDirectory(_assignmentDirectory);
        }

        return Directory.GetFiles(_assignmentDirectory, "*.json")
            .OrderBy(path => path)
            .Select(CreateAssignment)
            .ToList();
    }

    private static AssignmentInfo CreateAssignment(string filePath)
    {
        var content = File.ReadAllText(filePath);
        var title = Path.GetFileNameWithoutExtension(filePath);

        return new AssignmentInfo
        {
            Id = Path.GetFileNameWithoutExtension(filePath),
            Title = title,
            FilePath = filePath,
            Content = content
        };
    }
}
