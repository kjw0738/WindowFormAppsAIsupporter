using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MeetingMinutesApp.Services;

namespace MeetingMinutesApp;

public partial class Form1 : Form
{
    private static readonly Regex ProgressPattern = new(@"\((\d+)\s*/\s*(\d+)\)", RegexOptions.Compiled);
    private static readonly IReadOnlyDictionary<string, SttCategoryProfile> FallbackSttCategoryProfiles = new Dictionary<string, SttCategoryProfile>
    {
        ["일반 회의"] = new(
            ["일반 안건"],
            ["회의", "안건", "의견", "결정", "담당자", "일정", "확인 필요"],
            new Dictionary<string, string[]>
            {
                ["일반 안건"] = ["회의", "논의", "결정", "업무", "일정"]
            })
    };
    private static readonly IReadOnlyDictionary<string, SttCategoryProfile> SttCategoryProfiles = LoadSttCategoryProfiles();

    private AiConfiguration _configuration;
    private MeetingAiService _aiService;
    private readonly MeetingStorageService _storageService = new();

    private readonly TextBox _audioPathTextBox = new();
    private readonly ComboBox _sttCategoryComboBox = new();
    private readonly ComboBox _sttTopicComboBox = new();
    private readonly TextBox _sttTermsTextBox = new();
    private readonly Button _browseButton = new();
    private readonly Button _transcribeButton = new();
    private readonly Button _summarizeButton = new();
    private readonly Button _saveButton = new();
    private readonly Button _cancelButton = new();
    private readonly Button _clearCacheButton = new();
    private readonly Button _correctTranscriptButton = new();
    private readonly Button _refreshHistoryButton = new();
    private readonly Button _loadHistoryButton = new();
    private readonly Button _openMarkdownButton = new();
    private readonly Button _deleteHistoryButton = new();
    private readonly Button _deleteAllHistoryButton = new();
    private readonly Button _clearHistoryFilterButton = new();
    private readonly Label _statusLabel = new();
    private readonly Label _progressLabel = new();
    private readonly ProgressBar _progressBar = new();
    private readonly TextBox _transcriptTextBox = new();
    private readonly TextBox _summaryTextBox = new();
    private readonly TextBox _historySearchTextBox = new();
    private readonly TextBox _settingsApiKeyTextBox = new();
    private readonly TextBox _settingsSttProviderTextBox = new();
    private readonly TextBox _settingsOpenAiApiKeyTextBox = new();
    private readonly TextBox _settingsSttModelTextBox = new();
    private readonly TextBox _settingsSttChunkMinutesTextBox = new();
    private readonly TextBox _settingsAudioPreprocessingTextBox = new();
    private readonly TextBox _settingsSttPromptEnabledTextBox = new();
    private readonly TextBox _settingsChatModelTextBox = new();
    private readonly TextBox _settingsChatDelayTextBox = new();
    private readonly TextBox _settingsMaxRetryTextBox = new();
    private readonly TextBox _settingsHttpTimeoutTextBox = new();
    private readonly TextBox _settingsSummaryChunkSizeTextBox = new();
    private readonly TextBox _settingsPartialMaxTokensTextBox = new();
    private readonly TextBox _settingsFinalMaxTokensTextBox = new();
    private readonly TextBox _settingsLocalWhisperModelTextBox = new();
    private readonly Button _applySettingsButton = new();
    private readonly Button _reloadSettingsButton = new();
    private readonly DateTimePicker _historyFromDatePicker = new();
    private readonly DateTimePicker _historyToDatePicker = new();
    private readonly TabControl _tabs = new();
    private readonly DataGridView _historyGrid = new();
    private readonly System.Windows.Forms.Timer _historyFilterTimer = new() { Interval = 250 };

    private readonly BindingSource _historyBinding = new();
    private IReadOnlyList<MeetingRecordListItem> _historyRecords = [];
    private IReadOnlyList<MeetingHistoryRow> _historyRows = [];
    private readonly Dictionary<string, string> _historyTranscriptSearchCache = [];
    private CancellationTokenSource? _workCancellation;
    private bool _hasUnsavedWork;
    private bool _suppressChangeTracking;

    public Form1()
    {
        InitializeComponent();
        _configuration = AiConfiguration.Load();
        _aiService = new MeetingAiService(CreateHttpClient(), _configuration);
        _historyFilterTimer.Tick += (_, _) =>
        {
            _historyFilterTimer.Stop();
            ApplyHistoryFilter();
        };

        BuildUi();
        FormClosing += Form1_FormClosing;
        ApplyStartupGuard();
        RefreshHistory();
    }

    private void BuildUi()
    {
        Font = new Font("맑은 고딕", 10F);
        BackColor = Color.FromArgb(246, 248, 251);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            ColumnCount = 1,
            RowCount = 5
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(root);

        root.Controls.Add(new Label
        {
            AutoSize = true,
            Font = new Font("맑은 고딕", 20F, FontStyle.Bold),
            Text = "회의록 자동 정리",
            ForeColor = Color.FromArgb(16, 34, 58),
            Margin = new Padding(0, 0, 0, 6)
        }, 0, 0);

        var inputPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 4,
            RowCount = 3,
            Padding = new Padding(0, 12, 0, 12)
        };
        inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        root.Controls.Add(inputPanel, 0, 1);

        inputPanel.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "녹음 파일",
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 7, 12, 0)
        }, 0, 0);

        _audioPathTextBox.Dock = DockStyle.Top;
        _audioPathTextBox.ReadOnly = true;
        inputPanel.Controls.Add(_audioPathTextBox, 1, 0);

        ConfigureButton(_browseButton, "파일 선택");
        _browseButton.Click += BrowseButton_Click;
        inputPanel.Controls.Add(_browseButton, 2, 0);

        ConfigureButton(_transcribeButton, "STT 변환");
        _transcribeButton.Click += TranscribeButton_Click;
        inputPanel.Controls.Add(_transcribeButton, 3, 0);

        inputPanel.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "회의 카테고리",
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 7, 12, 0)
        }, 0, 1);

        _sttCategoryComboBox.Dock = DockStyle.Top;
        _sttCategoryComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _sttCategoryComboBox.Items.AddRange(SttCategoryProfiles.Keys.ToArray());
        _sttCategoryComboBox.SelectedIndexChanged += (_, _) => UpdateSttTopicOptions(resetTopic: true);
        inputPanel.Controls.Add(_sttCategoryComboBox, 1, 1);

        inputPanel.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "세부 주제",
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(18, 7, 12, 0)
        }, 2, 1);

        _sttTopicComboBox.Dock = DockStyle.Top;
        _sttTopicComboBox.MinimumSize = new Size(240, 0);
        _sttTopicComboBox.DropDownStyle = ComboBoxStyle.DropDown;
        _sttTopicComboBox.SelectedIndexChanged += (_, _) => UpdateSttTermsPreview();
        _sttTopicComboBox.TextChanged += (_, _) => UpdateSttTermsPreview();
        inputPanel.Controls.Add(_sttTopicComboBox, 3, 1);

        inputPanel.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "자동 용어",
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 7, 12, 0)
        }, 0, 2);

        _sttTermsTextBox.Dock = DockStyle.Top;
        _sttTermsTextBox.ReadOnly = true;
        _sttTermsTextBox.BackColor = Color.White;
        inputPanel.SetColumnSpan(_sttTermsTextBox, 3);
        inputPanel.Controls.Add(_sttTermsTextBox, 1, 2);
        _sttCategoryComboBox.SelectedItem = "기술/IT";
        UpdateSttTopicOptions(resetTopic: true);

        var progressPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(0, 0, 0, 14)
        };
        progressPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        progressPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        root.Controls.Add(progressPanel, 0, 2);

        _progressBar.Dock = DockStyle.Top;
        _progressBar.Height = 16;
        _progressBar.Minimum = 0;
        _progressBar.Maximum = 100;
        _progressBar.Value = 0;
        progressPanel.Controls.Add(_progressBar, 0, 0);

        _progressLabel.AutoSize = true;
        _progressLabel.Text = "대기";
        _progressLabel.ForeColor = Color.FromArgb(70, 82, 99);
        _progressLabel.Margin = new Padding(12, 0, 0, 0);
        progressPanel.Controls.Add(_progressLabel, 1, 0);

        _tabs.Dock = DockStyle.Fill;
        _tabs.Margin = new Padding(0, 0, 0, 16);
        root.Controls.Add(_tabs, 0, 3);

        _transcriptTextBox.Multiline = true;
        _transcriptTextBox.ScrollBars = ScrollBars.Both;
        _transcriptTextBox.AcceptsReturn = true;
        _transcriptTextBox.AcceptsTab = true;
        _transcriptTextBox.Dock = DockStyle.Fill;
        _transcriptTextBox.Font = new Font("Consolas", 10F);
        _transcriptTextBox.TextChanged += MarkUnsavedWork;
        _tabs.TabPages.Add(new TabPage("STT 원문 확인") { Controls = { _transcriptTextBox } });

        _summaryTextBox.Multiline = true;
        _summaryTextBox.ScrollBars = ScrollBars.Both;
        _summaryTextBox.AcceptsReturn = true;
        _summaryTextBox.AcceptsTab = true;
        _summaryTextBox.Dock = DockStyle.Fill;
        _summaryTextBox.Font = new Font("Consolas", 10F);
        _summaryTextBox.TextChanged += MarkUnsavedWork;
        _tabs.TabPages.Add(new TabPage("AI 회의록") { Controls = { _summaryTextBox } });

        _tabs.TabPages.Add(CreateHistoryTab());
        _tabs.TabPages.Add(CreateSettingsTab());

        var bottomPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            ColumnCount = 6
        };
        bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        root.Controls.Add(bottomPanel, 0, 4);

        _statusLabel.AutoSize = true;
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        _statusLabel.ForeColor = Color.FromArgb(39, 71, 110);
        _statusLabel.Margin = new Padding(0, 8, 12, 0);
        bottomPanel.Controls.Add(_statusLabel, 0, 0);

        ConfigureButton(_cancelButton, "취소");
        _cancelButton.Click += CancelButton_Click;
        _cancelButton.Enabled = false;
        bottomPanel.Controls.Add(_cancelButton, 1, 0);

        ConfigureButton(_clearCacheButton, "캐시 삭제");
        _clearCacheButton.Click += ClearCacheButton_Click;
        bottomPanel.Controls.Add(_clearCacheButton, 2, 0);

        ConfigureButton(_correctTranscriptButton, "원문 보정");
        _correctTranscriptButton.Click += CorrectTranscriptButton_Click;
        bottomPanel.Controls.Add(_correctTranscriptButton, 3, 0);

        ConfigureButton(_summarizeButton, "회의록 정리");
        _summarizeButton.Click += SummarizeButton_Click;
        bottomPanel.Controls.Add(_summarizeButton, 4, 0);

        ConfigureButton(_saveButton, "저장");
        _saveButton.Click += SaveButton_Click;
        bottomPanel.Controls.Add(_saveButton, 5, 0);
    }

    private TabPage CreateHistoryTab()
    {
        var tab = new TabPage("회의록 기록");
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(8)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tab.Controls.Add(root);

        var filterPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = true,
            Margin = new Padding(0, 0, 0, 8)
        };
        root.Controls.Add(filterPanel, 0, 0);

        filterPanel.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "검색",
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 6, 8, 0)
        });

        _historySearchTextBox.Width = 260;
        _historySearchTextBox.PlaceholderText = "파일명, 회의록, STT 원문";
        _historySearchTextBox.Margin = new Padding(0, 0, 12, 0);
        _historySearchTextBox.TextChanged += (_, _) => ScheduleHistoryFilter();
        filterPanel.Controls.Add(_historySearchTextBox);

        filterPanel.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "시작일",
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 6, 8, 0)
        });

        ConfigureHistoryDatePicker(_historyFromDatePicker);
        _historyFromDatePicker.Margin = new Padding(0, 0, 12, 0);
        _historyFromDatePicker.ValueChanged += (_, _) => ApplyHistoryFilter();
        filterPanel.Controls.Add(_historyFromDatePicker);

        filterPanel.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "종료일",
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 6, 8, 0)
        });

        ConfigureHistoryDatePicker(_historyToDatePicker);
        _historyToDatePicker.Margin = new Padding(0, 0, 12, 0);
        _historyToDatePicker.ValueChanged += (_, _) => ApplyHistoryFilter();
        filterPanel.Controls.Add(_historyToDatePicker);

        ConfigureButton(_clearHistoryFilterButton, "필터 초기화");
        _clearHistoryFilterButton.Click += (_, _) => ClearHistoryFilter();
        filterPanel.Controls.Add(_clearHistoryFilterButton);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 8)
        };
        root.Controls.Add(buttonPanel, 0, 1);

        ConfigureButton(_refreshHistoryButton, "새로고침");
        _refreshHistoryButton.Click += (_, _) => RefreshHistory();
        buttonPanel.Controls.Add(_refreshHistoryButton);

        ConfigureButton(_loadHistoryButton, "불러오기");
        _loadHistoryButton.Click += (_, _) => LoadSelectedHistory();
        buttonPanel.Controls.Add(_loadHistoryButton);

        ConfigureButton(_openMarkdownButton, "파일 열기");
        _openMarkdownButton.Click += (_, _) => OpenSelectedMarkdown();
        buttonPanel.Controls.Add(_openMarkdownButton);

        ConfigureButton(_deleteHistoryButton, "선택 삭제");
        _deleteHistoryButton.Click += (_, _) => DeleteSelectedHistory();
        buttonPanel.Controls.Add(_deleteHistoryButton);

        ConfigureButton(_deleteAllHistoryButton, "전체 삭제");
        _deleteAllHistoryButton.Click += (_, _) => DeleteAllHistory();
        buttonPanel.Controls.Add(_deleteAllHistoryButton);

        _historyGrid.Dock = DockStyle.Fill;
        _historyGrid.ReadOnly = true;
        _historyGrid.AllowUserToAddRows = false;
        _historyGrid.AllowUserToDeleteRows = false;
        _historyGrid.AutoGenerateColumns = false;
        _historyGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _historyGrid.MultiSelect = false;
        _historyGrid.RowHeadersVisible = false;
        _historyGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _historyGrid.DataSource = _historyBinding;
        _historyGrid.CellDoubleClick += (_, _) => LoadSelectedHistory();
        root.Controls.Add(_historyGrid, 0, 2);

        _historyGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "생성 일시",
            DataPropertyName = nameof(MeetingHistoryRow.CreatedAtText),
            FillWeight = 24
        });
        _historyGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "녹음 파일",
            DataPropertyName = nameof(MeetingHistoryRow.AudioFileName),
            FillWeight = 32
        });
        _historyGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "요약 미리보기",
            DataPropertyName = nameof(MeetingHistoryRow.SummaryPreview),
            FillWeight = 44
        });

        return tab;
    }

    private TabPage CreateSettingsTab()
    {
        var tab = new TabPage("설정")
        {
            AutoScroll = true
        };
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(16)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tab.Controls.Add(root);

        root.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "현재 설정 상태",
            Font = new Font("맑은 고딕", 13F, FontStyle.Bold),
            ForeColor = Color.FromArgb(16, 34, 58),
            Margin = new Padding(0, 0, 0, 12)
        }, 0, 0);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 3
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        root.Controls.Add(layout, 0, 1);

        AddSettingRow(layout, "API 키", _configuration.IsReady ? "등록됨" : "등록되지 않음", _configuration.ApiKeyStatus);
        AddSettingRow(layout, "STT 공급자", _configuration.SttProvider, _configuration.SttProviderSource);
        AddSettingRow(layout, "STT API 키", _configuration.IsSttReady ? "등록됨" : "등록되지 않음", _configuration.SttApiKeyStatus);
        AddSettingRow(layout, "STT 모델", _configuration.TranscriptionModel, _configuration.TranscriptionModelSource);
        AddSettingRow(layout, "AI 회의록 모델", _configuration.ChatModel, _configuration.ChatModelSource);
        AddSettingRow(layout, "AI 요청 대기 시간", $"{_configuration.ChatDelaySeconds}초", _configuration.ChatDelaySource);
        AddSettingRow(layout, "최대 자동 재시도 대기", $"{_configuration.MaxAutoRetrySeconds}초", _configuration.MaxAutoRetrySource);
        AddSettingRow(layout, "로컬 Whisper 모델", _configuration.LocalWhisperModel, _configuration.LocalWhisperModelSource);
        AddSettingRow(layout, "ffmpeg", "자동 탐색/자동 다운로드", "환경 변수 또는 AppData/Tools");

        var editHeaderRow = layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var editHeader = new Label
        {
            AutoSize = true,
            Text = "설정 변경",
            Font = new Font("맑은 고딕", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(16, 34, 58),
            Margin = new Padding(0, 16, 0, 8)
        };
        layout.SetColumnSpan(editHeader, 3);
        layout.Controls.Add(editHeader, 0, editHeaderRow);

        AddEditableSettingRow(layout, "GROQ_API_KEY", _settingsApiKeyTextBox, _configuration.ApiKey ?? "", usePassword: true);
        AddEditableSettingRow(layout, "STT_PROVIDER", _settingsSttProviderTextBox, _configuration.SttProvider);
        AddEditableSettingRow(layout, "OPENAI_API_KEY", _settingsOpenAiApiKeyTextBox, Environment.GetEnvironmentVariable("OPENAI_API_KEY", EnvironmentVariableTarget.User) ?? "", usePassword: true);
        AddEditableSettingRow(layout, "STT_MODEL", _settingsSttModelTextBox, _configuration.TranscriptionModel);
        AddEditableSettingRow(layout, "STT_CHUNK_MINUTES", _settingsSttChunkMinutesTextBox, ReadEnvironmentSetting("STT_CHUNK_MINUTES", "5"));
        AddEditableSettingRow(layout, "STT_AUDIO_PREPROCESSING", _settingsAudioPreprocessingTextBox, ReadEnvironmentSetting("STT_AUDIO_PREPROCESSING", "true"));
        AddEditableSettingRow(layout, "STT_PROMPT_ENABLED", _settingsSttPromptEnabledTextBox, ReadEnvironmentSetting("STT_PROMPT_ENABLED", "false"));
        AddEditableSettingRow(layout, "GROQ_CHAT_MODEL", _settingsChatModelTextBox, _configuration.ChatModel);
        AddEditableSettingRow(layout, "GROQ_CHAT_DELAY_SECONDS", _settingsChatDelayTextBox, _configuration.ChatDelaySeconds);
        AddEditableSettingRow(layout, "GROQ_MAX_AUTO_RETRY_SECONDS", _settingsMaxRetryTextBox, _configuration.MaxAutoRetrySeconds);
        AddEditableSettingRow(layout, "AI_HTTP_TIMEOUT_SECONDS", _settingsHttpTimeoutTextBox, ReadEnvironmentSetting("AI_HTTP_TIMEOUT_SECONDS", "300"));
        AddEditableSettingRow(layout, "GROQ_SUMMARY_CHUNK_SIZE", _settingsSummaryChunkSizeTextBox, ReadEnvironmentSetting("GROQ_SUMMARY_CHUNK_SIZE", "5500"));
        AddEditableSettingRow(layout, "GROQ_PARTIAL_MAX_TOKENS", _settingsPartialMaxTokensTextBox, ReadEnvironmentSetting("GROQ_PARTIAL_MAX_TOKENS", "500"));
        AddEditableSettingRow(layout, "GROQ_FINAL_MAX_TOKENS", _settingsFinalMaxTokensTextBox, ReadEnvironmentSetting("GROQ_FINAL_MAX_TOKENS", "1200"));
        AddEditableSettingRow(layout, "LOCAL_WHISPER_MODEL", _settingsLocalWhisperModelTextBox, _configuration.LocalWhisperModel);

        var settingButtonRow = layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var settingButtons = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Right,
            FlowDirection = FlowDirection.RightToLeft,
            Margin = new Padding(0, 8, 0, 0)
        };
        layout.SetColumnSpan(settingButtons, 3);
        layout.Controls.Add(settingButtons, 0, settingButtonRow);

        ConfigureButton(_applySettingsButton, "설정 적용");
        _applySettingsButton.Click += ApplySettingsButton_Click;
        settingButtons.Controls.Add(_applySettingsButton);

        ConfigureButton(_reloadSettingsButton, "다시 읽기");
        _reloadSettingsButton.Click += (_, _) => ReloadSettingsFromEnvironment(showMessage: true);
        settingButtons.Controls.Add(_reloadSettingsButton);

        var guide = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Top,
            Height = 170,
            Margin = new Padding(0, 16, 0, 0),
            Font = new Font("Consolas", 10F),
            Text = """
            환경 변수 안내

            필수:
            - GROQ_API_KEY: Groq API 키

            선택:
            - STT_PROVIDER: groq 또는 openai, 기본값 groq
            - OPENAI_API_KEY: OpenAI STT 사용 시 필요
            - STT_MODEL: STT 모델명, groq 기본값 whisper-large-v3-turbo, openai 기본값 gpt-4o-mini-transcribe
            - STT_CHUNK_MINUTES: 긴 녹음 분할 단위, 기본값 5
            - STT_AUDIO_PREPROCESSING: STT 전 음성 전처리 사용 여부, 기본값 true, 끄려면 false
            - STT_PROMPT_ENABLED: STT 프롬프트 사용 여부, 기본값 false, 켜려면 true
            - GROQ_CHAT_MODEL: 회의록 정리 모델명, 기본값 llama-3.3-70b-versatile
            - GROQ_CHAT_DELAY_SECONDS: AI 요청 사이 대기 시간, 기본값 20
            - GROQ_MAX_AUTO_RETRY_SECONDS: 429 제한 발생 시 자동으로 기다릴 최대 시간, 기본값 120
            - AI_HTTP_TIMEOUT_SECONDS: AI/STT 요청 타임아웃, 기본값 300
            - GROQ_SUMMARY_CHUNK_SIZE: 회의록 정리 구간 크기, 기본값 5500
            - GROQ_PARTIAL_MAX_TOKENS: 구간별 의견 추출 출력 상한, 기본값 500
            - GROQ_FINAL_MAX_TOKENS: 최종 회의록 출력 상한, 기본값 1200
            - LOCAL_WHISPER_MODEL: 로컬 STT 모델, 기본값 base
            - FFMPEG_PATH, FFPROBE_PATH: ffmpeg 직접 지정

            설정 적용 버튼은 사용자 환경 변수와 현재 실행 중인 프로그램 설정을 함께 갱신합니다.
            이미 열려 있는 다른 프로그램에는 즉시 반영되지 않을 수 있습니다.
            API 키는 보안을 위해 프로그램 내부 파일에 저장하지 않습니다.
            """
        };
        var guideRow = layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.SetColumnSpan(guide, 3);
        layout.Controls.Add(guide, 0, guideRow);

        return tab;
    }

    private static void AddEditableSettingRow(TableLayoutPanel layout, string name, TextBox textBox, string value, bool usePassword = false)
    {
        var row = layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(new Label
        {
            AutoSize = true,
            Text = name,
            Margin = new Padding(0, 6, 12, 6)
        }, 0, row);

        textBox.Text = value;
        textBox.Width = 360;
        textBox.UseSystemPasswordChar = usePassword;
        textBox.Margin = new Padding(0, 3, 12, 3);
        layout.Controls.Add(textBox, 1, row);

        layout.Controls.Add(new Label
        {
            AutoSize = true,
            Text = usePassword ? "사용자 환경 변수 저장" : "비우면 기본값 사용",
            ForeColor = Color.FromArgb(70, 82, 99),
            Margin = new Padding(0, 6, 0, 6)
        }, 2, row);
    }

    private static void AddSettingRow(TableLayoutPanel layout, string name, string value, string source)
    {
        var row = layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(new Label
        {
            AutoSize = true,
            Text = name,
            Font = new Font("맑은 고딕", 10F, FontStyle.Bold),
            Margin = new Padding(0, 6, 12, 6)
        }, 0, row);

        layout.Controls.Add(new Label
        {
            AutoSize = true,
            Text = value,
            Margin = new Padding(0, 6, 12, 6)
        }, 1, row);

        layout.Controls.Add(new Label
        {
            AutoSize = true,
            Text = source,
            ForeColor = Color.FromArgb(70, 82, 99),
            Margin = new Padding(0, 6, 0, 6)
        }, 2, row);
    }

    private void ApplySettingsButton_Click(object? sender, EventArgs e)
    {
        if (!ValidateNumericSetting(_settingsChatDelayTextBox.Text, "GROQ_CHAT_DELAY_SECONDS") ||
            !ValidateNumericSetting(_settingsMaxRetryTextBox.Text, "GROQ_MAX_AUTO_RETRY_SECONDS") ||
            !ValidateNumericSetting(_settingsHttpTimeoutTextBox.Text, "AI_HTTP_TIMEOUT_SECONDS") ||
            !ValidateNumericSetting(_settingsSttChunkMinutesTextBox.Text, "STT_CHUNK_MINUTES") ||
            !ValidateNumericSetting(_settingsSummaryChunkSizeTextBox.Text, "GROQ_SUMMARY_CHUNK_SIZE") ||
            !ValidateNumericSetting(_settingsPartialMaxTokensTextBox.Text, "GROQ_PARTIAL_MAX_TOKENS") ||
            !ValidateNumericSetting(_settingsFinalMaxTokensTextBox.Text, "GROQ_FINAL_MAX_TOKENS"))
        {
            return;
        }

        SaveEnvironmentSetting("GROQ_API_KEY", _settingsApiKeyTextBox.Text, removeWhenEmpty: true);
        SaveEnvironmentSetting("STT_PROVIDER", _settingsSttProviderTextBox.Text, removeWhenEmpty: true);
        SaveEnvironmentSetting("OPENAI_API_KEY", _settingsOpenAiApiKeyTextBox.Text, removeWhenEmpty: true);
        SaveEnvironmentSetting("STT_MODEL", _settingsSttModelTextBox.Text, removeWhenEmpty: true);
        SaveEnvironmentSetting("STT_CHUNK_MINUTES", _settingsSttChunkMinutesTextBox.Text, removeWhenEmpty: true);
        SaveEnvironmentSetting("STT_AUDIO_PREPROCESSING", _settingsAudioPreprocessingTextBox.Text, removeWhenEmpty: true);
        SaveEnvironmentSetting("STT_PROMPT_ENABLED", _settingsSttPromptEnabledTextBox.Text, removeWhenEmpty: true);
        SaveEnvironmentSetting("GROQ_CHAT_MODEL", _settingsChatModelTextBox.Text, removeWhenEmpty: true);
        SaveEnvironmentSetting("GROQ_CHAT_DELAY_SECONDS", _settingsChatDelayTextBox.Text, removeWhenEmpty: true);
        SaveEnvironmentSetting("GROQ_MAX_AUTO_RETRY_SECONDS", _settingsMaxRetryTextBox.Text, removeWhenEmpty: true);
        SaveEnvironmentSetting("AI_HTTP_TIMEOUT_SECONDS", _settingsHttpTimeoutTextBox.Text, removeWhenEmpty: true);
        SaveEnvironmentSetting("GROQ_SUMMARY_CHUNK_SIZE", _settingsSummaryChunkSizeTextBox.Text, removeWhenEmpty: true);
        SaveEnvironmentSetting("GROQ_PARTIAL_MAX_TOKENS", _settingsPartialMaxTokensTextBox.Text, removeWhenEmpty: true);
        SaveEnvironmentSetting("GROQ_FINAL_MAX_TOKENS", _settingsFinalMaxTokensTextBox.Text, removeWhenEmpty: true);
        SaveEnvironmentSetting("LOCAL_WHISPER_MODEL", _settingsLocalWhisperModelTextBox.Text, removeWhenEmpty: true);

        ReloadSettingsFromEnvironment(showMessage: false);
        _statusLabel.Text = "설정을 적용했습니다. 현재 실행 중인 프로그램에도 새 API 설정이 반영되었습니다.";
        MessageBox.Show("설정을 적용했습니다.\r\n\r\n현재 프로그램에는 즉시 반영되며, 다른 프로그램에는 새로 실행할 때 반영됩니다.", "설정 적용", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static IReadOnlyDictionary<string, SttCategoryProfile> LoadSttCategoryProfiles()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Data", "meeting-category-terms.json"),
            Path.Combine(AppContext.BaseDirectory, "meeting-category-terms.json")
        };

        foreach (var path in candidates)
        {
            if (!File.Exists(path))
            {
                continue;
            }

            try
            {
                var json = File.ReadAllText(path, Encoding.UTF8);
                var categories = JsonSerializer.Deserialize<List<SttCategoryDefinition>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var loaded = categories?
                    .Where(category => !string.IsNullOrWhiteSpace(category.Category))
                    .ToDictionary(
                        category => category.Category.Trim(),
                        category => new SttCategoryProfile(
                            NormalizeTerms(category.Topics),
                            NormalizeTerms(category.CommonTerms),
                            NormalizeTopicTerms(category.TopicTerms)),
                        StringComparer.Ordinal);

                if (loaded is { Count: > 0 })
                {
                    return loaded;
                }
            }
            catch
            {
                return FallbackSttCategoryProfiles;
            }
        }

        return FallbackSttCategoryProfiles;
    }

    private static string[] NormalizeTerms(IEnumerable<string>? terms)
    {
        return (terms ?? [])
            .Select(term => term.Trim())
            .Where(term => !string.IsNullOrWhiteSpace(term))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyDictionary<string, string[]> NormalizeTopicTerms(IReadOnlyDictionary<string, string[]>? topicTerms)
    {
        return (topicTerms ?? new Dictionary<string, string[]>())
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Key))
            .ToDictionary(
                pair => pair.Key.Trim(),
                pair => NormalizeTerms(pair.Value),
                StringComparer.OrdinalIgnoreCase);
    }

    private void ApplySttContext()
    {
        _aiService.SetSttContext(BuildSelectedSttContext(), BuildSelectedSttTerms());
    }

    private void UpdateSttTopicOptions(bool resetTopic)
    {
        var selectedCategory = GetSelectedSttCategory();
        if (!SttCategoryProfiles.TryGetValue(selectedCategory, out var profile))
        {
            return;
        }

        var previousTopic = _sttTopicComboBox.Text;
        _sttTopicComboBox.Items.Clear();
        _sttTopicComboBox.Items.AddRange(profile.Topics);

        if (resetTopic || string.IsNullOrWhiteSpace(previousTopic))
        {
            _sttTopicComboBox.Text = profile.Topics.FirstOrDefault() ?? "";
        }
        else
        {
            _sttTopicComboBox.Text = previousTopic;
        }

        UpdateSttTermsPreview();
    }

    private void UpdateSttTermsPreview()
    {
        _sttTermsTextBox.Text = BuildSelectedSttTerms();
    }

    private string BuildSelectedSttContext()
    {
        var category = GetSelectedSttCategory();
        var topic = _sttTopicComboBox.Text.Trim();
        return string.IsNullOrWhiteSpace(topic)
            ? category
            : $"{category} - {topic}";
    }

    private string BuildSelectedSttTerms()
    {
        var category = GetSelectedSttCategory();
        var topic = _sttTopicComboBox.Text.Trim();
        var terms = new List<string>();

        if (SttCategoryProfiles.TryGetValue(category, out var profile))
        {
            terms.AddRange(profile.CommonTerms);
            if (!string.IsNullOrWhiteSpace(topic) &&
                profile.TopicTerms.TryGetValue(topic, out var topicTerms))
            {
                terms.AddRange(topicTerms);
            }
        }

        if (!string.IsNullOrWhiteSpace(topic))
        {
            terms.Add(topic);
            terms.AddRange(Regex.Split(topic, @"[\s,/·()\[\]{}]+").Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        return string.Join(", ", terms
            .Select(term => term.Trim())
            .Where(term => !string.IsNullOrWhiteSpace(term))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(160));
    }

    private string GetSelectedSttCategory()
    {
        var selectedCategory = _sttCategoryComboBox.SelectedItem?.ToString();
        if (!string.IsNullOrWhiteSpace(selectedCategory))
        {
            return selectedCategory;
        }

        return SttCategoryProfiles.Keys.First();
    }

    private void ReloadSettingsFromEnvironment(bool showMessage)
    {
        _configuration = AiConfiguration.Load();
        _aiService = new MeetingAiService(CreateHttpClient(), _configuration);

        _settingsApiKeyTextBox.Text = _configuration.ApiKey ?? "";
        _settingsSttProviderTextBox.Text = _configuration.SttProvider;
        _settingsOpenAiApiKeyTextBox.Text = Environment.GetEnvironmentVariable("OPENAI_API_KEY", EnvironmentVariableTarget.User) ?? "";
        _settingsSttModelTextBox.Text = _configuration.TranscriptionModel;
        _settingsSttChunkMinutesTextBox.Text = ReadEnvironmentSetting("STT_CHUNK_MINUTES", "5");
        _settingsAudioPreprocessingTextBox.Text = ReadEnvironmentSetting("STT_AUDIO_PREPROCESSING", "true");
        _settingsSttPromptEnabledTextBox.Text = ReadEnvironmentSetting("STT_PROMPT_ENABLED", "false");
        _settingsChatModelTextBox.Text = _configuration.ChatModel;
        _settingsChatDelayTextBox.Text = _configuration.ChatDelaySeconds;
        _settingsMaxRetryTextBox.Text = _configuration.MaxAutoRetrySeconds;
        _settingsHttpTimeoutTextBox.Text = ReadEnvironmentSetting("AI_HTTP_TIMEOUT_SECONDS", "300");
        _settingsSummaryChunkSizeTextBox.Text = ReadEnvironmentSetting("GROQ_SUMMARY_CHUNK_SIZE", "5500");
        _settingsPartialMaxTokensTextBox.Text = ReadEnvironmentSetting("GROQ_PARTIAL_MAX_TOKENS", "500");
        _settingsFinalMaxTokensTextBox.Text = ReadEnvironmentSetting("GROQ_FINAL_MAX_TOKENS", "1200");
        _settingsLocalWhisperModelTextBox.Text = _configuration.LocalWhisperModel;

        _summarizeButton.Enabled = _configuration.IsReady && !string.IsNullOrWhiteSpace(_transcriptTextBox.Text);
        _correctTranscriptButton.Enabled = _configuration.IsReady && !string.IsNullOrWhiteSpace(_transcriptTextBox.Text);

        _statusLabel.Text = _configuration.IsReady
            ? "설정을 다시 읽었습니다. AI 기능을 사용할 수 있습니다."
            : "설정을 다시 읽었습니다. API 키가 없어 AI 회의록 정리는 사용할 수 없습니다.";

        if (showMessage)
        {
            MessageBox.Show(_statusLabel.Text, "설정 다시 읽기", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private static void SaveEnvironmentSetting(string name, string value, bool removeWhenEmpty)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (normalized is null && !removeWhenEmpty)
        {
            return;
        }

        Environment.SetEnvironmentVariable(name, normalized, EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable(name, normalized, EnvironmentVariableTarget.User);
    }

    private static string ReadEnvironmentSetting(string name, string fallback)
    {
        return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process) ??
            Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User) ??
            Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine) ??
            fallback;
    }

    private static HttpClient CreateHttpClient()
    {
        var configured = ReadEnvironmentSetting("AI_HTTP_TIMEOUT_SECONDS", "300");
        var seconds = double.TryParse(configured, out var value)
            ? Math.Clamp(value, 60, 900)
            : 300;

        return new HttpClient { Timeout = TimeSpan.FromSeconds(seconds) };
    }

    private static bool ValidateNumericSetting(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (double.TryParse(value.Trim(), out var seconds) && seconds >= 0)
        {
            return true;
        }

        MessageBox.Show($"{name} 값은 0 이상의 숫자여야 합니다.", "설정 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return false;
    }

    private void ApplyStartupGuard()
    {
        if (!_configuration.IsReady)
        {
            SetControlsEnabled(true);
            _summarizeButton.Enabled = false;
            _correctTranscriptButton.Enabled = false;
            _saveButton.Enabled = false;
            _statusLabel.Text = "AI API 키가 없어 STT는 로컬 Whisper로 처리합니다. 회의록 AI 정리는 API 키 등록 후 사용할 수 있습니다.";
            ResetProgress();
            MessageBox.Show(
                "AI API 키를 찾지 못했습니다.\r\n\r\nSTT 변환은 로컬 Whisper로 계속 사용할 수 있습니다.\r\n회의록 AI 정리는 GROQ_API_KEY 등록 후 사용할 수 있습니다.",
                "로컬 STT 모드",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        SetControlsEnabled(true);
        _summarizeButton.Enabled = false;
        _correctTranscriptButton.Enabled = false;
        _saveButton.Enabled = false;
        _statusLabel.Text = "회의 녹음 파일을 선택하세요.";
        ResetProgress();
    }

    private static void ConfigureButton(Button button, string text)
    {
        button.Text = text;
        button.AutoSize = false;
        button.Width = 110;
        button.Height = 32;
        button.Margin = new Padding(8, 0, 0, 0);
        button.FlatStyle = FlatStyle.System;
    }

    private static void ConfigureHistoryDatePicker(DateTimePicker picker)
    {
        picker.Format = DateTimePickerFormat.Short;
        picker.ShowCheckBox = true;
        picker.Checked = false;
        picker.Width = 130;
    }

    private async void TranscribeButton_Click(object? sender, EventArgs e)
    {
        if (!File.Exists(_audioPathTextBox.Text))
        {
            MessageBox.Show("회의 녹음 파일을 먼저 선택하세요.", "파일 필요", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        await RunBusyAsync("STT 변환 중입니다...", async token =>
        {
            var progress = new Progress<string>(UpdateProgress);
            ApplySttContext();
            SetEditorText(_transcriptTextBox, await _aiService.TranscribeAsync(_audioPathTextBox.Text, token, progress), markUnsaved: true);
            _summarizeButton.Enabled = _configuration.IsReady;
            _correctTranscriptButton.Enabled = _configuration.IsReady;
            _saveButton.Enabled = CanSaveCurrentWork();
            CompleteProgress();
            _statusLabel.Text = _configuration.IsReady
                ? "STT 변환 완료. 원문을 확인하거나 수정한 뒤 회의록 정리를 실행하세요."
                : "STT 변환 완료. AI 회의록 정리는 API 키 등록 후 사용할 수 있습니다.";
        });
    }

    private async void CorrectTranscriptButton_Click(object? sender, EventArgs e)
    {
        if (!_configuration.IsReady)
        {
            MessageBox.Show("STT 원문 보정은 AI API 키 등록 후 사용할 수 있습니다.", "API 키 필요", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            SelectSettingsTab();
            return;
        }

        if (string.IsNullOrWhiteSpace(_transcriptTextBox.Text))
        {
            MessageBox.Show("보정할 STT 원문이 없습니다.", "원문 필요", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!string.IsNullOrWhiteSpace(_summaryTextBox.Text))
        {
            var result = MessageBox.Show(
                "STT 원문을 보정하면 기존 AI 회의록은 이전 원문 기준이므로 지워집니다.\r\n계속 진행할까요?",
                "원문 보정 확인",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                return;
            }
        }

        await RunBusyAsync("STT 원문 보정 중입니다...", async token =>
        {
            var progress = new Progress<string>(UpdateProgress);
            ApplySttContext();
            SetEditorText(_transcriptTextBox, await _aiService.ImproveTranscriptAsync(_transcriptTextBox.Text, token, progress), markUnsaved: true);
            SetEditorText(_summaryTextBox, string.Empty, markUnsaved: false);
            _summarizeButton.Enabled = true;
            _saveButton.Enabled = CanSaveCurrentWork();
            CompleteProgress();
            _statusLabel.Text = "STT 원문 보정 완료. 내용을 확인한 뒤 회의록 정리를 실행하세요.";
        });
    }

    private async void SummarizeButton_Click(object? sender, EventArgs e)
    {
        await SummarizeWithRecoveryAsync(clearCacheBeforeRun: false);
    }

    private async Task SummarizeWithRecoveryAsync(bool clearCacheBeforeRun)
    {
        if (string.IsNullOrWhiteSpace(_transcriptTextBox.Text))
        {
            MessageBox.Show("회의록 정리를 하려면 STT 원문이 필요합니다.", "원문 필요", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (clearCacheBeforeRun)
        {
            _aiService.DeleteSummaryCache(_transcriptTextBox.Text);
        }

        var error = await RunBusyAsync("회의록 정리 중입니다...", async token =>
        {
            var progress = new Progress<string>(UpdateProgress);
            SetEditorText(_summaryTextBox, await _aiService.SummarizeAsync(_transcriptTextBox.Text, token, progress), markUnsaved: true);
            _saveButton.Enabled = true;
            CompleteProgress();
            _statusLabel.Text = "회의록 정리 완료. 내용을 확인한 뒤 저장하세요.";
        }, showErrorMessage: false);

        if (error is null)
        {
            return;
        }

        switch (ShowAiRecoveryDialog(error))
        {
            case AiRecoveryAction.Retry:
                await SummarizeWithRecoveryAsync(clearCacheBeforeRun: false);
                break;
            case AiRecoveryAction.ClearCacheAndRetry:
                await SummarizeWithRecoveryAsync(clearCacheBeforeRun: true);
                break;
            case AiRecoveryAction.OpenSettings:
                SelectSettingsTab();
                break;
        }
    }

    private void BrowseButton_Click(object? sender, EventArgs e)
    {
        if (!ConfirmDiscardUnsavedWork())
        {
            return;
        }

        using var dialog = new OpenFileDialog
        {
            Title = "회의 녹음 파일 선택",
            Filter = "오디오/비디오 파일|*.mp3;*.wav;*.m4a;*.mp4;*.ogg;*.webm;*.opus|모든 파일|*.*"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _audioPathTextBox.Text = dialog.FileName;
        SetEditorText(_transcriptTextBox, string.Empty, markUnsaved: false);
        SetEditorText(_summaryTextBox, string.Empty, markUnsaved: false);
        _hasUnsavedWork = false;
        _summarizeButton.Enabled = false;
        _correctTranscriptButton.Enabled = false;
        _saveButton.Enabled = false;
        _statusLabel.Text = "파일 선택 완료. STT 변환을 실행하세요.";
        ResetProgress();
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        if (!CanSaveCurrentWork())
        {
            MessageBox.Show("저장할 STT 원문 또는 AI 회의록이 없습니다.", "저장 불가", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var record = _storageService.Save(_audioPathTextBox.Text, _transcriptTextBox.Text, _summaryTextBox.Text);
        _hasUnsavedWork = false;
        RefreshHistory();
        _statusLabel.Text = $"저장 완료: {record.MarkdownPath}";
        MessageBox.Show($"회의록을 저장했습니다.\r\n\r\n{record.MarkdownPath}", "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void CancelButton_Click(object? sender, EventArgs e)
    {
        _workCancellation?.Cancel();
        _cancelButton.Enabled = false;
        _statusLabel.Text = "작업 취소를 요청했습니다. 현재 단계가 정리되면 중단됩니다.";
    }

    private void ClearCacheButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_transcriptTextBox.Text))
        {
            MessageBox.Show("삭제할 요약 캐시를 찾으려면 STT 원문이 필요합니다.", "원문 필요", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var result = MessageBox.Show(
            "현재 STT 원문에 연결된 AI 요약 캐시를 삭제할까요?\r\n삭제 후 다시 회의록 정리를 실행하면 AI가 처음부터 다시 정리합니다.",
            "요약 캐시 삭제",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
        {
            return;
        }

        var deleted = _aiService.DeleteSummaryCache(_transcriptTextBox.Text);
        _statusLabel.Text = deleted
            ? "현재 원문의 AI 요약 캐시를 삭제했습니다. 회의록 정리를 다시 실행할 수 있습니다."
            : "현재 원문에 연결된 AI 요약 캐시가 없습니다.";
    }

    private void RefreshHistory()
    {
        _historyRecords = _storageService.GetRecordList();
        _historyRows = _historyRecords.Select(MeetingHistoryRow.FromListItem).ToList();
        _historyTranscriptSearchCache.Clear();
        ApplyHistoryFilter();
    }

    private void ScheduleHistoryFilter()
    {
        _historyFilterTimer.Stop();
        _historyFilterTimer.Start();
    }

    private void ApplyHistoryFilter()
    {
        var keyword = _historySearchTextBox.Text.Trim().ToUpperInvariant();
        var rows = _historyRows.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            rows = rows.Where(row =>
                row.SearchText.Contains(keyword, StringComparison.Ordinal) ||
                HistoryTranscriptContains(row.Id, keyword));
        }

        if (_historyFromDatePicker.Checked)
        {
            var from = _historyFromDatePicker.Value.Date;
            rows = rows.Where(row => row.CreatedAt.Date >= from);
        }

        if (_historyToDatePicker.Checked)
        {
            var to = _historyToDatePicker.Value.Date;
            rows = rows.Where(row => row.CreatedAt.Date <= to);
        }

        var filteredRows = rows.ToList();
        _historyBinding.DataSource = filteredRows;
        _statusLabel.Text = _historyRecords.Count == 0
            ? "저장된 회의록 기록이 없습니다."
            : $"회의록 기록 {filteredRows.Count}개를 표시합니다. 전체 {_historyRecords.Count}개";
    }

    private void ClearHistoryFilter()
    {
        _historySearchTextBox.Clear();
        _historyFromDatePicker.Checked = false;
        _historyToDatePicker.Checked = false;
        ApplyHistoryFilter();
    }

    private static bool Contains(string? text, string keyword)
    {
        return !string.IsNullOrWhiteSpace(text) &&
            text.Contains(keyword, StringComparison.OrdinalIgnoreCase);
    }

    private bool HistoryTranscriptContains(string id, string normalizedKeyword)
    {
        if (_historyTranscriptSearchCache.TryGetValue(id, out var searchText))
        {
            return searchText.Contains(normalizedKeyword, StringComparison.Ordinal);
        }

        var record = _storageService.GetRecord(id);
        searchText = record?.Transcript.ToUpperInvariant() ?? string.Empty;
        _historyTranscriptSearchCache[id] = searchText;
        return searchText.Contains(normalizedKeyword, StringComparison.Ordinal);
    }

    private MeetingHistoryRow? GetSelectedHistoryRow()
    {
        return _historyGrid.CurrentRow?.DataBoundItem as MeetingHistoryRow;
    }

    private void LoadSelectedHistory()
    {
        if (!ConfirmDiscardUnsavedWork())
        {
            return;
        }

        var row = GetSelectedHistoryRow();
        if (row is null)
        {
            MessageBox.Show("불러올 회의록을 선택하세요.", "선택 필요", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var record = _storageService.GetRecord(row.Id);
        if (record is null)
        {
            MessageBox.Show("선택한 회의록 파일을 찾지 못했습니다.", "불러오기 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            RefreshHistory();
            return;
        }

        _audioPathTextBox.Text = record.AudioPath;
        SetEditorText(_transcriptTextBox, record.Transcript, markUnsaved: false);
        SetEditorText(_summaryTextBox, record.Summary, markUnsaved: false);
        _hasUnsavedWork = false;
        _summarizeButton.Enabled = _configuration.IsReady && !string.IsNullOrWhiteSpace(record.Transcript);
        _correctTranscriptButton.Enabled = _configuration.IsReady && !string.IsNullOrWhiteSpace(record.Transcript);
        _saveButton.Enabled = CanSaveCurrentWork();
        _statusLabel.Text = $"회의록 기록을 불러왔습니다: {Path.GetFileName(record.MarkdownPath)}";
    }

    private void OpenSelectedMarkdown()
    {
        var row = GetSelectedHistoryRow();
        if (row is null)
        {
            MessageBox.Show("열 회의록을 선택하세요.", "선택 필요", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var record = _storageService.GetRecord(row.Id);
        if (record is null || !File.Exists(record.MarkdownPath))
        {
            MessageBox.Show("Markdown 회의록 파일을 찾지 못했습니다.", "파일 없음", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            RefreshHistory();
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = record.MarkdownPath,
            UseShellExecute = true
        });
    }

    private void DeleteSelectedHistory()
    {
        var row = GetSelectedHistoryRow();
        if (row is null)
        {
            MessageBox.Show("삭제할 회의록을 선택하세요.", "선택 필요", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var result = MessageBox.Show(
            "선택한 회의록 기록을 삭제할까요?\r\nMarkdown과 JSON 파일이 함께 삭제됩니다.",
            "회의록 삭제",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
        {
            return;
        }

        _storageService.Delete(row.Id);
        RefreshHistory();
    }

    private void DeleteAllHistory()
    {
        var result = MessageBox.Show(
            "모든 회의록 기록을 삭제할까요?\r\nData/meetings의 Markdown과 JSON 파일이 삭제됩니다.",
            "전체 기록 삭제",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
        {
            return;
        }

        _storageService.DeleteAll();
        RefreshHistory();
    }

    private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _historyFilterTimer.Stop();
        if (!ConfirmDiscardUnsavedWork())
        {
            e.Cancel = true;
        }
    }

    private void MarkUnsavedWork(object? sender, EventArgs e)
    {
        if (!_suppressChangeTracking)
        {
            _hasUnsavedWork = true;
        }
    }

    private void SetEditorText(TextBox textBox, string text, bool markUnsaved)
    {
        _suppressChangeTracking = true;
        try
        {
            textBox.Text = text;
        }
        finally
        {
            _suppressChangeTracking = false;
        }

        if (markUnsaved)
        {
            _hasUnsavedWork = true;
        }
    }

    private bool ConfirmDiscardUnsavedWork()
    {
        if (!_hasUnsavedWork || string.IsNullOrWhiteSpace(_transcriptTextBox.Text + _summaryTextBox.Text))
        {
            return true;
        }

        var result = MessageBox.Show(
            "저장하지 않은 STT 원문 또는 AI 회의록이 있습니다.\r\n계속하면 현재 작업 내용이 사라질 수 있습니다.\r\n\r\n계속 진행할까요?",
            "저장되지 않은 작업",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        return result == DialogResult.Yes;
    }

    private bool CanSaveCurrentWork()
    {
        return !string.IsNullOrWhiteSpace(_transcriptTextBox.Text) ||
            !string.IsNullOrWhiteSpace(_summaryTextBox.Text);
    }

    private AiRecoveryAction ShowAiRecoveryDialog(Exception error)
    {
        using var dialog = new Form
        {
            Text = "AI 회의록 정리 오류",
            Width = 620,
            Height = 430,
            MinimizeBox = false,
            MaximizeBox = false,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog
        };

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(18)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        dialog.Controls.Add(root);

        root.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "AI 회의록 정리에 실패했습니다. 다음 작업을 선택하세요.",
            Font = new Font("맑은 고딕", 11F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 12)
        }, 0, 0);

        root.Controls.Add(new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            Font = new Font("맑은 고딕", 9F),
            Text = CreateFriendlyErrorMessage(error)
        }, 0, 1);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Margin = new Padding(0, 14, 0, 0)
        };
        root.Controls.Add(buttons, 0, 2);

        var selected = AiRecoveryAction.Close;
        AddRecoveryButton(buttons, "닫기", AiRecoveryAction.Close);
        AddRecoveryButton(buttons, "설정 확인", AiRecoveryAction.OpenSettings);
        AddRecoveryButton(buttons, "캐시 삭제 후 다시 시도", AiRecoveryAction.ClearCacheAndRetry, 160);
        AddRecoveryButton(buttons, "다시 시도", AiRecoveryAction.Retry);

        dialog.ShowDialog(this);
        return selected;

        void AddRecoveryButton(FlowLayoutPanel panel, string text, AiRecoveryAction action, int width = 110)
        {
            var button = new Button
            {
                Text = text,
                Width = width,
                Height = 32,
                Margin = new Padding(8, 0, 0, 0),
                FlatStyle = FlatStyle.System
            };
            button.Click += (_, _) =>
            {
                selected = action;
                dialog.Close();
            };
            panel.Controls.Add(button);
        }
    }

    private void SelectSettingsTab()
    {
        foreach (var page in _tabs.TabPages.OfType<TabPage>())
        {
            if (page.Text == "설정")
            {
                _tabs.SelectedTab = page;
                _statusLabel.Text = "설정 탭에서 API 키, 모델, 요청 대기 시간 상태를 확인하세요.";
                return;
            }
        }
    }

    private async Task<Exception?> RunBusyAsync(string status, Func<CancellationToken, Task> work, bool showErrorMessage = true)
    {
        Exception? error = null;
        try
        {
            _workCancellation?.Cancel();
            _workCancellation = new CancellationTokenSource();

            SetControlsEnabled(false);
            _cancelButton.Enabled = true;
            StartProgress(status);
            await work(_workCancellation.Token);
        }
        catch (OperationCanceledException)
        {
            _statusLabel.Text = "작업이 취소되었습니다.";
            ResetProgress("취소");
        }
        catch (Exception ex)
        {
            error = ex;
            var friendlyMessage = CreateFriendlyErrorMessage(ex);
            if (showErrorMessage && !string.IsNullOrWhiteSpace(friendlyMessage))
            {
                MessageBox.Show(friendlyMessage, "처리 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _statusLabel.Text = "처리 중 오류가 발생했습니다. 안내 내용을 확인해 주세요.";
                ResetProgress("오류");
            }
            else if (showErrorMessage)
            {
                MessageBox.Show(ex.Message, "처리 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _statusLabel.Text = "처리 중 오류가 발생했습니다.";
                ResetProgress("오류");
            }
            else
            {
                _statusLabel.Text = "AI 회의록 정리 중 오류가 발생했습니다. 다음 작업을 선택해 주세요.";
                ResetProgress("오류");
            }
        }
        finally
        {
            SetControlsEnabled(true);
            _cancelButton.Enabled = false;
            _summarizeButton.Enabled = _configuration.IsReady && !string.IsNullOrWhiteSpace(_transcriptTextBox.Text);
            _correctTranscriptButton.Enabled = _configuration.IsReady && !string.IsNullOrWhiteSpace(_transcriptTextBox.Text);
            _saveButton.Enabled = CanSaveCurrentWork();
        }

        return error;
    }

    private static string CreateFriendlyErrorMessage(Exception ex)
    {
        var message = ex.Message;
        var detail = Truncate(message, 900);

        if (ContainsAny(message, "429", "Too Many Requests", "rate_limit_exceeded"))
        {
            var wait = ExtractRetryWait(message);
            if (ContainsAny(message, "tokens per day", "TPD", "하루 토큰 한도"))
            {
                return $"""
                AI API 하루 토큰 한도에 걸렸습니다.

                원인:
                - 긴 회의록을 여러 구간으로 정리하면서 Groq 하루 토큰 한도(TPD)에 거의 도달했습니다.
                - 분당 제한과 달리 짧은 재시도나 대기 시간 증가만으로는 바로 해결되지 않습니다.
                - 이미 처리된 구간은 캐시에 저장되어 다음 실행 때 재사용됩니다.

                해결 방법:
                - {wait}
                - 다시 [회의록 정리]를 누르면 저장된 중간 결과부터 이어서 처리합니다.
                - 설정에서 GROQ_PARTIAL_MAX_TOKENS 값을 300~500 정도로 낮춰 구간별 출력량을 줄여 주세요.
                - 설정에서 GROQ_FINAL_MAX_TOKENS 값을 800~1200 정도로 낮춰 최종 정리 출력량을 줄여 주세요.
                - 급하면 GROQ_CHAT_MODEL을 더 가벼운 모델로 바꾸거나 다른 API 키/요금제를 사용해야 합니다.

                상세 오류:
                {detail}
                """;
            }

            return $"""
            AI API 사용량 제한에 걸렸습니다.

            원인:
            - 긴 회의록을 여러 구간으로 정리하면서 분당 토큰 제한을 초과했습니다.
            - 이미 처리된 구간은 캐시에 저장되어 다음 실행 때 재사용됩니다.

            해결 방법:
            - {wait}
            - 다시 [회의록 정리]를 누르면 저장된 중간 결과부터 이어서 처리합니다.
            - 제한이 자주 걸리면 환경 변수 GROQ_CHAT_DELAY_SECONDS 값을 30~60초로 늘려 주세요.
            - 자동 대기가 너무 길면 GROQ_MAX_AUTO_RETRY_SECONDS 값으로 중단 기준을 조정할 수 있습니다.

            상세 오류:
            {detail}
            """;
        }

        if (ContainsAny(message, "401", "403", "Incorrect API key", "invalid api key", "unauthorized"))
        {
            return $"""
            AI API 키를 사용할 수 없습니다.

            해결 방법:
            - 사용자 환경 변수에 GROQ_API_KEY 또는 XAI_API_KEY가 등록되어 있는지 확인해 주세요.
            - API 키를 새로 등록했다면 프로그램을 완전히 종료한 뒤 다시 실행해 주세요.
            - 키 앞뒤에 공백이나 따옴표가 들어가지 않았는지 확인해 주세요.

            상세 오류:
            {detail}
            """;
        }

        if (ContainsAny(message, "413", "Payload Too Large"))
        {
            return $"""
            한 번에 보낸 회의 내용이 API 허용 크기를 초과했습니다.

            해결 방법:
            - 현재 프로그램은 긴 원문을 자동으로 나누어 처리하도록 구성되어 있습니다.
            - 같은 오류가 반복되면 STT 원문에서 회의와 무관한 반복 문장을 줄인 뒤 다시 정리해 주세요.

            상세 오류:
            {detail}
            """;
        }

        if (ContainsAny(message, "400 Bad Request", "invalid_request_error", "unsupported"))
        {
            return $"""
            AI API 요청 형식 또는 모델 설정에 문제가 있습니다.

            해결 방법:
            - GROQ_CHAT_MODEL 값을 변경했다면 Groq에서 지원하는 채팅 모델인지 확인해 주세요.
            - 기본값은 llama-3.3-70b-versatile 입니다.
            - 환경 변수를 수정했다면 프로그램을 다시 실행해 주세요.

            상세 오류:
            {detail}
            """;
        }

        if (ContainsAny(message, "AI 응답", "회의록 내용을 찾지 못했습니다"))
        {
            return $"""
            AI 응답에서 회의록 본문을 찾지 못했습니다.

            해결 방법:
            - 다시 [회의록 정리]를 실행해 주세요.
            - 같은 문제가 반복되면 GROQ_CHAT_MODEL을 llama-3.3-70b-versatile로 설정해 주세요.

            상세 오류:
            {detail}
            """;
        }

        return $"""
        처리 중 오류가 발생했습니다.

        상세 오류:
        {detail}
        """;
    }

    private static bool ContainsAny(string text, params string[] values)
    {
        return values.Any(value => text.Contains(value, StringComparison.OrdinalIgnoreCase));
    }

    private static string ExtractRetryWait(string message)
    {
        var match = Regex.Match(message, @"try again in\s+([0-9.]+)s", RegexOptions.IgnoreCase);
        return match.Success
            ? $"약 {match.Groups[1].Value}초 후 다시 시도해 주세요."
            : "잠시 기다린 뒤 다시 시도해 주세요.";
    }

    private static string Truncate(string text, int maxLength)
    {
        return text.Length <= maxLength ? text : text[..maxLength] + "...";
    }
    private void StartProgress(string message)
    {
        _statusLabel.Text = message;
        _progressLabel.Text = "처리 중";
        _progressBar.Style = ProgressBarStyle.Marquee;
        _progressBar.MarqueeAnimationSpeed = 30;
    }

    private void UpdateProgress(string message)
    {
        _statusLabel.Text = message;

        var match = ProgressPattern.Match(message);
        if (!match.Success ||
            !int.TryParse(match.Groups[1].Value, out var current) ||
            !int.TryParse(match.Groups[2].Value, out var total) ||
            total <= 0)
        {
            _progressBar.Style = ProgressBarStyle.Marquee;
            _progressBar.MarqueeAnimationSpeed = 30;
            _progressLabel.Text = "처리 중";
            return;
        }

        var percent = Math.Clamp((int)Math.Round(current * 100.0 / total), 0, 100);
        _progressBar.Style = ProgressBarStyle.Continuous;
        _progressBar.MarqueeAnimationSpeed = 0;
        _progressBar.Value = percent;
        _progressLabel.Text = $"{percent}%";
    }

    private void CompleteProgress()
    {
        _progressBar.Style = ProgressBarStyle.Continuous;
        _progressBar.MarqueeAnimationSpeed = 0;
        _progressBar.Value = 100;
        _progressLabel.Text = "100%";
    }

    private void ResetProgress(string text = "대기")
    {
        _progressBar.Style = ProgressBarStyle.Continuous;
        _progressBar.MarqueeAnimationSpeed = 0;
        _progressBar.Value = 0;
        _progressLabel.Text = text;
    }

    private void SetControlsEnabled(bool enabled)
    {
        _browseButton.Enabled = enabled;
        _transcribeButton.Enabled = enabled;
        _sttCategoryComboBox.Enabled = enabled;
        _sttTopicComboBox.Enabled = enabled;
        _sttTermsTextBox.Enabled = enabled;
        _summarizeButton.Enabled = enabled;
        _correctTranscriptButton.Enabled = enabled;
        _saveButton.Enabled = enabled;
        _clearCacheButton.Enabled = enabled;
        _refreshHistoryButton.Enabled = enabled;
        _loadHistoryButton.Enabled = enabled;
        _openMarkdownButton.Enabled = enabled;
        _deleteHistoryButton.Enabled = enabled;
        _deleteAllHistoryButton.Enabled = enabled;
        _clearHistoryFilterButton.Enabled = enabled;
        _historySearchTextBox.Enabled = enabled;
        _historyFromDatePicker.Enabled = enabled;
        _historyToDatePicker.Enabled = enabled;
        _settingsApiKeyTextBox.Enabled = enabled;
        _settingsSttProviderTextBox.Enabled = enabled;
        _settingsOpenAiApiKeyTextBox.Enabled = enabled;
        _settingsSttModelTextBox.Enabled = enabled;
        _settingsSttChunkMinutesTextBox.Enabled = enabled;
        _settingsAudioPreprocessingTextBox.Enabled = enabled;
        _settingsSttPromptEnabledTextBox.Enabled = enabled;
        _settingsChatModelTextBox.Enabled = enabled;
        _settingsChatDelayTextBox.Enabled = enabled;
        _settingsMaxRetryTextBox.Enabled = enabled;
        _settingsHttpTimeoutTextBox.Enabled = enabled;
        _settingsSummaryChunkSizeTextBox.Enabled = enabled;
        _settingsPartialMaxTokensTextBox.Enabled = enabled;
        _settingsFinalMaxTokensTextBox.Enabled = enabled;
        _settingsLocalWhisperModelTextBox.Enabled = enabled;
        _applySettingsButton.Enabled = enabled;
        _reloadSettingsButton.Enabled = enabled;
        _cancelButton.Enabled = !enabled && _workCancellation is not null && !_workCancellation.IsCancellationRequested;
    }

    private enum AiRecoveryAction
    {
        Close,
        Retry,
        ClearCacheAndRetry,
        OpenSettings
    }

    private sealed record MeetingHistoryRow(
        string Id,
        DateTime CreatedAt,
        string CreatedAtText,
        string AudioFileName,
        string SummaryPreview,
        string SearchText)
    {
        public static MeetingHistoryRow FromListItem(MeetingRecordListItem record)
        {
            var source = string.IsNullOrWhiteSpace(record.Summary)
                ? "[STT 원문만 저장됨]"
                : record.Summary;
            var previewSource = source.Length > 500 ? source[..500] : source;
            var preview = Regex.Replace(previewSource, @"\s+", " ").Trim();
            if (preview.Length > 90)
            {
                preview = preview[..90] + "...";
            }

            return new MeetingHistoryRow(
                record.Id,
                record.CreatedAt,
                record.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                string.IsNullOrWhiteSpace(record.AudioPath) ? "(원본 없음)" : Path.GetFileName(record.AudioPath),
                preview,
                BuildSearchText(record));
        }

        private static string BuildSearchText(MeetingRecordListItem record)
        {
            return string.Join('\n',
                record.AudioPath,
                Path.GetFileName(record.AudioPath),
                record.Summary).ToUpperInvariant();
        }
    }

    private sealed record SttCategoryDefinition(
        string Category,
        string[] Topics,
        string[] CommonTerms,
        Dictionary<string, string[]> TopicTerms);

    private sealed record SttCategoryProfile(
        string[] Topics,
        string[] CommonTerms,
        IReadOnlyDictionary<string, string[]> TopicTerms);
}

