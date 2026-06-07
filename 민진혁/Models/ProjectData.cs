using System.Collections.Generic;
using WinFormsAppAIsupporter.Models;

namespace WinFormsAppAIsupporter.Models
{
    public class MeetingItem
    {
        public string MeetingId { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public List<string> Decisions { get; set; } = new List<string>();
    }

    public class ProjectData
    {
        public string ProjectId { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public List<MeetingItem> Meetings { get; set; } = new List<MeetingItem>();
        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
