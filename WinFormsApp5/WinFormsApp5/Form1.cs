using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace WinFormsApp5
{
    public partial class Form1 : Form
    {
        TextBox txtMeeting, txtSummary, txtEnglish, txtJapanese;
        Button btnAnalyze;
        TableLayoutPanel calendarPanel;

        static readonly HttpClient http = new HttpClient();

        public Form1()
        {
            InitializeComponent();
            MakeUI();
        }

        private void MakeUI()
        {
            this.Text = "회의 번역 및 일정 캘린더 시스템";
            this.Width = 1000;
            this.Height = 720;

            Label lblInput = new Label() { Text = "회의 내용 입력", Left = 20, Top = 20, Width = 200 };
            this.Controls.Add(lblInput);

            txtMeeting = new TextBox()
            {
                Left = 20,
                Top = 45,
                Width = 450,
                Height = 230,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Text = "회의 내용 테스트"
            };
            this.Controls.Add(txtMeeting);

            btnAnalyze = new Button()
            {
                Text = "분석하기",
                Left = 20,
                Top = 290,
                Width = 450,
                Height = 40
            };
            btnAnalyze.Click += BtnAnalyze_Click;
            this.Controls.Add(btnAnalyze);

            Label lblCalendar = new Label() { Text = "앞으로 일정 캘린더 출력", Left = 20, Top = 350, Width = 250 };
            this.Controls.Add(lblCalendar);

            calendarPanel = new TableLayoutPanel()
            {
                Left = 20,
                Top = 375,
                Width = 450,
                Height = 280,
                ColumnCount = 7,
                RowCount = 5,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            for (int i = 0; i < 7; i++)
                calendarPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.28f));

            for (int i = 0; i < 5; i++)
                calendarPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));

            this.Controls.Add(calendarPanel);
            MakeCalendar();

            Label lblSummary = new Label() { Text = "회의 요약", Left = 520, Top = 20, Width = 200 };
            this.Controls.Add(lblSummary);

            txtSummary = new TextBox()
            {
                Left = 520,
                Top = 45,
                Width = 430,
                Height = 120,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(txtSummary);

            Label lblEnglish = new Label() { Text = "영어 번역", Left = 520, Top = 180, Width = 200 };
            this.Controls.Add(lblEnglish);

            txtEnglish = new TextBox()
            {
                Left = 520,
                Top = 205,
                Width = 430,
                Height = 120,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(txtEnglish);

            Label lblJapanese = new Label() { Text = "일본어 번역", Left = 520, Top = 340, Width = 200 };
            this.Controls.Add(lblJapanese);

            txtJapanese = new TextBox()
            {
                Left = 520,
                Top = 365,
                Width = 430,
                Height = 120,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(txtJapanese);
        }

        private async void BtnAnalyze_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMeeting.Text))
            {
                MessageBox.Show("회의 내용을 입력해주세요.");
                return;
            }

            btnAnalyze.Enabled = false;
            btnAnalyze.Text = "분석 중...";

            try
            {
                string result = await AnalyzeMeeting(txtMeeting.Text);

                MeetingResult data = JsonSerializer.Deserialize<MeetingResult>(
                    result,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                txtSummary.Text = data.summary;
                txtEnglish.Text = data.englishTranslation;
                txtJapanese.Text = data.japaneseTranslation;

                MakeCalendar();

                if (data.schedules != null)
                {
                    foreach (var item in data.schedules)
                    {
                        AddScheduleToCalendar(item.day, $"{item.time}\n{item.title}");
                    }
                }
            }
            catch
            {
                // API 크레딧 없을 때 발표용 임시 결과
                txtSummary.Text = "회의에서 프로젝트 점검 일정과 발표 자료 준비 일정을 정했습니다.";
                txtEnglish.Text = "The team decided to hold a project review meeting and prepare presentation materials.";
                txtJapanese.Text = "チームはプロジェクト点検会議を行い、発表資料を準備することにしました。";

                MakeCalendar();
                AddScheduleToCalendar(20, "15:00\n중간 점검 회의");
                AddScheduleToCalendar(25, "10:00\n발표 자료 완성");

                MessageBox.Show("API 오류가 발생해서 임시 시연 데이터를 출력했습니다.");
            }
            finally
            {
                btnAnalyze.Enabled = true;
                btnAnalyze.Text = "분석하기";
            }
        }

        private async Task<string> AnalyzeMeeting(string meetingText)
        {
            string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("OPENAI_API_KEY 환경변수가 없습니다.");

            string prompt = $@"
다음 회의 내용을 분석해줘.
반드시 JSON만 출력해. 코드블럭 금지.

형식:
{{
  ""summary"": ""회의 요약"",
  ""englishTranslation"": ""영어 번역"",
  ""japaneseTranslation"": ""일본어 번역"",
  ""schedules"": [
    {{
      ""day"": 20,
      ""time"": ""15:00"",
      ""title"": ""중간 점검 회의""
    }}
  ]
}}

조건:
- 일정 날짜는 1일부터 31일 사이의 숫자 day로 출력
- 일정이 없으면 schedules는 빈 배열

회의 내용:
{meetingText}
";

            var requestData = new
            {
                model = "gpt-4o-mini",
                messages = new object[]
                {
                    new { role = "system", content = "너는 회의 내용을 요약, 번역, 일정 추출하는 AI 도우미야." },
                    new { role = "user", content = prompt }
                }
            };

            string json = JsonSerializer.Serialize(requestData);

            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.openai.com/v1/chat/completions"
            );

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await http.SendAsync(request);
            string responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(responseText);

            using JsonDocument doc = JsonDocument.Parse(responseText);

            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }

        private void MakeCalendar()
        {
            calendarPanel.Controls.Clear();

            for (int day = 1; day <= 31; day++)
            {
                Label label = new Label()
                {
                    Text = day.ToString(),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.TopLeft,
                    Padding = new Padding(4),
                    AutoSize = false
                };

                int index = day - 1;
                int row = index / 7;
                int col = index % 7;

                calendarPanel.Controls.Add(label, col, row);
            }
        }

        private void AddScheduleToCalendar(int day, string scheduleText)
        {
            if (day < 1 || day > 31) return;

            int row = (day - 1) / 7;
            int col = (day - 1) % 7;

            Label label = calendarPanel.GetControlFromPosition(col, row) as Label;

            if (label != null)
            {
                label.Text += "\n" + scheduleText;
                label.BackColor = Color.LightYellow;
            }
        }
    }

    public class MeetingResult
    {
        public string summary { get; set; }
        public string englishTranslation { get; set; }
        public string japaneseTranslation { get; set; }
        public List<ScheduleItem> schedules { get; set; }
    }

    public class ScheduleItem
    {
        public int day { get; set; }
        public string time { get; set; }
        public string title { get; set; }
    }
}
