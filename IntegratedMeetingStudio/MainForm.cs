using System;
using System.Drawing;
using System.Windows.Forms;
using IntegratedMeetingStudio.Services;
using IntegratedMeetingStudio.Models;

namespace IntegratedMeetingStudio;

public partial class MainForm : Form
{
    private bool _darkMode = false;

    private Color C_BgMain => _darkMode ? Color.FromArgb(28, 28, 45) : Color.FromArgb(245, 247, 250);
    private Color C_BgSidebar => _darkMode ? Color.FromArgb(20, 20, 38) : Color.FromArgb(44, 62, 80);
    private Color C_BgPanel => _darkMode ? Color.FromArgb(40, 40, 60) : Color.White;
    private Color C_TextDark => _darkMode ? Color.FromArgb(220, 220, 235) : Color.FromArgb(30, 30, 30);
    private Color C_TextMuted => _darkMode ? Color.FromArgb(150, 150, 170) : Color.FromArgb(120, 120, 120);

    private Panel sidebarPanel = null!;
    private Panel headerPanel = null!;
    private Panel mainPanel = null!;
    private Button btnDarkMode = null!;

    private Panel viewHome = null!;
    private Panel viewStt = null!;
    private Panel viewHistory = null!;
    private Panel viewKanban = null!;
    private Panel viewSettings = null!;

    public MainForm()
    {
        this.Text = "AI 통합 회의 및 업무 관리 스튜디오";
        this.Size = new Size(1100, 700);
        this.MinimumSize = new Size(900, 600);
        this.StartPosition = FormStartPosition.CenterScreen;

        _darkMode = EnvManager.Get("UI_THEME", "Light") == "Dark";

        BuildUI();
        ApplyTheme();
        
        var config = AiConfiguration.Load();
        ShowView(viewHome);
    }

    private void BuildUI()
    {
        viewHome = CreateViewPanel("홈 대시보드 - 환영합니다!");
        viewStt = new Panel { Size = new Size(1060, 700), Dock = DockStyle.Fill, Visible = false };
        SetupSttView();
        viewHistory = new Panel { Size = new Size(1060, 700), Dock = DockStyle.Fill, Visible = false };
        SetupHistoryView();
        viewKanban = new Panel { Size = new Size(1060, 700), Dock = DockStyle.Fill, Visible = false };
        SetupKanbanView();
        viewSettings = new Panel { Size = new Size(1060, 700), Dock = DockStyle.Fill, Visible = false };
        SetupSettingsView();

        sidebarPanel = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = C_BgSidebar };
        var lblLogo = new Label { Text = "🎙 AI Studio", Font = new Font("맑은 고딕", 16f, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(20, 20) };
        sidebarPanel.Controls.Add(lblLogo);

        int btnY = 80;
        sidebarPanel.Controls.Add(CreateMenuButton("🏠 홈 (대시보드)", btnY, viewHome));
        sidebarPanel.Controls.Add(CreateMenuButton("🎙 새 회의 분석", btnY += 50, viewStt));
        sidebarPanel.Controls.Add(CreateMenuButton("📁 회의 히스토리", btnY += 50, viewHistory));
        sidebarPanel.Controls.Add(CreateMenuButton("📋 칸반 보드", btnY += 50, viewKanban));
        sidebarPanel.Controls.Add(CreateMenuButton("⚙ 설정", btnY += 50, viewSettings));

        this.Controls.Add(sidebarPanel);

        headerPanel = new Panel { Dock = DockStyle.Top, Height = 60 };
        btnDarkMode = new Button
        {
            Text = "🌙 다크 모드",
            Size = new Size(140, 40),
            Location = new Point(1100, 10),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("맑은 고딕", 10f, FontStyle.Bold)
        };
        btnDarkMode.FlatAppearance.BorderSize = 0;
        btnDarkMode.Click += (s, e) => ToggleDarkMode();
        headerPanel.Controls.Add(btnDarkMode);

        this.Controls.Add(headerPanel);

        mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };
        mainPanel.Controls.Add(viewSettings);
        mainPanel.Controls.Add(viewKanban);
        mainPanel.Controls.Add(viewHistory);
        mainPanel.Controls.Add(viewStt);
        mainPanel.Controls.Add(viewHome);
        
        this.Controls.Add(mainPanel);

        sidebarPanel.BringToFront();
        headerPanel.BringToFront();
        mainPanel.BringToFront();
    }

    private Button CreateMenuButton(string text, int yPos, Panel targetView)
    {
        var btn = new Button
        {
            Text = text,
            Location = new Point(10, yPos),
            Size = new Size(200, 40),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0),
            Font = new Font("맑은 고딕", 11f, FontStyle.Regular),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 80, 100);
        btn.Click += (s, e) => ShowView(targetView);
        return btn;
    }

    private void SetupSttView()
    {
        viewStt.Controls.Clear();

        var lblTitle = new Label
        {
            Text = "새 회의 분석",
            Font = new Font("맑은 고딕", 20f, FontStyle.Bold),
            ForeColor = C_TextDark,
            AutoSize = true,
            Location = new Point(20, 20)
        };

        var pnlInputMode = new Panel { Location = new Point(20, 60), Size = new Size(300, 30) };
        var rdoAudio = new RadioButton { Text = "음성 파일", Checked = true, Location = new Point(0, 5), AutoSize = true, Cursor = Cursors.Hand };
        var rdoText = new RadioButton { Text = "직접 텍스트 입력", Location = new Point(100, 5), AutoSize = true, Cursor = Cursors.Hand };
        pnlInputMode.Controls.AddRange(new Control[] { rdoAudio, rdoText });

        var pnlAudioConfig = new Panel { Location = new Point(20, 100), Size = new Size(500, 50) };
        var btnSelectAudio = new Button
        {
            Text = "🎵 음성 파일 선택",
            Location = new Point(0, 0),
            Size = new Size(200, 45),
            BackColor = Color.FromArgb(52, 152, 219),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("맑은 고딕", 11f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnSelectAudio.FlatAppearance.BorderSize = 0;

        var lblSelectedFile = new Label
        {
            Text = "선택된 파일: 없음",
            Location = new Point(220, 12),
            AutoSize = true,
            Font = new Font("맑은 고딕", 10f),
            ForeColor = C_TextMuted
        };
        pnlAudioConfig.Controls.AddRange(new Control[] { btnSelectAudio, lblSelectedFile });

        var pnlTextConfig = new Panel { Location = new Point(20, 100), Size = new Size(500, 50), Visible = false };
        var lblInputHint = new Label
        {
            Text = "※ 발언자가 구분된 형태(예: 홍길동: 안녕하세요)의 텍스트를 아래에 입력하세요.",
            Location = new Point(0, 15),
            AutoSize = true,
            Font = new Font("맑은 고딕", 9f),
            ForeColor = C_TextMuted
        };
        pnlTextConfig.Controls.Add(lblInputHint);

        var btnStartAnalysis = new Button
        {
            Text = "🎙 음성 인식 시작",
            Location = new Point(520, 100),
            Size = new Size(200, 45),
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("맑은 고딕", 11f, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnStartAnalysis.FlatAppearance.BorderSize = 0;

        var txtInput = new RichTextBox
        {
            Location = new Point(20, 160),
            Size = new Size(700, 150),
            Font = new Font("맑은 고딕", 10f),
            BackColor = Color.White,
            ForeColor = Color.Black,
            BorderStyle = BorderStyle.FixedSingle,
            Visible = false
        };

        var txtOutput = new RichTextBox
        {
            Location = new Point(20, 160),
            Size = new Size(700, 430),
            Font = new Font("맑은 고딕", 10f),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = C_BgPanel,
            ForeColor = C_TextDark,
            ReadOnly = true,
            BorderStyle = BorderStyle.FixedSingle
        };

        var lblStatus = new Label
        {
            Text = "대기 중...",
            Location = new Point(20, 610),
            AutoSize = true,
            Font = new Font("맑은 고딕", 10f, FontStyle.Bold),
            ForeColor = C_TextMuted,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left
        };

        rdoAudio.CheckedChanged += (s, e) =>
        {
            pnlAudioConfig.Visible = rdoAudio.Checked;
            if (rdoAudio.Checked)
            {
                btnStartAnalysis.Text = "🎙 음성 인식 시작";
                txtInput.Visible = false;
                txtOutput.Location = new Point(20, 160);
                txtOutput.Size = new Size(700, 430);
                btnStartAnalysis.Enabled = lblSelectedFile.Text != "선택된 파일: 없음";
            }
        };

        rdoText.CheckedChanged += (s, e) =>
        {
            pnlTextConfig.Visible = rdoText.Checked;
            if (rdoText.Checked)
            {
                btnStartAnalysis.Text = "📝 텍스트 분석 시작";
                txtInput.Visible = true;
                txtOutput.Location = new Point(20, 320);
                txtOutput.Size = new Size(700, 270);
                btnStartAnalysis.Enabled = true;
            }
        };

        string selectedFilePath = "";
        btnSelectAudio.Click += (s, e) =>
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "Audio Files (*.mp3;*.m4a;*.wav;*.mp4)|*.mp3;*.m4a;*.wav;*.mp4",
                Title = "음성 파일 선택"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                selectedFilePath = ofd.FileName;
                lblSelectedFile.Text = $"선택된 파일: {System.IO.Path.GetFileName(selectedFilePath)}";
                btnStartAnalysis.Enabled = true;
            }
        };

        btnStartAnalysis.Click += async (s, e) =>
        {
            var config = AiConfiguration.Load();
            if (!config.IsReady || !config.IsSttReady)
            {
                MessageBox.Show("API Key가 설정되지 않았습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (rdoText.Checked && string.IsNullOrWhiteSpace(txtInput.Text))
            {
                MessageBox.Show("텍스트를 입력해 주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnStartAnalysis.Enabled = false;
            rdoAudio.Enabled = false;
            rdoText.Enabled = false;
            if (rdoAudio.Checked) btnSelectAudio.Enabled = false;
            else txtInput.ReadOnly = true;

            lblStatus.Text = "분석 진행 중...";
            lblStatus.ForeColor = Color.Orange;
            txtOutput.Text = "분석을 시작합니다. 잠시만 기다려 주세요...\n";

            try
            {
                var aiService = new MeetingAiService(new System.Net.Http.HttpClient(), config);
                string transcript = "";
                string virtualAudioPath = selectedFilePath;

                if (rdoAudio.Checked)
                {
                    txtOutput.Text = "서버로 오디오 전송 중...\n";
                    transcript = await aiService.TranscribeAsync(selectedFilePath, System.Threading.CancellationToken.None);
                    txtOutput.Text += "\n[변환 완료]\n\n" + transcript;
                }
                else
                {
                    transcript = txtInput.Text;
                    txtOutput.Text += "\n[입력 텍스트 사용]\n\n" + transcript;
                    virtualAudioPath = "text_input_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                }
                
                lblStatus.Text = "분석 중: AI 요약 및 제목 생성...";
                var summary = await aiService.SummarizeAsync(transcript, System.Threading.CancellationToken.None);
                var title = await aiService.GenerateTitleAsync(summary, System.Threading.CancellationToken.None);
                txtOutput.Text = $"[{title}]\n\n[AI 요약]\n" + summary + "\n\n" + new string('-', 50) + "\n\n" + txtOutput.Text;
                
                var storage = new MeetingStorageService();
                var record = storage.Save(virtualAudioPath, transcript, summary, title);

                lblStatus.Text = "분석 중: 타임라인, 통계, 안건 및 기존 분석...";
                
                var swotRoleAi = new SwotRoleAiService();
                var swotTask = swotRoleAi.AnalyzeSwotAsync(summary);

                var timeline = aiService.ParseTimeline(transcript);
                var statsTask = aiService.AnalyzeSpeakerStatsAsync(transcript, System.Threading.CancellationToken.None);
                var agendaTask = aiService.SuggestNextAgendaAsync(summary, System.Threading.CancellationToken.None);

                await Task.WhenAll(swotTask, statsTask, agendaTask);
                
                string swotJson = swotTask.Result != null ? System.Text.Json.JsonSerializer.Serialize(swotTask.Result) : null;
                string roleJson = null; // 역할 분배는 이제 수동으로 진행됨
                string timelineJson = System.Text.Json.JsonSerializer.Serialize(timeline);
                string statsJson = statsTask.Result != null ? System.Text.Json.JsonSerializer.Serialize(statsTask.Result) : null;
                string agendaJson = agendaTask.Result != null ? System.Text.Json.JsonSerializer.Serialize(agendaTask.Result) : null;

                record = record with { SwotJson = swotJson, RolesJson = roleJson, TimelineJson = timelineJson, SpeakerStatsJson = statsJson, NextAgendaJson = agendaJson };
                storage.Update(record);

                lblStatus.Text = "모든 분석 및 자동 저장 완료!";
                lblStatus.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "오류 발생!";
                lblStatus.ForeColor = Color.Red;
                txtOutput.Text += $"\n[오류] {ex.Message}";
            }
            finally
            {
                rdoAudio.Enabled = true;
                rdoText.Enabled = true;
                if (rdoAudio.Checked) 
                {
                    btnSelectAudio.Enabled = true;
                    btnStartAnalysis.Enabled = true;
                }
                else 
                {
                    txtInput.ReadOnly = false;
                    btnStartAnalysis.Enabled = true;
                }
            }
        };

        viewStt.Controls.AddRange(new Control[] { lblTitle, pnlInputMode, pnlAudioConfig, pnlTextConfig, btnStartAnalysis, txtInput, txtOutput, lblStatus });
    }

    private ProjectData _projectData;
    private IntegratedMeetingStudio.Controls.KanbanBoardView _kanbanBoardView;

    private void SetupHistoryView()
    {
        viewHistory.Controls.Clear();

        var lblTitle = new Label
        {
            Text = "회의 히스토리",
            Font = new Font("맑은 고딕", 20f, FontStyle.Bold),
            ForeColor = C_TextDark,
            AutoSize = true,
            Location = new Point(20, 20)
        };

        var lstMeetings = new ListBox
        {
            Location = new Point(20, 70),
            Size = new Size(250, 400),
            Font = new Font("맑은 고딕", 10f),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
            BackColor = C_BgPanel,
            ForeColor = C_TextDark,
            BorderStyle = BorderStyle.FixedSingle,
            IntegralHeight = false,
            FormattingEnabled = true
        };
        
        var storageService = new MeetingStorageService();

        lstMeetings.Format += (s, e) =>
        {
            if (e.ListItem is MeetingRecordListItem item)
            {
                string displayTitle = item.Title;
                if (string.IsNullOrWhiteSpace(displayTitle))
                {
                    displayTitle = item.AudioPath.StartsWith("text_input_") ? "텍스트 분석 회의" : System.IO.Path.GetFileNameWithoutExtension(item.AudioPath);
                }
                e.Value = $"[{item.CreatedAt:yyyy-MM-dd HH:mm}] {displayTitle}";
            }
        };

        // Tab Buttons
        var pnlTabs = new FlowLayoutPanel
        {
            Location = new Point(290, 70),
            Size = new Size(750, 80),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            BackColor = Color.Transparent
        };

        var btnTabStt = new Button { Text = "STT 원문 확인", Size = new Size(130, 35), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
        var btnTabAiSummary = new Button { Text = "AI 요약/정리", Size = new Size(130, 35), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
        var btnTabSwot = new Button { Text = "SWOT 분석", Size = new Size(130, 35), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
        var btnTabRole = new Button { Text = "역할 분배", Size = new Size(130, 35), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
        var btnTabTimeline = new Button { Text = "타임라인", Size = new Size(130, 35), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
        var btnTabSpeakerStats = new Button { Text = "화자 통계/분석", Size = new Size(130, 35), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
        var btnTabNextAgenda = new Button { Text = "다음 안건 제안", Size = new Size(130, 35), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };

        void SetTabActive(Button activeBtn)
        {
            var tabs = new[] { btnTabStt, btnTabAiSummary, btnTabSwot, btnTabRole, btnTabTimeline, btnTabSpeakerStats, btnTabNextAgenda };
            foreach (var t in tabs)
            {
                t.BackColor = C_BgPanel;
                t.ForeColor = C_TextDark;
            }
            activeBtn.BackColor = Color.FromArgb(52, 152, 219);
            activeBtn.ForeColor = Color.White;
        }

        pnlTabs.Controls.AddRange(new Control[] { btnTabStt, btnTabAiSummary, btnTabSwot, btnTabRole, btnTabTimeline, btnTabSpeakerStats, btnTabNextAgenda });

        // Container for Tab Content
        var pnlDetailsContainer = new Panel
        {
            Location = new Point(290, 155),
            Size = new Size(750, 265),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = C_BgPanel,
            BorderStyle = BorderStyle.FixedSingle
        };

        var txtStt = new RichTextBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("맑은 고딕", 10f),
            BackColor = C_BgPanel,
            ForeColor = C_TextDark,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            Visible = true
        };

        var pnlSummaryView = new Panel { Dock = DockStyle.Fill, Visible = false, BackColor = C_BgPanel };
        var pnlSummaryTop = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = C_BgPanel };
        var btnRefreshSummary = new Button { Text = "🔄 AI 전체 다시 추출하기 (Host)", AutoSize = true, Location = new Point(10, 5), FlatStyle = FlatStyle.Flat, BackColor = Color.White };
        pnlSummaryTop.Controls.Add(btnRefreshSummary);

        var txtSummary = new RichTextBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("맑은 고딕", 10f),
            BackColor = C_BgPanel,
            ForeColor = C_TextDark,
            ReadOnly = true,
            BorderStyle = BorderStyle.None
        };
        pnlSummaryView.Controls.Add(txtSummary);
        pnlSummaryView.Controls.Add(pnlSummaryTop);
        txtSummary.BringToFront();

        var pnlSwotView = new Panel { Dock = DockStyle.Fill, Visible = false, AutoScroll = true, BackColor = C_BgPanel };
        var pnlRoleView = new Panel { Dock = DockStyle.Fill, Visible = false, AutoScroll = true, BackColor = C_BgPanel };
        var pnlTimelineView = new Panel { Dock = DockStyle.Fill, Visible = false, AutoScroll = true, BackColor = C_BgPanel };
        var pnlSpeakerStatsView = new Panel { Dock = DockStyle.Fill, Visible = false, AutoScroll = true, BackColor = C_BgPanel };
        var pnlNextAgendaView = new Panel { Dock = DockStyle.Fill, Visible = false, AutoScroll = true, BackColor = C_BgPanel };

        pnlDetailsContainer.Controls.AddRange(new Control[] { txtStt, pnlSummaryView, pnlSwotView, pnlRoleView, pnlTimelineView, pnlSpeakerStatsView, pnlNextAgendaView });

        void HideAllViews()
        {
            txtStt.Visible = false;
            pnlSummaryView.Visible = false;
            pnlSwotView.Visible = false;
            pnlRoleView.Visible = false;
            pnlTimelineView.Visible = false;
            pnlSpeakerStatsView.Visible = false;
            pnlNextAgendaView.Visible = false;
        }

        btnTabStt.Click += (s, e) => { SetTabActive(btnTabStt); HideAllViews(); txtStt.Visible = true; };
        btnTabAiSummary.Click += (s, e) => { SetTabActive(btnTabAiSummary); HideAllViews(); pnlSummaryView.Visible = true; };
        
        btnRefreshSummary.Click += async (s, e) =>
        {
            if (lstMeetings.SelectedItem is MeetingRecordListItem item)
            {
                var fullRecord = storageService.GetRecord(item.Id);
                if (fullRecord != null && !string.IsNullOrEmpty(fullRecord.Transcript))
                {
                    btnRefreshSummary.Enabled = false;
                    btnRefreshSummary.Text = "⏳ 다시 추출 중...";
                    txtSummary.Text = "다시 요약하는 중...";
                    try
                    {
                        var aiConfig = AiConfiguration.Load();
                        var aiService = new MeetingAiService(new System.Net.Http.HttpClient(), aiConfig);
                        var summary = await aiService.SummarizeAsync(fullRecord.Transcript, System.Threading.CancellationToken.None);
                        
                        fullRecord = fullRecord with 
                        { 
                            Summary = summary,
                            SwotJson = null,
                            RolesJson = null,
                            TimelineJson = null,
                            SpeakerStatsJson = null,
                            NextAgendaJson = null
                        };
                        storageService.Update(fullRecord);
                        
                        txtSummary.Text = summary;
                        pnlSwotView.Controls.Clear();
                        pnlRoleView.Controls.Clear();
                        pnlTimelineView.Controls.Clear();
                        pnlSpeakerStatsView.Controls.Clear();
                        pnlNextAgendaView.Controls.Clear();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("다시 추출 실패: " + ex.Message);
                        txtSummary.Text = fullRecord.Summary; // revert
                    }
                    finally
                    {
                        btnRefreshSummary.Enabled = true;
                        btnRefreshSummary.Text = "🔄 AI 전체 다시 추출하기 (Host)";
                    }
                }
            }
        };
        SetTabActive(btnTabStt);

        // Action Buttons at the bottom
        var btnExtractTask = new Button
        {
            Text = "🤖 AI 업무 자동 추출 및 칸반 보드 등록",
            Location = new Point(290, 430),
            Size = new Size(350, 30),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            BackColor = Color.FromArgb(52, 73, 94),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("맑은 고딕", 9f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnExtractTask.FlatAppearance.BorderSize = 0;

        var btnQuiz = new Button
        {
            Text = "🤔 리마인드 퀴즈",
            Location = new Point(650, 430),
            Size = new Size(150, 30),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("맑은 고딕", 9f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnQuiz.FlatAppearance.BorderSize = 0;

        var btnRefresh = new Button
        {
            Text = "🔄 새로고침",
            Location = new Point(20, 480),
            Size = new Size(250, 30),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };

        Action loadList = () =>
        {
            lstMeetings.Items.Clear();
            var list = storageService.GetRecordList();
            foreach (var item in list)
            {
                lstMeetings.Items.Add(item);
            }
            if (lstMeetings.Items.Count > 0)
                lstMeetings.SelectedIndex = 0;
        };

        btnRefresh.Click += (s, e) => loadList();

        lstMeetings.SelectedIndexChanged += (s, e) =>
        {
            if (lstMeetings.SelectedItem is MeetingRecordListItem item)
            {
                var fullRecord = storageService.GetRecord(item.Id);
                if (fullRecord != null)
                {
                    txtStt.Text = fullRecord.Transcript;
                    txtSummary.Text = fullRecord.Summary;
                    pnlSwotView.Controls.Clear();
                    pnlRoleView.Controls.Clear();
                    SetTabActive(btnTabStt);
                    HideAllViews();
                    txtStt.Visible = true;
                }
            }
        };

        btnTabSwot.Click += async (s, e) =>
        {
            SetTabActive(btnTabSwot);
            HideAllViews();
            pnlSwotView.Visible = true;

            if (pnlSwotView.Controls.Count > 0) return; // Already loaded

            if (lstMeetings.SelectedItem is MeetingRecordListItem item)
            {
                var fullRecord = storageService.GetRecord(item.Id);
                if (fullRecord != null && !string.IsNullOrEmpty(fullRecord.Summary))
                {
                    btnTabSwot.Enabled = false;
                    var lblLoading = new Label { Text = "SWOT 분석 생성 중...", AutoSize = true, Location = new Point(20, 20) };
                    pnlSwotView.Controls.Add(lblLoading);
                    try
                    {
                        SwotAnalysisResult swotResult = null;
                        if (!string.IsNullOrEmpty(fullRecord.SwotJson))
                        {
                            try { swotResult = System.Text.Json.JsonSerializer.Deserialize<SwotAnalysisResult>(fullRecord.SwotJson); } catch { }
                        }
                        if (swotResult == null)
                        {
                            var ai = new SwotRoleAiService();
                            swotResult = await ai.AnalyzeSwotAsync(fullRecord.Summary);
                            if (swotResult != null)
                            {
                                fullRecord = fullRecord with { SwotJson = System.Text.Json.JsonSerializer.Serialize(swotResult) };
                                storageService.Update(fullRecord);
                            }
                        }
                        pnlSwotView.Controls.Clear();
                        if (swotResult != null)
                        {
                            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = C_BgPanel };
                            var btnRefreshUtil = new Button { Text = "🔄 다시 추출하기 (Util)", AutoSize = true, Location = new Point(10, 5), FlatStyle = FlatStyle.Flat, BackColor = Color.White };
                            btnRefreshUtil.Click += (s2, e2) =>
                            {
                                fullRecord = fullRecord with { SwotJson = null };
                                storageService.Update(fullRecord);
                                pnlSwotView.Controls.Clear();
                                btnTabSwot.PerformClick();
                            };
                            pnlTop.Controls.Add(btnRefreshUtil);
                            pnlSwotView.Controls.Add(pnlTop);

                            var contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = C_BgPanel };
                            pnlSwotView.Controls.Add(contentPanel);
                            contentPanel.BringToFront();

                            RenderSwotGrid(contentPanel, swotResult);
                        }
                        else
                        {
                            pnlSwotView.Controls.Add(new Label { Text = "분석 실패", AutoSize = true, Location = new Point(20, 20) });
                        }
                    }
                    catch (Exception ex)
                    {
                        pnlSwotView.Controls.Clear();
                        pnlSwotView.Controls.Add(new Label { Text = $"오류 발생: {ex.Message}", AutoSize = true, Location = new Point(20, 20) });
                    }
                    finally
                    {
                        btnTabSwot.Enabled = true;
                    }
                }
            }
        };

        btnTabRole.Click += (s, e) =>
        {
            SetTabActive(btnTabRole);
            HideAllViews();
            pnlRoleView.Visible = true;

            if (pnlRoleView.Controls.Count > 0) return;

            if (lstMeetings.SelectedItem is MeetingRecordListItem item)
            {
                var fullRecord = storageService.GetRecord(item.Id);
                if (fullRecord != null && !string.IsNullOrEmpty(fullRecord.Summary))
                {
                    List<IntegratedMeetingStudio.Models.RoleDistributionResult> roleResult = null;
                    if (!string.IsNullOrEmpty(fullRecord.RolesJson))
                    {
                        try { roleResult = System.Text.Json.JsonSerializer.Deserialize<List<IntegratedMeetingStudio.Models.RoleDistributionResult>>(fullRecord.RolesJson); } catch { }
                    }

                    pnlRoleView.Controls.Clear();
                    if (roleResult != null && roleResult.Count > 0)
                    {
                        RenderRoleList(pnlRoleView, roleResult);
                        var btnReDistribute = new Button
                        {
                            Text = "🔄 다시 추출하기 (Util)",
                            Location = new Point(pnlRoleView.Width - 190, 10),
                            Size = new Size(170, 30),
                            BackColor = Color.LightGray,
                            FlatStyle = FlatStyle.Flat
                        };
                        btnReDistribute.Click += (s2, e2) => RenderParticipantSetupUI(pnlRoleView, fullRecord);
                        pnlRoleView.Controls.Add(btnReDistribute);
                        btnReDistribute.BringToFront();
                    }
                    else
                    {
                        RenderParticipantSetupUI(pnlRoleView, fullRecord);
                    }
                }
            }
        };

        btnTabTimeline.Click += (s, e) =>
        {
            SetTabActive(btnTabTimeline);
            HideAllViews();
            pnlTimelineView.Visible = true;

            if (pnlTimelineView.Controls.Count > 0) return;

            if (lstMeetings.SelectedItem is MeetingRecordListItem item)
            {
                var fullRecord = storageService.GetRecord(item.Id);
                if (fullRecord != null)
                {
                    List<IntegratedMeetingStudio.Models.TimelineItem> timeline = null;
                    if (!string.IsNullOrEmpty(fullRecord.TimelineJson))
                    {
                        try { timeline = System.Text.Json.JsonSerializer.Deserialize<List<IntegratedMeetingStudio.Models.TimelineItem>>(fullRecord.TimelineJson); } catch { }
                    }
                    
                    if (timeline == null)
                    {
                        var aiConfig = AiConfiguration.Load();
                        var aiService = new MeetingAiService(new System.Net.Http.HttpClient(), aiConfig);
                        timeline = aiService.ParseTimeline(fullRecord.Transcript);
                        if (timeline != null)
                        {
                            fullRecord = fullRecord with { TimelineJson = System.Text.Json.JsonSerializer.Serialize(timeline) };
                            storageService.Update(fullRecord);
                        }
                    }

                    pnlTimelineView.Controls.Clear();
                    if (timeline != null && timeline.Count > 0)
                    {
                        var pnlTop = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = C_BgPanel };
                        var btnRefreshUtil = new Button { Text = "🔄 다시 추출하기 (Util)", AutoSize = true, Location = new Point(10, 5), FlatStyle = FlatStyle.Flat, BackColor = Color.White };
                        btnRefreshUtil.Click += (s2, e2) =>
                        {
                            fullRecord = fullRecord with { TimelineJson = null };
                            storageService.Update(fullRecord);
                            pnlTimelineView.Controls.Clear();
                            btnTabTimeline.PerformClick();
                        };
                        pnlTop.Controls.Add(btnRefreshUtil);
                        pnlTimelineView.Controls.Add(pnlTop);

                        var contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = C_BgPanel };
                        pnlTimelineView.Controls.Add(contentPanel);
                        contentPanel.BringToFront();

                        RenderTimeline(contentPanel, timeline);
                    }
                    else
                    {
                        pnlTimelineView.Controls.Add(new Label { Text = "타임라인 정보가 없습니다.", AutoSize = true, Location = new Point(20, 20) });
                    }
                }
            }
        };

        btnTabSpeakerStats.Click += async (s, e) =>
        {
            SetTabActive(btnTabSpeakerStats);
            HideAllViews();
            pnlSpeakerStatsView.Visible = true;

            if (pnlSpeakerStatsView.Controls.Count > 0) return;

            if (lstMeetings.SelectedItem is MeetingRecordListItem item)
            {
                var fullRecord = storageService.GetRecord(item.Id);
                if (fullRecord != null)
                {
                    btnTabSpeakerStats.Enabled = false;
                    var lblLoading = new Label { Text = "화자 통계 및 분석 중...", AutoSize = true, Location = new Point(20, 20) };
                    pnlSpeakerStatsView.Controls.Add(lblLoading);

                    try
                    {
                        List<IntegratedMeetingStudio.Models.SpeakerStat> stats = null;
                        if (!string.IsNullOrEmpty(fullRecord.SpeakerStatsJson))
                        {
                            try { stats = System.Text.Json.JsonSerializer.Deserialize<List<IntegratedMeetingStudio.Models.SpeakerStat>>(fullRecord.SpeakerStatsJson); } catch { }
                        }
                        
                        if (stats == null)
                        {
                            var aiConfig = AiConfiguration.Load();
                            var aiService = new MeetingAiService(new System.Net.Http.HttpClient(), aiConfig);
                            stats = await aiService.AnalyzeSpeakerStatsAsync(fullRecord.Transcript, System.Threading.CancellationToken.None);
                            if (stats != null)
                            {
                                fullRecord = fullRecord with { SpeakerStatsJson = System.Text.Json.JsonSerializer.Serialize(stats) };
                                storageService.Update(fullRecord);
                            }
                        }

                        pnlSpeakerStatsView.Controls.Clear();
                        if (stats != null && stats.Count > 0)
                        {
                            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = C_BgPanel };
                            var btnRefreshUtil = new Button { Text = "🔄 다시 추출하기 (Util)", AutoSize = true, Location = new Point(10, 5), FlatStyle = FlatStyle.Flat, BackColor = Color.White };
                            btnRefreshUtil.Click += (s2, e2) =>
                            {
                                fullRecord = fullRecord with { SpeakerStatsJson = null };
                                storageService.Update(fullRecord);
                                pnlSpeakerStatsView.Controls.Clear();
                                btnTabSpeakerStats.PerformClick();
                            };
                            pnlTop.Controls.Add(btnRefreshUtil);
                            pnlSpeakerStatsView.Controls.Add(pnlTop);

                            var contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = C_BgPanel };
                            pnlSpeakerStatsView.Controls.Add(contentPanel);
                            contentPanel.BringToFront();

                            RenderSpeakerStats(contentPanel, stats);
                        }
                        else
                        {
                            pnlSpeakerStatsView.Controls.Add(new Label { Text = "화자 통계 정보가 없습니다.", AutoSize = true, Location = new Point(20, 20) });
                        }
                    }
                    catch (Exception ex)
                    {
                        pnlSpeakerStatsView.Controls.Clear();
                        pnlSpeakerStatsView.Controls.Add(new Label { Text = $"오류 발생: {ex.Message}", AutoSize = true, Location = new Point(20, 20) });
                    }
                    finally
                    {
                        btnTabSpeakerStats.Enabled = true;
                    }
                }
            }
        };

        btnTabNextAgenda.Click += async (s, e) =>
        {
            SetTabActive(btnTabNextAgenda);
            HideAllViews();
            pnlNextAgendaView.Visible = true;

            if (pnlNextAgendaView.Controls.Count > 0) return;

            if (lstMeetings.SelectedItem is MeetingRecordListItem item)
            {
                var fullRecord = storageService.GetRecord(item.Id);
                if (fullRecord != null)
                {
                    btnTabNextAgenda.Enabled = false;
                    var lblLoading = new Label { Text = "다음 안건 분석 중...", AutoSize = true, Location = new Point(20, 20) };
                    pnlNextAgendaView.Controls.Add(lblLoading);

                    try
                    {
                        List<IntegratedMeetingStudio.Models.NextAgendaItem> agenda = null;
                        if (!string.IsNullOrEmpty(fullRecord.NextAgendaJson))
                        {
                            try { agenda = System.Text.Json.JsonSerializer.Deserialize<List<IntegratedMeetingStudio.Models.NextAgendaItem>>(fullRecord.NextAgendaJson); } catch { }
                        }
                        
                        if (agenda == null)
                        {
                            var aiConfig = AiConfiguration.Load();
                            var aiService = new MeetingAiService(new System.Net.Http.HttpClient(), aiConfig);
                            agenda = await aiService.SuggestNextAgendaAsync(fullRecord.Summary, System.Threading.CancellationToken.None);
                            if (agenda != null)
                            {
                                fullRecord = fullRecord with { NextAgendaJson = System.Text.Json.JsonSerializer.Serialize(agenda) };
                                storageService.Update(fullRecord);
                            }
                        }

                        pnlNextAgendaView.Controls.Clear();
                        if (agenda != null && agenda.Count > 0)
                        {
                            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = C_BgPanel };
                            var btnRefreshUtil = new Button { Text = "🔄 다시 추출하기 (Util)", AutoSize = true, Location = new Point(10, 5), FlatStyle = FlatStyle.Flat, BackColor = Color.White };
                            btnRefreshUtil.Click += (s2, e2) =>
                            {
                                fullRecord = fullRecord with { NextAgendaJson = null };
                                storageService.Update(fullRecord);
                                pnlNextAgendaView.Controls.Clear();
                                btnTabNextAgenda.PerformClick();
                            };
                            pnlTop.Controls.Add(btnRefreshUtil);
                            pnlNextAgendaView.Controls.Add(pnlTop);

                            var contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = C_BgPanel };
                            pnlNextAgendaView.Controls.Add(contentPanel);
                            contentPanel.BringToFront();

                            RenderNextAgenda(contentPanel, agenda);
                        }
                        else
                        {
                            pnlNextAgendaView.Controls.Add(new Label { Text = "추천 안건 정보가 없습니다.", AutoSize = true, Location = new Point(20, 20) });
                        }
                    }
                    catch (Exception ex)
                    {
                        pnlNextAgendaView.Controls.Clear();
                        pnlNextAgendaView.Controls.Add(new Label { Text = $"오류 발생: {ex.Message}", AutoSize = true, Location = new Point(20, 20) });
                    }
                    finally
                    {
                        btnTabNextAgenda.Enabled = true;
                    }
                }
            }
        };

        btnExtractTask.Click += async (s, e) =>
        {
            if (lstMeetings.SelectedItem is MeetingRecordListItem item)
            {
                var fullRecord = storageService.GetRecord(item.Id);
                if (fullRecord != null && !string.IsNullOrEmpty(fullRecord.Summary))
                {
                    btnExtractTask.Enabled = false;
                    string previousText = btnExtractTask.Text;
                    btnExtractTask.Text = "추출 및 등록 중...";
                    try
                    {
                        var ai = new SwotRoleAiService();
                        var tasks = await ai.ExtractTasksAsync(fullRecord.Summary, fullRecord.RolesJson);
                        
                        if (tasks != null && tasks.Count > 0)
                        {
                            foreach (var task in tasks)
                            {
                                task.Status = "Not Started";
                                _projectData.Tasks.Add(task);
                            }
                            DataManager.SaveData(_projectData);
                            _kanbanBoardView.RefreshBoard();
                            MessageBox.Show($"{tasks.Count}개의 업무가 칸반 보드(To-Do)에 등록되었습니다!", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("추출된 업무가 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"업무 추출 실패: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        btnExtractTask.Text = previousText;
                        btnExtractTask.Enabled = true;
                    }
                }
            }
        };

        btnQuiz.Click += async (s, e) =>
        {
            if (lstMeetings.SelectedItem is MeetingRecordListItem item)
            {
                var fullRecord = storageService.GetRecord(item.Id);
                if (fullRecord != null && !string.IsNullOrEmpty(fullRecord.Summary))
                {
                    btnQuiz.Enabled = false;
                    string previousText = btnQuiz.Text;
                    btnQuiz.Text = "생성 중...";
                    try
                    {
                        var ai = new SwotRoleAiService();
                        var quizzes = await ai.GenerateQuizAsync(fullRecord.Summary);
                        
                        if (quizzes != null && quizzes.Count > 0)
                        {
                            using (var form = new Form
                            {
                                Text = "AI 리마인드 퀴즈",
                                Size = new Size(600, 500),
                                StartPosition = FormStartPosition.CenterParent,
                                BackColor = Color.White
                            })
                            {
                                FlowLayoutPanel flowPanel = new FlowLayoutPanel
                                {
                                    Dock = DockStyle.Fill,
                                    FlowDirection = FlowDirection.TopDown,
                                    AutoScroll = true,
                                    Padding = new Padding(20),
                                    WrapContents = false
                                };

                                foreach (var quiz in quizzes)
                                {
                                    var panel = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, Width = 520, Padding = new Padding(0, 0, 0, 20), WrapContents = false };
                                    var lblQ = new Label { Text = "Q: " + quiz.Question, AutoSize = true, Font = new Font("맑은 고딕", 12f, FontStyle.Bold), MaximumSize = new Size(500, 0) };
                                    var btnToggle = new Button { Text = "정답 확인", Size = new Size(100, 30), BackColor = Color.LightGray, FlatStyle = FlatStyle.Flat };
                                    var lblA = new Label { Text = "A: " + quiz.Answer, AutoSize = true, Font = new Font("맑은 고딕", 10f, FontStyle.Bold), ForeColor = Color.Blue, Visible = false, MaximumSize = new Size(500, 0), Margin = new Padding(0, 10, 0, 0) };
                                    var lblE = new Label { Text = "해설: " + quiz.Explanation, AutoSize = true, Font = new Font("맑은 고딕", 10f), ForeColor = Color.DimGray, Visible = false, MaximumSize = new Size(500, 0) };
                                    
                                    btnToggle.FlatAppearance.BorderSize = 0;
                                    btnToggle.Click += (s2, e2) => { lblA.Visible = !lblA.Visible; lblE.Visible = !lblE.Visible; };
                                    
                                    panel.Controls.AddRange(new Control[] { lblQ, btnToggle, lblA, lblE });
                                    flowPanel.Controls.Add(panel);
                                }
                                form.Controls.Add(flowPanel);
                                form.ShowDialog(this);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"퀴즈 생성 실패: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        btnQuiz.Text = previousText;
                        btnQuiz.Enabled = true;
                    }
                }
            }
        };

        viewHistory.Controls.AddRange(new Control[] { lblTitle, lstMeetings, pnlTabs, pnlDetailsContainer, btnExtractTask, btnQuiz, btnRefresh });
        
        loadList();
    }

    private void RenderSwotGrid(Panel container, IntegratedMeetingStudio.Models.SwotAnalysisResult swot)
    {
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(10)
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        Control CreateSwotCell(string title, List<string> items)
        {
            var pnl = new Panel { Dock = DockStyle.Fill, Margin = new Padding(5), Padding = new Padding(10), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            var lbl = new Label { Text = title, Dock = DockStyle.Top, Font = new Font("맑은 고딕", 12f, FontStyle.Bold), AutoSize = true, Padding = new Padding(0, 0, 0, 10) };
            var txt = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Font = new Font("맑은 고딕", 10f), BackColor = Color.White, BorderStyle = BorderStyle.None };
            txt.Text = "• " + string.Join("\r\n• ", items);
            pnl.Controls.Add(txt);
            pnl.Controls.Add(lbl);
            return pnl;
        }

        grid.Controls.Add(CreateSwotCell("Strengths (강점)", swot.Strengths), 0, 0);
        grid.Controls.Add(CreateSwotCell("Weaknesses (약점)", swot.Weaknesses), 1, 0);
        grid.Controls.Add(CreateSwotCell("Opportunities (기회)", swot.Opportunities), 0, 1);
        grid.Controls.Add(CreateSwotCell("Threats (위협)", swot.Threats), 1, 1);

        container.Controls.Add(grid);
    }

    private void RenderParticipantSetupUI(Panel container, MeetingRecord fullRecord)
    {
        container.Controls.Clear();
        
        var lblTitle = new Label { Text = "역할 분배 참여자 설정", Font = new Font("맑은 고딕", 12f, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
        container.Controls.Add(lblTitle);

        var lblDesc = new Label { Text = "역할을 분배받을 참여자를 추가하거나 삭제하세요.", AutoSize = true, Location = new Point(20, 50) };
        container.Controls.Add(lblDesc);

        var txtNewParticipant = new TextBox { Location = new Point(20, 80), Width = 150 };
        container.Controls.Add(txtNewParticipant);

        var btnAdd = new Button { Text = "추가", Location = new Point(180, 78), Width = 60, BackColor = Color.LightGray, FlatStyle = FlatStyle.Flat };
        container.Controls.Add(btnAdd);

        var flowParticipants = new FlowLayoutPanel
        {
            Location = new Point(20, 120),
            Width = container.Width - 40,
            Height = 150,
            AutoScroll = true,
            BorderStyle = BorderStyle.FixedSingle
        };
        container.Controls.Add(flowParticipants);

        var participantsList = new List<string>();

        if (!string.IsNullOrEmpty(fullRecord.SpeakerStatsJson))
        {
            try
            {
                var stats = System.Text.Json.JsonSerializer.Deserialize<List<IntegratedMeetingStudio.Models.SpeakerStat>>(fullRecord.SpeakerStatsJson);
                if (stats != null)
                {
                    foreach (var stat in stats)
                    {
                        if (!participantsList.Contains(stat.Speaker))
                            participantsList.Add(stat.Speaker);
                    }
                }
            }
            catch {}
        }

        if (participantsList.Count == 0)
        {
            participantsList.AddRange(new[] { "화자 1", "화자 2", "화자 3" });
        }

        void RefreshParticipantTags()
        {
            flowParticipants.Controls.Clear();
            foreach (var p in participantsList)
            {
                var pnlTag = new Panel { Width = 100, Height = 30, BackColor = Color.LightSkyBlue, Margin = new Padding(5) };
                var lblName = new Label { Text = p, AutoSize = true, Location = new Point(5, 7) };
                var btnDel = new Button { Text = "X", Location = new Point(75, 3), Width = 20, Height = 20, FlatStyle = FlatStyle.Flat, BackColor = Color.White };
                btnDel.FlatAppearance.BorderSize = 0;
                
                string currentP = p;
                btnDel.Click += (s, e) => {
                    participantsList.Remove(currentP);
                    RefreshParticipantTags();
                };

                pnlTag.Controls.Add(lblName);
                pnlTag.Controls.Add(btnDel);
                flowParticipants.Controls.Add(pnlTag);
            }
        }

        RefreshParticipantTags();

        btnAdd.Click += (s, e) => {
            if (!string.IsNullOrWhiteSpace(txtNewParticipant.Text) && !participantsList.Contains(txtNewParticipant.Text.Trim()))
            {
                participantsList.Add(txtNewParticipant.Text.Trim());
                txtNewParticipant.Text = "";
                RefreshParticipantTags();
            }
        };

        var btnDistribute = new Button
        {
            Text = "🚀 이 참여자들로 역할 분배하기",
            Font = new Font("맑은 고딕", 11f, FontStyle.Bold),
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Location = new Point(20, 290),
            Size = new Size(250, 45),
            Cursor = Cursors.Hand
        };
        container.Controls.Add(btnDistribute);

        btnDistribute.Click += async (s, e) => {
            if (participantsList.Count == 0)
            {
                MessageBox.Show("최소 1명 이상의 참여자가 필요합니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnDistribute.Enabled = false;
            btnDistribute.Text = "역할 분배 진행 중...";
            
            try
            {
                var ai = new SwotRoleAiService();
                var roleResult = await ai.DistributeRolesAsync(fullRecord.Summary, participantsList);
                if (roleResult != null && roleResult.Count > 0)
                {
                    var updatedRecord = fullRecord with { RolesJson = System.Text.Json.JsonSerializer.Serialize(roleResult) };
                    var storageService = new MeetingStorageService();
                    storageService.Update(updatedRecord);
                    
                    container.Controls.Clear();
                    RenderRoleList(container, roleResult);
                    
                    var btnReDistribute = new Button
                    {
                        Text = "🔄 다시 분배하기",
                        Location = new Point(container.Width - 140, 10),
                        Size = new Size(120, 30),
                        BackColor = Color.LightGray,
                        FlatStyle = FlatStyle.Flat
                    };
                    btnReDistribute.Click += (s2, e2) => RenderParticipantSetupUI(container, updatedRecord);
                    container.Controls.Add(btnReDistribute);
                    btnReDistribute.BringToFront();
                }
                else
                {
                    MessageBox.Show("역할 분배에 실패했습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnDistribute.Enabled = true;
                    btnDistribute.Text = "🚀 이 참여자들로 역할 분배하기";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnDistribute.Enabled = true;
                btnDistribute.Text = "🚀 이 참여자들로 역할 분배하기";
            }
        };
    }

    private void RenderRoleList(Panel container, List<IntegratedMeetingStudio.Models.RoleDistributionResult> roles)
    {
        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            AutoScroll = true,
            Padding = new Padding(10),
            WrapContents = false
        };

        var title = new Label { Text = "지정형 역할 분배", Font = new Font("맑은 고딕", 16f, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 0, 0, 20) };
        flow.Controls.Add(title);

        foreach (var role in roles)
        {
            var row = new Panel { Width = container.Width - 40, Height = 100, Margin = new Padding(0, 0, 0, 10) };
            var lblPart = new Label { Text = role.Participant, Font = new Font("맑은 고딕", 10f, FontStyle.Bold), Location = new Point(10, 10), Size = new Size(140, 80), AutoSize = false, TextAlign = ContentAlignment.TopLeft };
            var txtRoles = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, Font = new Font("맑은 고딕", 9f), Location = new Point(160, 10), Size = new Size(row.Width - 290, 80) };
            if (role.Roles.Count > 0)
                txtRoles.Text = "- " + string.Join("\r\n- ", role.Roles);
            
            var btnEdit = new Button { Text = "수정", Location = new Point(row.Width - 110, 10), Size = new Size(80, 30), FlatStyle = FlatStyle.Flat, BackColor = Color.White };
            btnEdit.Click += (s, e) =>
            {
                if (btnEdit.Text == "수정")
                {
                    txtRoles.ReadOnly = false;
                    txtRoles.BackColor = Color.LightYellow;
                    btnEdit.Text = "저장";
                }
                else
                {
                    txtRoles.ReadOnly = true;
                    txtRoles.BackColor = Color.White;
                    btnEdit.Text = "수정";
                }
            };
            txtRoles.ReadOnly = true;

            row.Controls.Add(lblPart);
            row.Controls.Add(txtRoles);
            row.Controls.Add(btnEdit);
            flow.Controls.Add(row);
        }

        container.Controls.Add(flow);
    }

    private void RenderTimeline(Panel container, List<IntegratedMeetingStudio.Models.TimelineItem> timeline)
    {
        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            AutoScroll = true,
            Padding = new Padding(10),
            WrapContents = false
        };

        var title = new Label { Text = "회의 타임라인", Font = new Font("맑은 고딕", 16f, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 0, 0, 20) };
        flow.Controls.Add(title);

        var speakerColors = new Dictionary<string, Color>();
        var baseColors = new[] { Color.CornflowerBlue, Color.IndianRed, Color.MediumSeaGreen, Color.MediumPurple, Color.DarkOrange, Color.Teal };
        int colorIdx = 0;

        foreach (var item in timeline)
        {
            if (!speakerColors.ContainsKey(item.Speaker))
            {
                speakerColors[item.Speaker] = baseColors[colorIdx % baseColors.Length];
                colorIdx++;
            }

            var color = speakerColors[item.Speaker];
            bool hasTime = !string.IsNullOrEmpty(item.Timestamp);
            int w = container.Width - 40;
            int contentWidth = w - 150;

            var contentFont = new Font("맑은 고딕", 9.5f);
            var textSize = TextRenderer.MeasureText(
                item.Text, contentFont,
                new Size(contentWidth, int.MaxValue),
                TextFormatFlags.WordBreak);

            int contentH = Math.Max(20, textSize.Height);
            int leftColH = 10 + 16 + (hasTime ? 16 : 0) + 10;
            int h = Math.Max(60, Math.Max(leftColH, 10 + contentH + 10));

            var card = new Panel { Width = w, Height = h, BackColor = Color.White, Margin = new Padding(0, 0, 0, 0) };
            card.Paint += (_, e) =>
            {
                using var b = new SolidBrush(color); e.Graphics.FillRectangle(b, 0, 0, 5, card.Height);
                using var p = new Pen(Color.FromArgb(230, 230, 230)); e.Graphics.DrawLine(p, 0, h - 1, w, h - 1);
            };

            int nameY = 10;
            int timeY = nameY + 18;

            card.Controls.Add(new Label
            {
                Text = item.Speaker,
                Font = new Font("맑은 고딕", 9.5f, FontStyle.Bold),
                ForeColor = color, AutoSize = true,
                Location = new Point(16, nameY)
            });
            card.Controls.Add(new Label
            {
                Text = "🕐 " + item.Timestamp,
                Font = new Font("맑은 고딕", 8.5f),
                ForeColor = Color.Gray, AutoSize = true,
                Location = new Point(16, timeY), Visible = hasTime
            });
            card.Controls.Add(new Label
            {
                Text = item.Text,
                Font = contentFont, ForeColor = Color.FromArgb(50, 50, 50),
                MaximumSize = new Size(contentWidth, 0),
                AutoSize = true,
                Location = new Point(140, nameY + 8)
            });
            
            flow.Controls.Add(card);
        }

        container.Controls.Add(flow);
    }

    private void RenderSpeakerStats(Panel container, List<IntegratedMeetingStudio.Models.SpeakerStat> stats)
    {
        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            AutoScroll = true,
            Padding = new Padding(10),
            WrapContents = false
        };

        var title = new Label { Text = "참여자 통계 및 분석", Font = new Font("맑은 고딕", 16f, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 0, 0, 20) };
        flow.Controls.Add(title);

        // Simple Chart (Mock using Panels or MSChart if available. For simplicity, we use custom drawing or panels for bar chart)
        var pnlChart = new Panel { Width = container.Width - 40, Height = 200, Margin = new Padding(0, 0, 0, 20), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        
        int yOffset = 20;
        foreach (var stat in stats)
        {
            var lblName = new Label { Text = stat.Speaker, AutoSize = true, Location = new Point(10, yOffset + 5) };
            
            // Limit ratio
            double ratio = stat.SpeechRatio > 1.0 ? stat.SpeechRatio / 100.0 : stat.SpeechRatio;
            int barWidth = (int)((pnlChart.Width - 150) * ratio);
            
            var pnlBar = new Panel { Location = new Point(100, yOffset), Size = new Size(barWidth, 25), BackColor = Color.FromArgb(52, 152, 219) };
            var lblValue = new Label { Text = $"{stat.SpeechCount}회 ({(ratio*100):0.0}%)", AutoSize = true, Location = new Point(100 + barWidth + 10, yOffset + 5) };

            pnlChart.Controls.Add(lblName);
            pnlChart.Controls.Add(pnlBar);
            pnlChart.Controls.Add(lblValue);
            yOffset += 40;
        }
        flow.Controls.Add(pnlChart);

        foreach (var stat in stats)
        {
            var card = new Panel { Width = container.Width - 40, Height = 100, Margin = new Padding(0, 0, 0, 10), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            var lblPart = new Label { Text = stat.Speaker, Font = new Font("맑은 고딕", 11f, FontStyle.Bold), Location = new Point(10, 10), AutoSize = true };
            var txtAnalysis = new TextBox { Text = stat.AiAnalysis, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Font = new Font("맑은 고딕", 9f), Location = new Point(10, 40), Size = new Size(card.Width - 20, 50), BorderStyle = BorderStyle.None, BackColor = Color.White };

            card.Controls.Add(lblPart);
            card.Controls.Add(txtAnalysis);
            flow.Controls.Add(card);
        }

        container.Controls.Add(flow);
    }

    private void RenderNextAgenda(Panel container, List<IntegratedMeetingStudio.Models.NextAgendaItem> agenda)
    {
        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            AutoScroll = true,
            Padding = new Padding(10),
            WrapContents = false
        };

        var title = new Label { Text = "추천 안건 (다음 회의 제안)", Font = new Font("맑은 고딕", 16f, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 0, 0, 20) };
        flow.Controls.Add(title);

        foreach (var item in agenda)
        {
            var card = new Panel { Width = container.Width - 40, AutoSize = true, Margin = new Padding(0, 0, 0, 10), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10) };
            
            var lblTitle = new Label { Text = "📌 " + item.Title, Font = new Font("맑은 고딕", 11f, FontStyle.Bold), AutoSize = true, Location = new Point(10, 10) };
            var lblDesc = new Label { Text = item.Description, Font = new Font("맑은 고딕", 9f), AutoSize = true, Location = new Point(25, 35), MaximumSize = new Size(card.Width - 40, 0) };

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblDesc);
            flow.Controls.Add(card);
        }

        container.Controls.Add(flow);
    }


    private void SetupKanbanView()
    {
        viewKanban.Controls.Clear();
        
        _projectData = DataManager.LoadData();
        
        _kanbanBoardView = new IntegratedMeetingStudio.Controls.KanbanBoardView();
        _kanbanBoardView.Dock = DockStyle.Fill;
        _kanbanBoardView.SetData(_projectData);
        
        viewKanban.Controls.Add(_kanbanBoardView);
    }

    private Panel CreateViewPanel(string title)
    {
        var pnl = new Panel { Dock = DockStyle.Fill, Visible = false };
        var lbl = new Label
        {
            Text = title,
            Font = new Font("맑은 고딕", 20f, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 20)
        };
        pnl.Controls.Add(lbl);
        return pnl;
    }

    private void ShowView(Panel view)
    {
        viewHome.Visible = false;
        viewStt.Visible = false;
        viewHistory.Visible = false;
        viewKanban.Visible = false;
        viewSettings.Visible = false;

        if (view != null)
        {
            view.Visible = true;
            view.BringToFront();
        }
    }

    private void ToggleDarkMode()
    {
        _darkMode = !_darkMode;
        btnDarkMode.Text = _darkMode ? "☀️ 라이트 모드" : "🌙 다크 모드";
        EnvManager.Set("UI_THEME", _darkMode ? "Dark" : "Light");
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        this.BackColor = C_BgMain;
        sidebarPanel.BackColor = C_BgSidebar;
        headerPanel.BackColor = C_BgPanel;
        btnDarkMode.BackColor = C_BgMain;
        btnDarkMode.ForeColor = C_TextDark;

        foreach (Control ctrl in mainPanel.Controls)
        {
            if (ctrl is Panel view)
            {
                view.BackColor = C_BgMain;
                ApplyThemeRecursive(view);
            }
        }
    }

    private void ApplyThemeRecursive(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            if (child is Label lbl) 
            {
                lbl.ForeColor = C_TextDark;
            }
            else if (child is TextBox txt)
            {
                txt.BackColor = C_BgPanel;
                txt.ForeColor = C_TextDark;
                txt.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (child is ComboBox cmb)
            {
                cmb.BackColor = C_BgPanel;
                cmb.ForeColor = C_TextDark;
                cmb.FlatStyle = FlatStyle.Flat;
            }
            else if (child is GroupBox gb)
            {
                gb.ForeColor = C_TextDark;
            }
            
            if (child.HasChildren)
            {
                ApplyThemeRecursive(child);
            }
        }
    }

    private void SetupSettingsView()
    {
        viewSettings.Controls.Clear();

        var lblTitle = new Label
        {
            Text = "⚙ 설정 및 API 연동",
            Font = new Font("맑은 고딕", 20f, FontStyle.Bold),
            ForeColor = C_TextDark,
            AutoSize = true,
            Location = new Point(20, 20)
        };
        viewSettings.Controls.Add(lblTitle);

        var pnlScroll = new Panel { Location = new Point(20, 70), Size = new Size(1000, 600), AutoScroll = true };
        viewSettings.Controls.Add(pnlScroll);

        int yPos = 10;

        // --- Theme Settings ---
        var grpTheme = new GroupBox { Text = "테마 설정", Location = new Point(0, yPos), Size = new Size(900, 80), Font = new Font("맑은 고딕", 11f, FontStyle.Bold) };
        var rdoLight = new RadioButton { Text = "☀️ 라이트 모드", Location = new Point(20, 35), AutoSize = true, Checked = !_darkMode, Cursor = Cursors.Hand };
        var rdoDark = new RadioButton { Text = "🌙 다크 모드", Location = new Point(180, 35), AutoSize = true, Checked = _darkMode, Cursor = Cursors.Hand };
        
        rdoLight.CheckedChanged += (s, e) => { if (rdoLight.Checked && _darkMode) ToggleDarkMode(); };
        rdoDark.CheckedChanged += (s, e) => { if (rdoDark.Checked && !_darkMode) ToggleDarkMode(); };

        grpTheme.Controls.AddRange(new Control[] { rdoLight, rdoDark });
        pnlScroll.Controls.Add(grpTheme);
        yPos += 100;

        // --- API Keys ---
        var grpApi = new GroupBox { Text = "API 키 설정", Location = new Point(0, yPos), Size = new Size(900, 160), Font = new Font("맑은 고딕", 11f, FontStyle.Bold) };
        
        var lblGroq = new Label { Text = "GROQ API Key:", Location = new Point(20, 40), AutoSize = true, Font = new Font("맑은 고딕", 10f) };
        var txtGroq = new TextBox { Location = new Point(180, 38), Size = new Size(400, 25), Font = new Font("맑은 고딕", 10f), UseSystemPasswordChar = true };
        txtGroq.Text = EnvManager.Get("GROQ_API_KEY");

        var lblOpenAI = new Label { Text = "OPENAI API Key:", Location = new Point(20, 80), AutoSize = true, Font = new Font("맑은 고딕", 10f) };
        var txtOpenAI = new TextBox { Location = new Point(180, 78), Size = new Size(400, 25), Font = new Font("맑은 고딕", 10f), UseSystemPasswordChar = true };
        txtOpenAI.Text = EnvManager.Get("OPENAI_API_KEY");

        var lblTip = new Label { Text = "* 입력된 키는 로컬 .env 파일에 안전하게 저장됩니다.", Location = new Point(180, 120), AutoSize = true, Font = new Font("맑은 고딕", 9f), ForeColor = Color.Gray };

        grpApi.Controls.AddRange(new Control[] { lblGroq, txtGroq, lblOpenAI, txtOpenAI, lblTip });
        pnlScroll.Controls.Add(grpApi);
        yPos += 180;

        // --- Model Settings ---
        var grpModel = new GroupBox { Text = "AI 모델 설정", Location = new Point(0, yPos), Size = new Size(900, 160), Font = new Font("맑은 고딕", 11f, FontStyle.Bold) };
        
        var lblChatModel = new Label { Text = "LLM 추론 모델:", Location = new Point(20, 40), AutoSize = true, Font = new Font("맑은 고딕", 10f) };
        var cmbChatModel = new ComboBox { Location = new Point(180, 38), Size = new Size(300, 25), Font = new Font("맑은 고딕", 10f), DropDownStyle = ComboBoxStyle.DropDownList };
        cmbChatModel.Items.AddRange(new[] { "llama-3.3-70b-versatile", "mixtral-8x7b-32768", "gemma2-9b-it" });
        cmbChatModel.SelectedItem = EnvManager.Get("GROQ_CHAT_MODEL", "llama-3.3-70b-versatile");

        var lblSttProvider = new Label { Text = "STT (음성 인식):", Location = new Point(20, 80), AutoSize = true, Font = new Font("맑은 고딕", 10f) };
        var cmbSttProvider = new ComboBox { Location = new Point(180, 78), Size = new Size(300, 25), Font = new Font("맑은 고딕", 10f), DropDownStyle = ComboBoxStyle.DropDownList };
        cmbSttProvider.Items.AddRange(new[] { "groq", "openai" });
        cmbSttProvider.SelectedItem = EnvManager.Get("STT_PROVIDER", "groq");

        var lblSttTip = new Label { Text = "* OpenAI의 경우 OPENAI_API_KEY 등록 필수", Location = new Point(180, 120), AutoSize = true, Font = new Font("맑은 고딕", 9f), ForeColor = Color.Gray };

        grpModel.Controls.AddRange(new Control[] { lblChatModel, cmbChatModel, lblSttProvider, cmbSttProvider, lblSttTip });
        pnlScroll.Controls.Add(grpModel);
        yPos += 180;

        var btnSave = new Button
        {
            Text = "💾 설정 저장",
            Location = new Point(0, yPos),
            Size = new Size(150, 40),
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("맑은 고딕", 11f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (s, e) => 
        {
            EnvManager.Set("GROQ_API_KEY", txtGroq.Text);
            EnvManager.Set("OPENAI_API_KEY", txtOpenAI.Text);
            EnvManager.Set("GROQ_CHAT_MODEL", cmbChatModel.SelectedItem?.ToString() ?? "llama-3.3-70b-versatile");
            EnvManager.Set("STT_PROVIDER", cmbSttProvider.SelectedItem?.ToString() ?? "groq");

            MessageBox.Show("설정이 저장되었습니다.\n일부 설정은 앱을 다시 시작해야 완전히 반영될 수 있습니다.", "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };

        pnlScroll.Controls.Add(btnSave);
    }
}
