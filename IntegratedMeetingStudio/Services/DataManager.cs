using System;
using System.IO;
using System.Text.Json;
using System.Text.Encodings.Web;
using IntegratedMeetingStudio.Models;

namespace IntegratedMeetingStudio.Services
{
    public static class DataManager
    {
        private static readonly string FileName = Path.Combine(AppContext.BaseDirectory, "Data", "tasks", "project_data.json");
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 한글 깨짐 방지
        };

        public static ProjectData LoadData()
        {
            try
            {
                if (!File.Exists(FileName))
                {
                    return CreateDefaultData();
                }

                string jsonString = File.ReadAllText(FileName);
                var data = JsonSerializer.Deserialize<ProjectData>(jsonString);
                return data ?? CreateDefaultData();
            }
            catch (Exception)
            {
                return CreateDefaultData();
            }
        }

        public static void SaveData(ProjectData data)
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(data, Options);
                File.WriteAllText(FileName, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"저장 중 오류 발생: {ex.Message}");
            }
        }

        private static ProjectData CreateDefaultData()
        {
            return new ProjectData
            {
                ProjectId = "PROJ-2026-01",
                ProjectName = "AI WinForms 프로젝트",
                Meetings = new System.Collections.Generic.List<MeetingItem>(),
                Tasks = new System.Collections.Generic.List<TaskItem>
                {
                    new TaskItem("TASK-01", "김철수", "JSON 입출력 클래스 설계", "2026-06-10", "In Progress"),
                    new TaskItem("TASK-02", "이영희", "UI 레이아웃 구현", "2026-06-07", "Not Started")
                }
            };
        }
    }
}
