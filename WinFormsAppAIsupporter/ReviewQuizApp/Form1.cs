using ReviewQuizApp.Forms;
using ReviewQuizApp.Models;
using ReviewQuizApp.Services;

namespace ReviewQuizApp;

public partial class Form1 : Form
{
    private readonly AssignmentRepository _assignmentRepository = new();
    private readonly AssignmentQuizGenerator _generator = new();
    private readonly QuizHistoryService _historyService = new();
    private readonly Dictionary<string, string> _answers = [];

    private List<AssignmentInfo> _assignments = [];
    private List<QuizResultRecord> _historyRecords = [];
    private AssignmentInfo? _currentAssignment;
    private QuizDataFile _quiz = new();
    private int _currentIndex;

    private Label _titleLabel = null!;
    private Label _metaLabel = null!;
    private Label _statusLabel = null!;
    private Label _questionLabel = null!;
    private Label _feedbackLabel = null!;
    private Label _historySummaryLabel = null!;
    private Panel _settingsPage = null!;
    private Panel _quizPage = null!;
    private Panel _historyPage = null!;
    private ComboBox _assignmentBox = null!;
    private ComboBox _goalTemplateBox = null!;
    private TextBox _goalBox = null!;
    private CheckBox _trueFalseBox = null!;
    private CheckBox _multipleChoiceBox = null!;
    private CheckBox _shortAnswerTypeBox = null!;
    private NumericUpDown _questionCountInput = null!;
    private ComboBox _difficultyBox = null!;
    private FlowLayoutPanel _answerPanel = null!;
    private TextBox? _shortAnswerBox;
    private ProgressBar _progressBar = null!;
    private ListView _historyList = null!;
    private ListView _historyDetailList = null!;
    private Button _generateButton = null!;
    private Button _historyButton = null!;
    private Button _backButton = null!;
    private Button _previousButton = null!;
    private Button _nextButton = null!;
    private Button _checkButton = null!;
    private Button _submitButton = null!;
    private Button _historyBackButton = null!;
    private Button _retryQuizButton = null!;
    private Button _deleteHistoryButton = null!;
    private Button _deleteAllHistoryButton = null!;

    public Form1()
    {
        InitializeComponent();
        BuildLayout();
        LoadAssignments();
        ShowSettingsPage();
        ApplyStartupGuards();
    }

    private void BuildLayout()
    {
        Text = "과제 복습 퀴즈";
        MinimumSize = new Size(980, 720);
        Size = new Size(1120, 800);
        BackColor = Color.FromArgb(248, 250, 252);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(28),
            BackColor = Color.FromArgb(248, 250, 252)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(BuildHeader(), 0, 0);

        var content = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 250, 252) };
        _settingsPage = BuildSettingsPage();
        _quizPage = BuildQuizPage();
        _historyPage = BuildHistoryPage();
        content.Controls.Add(_settingsPage);
        content.Controls.Add(_quizPage);
        content.Controls.Add(_historyPage);

        root.Controls.Add(content, 0, 1);
        Controls.Add(root);
    }

    private Control BuildHeader()
    {
        var header = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, RowCount = 2, AutoSize = true };
        _titleLabel = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 22F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            Text = "과제 복습 퀴즈 설정"
        };
        _metaLabel = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 10F),
            ForeColor = Color.FromArgb(71, 85, 105),
            Padding = new Padding(0, 6, 0, 0),
            Text = "1단계: 과제와 퀴즈 조건을 설정하세요."
        };
        header.Controls.Add(_titleLabel, 0, 0);
        header.Controls.Add(_metaLabel, 0, 1);
        return header;
    }

    private Panel BuildSettingsPage()
    {
        var page = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 250, 252) };
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 5,
            AutoSize = true,
            BackColor = Color.White,
            Padding = new Padding(24),
            Margin = new Padding(0, 24, 0, 0)
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _assignmentBox = new ComboBox { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
        _assignmentBox.SelectedIndexChanged += (_, _) =>
        {
            _currentAssignment = _assignmentBox.SelectedItem as AssignmentInfo;
            UpdateSettingsStatus();
        };

        _goalTemplateBox = new ComboBox
        {
            Dock = DockStyle.Top,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 10F),
            Margin = new Padding(0, 0, 0, 6)
        };
        _goalTemplateBox.Items.AddRange(GetGoalTemplates().Select(template => template.Name).ToArray());
        _goalTemplateBox.SelectedIndexChanged += (_, _) => ApplySelectedGoalTemplate();

        _goalBox = new TextBox
        {
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 10F),
            PlaceholderText = "예: OX 2개, 객관식 2개, 주관식 1개로 과제 이해도 확인"
        };
        _goalTemplateBox.SelectedIndex = 0;

        var goalPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            RowCount = 2,
            AutoSize = true
        };
        goalPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        goalPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        goalPanel.Controls.Add(_goalTemplateBox, 0, 0);
        goalPanel.Controls.Add(_goalBox, 0, 1);

        _trueFalseBox = CreateCheckBox("O/X");
        _multipleChoiceBox = CreateCheckBox("객관식");
        _shortAnswerTypeBox = CreateCheckBox("주관식");
        _trueFalseBox.Checked = true;
        _multipleChoiceBox.Checked = true;
        _shortAnswerTypeBox.Checked = true;

        var typePanel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
        typePanel.Controls.Add(_trueFalseBox);
        typePanel.Controls.Add(_multipleChoiceBox);
        typePanel.Controls.Add(_shortAnswerTypeBox);

        _questionCountInput = new NumericUpDown { Minimum = 1, Maximum = 15, Value = 6, Width = 72 };
        _difficultyBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 110 };
        _difficultyBox.Items.AddRange(["쉬움", "보통", "어려움"]);
        _difficultyBox.SelectedIndex = 1;

        _generateButton = CreateButton("퀴즈 생성");
        _generateButton.Click += async (_, _) => await GenerateAssignmentQuizAsync();
        _historyButton = CreateButton("기록 조회");
        _historyButton.Click += (_, _) => ShowHistoryPage();

        var optionPanel = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
        optionPanel.Controls.Add(CreateSmallLabel("문항 수"));
        optionPanel.Controls.Add(_questionCountInput);
        optionPanel.Controls.Add(CreateSmallLabel("난이도"));
        optionPanel.Controls.Add(_difficultyBox);
        optionPanel.Controls.Add(_generateButton);
        optionPanel.Controls.Add(_historyButton);

        _statusLabel = new Label
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10F),
            ForeColor = Color.FromArgb(71, 85, 105),
            Padding = new Padding(0, 8, 0, 0)
        };

        panel.Controls.Add(CreateSmallLabel("과제"), 0, 0);
        panel.Controls.Add(_assignmentBox, 1, 0);
        panel.Controls.Add(CreateSmallLabel("생성 목적"), 0, 1);
        panel.Controls.Add(goalPanel, 1, 1);
        panel.Controls.Add(CreateSmallLabel("문제 유형"), 0, 2);
        panel.Controls.Add(typePanel, 1, 2);
        panel.Controls.Add(CreateSmallLabel("퀴즈 설정"), 0, 3);
        panel.Controls.Add(optionPanel, 1, 3);
        panel.Controls.Add(CreateSmallLabel("상태"), 0, 4);
        panel.Controls.Add(_statusLabel, 1, 4);
        page.Controls.Add(panel);
        return page;
    }

    private Panel BuildQuizPage()
    {
        var page = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 250, 252) };
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(0, 24, 0, 0),
            BackColor = Color.FromArgb(248, 250, 252)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _progressBar = new ProgressBar { Dock = DockStyle.Top, Height = 18, Margin = new Padding(0, 0, 0, 16) };

        var questionArea = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Color.White, Padding = new Padding(24) };
        questionArea.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        questionArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        questionArea.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _questionLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            Height = 96
        };
        _answerPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
        _feedbackLabel = new Label
        {
            AutoSize = false,
            Dock = DockStyle.Bottom,
            Font = new Font("Segoe UI", 11F),
            ForeColor = Color.FromArgb(51, 65, 85),
            Height = 112,
            Padding = new Padding(0, 12, 0, 0)
        };

        questionArea.Controls.Add(_questionLabel, 0, 0);
        questionArea.Controls.Add(_answerPanel, 0, 1);
        questionArea.Controls.Add(_feedbackLabel, 0, 2);

        var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Padding = new Padding(0, 18, 0, 0) };
        _submitButton = CreateButton("제출");
        _checkButton = CreateButton("정답 확인");
        _nextButton = CreateButton("다음");
        _previousButton = CreateButton("이전");
        _backButton = CreateButton("설정으로");
        _submitButton.Click += (_, _) => SubmitQuiz();
        _checkButton.Click += (_, _) => CheckCurrentAnswer();
        _nextButton.Click += (_, _) => MoveQuestion(1);
        _previousButton.Click += (_, _) => MoveQuestion(-1);
        _backButton.Click += (_, _) => ShowSettingsPage();
        footer.Controls.Add(_submitButton);
        footer.Controls.Add(_nextButton);
        footer.Controls.Add(_previousButton);
        footer.Controls.Add(_checkButton);
        footer.Controls.Add(_backButton);

        root.Controls.Add(_progressBar, 0, 0);
        root.Controls.Add(questionArea, 0, 1);
        root.Controls.Add(footer, 0, 2);
        page.Controls.Add(root);
        return page;
    }

    private Panel BuildHistoryPage()
    {
        var page = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 250, 252) };
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(0, 24, 0, 0), BackColor = Color.FromArgb(248, 250, 252) };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _historyList = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true };
        _historyList.Columns.Add("풀이일", 150);
        _historyList.Columns.Add("퀴즈", 300);
        _historyList.Columns.Add("과제", 180);
        _historyList.Columns.Add("점수", 80);
        _historyList.SelectedIndexChanged += (_, _) => RenderSelectedHistoryDetail();

        var detailPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.White, Padding = new Padding(12) };
        detailPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        detailPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _historySummaryLabel = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            Margin = new Padding(0, 0, 0, 10),
            Text = "기록을 선택하세요."
        };

        _historyDetailList = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true };
        _historyDetailList.Columns.Add("문항", 70);
        _historyDetailList.Columns.Add("결과", 70);
        _historyDetailList.Columns.Add("내 답", 180);
        _historyDetailList.Columns.Add("정답", 180);
        _historyDetailList.Columns.Add("질문", 360);
        detailPanel.Controls.Add(_historySummaryLabel, 0, 0);
        detailPanel.Controls.Add(_historyDetailList, 0, 1);

        var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Padding = new Padding(0, 18, 0, 0) };
        _retryQuizButton = CreateButton("다시 풀기");
        _deleteHistoryButton = CreateButton("선택 삭제");
        _deleteAllHistoryButton = CreateButton("전체 삭제");
        _historyBackButton = CreateButton("설정으로");
        _retryQuizButton.Enabled = false;
        _deleteHistoryButton.Enabled = false;
        _retryQuizButton.Click += (_, _) => RetrySelectedHistoryQuiz();
        _deleteHistoryButton.Click += (_, _) => DeleteSelectedHistory();
        _deleteAllHistoryButton.Click += (_, _) => DeleteAllHistory();
        _historyBackButton.Click += (_, _) => ShowSettingsPage();
        footer.Controls.Add(_retryQuizButton);
        footer.Controls.Add(_deleteHistoryButton);
        footer.Controls.Add(_deleteAllHistoryButton);
        footer.Controls.Add(_historyBackButton);

        root.Controls.Add(_historyList, 0, 0);
        root.Controls.Add(detailPanel, 0, 1);
        root.Controls.Add(footer, 0, 2);
        page.Controls.Add(root);
        return page;
    }

    private void ShowSettingsPage()
    {
        _settingsPage.Visible = true;
        _quizPage.Visible = false;
        _historyPage.Visible = false;
        _settingsPage.BringToFront();
        _titleLabel.Text = _generator.IsConfigured ? "과제 복습 퀴즈 설정" : "AI API 설정이 필요합니다";
        _metaLabel.Text = _generator.IsConfigured
            ? "1단계: 과제와 퀴즈 조건을 설정하세요."
            : "GROQ_API_KEY, XAI_API_KEY, GEMINI_API_KEY 또는 OPENAI_API_KEY 환경 변수를 설정한 뒤 프로그램을 다시 실행하세요.";
        UpdateSettingsStatus();
    }

    private void ShowQuizPage()
    {
        _settingsPage.Visible = false;
        _quizPage.Visible = true;
        _historyPage.Visible = false;
        _quizPage.BringToFront();
        _titleLabel.Text = _quiz.QuizTitle;
        _metaLabel.Text = $"2단계: 퀴즈 풀이 | 생성일 {_quiz.GeneratedAt:yyyy/MM/dd HH:mm:ss} | 문항 수 {_quiz.Questions.Count}";
    }

    private void ShowHistoryPage()
    {
        _settingsPage.Visible = false;
        _quizPage.Visible = false;
        _historyPage.Visible = true;
        _historyPage.BringToFront();
        _titleLabel.Text = "퀴즈 기록 조회";
        _metaLabel.Text = "이전에 푼 퀴즈의 정답/오답 기록을 보고 다시 풀 수 있습니다.";
        LoadHistoryRecords();
    }

    private void LoadAssignments()
    {
        _assignments = _assignmentRepository.LoadAssignments();
        _assignmentBox.Items.Clear();
        foreach (var assignment in _assignments)
        {
            _assignmentBox.Items.Add(assignment);
        }
        if (_assignmentBox.Items.Count > 0)
        {
            _assignmentBox.SelectedIndex = 0;
        }
    }

    private void ApplyStartupGuards()
    {
        if (!_generator.IsConfigured)
        {
            _statusLabel.Text = "API 키를 찾을 수 없어 퀴즈 모듈 기능이 모두 중지되었습니다.";
            _statusLabel.ForeColor = Color.FromArgb(185, 28, 28);
            SetAllFeatureControlsEnabled(false);
            return;
        }
        if (_assignments.Count == 0)
        {
            _titleLabel.Text = "과제 JSON 데이터가 필요합니다";
            _metaLabel.Text = "Data/assignments 폴더에 과제 JSON 파일을 추가한 뒤 프로그램을 다시 실행하세요.";
            _statusLabel.Text = "과제 데이터가 없어 퀴즈 생성 기능이 중지되었습니다.";
            _statusLabel.ForeColor = Color.FromArgb(185, 28, 28);
            SetAllFeatureControlsEnabled(false);
        }
    }

    private async Task GenerateAssignmentQuizAsync()
    {
        if (!_generator.IsConfigured)
        {
            ShowApiMissingWarning();
            return;
        }
        if (_assignments.Count == 0)
        {
            ShowAssignmentMissingWarning();
            return;
        }

        try
        {
            SetGenerationState(true);
            var request = new QuizGenerationRequest
            {
                Assignment = _currentAssignment ?? throw new InvalidOperationException("과제를 먼저 선택하세요."),
                Goal = _goalBox.Text.Trim(),
                QuestionCount = (int)_questionCountInput.Value,
                Difficulty = _difficultyBox.SelectedItem?.ToString() ?? "보통",
                QuestionTypes = GetSelectedQuestionTypes()
            };
            if (request.QuestionTypes.Count == 0)
            {
                throw new InvalidOperationException("문제 유형을 하나 이상 선택하세요.");
            }

            _quiz = await _generator.GenerateAsync(request);
            _answers.Clear();
            _currentIndex = 0;
            _currentAssignment = request.Assignment;

            SaveGeneratedQuiz(_quiz);
            RenderQuestion();
            ShowQuizPage();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"과제 퀴즈를 생성하지 못했습니다.\n\n{ex.Message}", "생성 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetGenerationState(false);
        }
    }

    private List<QuestionType> GetSelectedQuestionTypes()
    {
        var types = new List<QuestionType>();
        if (_trueFalseBox.Checked) types.Add(QuestionType.TrueFalse);
        if (_multipleChoiceBox.Checked) types.Add(QuestionType.MultipleChoice);
        if (_shortAnswerTypeBox.Checked) types.Add(QuestionType.ShortAnswer);
        return types;
    }

    private void ApplySelectedGoalTemplate()
    {
        var selectedIndex = _goalTemplateBox.SelectedIndex;
        var templates = GetGoalTemplates();

        if (selectedIndex < 0 || selectedIndex >= templates.Count)
        {
            return;
        }

        _goalBox.Text = templates[selectedIndex].Text;
    }

    private static List<(string Name, string Text)> GetGoalTemplates()
    {
        return
        [
            (
                "보고서/레포트 핵심 이해",
                "보고서 과제의 주제, 핵심 개념, 근거, 결론을 제대로 이해했는지 확인하는 복습 퀴즈를 생성"
            ),
            (
                "발표/프레젠테이션 점검",
                "발표 자료의 핵심 메시지, 주요 사례, 발표 순서, 예상 질문을 점검할 수 있는 퀴즈를 생성"
            ),
            (
                "코딩/실습 과제 검토",
                "구현 요구사항, 주요 로직, 예외 처리, 테스트 관점을 확인하는 실습형 복습 퀴즈를 생성"
            ),
            (
                "중간/기말 시험 대비",
                "시험 대비용으로 개념 정의, 비교, 적용 문제를 섞어 학습 성취도를 확인하는 퀴즈를 생성"
            ),
            (
                "토론/논술 과제 준비",
                "쟁점, 찬반 근거, 반론, 자신의 주장 정리를 점검할 수 있는 토론/논술형 퀴즈를 생성"
            )
        ];
    }

    private void UpdateSettingsStatus()
    {
        if (_statusLabel == null) return;
        if (!_generator.IsConfigured)
        {
            _statusLabel.ForeColor = Color.FromArgb(185, 28, 28);
            _statusLabel.Text = "AI API 키가 없어 모든 기능이 잠겨 있습니다.";
            return;
        }
        if (_assignments.Count == 0)
        {
            _statusLabel.ForeColor = Color.FromArgb(185, 28, 28);
            _statusLabel.Text = "과제 JSON 데이터가 없어 퀴즈 생성 기능이 잠겨 있습니다.";
            return;
        }
        _statusLabel.ForeColor = Color.FromArgb(71, 85, 105);
        _statusLabel.Text = _currentAssignment == null
            ? "Data/assignments 폴더에서 과제 JSON 파일을 찾지 못했습니다."
            : $"과제 파일을 내부 객체로 읽어 {_generator.Provider} API에 퀴즈 생성 요청을 전달합니다.";
    }

    private void SetGenerationState(bool isGenerating)
    {
        if (!_generator.IsConfigured || _assignments.Count == 0)
        {
            SetAllFeatureControlsEnabled(false);
            return;
        }
        _generateButton.Enabled = !isGenerating;
        _historyButton.Enabled = !isGenerating;
        _assignmentBox.Enabled = !isGenerating;
        _goalTemplateBox.Enabled = !isGenerating;
        _goalBox.Enabled = !isGenerating;
        _trueFalseBox.Enabled = !isGenerating;
        _multipleChoiceBox.Enabled = !isGenerating;
        _shortAnswerTypeBox.Enabled = !isGenerating;
        _questionCountInput.Enabled = !isGenerating;
        _difficultyBox.Enabled = !isGenerating;
        if (isGenerating)
        {
            _statusLabel.Text = "과제 정보를 바탕으로 AI 퀴즈를 생성하는 중입니다...";
        }
    }

    private void SetAllFeatureControlsEnabled(bool enabled)
    {
        _assignmentBox.Enabled = enabled;
        _goalTemplateBox.Enabled = enabled;
        _goalBox.Enabled = enabled;
        _trueFalseBox.Enabled = enabled;
        _multipleChoiceBox.Enabled = enabled;
        _shortAnswerTypeBox.Enabled = enabled;
        _questionCountInput.Enabled = enabled;
        _difficultyBox.Enabled = enabled;
        _generateButton.Enabled = enabled;
        _historyButton.Enabled = enabled;
        _backButton.Enabled = enabled;
        _previousButton.Enabled = enabled;
        _nextButton.Enabled = enabled;
        _checkButton.Enabled = enabled;
        _submitButton.Enabled = enabled;
        _historyBackButton.Enabled = enabled;
        _retryQuizButton.Enabled = enabled;
        _deleteHistoryButton.Enabled = enabled;
        _deleteAllHistoryButton.Enabled = enabled;
        if (_shortAnswerBox != null) _shortAnswerBox.Enabled = enabled;
    }

    private static void SaveGeneratedQuiz(QuizDataFile quiz)
    {
        var dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
        Directory.CreateDirectory(dataDirectory);
        File.WriteAllText(Path.Combine(dataDirectory, "generated-quiz-latest.json"), System.Text.Json.JsonSerializer.Serialize(quiz, QuizJson.Options));
    }

    private void LoadHistoryRecords()
    {
        _historyRecords = _historyService.Load().OrderByDescending(record => record.SolvedAt).ToList();
        _historyList.Items.Clear();
        _historyDetailList.Items.Clear();
        _historySummaryLabel.Text = _historyRecords.Count == 0 ? "저장된 퀴즈 기록이 없습니다." : "기록을 선택하세요.";
        _retryQuizButton.Enabled = false;
        _deleteHistoryButton.Enabled = false;
        _deleteAllHistoryButton.Enabled = _historyRecords.Count > 0;

        foreach (var record in _historyRecords)
        {
            var item = new ListViewItem(record.SolvedAt.ToString("yyyy/MM/dd HH:mm"));
            item.SubItems.Add(record.QuizTitle);
            item.SubItems.Add(string.IsNullOrWhiteSpace(record.AssignmentTitle) ? "-" : record.AssignmentTitle);
            item.SubItems.Add($"{record.Score}점");
            item.Tag = record;
            _historyList.Items.Add(item);
        }
    }

    private void RenderSelectedHistoryDetail()
    {
        _historyDetailList.Items.Clear();
        var record = SelectedHistoryRecord;
        if (record == null)
        {
            _historySummaryLabel.Text = "기록을 선택하세요.";
            _retryQuizButton.Enabled = false;
            _deleteHistoryButton.Enabled = false;
            return;
        }

        _historySummaryLabel.Text = $"{record.QuizTitle} | {record.CorrectCount}/{record.TotalCount} | {record.Score}점";
        _retryQuizButton.Enabled = File.Exists(record.QuizFilePath);
        _deleteHistoryButton.Enabled = true;

        for (var i = 0; i < record.Answers.Count; i++)
        {
            var answer = record.Answers[i];
            var item = new ListViewItem($"{i + 1}번");
            item.SubItems.Add(answer.IsCorrect ? "정답" : "오답");
            item.SubItems.Add(string.IsNullOrWhiteSpace(answer.UserAnswer) ? "(미응답)" : answer.UserAnswer);
            item.SubItems.Add(answer.CorrectAnswer);
            item.SubItems.Add(answer.QuestionTitle);
            item.BackColor = answer.IsCorrect ? Color.FromArgb(236, 253, 245) : Color.FromArgb(254, 242, 242);
            _historyDetailList.Items.Add(item);
        }
    }

    private void DeleteSelectedHistory()
    {
        var record = SelectedHistoryRecord;
        if (record == null) return;

        var confirm = MessageBox.Show("선택한 퀴즈 기록을 삭제할까요?", "기록 삭제", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        _historyService.Delete(record.ResultId);
        LoadHistoryRecords();
    }

    private void DeleteAllHistory()
    {
        var confirm = MessageBox.Show("모든 퀴즈 기록을 삭제할까요? 이 작업은 되돌릴 수 없습니다.", "전체 기록 삭제", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        _historyService.DeleteAll();
        LoadHistoryRecords();
    }

    private void RetrySelectedHistoryQuiz()
    {
        var record = SelectedHistoryRecord;
        if (record == null) return;

        try
        {
            _quiz = _historyService.LoadQuizSnapshot(record.QuizFilePath);
            _answers.Clear();
            _currentIndex = 0;
            RenderQuestion();
            ShowQuizPage();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"저장된 퀴즈를 불러오지 못했습니다.\n\n{ex.Message}", "퀴즈 불러오기 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RenderQuestion()
    {
        if (_quiz.Questions.Count == 0) return;
        var question = CurrentQuestion;
        _questionLabel.Text = $"{_currentIndex + 1}. {question.Title}";
        _feedbackLabel.Text = string.Empty;
        _answerPanel.Controls.Clear();

        if (question.Type == QuestionType.ShortAnswer) RenderShortAnswer(question);
        else RenderChoiceQuestion(question);

        _progressBar.Minimum = 0;
        _progressBar.Maximum = _quiz.Questions.Count;
        _progressBar.Value = Math.Min(_currentIndex + 1, _quiz.Questions.Count);
        _previousButton.Enabled = _currentIndex > 0;
        _nextButton.Enabled = _currentIndex < _quiz.Questions.Count - 1;
        _checkButton.Enabled = true;
        _submitButton.Enabled = true;
    }

    private void RenderChoiceQuestion(QuizQuestion question)
    {
        foreach (var option in question.Options)
        {
            var radio = new RadioButton
            {
                AutoSize = false,
                Width = Math.Max(360, _answerPanel.ClientSize.Width - 24),
                Height = 42,
                Font = new Font("Segoe UI", 12F),
                Text = option,
                Checked = _answers.TryGetValue(question.Id, out var answer) && QuizDataLoader.SameAnswer(answer, option)
            };
            radio.CheckedChanged += (_, _) =>
            {
                if (radio.Checked) _answers[question.Id] = option;
            };
            _answerPanel.Controls.Add(radio);
        }
    }

    private void RenderShortAnswer(QuizQuestion question)
    {
        _shortAnswerBox = new TextBox
        {
            Width = Math.Max(360, _answerPanel.ClientSize.Width - 24),
            Font = new Font("Segoe UI", 14F),
            Height = 42,
            PlaceholderText = "주관식 답안을 입력하세요.",
            Text = _answers.TryGetValue(question.Id, out var answer) ? answer : string.Empty
        };
        _shortAnswerBox.TextChanged += (_, _) => _answers[question.Id] = _shortAnswerBox.Text;
        _answerPanel.Controls.Add(_shortAnswerBox);
        _shortAnswerBox.Focus();
    }

    private void MoveQuestion(int offset)
    {
        SaveCurrentAnswer();
        _currentIndex = Math.Clamp(_currentIndex + offset, 0, _quiz.Questions.Count - 1);
        RenderQuestion();
    }

    private void SaveCurrentAnswer()
    {
        if (_quiz.Questions.Count == 0) return;
        var question = CurrentQuestion;
        if (question.Type == QuestionType.ShortAnswer && _shortAnswerBox != null)
        {
            _answers[question.Id] = _shortAnswerBox.Text;
        }
    }

    private void CheckCurrentAnswer()
    {
        SaveCurrentAnswer();
        var question = CurrentQuestion;
        _answers.TryGetValue(question.Id, out var answer);
        var isCorrect = QuizDataLoader.SameAnswer(answer ?? string.Empty, question.CorrectAnswer);
        _feedbackLabel.ForeColor = isCorrect ? Color.FromArgb(22, 101, 52) : Color.FromArgb(153, 27, 27);
        _feedbackLabel.Text =
            $"결과: {(isCorrect ? "정답" : "오답")}\n" +
            $"정답: {question.CorrectAnswer}\n" +
            $"해설: {question.Explanation}";
    }

    private void SubmitQuiz()
    {
        SaveCurrentAnswer();
        var unansweredCount = _quiz.Questions.Count(question => !_answers.TryGetValue(question.Id, out var answer) || string.IsNullOrWhiteSpace(answer));
        if (unansweredCount > 0)
        {
            var confirm = MessageBox.Show($"{unansweredCount}개 문항이 아직 비어 있습니다. 그래도 제출할까요?", "미응답 문항 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;
        }

        var correctCount = CountCorrectAnswers();
        SaveResultHistory(correctCount);
        using var resultForm = new QuizResultForm(_quiz.Questions, _answers);
        resultForm.ShowDialog(this);
        ShowSettingsPage();
    }

    private int CountCorrectAnswers()
    {
        return _quiz.Questions.Count(question => _answers.TryGetValue(question.Id, out var answer) && QuizDataLoader.SameAnswer(answer, question.CorrectAnswer));
    }

    private void SaveResultHistory(int correctCount)
    {
        try
        {
            var quizFilePath = _historyService.SaveQuizSnapshot(_quiz);
            _historyService.Append(new QuizResultRecord
            {
                QuizId = _quiz.QuizId,
                QuizTitle = _quiz.QuizTitle,
                QuizFilePath = quizFilePath,
                AssignmentId = _currentAssignment?.Id ?? string.Empty,
                AssignmentTitle = _currentAssignment?.Title ?? string.Empty,
                SolvedAt = DateTime.Now,
                CorrectCount = correctCount,
                TotalCount = _quiz.Questions.Count,
                Answers = BuildAnswerRecords()
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"퀴즈 결과는 계산됐지만 기록 저장에 실패했습니다.\n\n{ex.Message}", "기록 저장 경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private List<QuizAnswerRecord> BuildAnswerRecords()
    {
        return _quiz.Questions.Select(question =>
        {
            _answers.TryGetValue(question.Id, out var userAnswer);
            return new QuizAnswerRecord
            {
                QuestionId = question.Id,
                QuestionTitle = question.Title,
                UserAnswer = userAnswer ?? string.Empty,
                CorrectAnswer = question.CorrectAnswer,
                IsCorrect = QuizDataLoader.SameAnswer(userAnswer ?? string.Empty, question.CorrectAnswer)
            };
        }).ToList();
    }

    private static Label CreateSmallLabel(string text)
    {
        return new Label { AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(51, 65, 85), Margin = new Padding(0, 6, 12, 6), Text = text };
    }

    private static CheckBox CreateCheckBox(string text)
    {
        return new CheckBox { AutoSize = true, Font = new Font("Segoe UI", 10F), Margin = new Padding(0, 4, 16, 4), Text = text };
    }

    private static Button CreateButton(string text)
    {
        return new Button { AutoSize = true, MinimumSize = new Size(110, 38), Margin = new Padding(8, 0, 0, 0), Padding = new Padding(12, 6, 12, 6), Font = new Font("Segoe UI", 10F, FontStyle.Bold), Text = text, UseVisualStyleBackColor = true };
    }

    private static void ShowApiMissingWarning()
    {
        MessageBox.Show("AI API 키를 찾을 수 없습니다.\n\nGROQ_API_KEY, XAI_API_KEY, GEMINI_API_KEY 또는 OPENAI_API_KEY 환경 변수를 설정한 뒤 프로그램을 다시 실행하세요.", "AI API 키 필요", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private static void ShowAssignmentMissingWarning()
    {
        MessageBox.Show("과제 JSON 데이터를 찾을 수 없습니다.\n\nData/assignments 폴더에 과제 JSON 파일을 추가한 뒤 프로그램을 다시 실행하세요.", "과제 데이터 필요", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private QuizQuestion CurrentQuestion => _quiz.Questions[_currentIndex];

    private QuizResultRecord? SelectedHistoryRecord =>
        _historyList.SelectedItems.Count == 0 ? null : _historyList.SelectedItems[0].Tag as QuizResultRecord;
}
