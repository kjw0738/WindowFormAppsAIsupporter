using System.Collections.Generic;
using IntegratedMeetingStudio.Models;

namespace IntegratedMeetingStudio.Models
{
    public class QuizQuestion
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
    }

    public class MeetingItem
    {
        public string MeetingId { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public List<string> Decisions { get; set; } = new List<string>();
        public List<QuizQuestion> Quizzes { get; set; } = new List<QuizQuestion>();
    }

    public class ProjectData
    {
        public string ProjectId { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public List<MeetingItem> Meetings { get; set; } = new List<MeetingItem>();
        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
