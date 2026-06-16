namespace IntegratedMeetingStudio.Models
{
    public class TaskItem
    {
        public string TaskId { get; set; } = string.Empty;
        public string Assignee { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string DueDate { get; set; } = string.Empty;
        public string Status { get; set; } = "Not Started"; // "Not Started", "In Progress", "Done"
        public string Priority { get; set; } = "보통"; // "긴급", "보통", "낮음"
        public int Order { get; set; } = 0; // 카드 순서

        public TaskItem() { }

        public TaskItem(string taskId, string assignee, string content, string dueDate, string status, string priority = "보통", int order = 0)
        {
            TaskId = taskId;
            Assignee = assignee;
            Content = content;
            DueDate = dueDate;
            Status = status;
            Priority = priority;
            Order = order;
        }
    }
}
