using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text.Json;
using WinFormsAppAIsupporter.Models;
using WinFormsAppAIsupporter.Services;
using System.Linq;

namespace WinFormsAppAIsupporter.Controls
{
    public partial class AIQuestionView : UserControl
    {
        private ProjectData _projectData;
        
        // [수정] 기본 API 키 설정
        private const string DefaultApiKey = "여기에_사용자의_API_키_입력";

        public AIQuestionView()
        {
            InitializeComponent();
        }

        public void SetData(ProjectData data)
        {
            _projectData = data;
            UpdateHistoryList();
        }

        private void UpdateHistoryList()
        {
            lstHistory.Items.Clear();
            if (_projectData?.Meetings == null) return;

            foreach (var meeting in _projectData.Meetings.OrderByDescending(m => m.Date))
            {
                lstHistory.Items.Add($"{meeting.Date} - {meeting.Title}");
            }
        }

        private void lstHistory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstHistory.SelectedIndex == -1) return;

            // 선택된 회의 데이터 찾기 (아이템 텍스트 형식: "날짜 - 제목")
            string selectedText = lstHistory.SelectedItem.ToString() ?? "";
            var meeting = _projectData.Meetings.FirstOrDefault(m => $"{m.Date} - {m.Title}" == selectedText);

            if (meeting != null)
            {
                // UI 업데이트
                txtMeetingSummary.Text = meeting.Summary;
                pnlQuestions.Controls.Clear();
                
                // 퀴즈 카드 다시 생성
                foreach (var quiz in meeting.Quizzes)
                {
                    AddQuizToPanel(quiz);
                }
            }
        }

        private string CleanJsonResponse(string rawResponse)
        {
            if (string.IsNullOrWhiteSpace(rawResponse)) return "{}";

            try
            {
                string cleaned = rawResponse.Trim();
                int startIndex = cleaned.IndexOf('{');
                int endIndex = cleaned.LastIndexOf('}');

                if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
                {
                    return cleaned.Substring(startIndex, endIndex - startIndex + 1);
                }
            }
            catch { }

            return rawResponse;
        }

        private async void btnGenerateQuestions_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMeetingSummary.Text))
            {
                MessageBox.Show("회의록 내용을 입력해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string apiKey = string.IsNullOrWhiteSpace(txtApiKey.Text) ? DefaultApiKey : txtApiKey.Text;
            if (apiKey == "여기에_사용자의_API_키_입력")
            {
                MessageBox.Show("Groq API Key를 입력하거나 코드에서 설정해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnGenerateQuestions.Enabled = false;
            btnGenerateQuestions.Text = "⏳ 분석 중...";

            string rawResponse = "";
            try
            {
                var groqService = new GroqService(apiKey);
                string prompt = $@"
아래 회의록 내용을 바탕으로 팀원들의 이해도를 확인할 수 있는 복습 질문 3개를 생성해줘.
응답은 반드시 아래 JSON 형식을 지켜줘:
{{
  ""quizzes"": [
    {{ ""question"": ""질문 내용"", ""answer"": ""정답"", ""explanation"": ""해설"" }}
  ]
}}
*중요: 응답은 반드시 {{로 시작해서 }}로 끝나는 순수 JSON 데이터만 보내주세요.*

회의록 내용:
{txtMeetingSummary.Text}";

                rawResponse = await groqService.GetCompletionAsync(prompt, "You are a professional meeting analyzer that outputs ONLY raw JSON.");
                string cleanedResponse = CleanJsonResponse(rawResponse);
                
                try 
                {
                    var result = JsonSerializer.Deserialize<QuizResponse>(cleanedResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (result != null && result.Quizzes != null)
                    {
                        pnlQuestions.Controls.Clear();
                        foreach (var quiz in result.Quizzes)
                        {
                            AddQuizToPanel(quiz);
                        }

                        // 새로운 회의 데이터로 추가 또는 기존 데이터 업데이트
                        var existingMeeting = _projectData.Meetings.FirstOrDefault(m => m.Summary == txtMeetingSummary.Text);
                        if (existingMeeting != null)
                        {
                            existingMeeting.Quizzes = result.Quizzes;
                        }
                        else
                        {
                            string title = txtMeetingSummary.Text.Length > 20 ? txtMeetingSummary.Text.Substring(0, 20) + "..." : txtMeetingSummary.Text;
                            _projectData.Meetings.Add(new MeetingItem 
                            { 
                                MeetingId = "MEET-" + DateTime.Now.Ticks, 
                                Title = title, 
                                Date = DateTime.Now.ToString("yyyy-MM-dd"), 
                                Summary = txtMeetingSummary.Text,
                                Quizzes = result.Quizzes 
                            });
                        }
                        
                        DataManager.SaveData(_projectData);
                        UpdateHistoryList(); // 리스트 갱신
                        MessageBox.Show("AI 질문 생성이 완료되었습니다!", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (JsonException jEx)
                {
                    MessageBox.Show($"데이터 해석 중 오류가 발생했습니다.\n\n[오류]: {jEx.Message}\n\n[AI 실제 응답]:\n{rawResponse}", "데이터 파싱 에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류 발생: {ex.Message}", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnGenerateQuestions.Enabled = true;
                btnGenerateQuestions.Text = "🤖 질문 생성";
            }
        }

        private void AddQuizToPanel(QuizQuestion quiz)
        {
            Panel quizCard = new Panel
            {
                Width = pnlQuestions.Width - 45,
                AutoSize = true,
                MinimumSize = new Size(pnlQuestions.Width - 45, 60),
                BackColor = Color.White,
                Padding = new Padding(15),
                Margin = new Padding(0, 0, 0, 15),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblQ = new Label
            {
                Text = $"Q. {quiz.Question}",
                Font = new Font("맑은 고딕", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 45, 48),
                Dock = DockStyle.Top,
                AutoSize = true,
                MaximumSize = new Size(pnlQuestions.Width - 60, 0), // 너비 제한 추가
                Padding = new Padding(0, 0, 0, 10)
            };

            Panel pnlAnswerArea = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Visible = false,
                Padding = new Padding(0, 5, 0, 10)
            };

            Label lblA = new Label
            {
                Text = $"정답: {quiz.Answer}",
                Font = new Font("맑은 고딕", 9F, FontStyle.Bold),
                ForeColor = Color.MediumSlateBlue,
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(0, 0, 0, 5)
            };

            Label lblE = new Label
            {
                Text = quiz.Explanation,
                Font = new Font("맑은 고딕", 8.5F),
                ForeColor = Color.DimGray,
                Dock = DockStyle.Top,
                AutoSize = true
            };
            pnlAnswerArea.Controls.Add(lblE);
            pnlAnswerArea.Controls.Add(lblA);

            Label btnToggle = new Label
            {
                Text = "정답 확인 ▼",
                Font = new Font("맑은 고딕", 8F, FontStyle.Bold),
                ForeColor = Color.Gray,
                BackColor = Color.FromArgb(245, 245, 245),
                Cursor = Cursors.Hand,
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleCenter
            };

            bool isExpanded = false;
            btnToggle.Click += (s, e) =>
            {
                isExpanded = !isExpanded;
                pnlAnswerArea.Visible = isExpanded;
                btnToggle.Text = isExpanded ? "정답 가리기 ▲" : "정답 확인 ▼";
            };

            quizCard.Controls.Add(pnlAnswerArea);
            quizCard.Controls.Add(btnToggle);
            quizCard.Controls.Add(lblQ);

            pnlQuestions.Controls.Add(quizCard);
        }

        private async void btnExtractTasks_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMeetingSummary.Text))
            {
                MessageBox.Show("회의록 내용을 입력해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string apiKey = string.IsNullOrWhiteSpace(txtApiKey.Text) ? DefaultApiKey : txtApiKey.Text;
            if (apiKey == "여기에_사용자의_API_키_입력")
            {
                MessageBox.Show("Groq API Key를 입력하거나 코드에서 설정해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnExtractTasks.Enabled = false;
            btnExtractTasks.Text = "⏳ 추출 중...";

            string rawResponse = "";
            try
            {
                var groqService = new GroqService(apiKey);
                string prompt = $@"
아래 회의록에서 할 일(To-Do) 및 다음 회의 일정을 추출해줘. 
각 항목은 담당자(회의 일정의 경우 '일정 안내'), 과제/일정 내용, 마감일(YYYY-MM-DD 형식)을 포함해야 해. 
마감일이 명확하지 않으면 오늘로부터 3일 뒤로 설정해줘.
응답은 반드시 아래 JSON 형식을 지켜줘:
{{
  ""tasks"": [
    {{ ""assignee"": ""이름 또는 일정 안내"", ""content"": ""할일 또는 회의 내용"", ""dueDate"": ""YYYY-MM-DD"", ""priority"": ""긴급/보통/낮음"" }}
  ]
}}
*중요: 응답은 반드시 {{로 시작해서 }}로 끝나는 순수 JSON 데이터만 보내주세요.*

회의록 내용:
{txtMeetingSummary.Text}";

                rawResponse = await groqService.GetCompletionAsync(prompt, "You are a professional task manager that outputs ONLY raw JSON.");
                string cleanedResponse = CleanJsonResponse(rawResponse);
                
                try
                {
                    var result = JsonSerializer.Deserialize<TaskResponse>(cleanedResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result != null && result.Tasks != null)
                    {
                        int addedCount = 0;
                        foreach (var t in result.Tasks)
                        {
                            var newTask = new TaskItem(
                                $"TASK-AI-{DateTime.Now:MMddHHmm}{addedCount}",
                                t.Assignee,
                                t.Content,
                                t.DueDate,
                                "Not Started",
                                t.Priority
                            );
                            _projectData.Tasks.Add(newTask);
                            addedCount++;
                        }

                        DataManager.SaveData(_projectData);
                        MessageBox.Show($"{addedCount}개의 할 일을 추출하여 칸반 보드에 추가했습니다!", "추출 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (JsonException jEx)
                {
                    MessageBox.Show($"데이터 해석 중 오류가 발생했습니다.\n\n[오류]: {jEx.Message}\n\n[AI 실제 응답]:\n{rawResponse}", "데이터 파싱 에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"추출 중 오류 발생: {ex.Message}", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnExtractTasks.Enabled = true;
                btnExtractTasks.Text = "📋 할일 추출";
            }
        }

        private class QuizResponse { public List<QuizQuestion>? Quizzes { get; set; } }
        private class TaskResponse { public List<AITask>? Tasks { get; set; } }
        private class AITask 
        { 
            public string Assignee { get; set; } = ""; 
            public string Content { get; set; } = ""; 
            public string DueDate { get; set; } = ""; 
            public string Priority { get; set; } = "보통";
        }
    }
}
