using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIMeetingMinutes
{
    public class MainForm : Form
    {
        // ═══════════════════════════════════════
        // 테마
        // ═══════════════════════════════════════
        private bool _darkMode = false;

        private Color C_BgMain    => _darkMode ? Color.FromArgb(28, 28, 45)    : Color.FromArgb(245, 247, 250);
        private Color C_BgPanel   => _darkMode ? Color.FromArgb(40, 40, 60)    : Color.White;
        private Color C_Header    => _darkMode ? Color.FromArgb(20, 20, 38)    : Color.FromArgb(74, 144, 226);
        private Color C_KeyBar    => _darkMode ? Color.FromArgb(32, 32, 50)    : Color.FromArgb(235, 240, 248);
        private Color C_StatusBar => _darkMode ? Color.FromArgb(22, 22, 38)    : Color.FromArgb(230, 230, 235);
        private Color C_TextDark  => _darkMode ? Color.FromArgb(220, 220, 235) : Color.FromArgb(30, 30, 30);
        private Color C_TextMuted => _darkMode ? Color.FromArgb(150, 150, 170) : Color.FromArgb(120, 120, 120);
        private Color C_Border    => _darkMode ? Color.FromArgb(60, 60, 85)    : Color.FromArgb(220, 220, 230);

        private static readonly Color AccentBlue   = Color.FromArgb(74, 144, 226);
        private static readonly Color AccentGreen  = Color.FromArgb(39, 174, 96);
        private static readonly Color AccentOrange = Color.FromArgb(230, 126, 34);
        private static readonly Color AccentPurple = Color.FromArgb(155, 89, 182);

        private static readonly Color[] SpeakerColors = {
            Color.FromArgb(74, 144, 226), Color.FromArgb(231, 76, 60),
            Color.FromArgb(39, 174, 96),  Color.FromArgb(155, 89, 182),
            Color.FromArgb(230, 126, 34), Color.FromArgb(26, 188, 156),
        };

        // ═══════════════════════════════════════
        // 폰트
        // ═══════════════════════════════════════
        private int _fontSize = 10;
        private Font GetFont(float size = 0, FontStyle style = FontStyle.Regular) =>
            new Font("맑은 고딕", size > 0 ? size : _fontSize, style);

        // ═══════════════════════════════════════
        // 컨트롤
        // ═══════════════════════════════════════
        private TextBox         txtApiKey         = null!;
        private TextBox         txtMeetingTitle   = null!;
        private DateTimePicker  dtpDate           = null!;
        private TextBox         txtTranscript     = null!;
        private Button          btnParse          = null!;
        private Button          btnNextAgenda     = null!;
        private Button          btnClear          = null!;
        private Button          btnDarkMode       = null!;
        private TrackBar        trkFont           = null!;
        private Label           lblFontVal        = null!;
        private Label           lblSectionInput   = null!;
        private Label           lblApiKeyLabel    = null!;
        private Label           lblHint           = null!;
        private Panel           headerPanel       = null!;
        private Panel           keyBarPanel       = null!;
        private Panel           metaPanel         = null!;
        private Panel           timelinePanel     = null!;
        private Panel           statsPanel        = null!;

        private Panel           participantPanel  = null!;
        private RichTextBox     rtbNextAgenda     = null!;
        private Label           lblStatus         = null!;
        private TabControl      tabRight          = null!;
        private FlowLayoutPanel speakerFilterPanel= null!;
        private SplitContainer  mainSplit         = null!;

        // ═══════════════════════════════════════
        // 상태
        // ═══════════════════════════════════════
        private List<MeetingEntry>         _entries       = new();
        private string?                    _filterSpeaker = null;
        private string                     _nextAgendaText= "";
        private readonly string            _configPath;
        private readonly Dictionary<string, Color>  _speakerColorMap     = new();
        private readonly Dictionary<string, string> _participantSummaries= new();

        // ═══════════════════════════════════════
        // 생성자
        // ═══════════════════════════════════════
        public MainForm()
        {
            _configPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AIMeetingMinutes", "config.txt");
            Text = "AI 회의록";
            Size = new Size(1400, 900);
            MinimumSize = new Size(1000, 650);
            StartPosition = FormStartPosition.CenterScreen;
            Build();
            LoadApiKey();
        }

        // ═══════════════════════════════════════
        // UI 빌드
        // ═══════════════════════════════════════
        private void Build()
        {
            lblStatus = new Label { Dock = DockStyle.Bottom, Height = 26, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0), Text = "준비됨" };
            Controls.Add(lblStatus);

            // 헤더
            headerPanel = new Panel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(16, 0, 16, 0) };
            var lblTitle = new Label { Text = "🎙  AI 회의록", ForeColor = Color.White, Font = new Font("맑은 고딕", 14f, FontStyle.Bold), AutoSize = true, Top = 14 };
            btnDarkMode = new Button
            {
                Text = "🌙 다크모드", ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Height = 30, Width = 95, Top = 12, Cursor = Cursors.Hand,
                BackColor = Color.FromArgb(50, 120, 200),
                FlatAppearance = { BorderSize = 1, BorderColor = Color.White }
            };
            btnDarkMode.Click += (_, _) => ToggleDarkMode();

            lblFontVal = new Label { Text = "10pt", ForeColor = Color.White, AutoSize = true, Top = 19 };
            var lblFontLbl = new Label { Text = "글자크기:", ForeColor = Color.White, AutoSize = true, Top = 19 };
            trkFont = new TrackBar { Minimum = 8, Maximum = 16, Value = 10, Width = 100, Height = 30, Top = 14, TickStyle = TickStyle.None };
            trkFont.ValueChanged += (_, _) => { _fontSize = trkFont.Value; lblFontVal.Text = $"{_fontSize}pt"; ApplyFontSize(); };

            headerPanel.Controls.AddRange(new Control[] { lblTitle, btnDarkMode, lblFontVal, trkFont, lblFontLbl });
            headerPanel.Layout += (_, _) =>
            {
                int r = headerPanel.ClientSize.Width - 12;
                btnDarkMode.Left = r - 95;
                trkFont.Left = r - 210;
                lblFontVal.Left = r - 255;
                lblFontLbl.Left = r - 330;
            };
            Controls.Add(headerPanel);

            // API Key 바
            keyBarPanel = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(16, 6, 16, 6) };
            lblApiKeyLabel = new Label { Text = "Groq API Key:", AutoSize = true, Top = 12, Left = 4 };
            var lblKey = lblApiKeyLabel;
            txtApiKey = new TextBox { PasswordChar = '●', Width = 380, Top = 8, Left = 115, BorderStyle = BorderStyle.FixedSingle };
            var btnSave = MakeButton("저장", AccentBlue, 28);
            btnSave.Location = new Point(503, 8); btnSave.Width = 55;
            btnSave.Click += (_, _) => SaveApiKey();
            keyBarPanel.Controls.AddRange(new Control[] { lblKey, txtApiKey, btnSave });
            Controls.Add(keyBarPanel);

            // 메인 분할
            mainSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, BorderStyle = BorderStyle.None };
            this.Load += (_, _) => { mainSplit.Panel1MinSize = 280; mainSplit.Panel2MinSize = 350; mainSplit.SplitterDistance = mainSplit.Width * 38 / 100; };
            Controls.Add(mainSplit);
            mainSplit.BringToFront();

            BuildLeftPanel(mainSplit.Panel1);
            BuildRightPanel(mainSplit.Panel2);
            ApplyTheme();
        }

        // ── 왼쪽 패널 ──────────────────────────
        private void BuildLeftPanel(SplitterPanel panel)
        {
            panel.Padding = new Padding(12, 8, 6, 8);

            lblSectionInput = new Label { Text = "📝  회의 내용 입력", Font = new Font("맑은 고딕", 10f, FontStyle.Bold), Dock = DockStyle.Top, Height = 28, TextAlign = ContentAlignment.MiddleLeft };

            // 회의 제목 + 날짜
            metaPanel = new Panel { Dock = DockStyle.Top, Height = 34, Padding = new Padding(0, 2, 0, 2) };
            txtMeetingTitle = new TextBox { PlaceholderText = "회의 제목 (선택)", BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill };
            dtpDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 115, Dock = DockStyle.Right };
            metaPanel.Controls.AddRange(new Control[] { txtMeetingTitle, dtpDate });

            lblHint = new Label { Text = "형식: 이름 09:30  (시간 생략 가능)", Dock = DockStyle.Top, Height = 22, Font = new Font("맑은 고딕", 8.5f), Padding = new Padding(2, 0, 0, 0) };
            var hint = lblHint;

            txtTranscript = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Font = GetFont(), AcceptsReturn = true };
            txtTranscript.Text = GetSampleText();

            // 버튼 2행
            var btnPanel = new Panel { Dock = DockStyle.Bottom, Height = 90 };

            var row1 = new Panel { Dock = DockStyle.Top, Height = 42 };
            btnParse = MakeButton("📋  분석하기", AccentBlue, 34);
            btnParse.Dock = DockStyle.Left; btnParse.Width = 125; btnParse.Click += BtnParse_Click;
            btnNextAgenda = MakeButton("📅  다음 안건", AccentPurple, 34);
            btnNextAgenda.Dock = DockStyle.Left; btnNextAgenda.Width = 115; btnNextAgenda.Click += BtnNextAgenda_Click;
            row1.Controls.AddRange(new Control[] { btnNextAgenda, btnParse });

            var row2 = new Panel { Dock = DockStyle.Bottom, Height = 42 };
            var btnTxt = MakeButton("💾 TXT", AccentOrange, 34);
            btnTxt.Dock = DockStyle.Left; btnTxt.Width = 82; btnTxt.Click += (_, _) => ExportFile("txt");
            var btnHtml = MakeButton("🌐 HTML", Color.FromArgb(52, 152, 219), 34);
            btnHtml.Dock = DockStyle.Left; btnHtml.Width = 88; btnHtml.Click += (_, _) => ExportFile("html");
            var btnPdf = MakeButton("📄 PDF", Color.FromArgb(192, 57, 43), 34);
            btnPdf.Dock = DockStyle.Left; btnPdf.Width = 80; btnPdf.Click += (_, _) => ExportPdf();
            var btnCopy = MakeButton("📋 복사", Color.FromArgb(100, 100, 120), 34);
            btnCopy.Dock = DockStyle.Left; btnCopy.Width = 80; btnCopy.Click += BtnCopy_Click;
            btnClear = MakeButton("🗑 초기화", Color.FromArgb(170, 170, 175), 34);
            btnClear.Dock = DockStyle.Right; btnClear.Width = 80; btnClear.Click += BtnClear_Click;
            row2.Controls.AddRange(new Control[] { btnClear, btnCopy, btnPdf, btnHtml, btnTxt });

            btnPanel.Controls.AddRange(new Control[] { row2, row1 });

            panel.Controls.Add(txtTranscript);
            panel.Controls.Add(btnPanel);
            panel.Controls.Add(hint);
            panel.Controls.Add(metaPanel);
            panel.Controls.Add(lblSectionInput);
        }

        // ── 오른쪽 패널 ────────────────────────
        private void BuildRightPanel(SplitterPanel panel)
        {
            panel.Padding = new Padding(6, 8, 12, 8);
            tabRight = new TabControl { Dock = DockStyle.Fill, Font = new Font("맑은 고딕", 9.5f) };

            // 타임라인 탭
            var tabTimeline = new TabPage("⏱  타임라인");
            speakerFilterPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoScroll = true, Padding = new Padding(4, 4, 4, 0) };
            timelinePanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(4) };
            tabTimeline.Controls.Add(timelinePanel);
            tabTimeline.Controls.Add(speakerFilterPanel);

            // 발언 통계 탭
            var tabStats = new TabPage("📊  발언 통계");
            statsPanel = new Panel { Dock = DockStyle.Fill };
            statsPanel.Paint += StatsPanel_Paint;
            tabStats.Controls.Add(statsPanel);

            // 참여자 탭
            var tabParticipant = new TabPage("👥  참여자");
            participantPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(8) };
            tabParticipant.Controls.Add(participantPanel);

            // 다음 안건 탭
            var tabNext = new TabPage("📅  다음 안건");
            rtbNextAgenda = new RichTextBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, Font = new Font("맑은 고딕", 10f), ReadOnly = true, ScrollBars = RichTextBoxScrollBars.Vertical };
            tabNext.Controls.Add(rtbNextAgenda);

            tabRight.TabPages.AddRange(new[] { tabTimeline, tabStats, tabParticipant, tabNext });
            panel.Controls.Add(tabRight);
        }

        // ═══════════════════════════════════════
        // 버튼 이벤트
        // ═══════════════════════════════════════
        private void BtnParse_Click(object? sender, EventArgs e)
        {
            var text = txtTranscript.Text.Trim();
            if (string.IsNullOrEmpty(text)) { SetStatus("⚠ 회의 내용을 입력해주세요."); return; }
            _entries = MeetingParser.Parse(text);
            if (_entries.Count == 0) { SetStatus("⚠ 인식된 발화가 없습니다. 형식을 확인하세요."); return; }
            BuildSpeakerColorMap();
            BuildFilterButtons();
            RenderTimeline();
            statsPanel.Invalidate();
            BuildParticipantCards();
            tabRight.SelectedIndex = 0;
            SetStatus($"✅ {_entries.Count}개 발화, {_speakerColorMap.Count}명 분석 완료");
        }

        private async void BtnNextAgenda_Click(object? sender, EventArgs e)
        {
            if (!ValidateApi()) return;
            SetButtonState(btnNextAgenda, false, "⏳  처리 중...");
            SetStatus("🤖 다음 회의 안건 생성 중...");
            try
            {
                _nextAgendaText = await new ClaudeApiService(txtApiKey.Text.Trim()).SuggestNextAgendaAsync(txtTranscript.Text.Trim());
                rtbNextAgenda.Text = _nextAgendaText;
                tabRight.SelectedIndex = 3;
                SetStatus("✅ 다음 회의 안건 생성 완료!");
            }
            catch (Exception ex) { ShowError(ex.Message); }
            finally { SetButtonState(btnNextAgenda, true, "📅  다음 안건"); }
        }

        private void BtnCopy_Click(object? sender, EventArgs e)
        {
            string text = "";

            if (tabRight.SelectedIndex == 3 && !string.IsNullOrEmpty(_nextAgendaText))
                text = _nextAgendaText;
            else if (_entries.Count > 0)
                text = string.Join("\r\n", _entries
                    .Where(x => _filterSpeaker == null || x.Speaker == _filterSpeaker)
                    .Select(x => $"[{(string.IsNullOrEmpty(x.Timestamp) ? "-" : x.Timestamp)}] {x.Speaker}: {x.Content}"));

            if (string.IsNullOrEmpty(text)) { SetStatus("⚠ 복사할 내용이 없습니다. 먼저 분석하기를 실행해주세요."); return; }

            bool copied = false;
            for (int i = 0; i < 10 && !copied; i++)
            {
                try { Clipboard.SetText(text); copied = true; }
                catch { Application.DoEvents(); }
            }
            SetStatus(copied ? "✅ 클립보드에 복사되었습니다." : "⚠ 복사 실패: 잠시 후 다시 시도해주세요.");
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            txtTranscript.Clear(); ClearTimeline(); rtbNextAgenda.Clear();
            _entries.Clear(); _nextAgendaText = ""; _filterSpeaker = null;
            _speakerColorMap.Clear(); _participantSummaries.Clear();
            speakerFilterPanel.Controls.Clear();
            foreach (Control c in participantPanel.Controls) c.Dispose();
            participantPanel.Controls.Clear();
            statsPanel.Invalidate();
        }

        // ═══════════════════════════════════════
        // 화자 필터 버튼
        // ═══════════════════════════════════════
        private void BuildFilterButtons()
        {
            speakerFilterPanel.Controls.Clear();
            _filterSpeaker = null;
            var btnAll = MakeFilterButton("전체", AccentBlue, true);
            btnAll.Click += (_, _) => { _filterSpeaker = null; Highlight(btnAll); RenderTimeline(); };
            speakerFilterPanel.Controls.Add(btnAll);
            foreach (var sp in _speakerColorMap.Keys)
            {
                var s = sp; var col = _speakerColorMap[s];
                var btn = MakeFilterButton(s, col, false);
                btn.Click += (_, _) => { _filterSpeaker = s; Highlight(btn); RenderTimeline(); };
                speakerFilterPanel.Controls.Add(btn);
            }
        }

        private void Highlight(Button active)
        {
            foreach (Control c in speakerFilterPanel.Controls)
                if (c is Button b) b.FlatAppearance.BorderSize = b == active ? 2 : 0;
        }

        // ═══════════════════════════════════════
        // 타임라인 렌더링
        // ═══════════════════════════════════════
        private void RenderTimeline()
        {
            var list = _entries.Where(e => _filterSpeaker == null || e.Speaker == _filterSpeaker).ToList();
            timelinePanel.SuspendLayout();
            ClearTimeline();
            int y = 8;
            foreach (var entry in list)
            {
                var card = BuildCard(entry, _speakerColorMap.TryGetValue(entry.Speaker, out var c) ? c : AccentBlue);
                card.Top = y;
                timelinePanel.Controls.Add(card);
                y += card.Height + 6;
            }
            timelinePanel.ResumeLayout();
        }

        private Panel BuildCard(MeetingEntry entry, Color color)
        {
            bool hasTime = !string.IsNullOrEmpty(entry.Timestamp);
            int w = Math.Max(200, timelinePanel.ClientSize.Width - 24);
            int contentWidth = w - 120;

            // 실제 텍스트 높이 측정
            var contentFont = GetFont();
            var textSize = System.Windows.Forms.TextRenderer.MeasureText(
                entry.Content, contentFont,
                new Size(contentWidth, int.MaxValue),
                System.Windows.Forms.TextFormatFlags.WordBreak);

            int contentH = Math.Max(_fontSize + 8, textSize.Height);
            int h = Math.Max(60, 10 + (_fontSize + 6) + (hasTime ? _fontSize + 6 : 0) + 8 + contentH + 12);

            var card = new Panel { Width = w, Height = h, BackColor = C_BgPanel };
            card.Paint += (_, e) =>
            {
                using var b = new SolidBrush(color); e.Graphics.FillRectangle(b, 0, 0, 5, card.Height);
                using var p = new Pen(C_Border); e.Graphics.DrawLine(p, 0, h - 1, w, h - 1);
            };

            int nameY = 10;
            int timeY = nameY + _fontSize + 6;
            int contentY = (hasTime ? timeY + _fontSize + 4 : nameY + _fontSize + 4) + 4;

            card.Controls.Add(new Label
            {
                Text = entry.Speaker,
                Font = new Font("맑은 고딕", 9.5f, FontStyle.Bold),
                ForeColor = color, AutoSize = true,
                Location = new Point(16, nameY)
            });
            card.Controls.Add(new Label
            {
                Text = "🕐 " + entry.Timestamp,
                Font = new Font("맑은 고딕", 8.5f),
                ForeColor = C_TextMuted, AutoSize = true,
                Location = new Point(16, timeY), Visible = hasTime
            });
            card.Controls.Add(new Label
            {
                Text = entry.Content,
                Font = contentFont, ForeColor = C_TextDark,
                MaximumSize = new Size(contentWidth, 0),
                AutoSize = true,
                Location = new Point(110, contentY)
            });
            return card;
        }

        private void ClearTimeline() { foreach (Control c in timelinePanel.Controls) c.Dispose(); timelinePanel.Controls.Clear(); }

        // ═══════════════════════════════════════
        // 발언 통계
        // ═══════════════════════════════════════
        private void StatsPanel_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics; g.Clear(C_BgPanel);
            if (_entries.Count == 0) { DrawHint(g, "먼저 📋 분석하기를 실행해주세요."); return; }

            var speakers = _speakerColorMap.Keys.ToList();
            int total = _entries.Count;
            int sx = 175, sy = 55, bh = 30, gap = 16, maxW = statsPanel.ClientSize.Width - sx - 160;

            GText(g, "화자별 발언 통계", sx, 12, 12, FontStyle.Bold, C_TextDark);
            var dur = CalcDuration();
            if (!string.IsNullOrEmpty(dur)) GText(g, $"⏱ 총 회의 시간: {dur}", sx, 34, 9, FontStyle.Regular, C_TextMuted);
            GText(g, "발화 횟수", sx, sy - 16, 8, FontStyle.Regular, C_TextMuted);

            int y = sy;
            foreach (var sp in speakers)
            {
                int cnt = _entries.Count(x => x.Speaker == sp);
                var col = _speakerColorMap[sp];

                using var nf = new Font("맑은 고딕", 9.5f, FontStyle.Bold);
                using var nb = new SolidBrush(C_TextDark);
                var ns = g.MeasureString(sp, nf);
                g.DrawString(sp, nf, nb, sx - ns.Width - 10, y + (bh - ns.Height) / 2);

                int b1 = Math.Max(4, (int)((double)cnt / total * maxW));
                using var bb = new SolidBrush(col);
                g.FillRectangle(bb, sx, y, b1, bh);
                GText(g, $"{cnt}회 ({(double)cnt / total * 100:F0}%)", sx + b1 + 4, y + (bh - 14) / 2, 8.5f, FontStyle.Regular, C_TextDark);
                y += bh + gap;
            }
            GText(g, $"전체 발화: {total}회", sx, y + 6, 8.5f, FontStyle.Regular, C_TextMuted);
        }

        private string CalcDuration()
        {
            var mins = _entries.Where(e => !string.IsNullOrEmpty(e.Timestamp)).Select(e => {
                var p = e.Timestamp.Split(':');
                return p.Length >= 2 && int.TryParse(p[0], out int h) && int.TryParse(p[1], out int m) ? (int?)(h * 60 + m) : null;
            }).Where(t => t.HasValue).Select(t => t!.Value).ToList();
            if (mins.Count < 2) return "";
            int d = mins.Max() - mins.Min();
            return d >= 60 ? $"{d / 60}시간 {d % 60}분" : $"{d}분";
        }

        // ═══════════════════════════════════════
        // 흐름 차트
        // ═══════════════════════════════════════

        // ═══════════════════════════════════════
        // 참여자 카드
        // ═══════════════════════════════════════
        private void BuildParticipantCards()
        {
            foreach (Control c in participantPanel.Controls) c.Dispose();
            participantPanel.Controls.Clear();

            int y = 8;
            foreach (var sp in _speakerColorMap.Keys)
            {
                var speaker = sp;
                var color = _speakerColorMap[sp];
                var myEntries = _entries.Where(x => x.Speaker == sp).ToList();

                var card = new Panel { Width = Math.Max(200, participantPanel.ClientSize.Width - 24), Top = y, BackColor = C_BgPanel, Padding = new Padding(16, 10, 10, 10) };
                card.Paint += (_, e) => { using var b = new SolidBrush(color); e.Graphics.FillRectangle(b, 0, 0, 6, card.Height); };

                var lblName = new Label { Text = speaker, Font = new Font("맑은 고딕", 11f, FontStyle.Bold), ForeColor = color, AutoSize = true, Location = new Point(20, 10) };
                var lblStat = new Label { Text = $"발화 {myEntries.Count}회", Font = new Font("맑은 고딕", 8.5f), ForeColor = C_TextMuted, AutoSize = true, Location = new Point(20, 32) };

                var sb = new StringBuilder();
                foreach (var ent in myEntries)
                    sb.AppendLine($"• {(string.IsNullOrEmpty(ent.Timestamp) ? "" : $"[{ent.Timestamp}] ")}{ent.Content}");
                string originalText = sb.ToString().Trim();

                var rtb = new RichTextBox
                {
                    Text = originalText, Font = GetFont(), ForeColor = C_TextDark,
                    BackColor = C_BgPanel, BorderStyle = BorderStyle.None, ReadOnly = true,
                    ScrollBars = RichTextBoxScrollBars.None, Location = new Point(20, 52),
                    Width = card.Width - 140
                };
                rtb.ContentsResized += (_, e) => { rtb.Height = e.NewRectangle.Height + 4; card.Height = rtb.Top + rtb.Height + 16; };

                var btnAi = MakeButton("🤖 AI 분석", color, 28);
                btnAi.Width = 90; btnAi.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                btnAi.Location = new Point(card.Width - 105, 10);
                btnAi.Click += async (_, _) =>
                {
                    if (string.IsNullOrEmpty(txtApiKey.Text.Trim())) { SetStatus("⚠ API Key를 입력해주세요."); return; }
                    btnAi.Enabled = false; btnAi.Text = "분석 중...";
                    try
                    {
                        var svc = new ClaudeApiService(txtApiKey.Text.Trim());
                        var summary = await svc.SummarizeParticipantAsync(speaker, myEntries.Select(x => x.Content).ToList());
                        _participantSummaries[speaker] = summary;
                        rtb.Text = summary;
                        SetStatus($"✅ {speaker} AI 분석 완료");
                    }
                    catch (Exception ex) { ShowError(ex.Message); }
                    finally { btnAi.Enabled = true; btnAi.Text = "🤖 AI 분석"; }
                };

                var btnReset = MakeButton("↩ 원문", Color.FromArgb(140, 140, 150), 28);
                btnReset.Width = 90; btnReset.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                btnReset.Location = new Point(card.Width - 105, 44);
                btnReset.Click += (_, _) => rtb.Text = originalText;

                card.Controls.AddRange(new Control[] { lblName, lblStat, rtb, btnAi, btnReset });
                card.Height = Math.Max(100, 52 + 60 + 16);
                participantPanel.Controls.Add(card);
                y += card.Height + 10;
            }
        }

        // ═══════════════════════════════════════
        // 내보내기
        // ═══════════════════════════════════════
        private void ExportFile(string fmt)
        {
            if (_entries.Count == 0 && string.IsNullOrEmpty(_nextAgendaText)) { SetStatus("⚠ 먼저 분석하기를 실행해주세요."); return; }
            string title = string.IsNullOrEmpty(txtMeetingTitle.Text) ? "회의록" : txtMeetingTitle.Text;
            string date  = dtpDate.Value.ToString("yyyy년 MM월 dd일");
            bool   html  = fmt == "html";

            using var dlg = new SaveFileDialog
            {
                Title = "회의록 내보내기",
                Filter = html ? "HTML 파일 (*.html)|*.html" : "텍스트 파일 (*.txt)|*.txt",
                FileName = $"{title}_{DateTime.Now:yyyyMMdd_HHmm}.{(html ? "html" : "txt")}",
                DefaultExt = html ? "html" : "txt"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                File.WriteAllText(dlg.FileName, html ? BuildHtml(title, date) : BuildTxt(title, date), Encoding.UTF8);
                SetStatus($"✅ 저장 완료: {Path.GetFileName(dlg.FileName)}");
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        private void ExportPdf()
        {
            if (_entries.Count == 0 && string.IsNullOrEmpty(_nextAgendaText))
            { SetStatus("⚠ 먼저 분석하기를 실행해주세요."); return; }

            string title   = string.IsNullOrEmpty(txtMeetingTitle.Text) ? "회의록" : txtMeetingTitle.Text;
            string date    = dtpDate.Value.ToString("yyyy년 MM월 dd일");

            using var dlg = new SaveFileDialog
            {
                Title      = "PDF로 저장",
                Filter     = "PDF 파일 (*.pdf)|*.pdf",
                FileName   = $"{title}_{DateTime.Now:yyyyMMdd_HHmm}.pdf",
                DefaultExt = "pdf"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            string pdfPath = dlg.FileName;
            string content = BuildTxt(title, date);
            string[] lines = content.Split(new[] { '\r', '\n' });
            int lineIdx = 0;

            var pd = new System.Drawing.Printing.PrintDocument();
            pd.DocumentName = title;
            pd.PrinterSettings.PrinterName  = "Microsoft Print to PDF";
            pd.PrinterSettings.PrintToFile  = true;
            pd.PrinterSettings.PrintFileName = pdfPath;

            pd.PrintPage += (_, e) =>
            {
                if (e.Graphics == null) return;
                using var f  = new Font("맑은 고딕", 10f);
                using var sf = new StringFormat { Trimming = StringTrimming.Word, FormatFlags = StringFormatFlags.LineLimit };
                float pageW = e.MarginBounds.Width;
                float y     = e.MarginBounds.Top;
                float lineH = f.GetHeight(e.Graphics);

                while (lineIdx < lines.Length)
                {
                    string line = lines[lineIdx];
                    float h = string.IsNullOrWhiteSpace(line)
                        ? lineH * 0.6f
                        : e.Graphics.MeasureString(line, f, (int)pageW, sf).Height;
                    h = Math.Max(lineH, h);

                    if (y + h > e.MarginBounds.Bottom) { e.HasMorePages = true; break; }

                    if (!string.IsNullOrWhiteSpace(line))
                        e.Graphics.DrawString(line, f, Brushes.Black,
                            new RectangleF(e.MarginBounds.Left, y, pageW, h + 2), sf);

                    y += h;
                    lineIdx++;
                }
            };

            try
            {
                pd.Print();
                SetStatus($"✅ PDF 저장 완료: {Path.GetFileName(pdfPath)}");
            }
            catch (Exception ex) { ShowError(ex.Message); }
            finally { pd.Dispose(); }
        }

        private string BuildTxt(string title, string date)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{'═'.ToString().PadRight(44, '═')}");
            sb.AppendLine($"  {title}  |  {date}  |  생성: {DateTime.Now:HH:mm}");
            sb.AppendLine($"{'═'.ToString().PadRight(44, '═')}");
            if (_entries.Count > 0)
            {
                sb.AppendLine("\n【 참여자 통계 】");
                int total = _entries.Count;
                foreach (var sp in _speakerColorMap.Keys)
                {
                    int cnt = _entries.Count(x => x.Speaker == sp);
                    int ch  = _entries.Where(x => x.Speaker == sp).Sum(x => x.Content.Length);
                    sb.AppendLine($"  {sp}: {cnt}회 ({(double)cnt / total * 100:F0}%) / {ch:N0}글자");
                }
                var dur = CalcDuration();
                if (!string.IsNullOrEmpty(dur)) sb.AppendLine($"  총 회의 시간: {dur}");
            }
            if (!string.IsNullOrEmpty(_nextAgendaText)) { sb.AppendLine("\n【 다음 회의 안건 】"); sb.AppendLine(_nextAgendaText); }
            if (_entries.Count > 0)
            {
                sb.AppendLine("\n【 회의 타임라인 】");
                foreach (var x in _entries)
                    sb.AppendLine($"  [{(string.IsNullOrEmpty(x.Timestamp) ? "  -  " : x.Timestamp)}] {x.Speaker}: {x.Content}");
            }
            return sb.ToString();
        }

        private string BuildHtml(string title, string date)
        {
            var sb = new StringBuilder();
            sb.Append($@"<!DOCTYPE html><html lang='ko'><head><meta charset='UTF-8'><title>{title}</title><style>
body{{font-family:'맑은 고딕',sans-serif;max-width:900px;margin:40px auto;background:#f5f7fa;color:#222}}
h1{{color:#4a90e2}}h2{{border-bottom:2px solid #4a90e2;padding-bottom:6px;margin-top:28px}}
table{{width:100%;border-collapse:collapse}}td,th{{padding:8px 12px;border:1px solid #ddd}}
th{{background:#4a90e2;color:#fff}}tr:nth-child(even){{background:#f0f4f8}}
.entry{{border-left:4px solid;padding:8px 14px;margin:6px 0;background:#fff;border-radius:0 6px 6px 0}}
.time{{color:#888;font-size:.85em}}.agenda{{background:#f8f0ff;padding:16px;border-radius:8px;white-space:pre-wrap}}
</style></head><body>
<h1>🎙 {title}</h1><p style='color:#888'>{date} | 생성: {DateTime.Now:HH:mm}</p>");

            if (_entries.Count > 0)
            {
                sb.Append("<h2>📊 참여자 통계</h2><table><tr><th>참여자</th><th>발화</th><th>비율</th><th>글자수</th></tr>");
                int tot = _entries.Count;
                foreach (var sp in _speakerColorMap.Keys)
                {
                    int cnt = _entries.Count(x => x.Speaker == sp);
                    int ch  = _entries.Where(x => x.Speaker == sp).Sum(x => x.Content.Length);
                    var col = _speakerColorMap[sp];
                    sb.Append($"<tr><td style='color:#{col.R:X2}{col.G:X2}{col.B:X2};font-weight:bold'>{sp}</td><td>{cnt}회</td><td>{(double)cnt / tot * 100:F1}%</td><td>{ch:N0}자</td></tr>");
                }
                sb.Append("</table>");
                var dur = CalcDuration();
                if (!string.IsNullOrEmpty(dur)) sb.Append($"<p>⏱ 총 회의 시간: <strong>{dur}</strong></p>");
            }
            if (!string.IsNullOrEmpty(_nextAgendaText))
                sb.Append($"<h2>📅 다음 회의 안건</h2><div class='agenda'>{_nextAgendaText}</div>");
            if (_entries.Count > 0)
            {
                sb.Append("<h2>⏱ 회의 타임라인</h2>");
                foreach (var x in _entries)
                {
                    var col = _speakerColorMap.TryGetValue(x.Speaker, out var c) ? c : AccentBlue;
                    sb.Append($"<div class='entry' style='border-color:#{col.R:X2}{col.G:X2}{col.B:X2}'><strong style='color:#{col.R:X2}{col.G:X2}{col.B:X2}'>{x.Speaker}</strong>");
                    if (!string.IsNullOrEmpty(x.Timestamp)) sb.Append($" <span class='time'>🕐 {x.Timestamp}</span>");
                    sb.Append($"<p style='margin:4px 0 0'>{x.Content}</p></div>");
                }
            }
            sb.Append("</body></html>");
            return sb.ToString();
        }

        // ═══════════════════════════════════════
        // 테마 / 폰트
        // ═══════════════════════════════════════
        private void ToggleDarkMode()
        {
            _darkMode = !_darkMode;
            btnDarkMode.Text = _darkMode ? "☀ 라이트모드" : "🌙 다크모드";
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            BackColor = C_BgMain;
            headerPanel.BackColor = C_Header;
            keyBarPanel.BackColor = C_KeyBar;
            lblApiKeyLabel.ForeColor = C_TextDark;
            txtApiKey.BackColor = C_BgPanel; txtApiKey.ForeColor = C_TextDark;
            lblStatus.BackColor = C_StatusBar; lblStatus.ForeColor = C_TextMuted;
            lblSectionInput.ForeColor = C_TextDark;
            lblHint.ForeColor = C_TextMuted;
            metaPanel.BackColor = C_BgMain;
            txtMeetingTitle.BackColor = C_BgPanel; txtMeetingTitle.ForeColor = C_TextDark;
            dtpDate.CalendarForeColor = C_TextDark; dtpDate.CalendarMonthBackground = C_BgPanel;
            mainSplit.Panel1.BackColor = C_BgMain;
            mainSplit.Panel2.BackColor = C_BgMain;
            txtTranscript.BackColor = C_BgPanel; txtTranscript.ForeColor = C_TextDark;
            tabRight.BackColor = C_BgMain;
            foreach (TabPage tp in tabRight.TabPages) tp.BackColor = C_BgPanel;
            timelinePanel.BackColor = C_BgMain; speakerFilterPanel.BackColor = C_BgMain;
            rtbNextAgenda.BackColor = C_BgPanel; rtbNextAgenda.ForeColor = C_TextDark;
            participantPanel.BackColor = C_BgPanel;
            statsPanel.BackColor = C_BgPanel; statsPanel.Invalidate();
            if (_entries.Count > 0) { RenderTimeline(); BuildParticipantCards(); }
        }

        private void ApplyFontSize()
        {
            var f = GetFont();
            txtTranscript.Font = f;
            rtbNextAgenda.Font = f;
            if (_entries.Count > 0) { RenderTimeline(); BuildParticipantCards(); }
        }

        // ═══════════════════════════════════════
        // 드로잉 유틸
        // ═══════════════════════════════════════
        private void GText(Graphics g, string text, int x, int y, float size, FontStyle style, Color color)
        {
            using var f = new Font("맑은 고딕", size, style);
            using var b = new SolidBrush(color);
            g.DrawString(text, f, b, x, y);
        }

        private void DrawHint(Graphics g, string text)
        {
            using var f = new Font("맑은 고딕", 11f);
            using var b = new SolidBrush(C_TextMuted);
            g.DrawString(text, f, b, 40, 40);
        }

        // ═══════════════════════════════════════
        // 유틸
        // ═══════════════════════════════════════
        private void BuildSpeakerColorMap()
        {
            _speakerColorMap.Clear();
            int idx = 0;
            foreach (var e in _entries)
                if (!_speakerColorMap.ContainsKey(e.Speaker))
                    _speakerColorMap[e.Speaker] = SpeakerColors[idx++ % SpeakerColors.Length];
        }

        private bool ValidateApi()
        {
            if (string.IsNullOrEmpty(txtApiKey.Text.Trim())) { SetStatus("⚠ API Key를 입력해주세요."); return false; }
            if (string.IsNullOrEmpty(txtTranscript.Text.Trim())) { SetStatus("⚠ 회의 내용을 입력해주세요."); return false; }
            return true;
        }

        private void SetButtonState(Button b, bool en, string t) { b.Enabled = en; b.Text = t; }
        private void ShowError(string msg) { MessageBox.Show($"오류:\n{msg}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error); SetStatus("❌ 오류 발생"); }
        private void SetStatus(string msg) => lblStatus.Text = "  " + msg;

        private void SaveApiKey()
        {
            try { Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!); File.WriteAllText(_configPath, txtApiKey.Text.Trim()); SetStatus("✅ API Key 저장 완료"); }
            catch { SetStatus("⚠ API Key 저장 실패"); }
        }

        private void LoadApiKey() { if (File.Exists(_configPath)) txtApiKey.Text = File.ReadAllText(_configPath).Trim(); }

        private static Button MakeFilterButton(string text, Color color, bool active) => new Button
        {
            Text = text, BackColor = color, ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat, Height = 28, AutoSize = true,
            Margin = new Padding(2, 0, 2, 0), Font = new Font("맑은 고딕", 8.5f),
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderSize = active ? 2 : 0, BorderColor = Color.White }
        };

        private static Button MakeButton(string text, Color back, int h) => new Button
        {
            Text = text, BackColor = back, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
            Height = h, Font = new Font("맑은 고딕", 9.5f), Cursor = Cursors.Hand, FlatAppearance = { BorderSize = 0 }
        };

        private static string GetSampleText() =>
@"유민정 09:05
오늘 회의는 다음 달 출시 예정인 앱의 기능 우선순위를 결정하는 자리입니다.

김태호 09:07
저는 로그인 기능이 가장 먼저 완성되어야 한다고 생각해요. 다른 모든 기능이 로그인에 의존하거든요.

이세진 09:10
동의합니다. 메인 피드 화면도 구현해야 할 것 같아요.

박지수 09:13
알림 기능도 초기에 넣어야 한다고 봐요. 사용자 리텐션에 직접적인 영향을 주거든요.

김태호 09:16
알림은 2차 개발로 미루는 게 낫지 않을까요?

유민정 09:19
1차에는 로그인, 메인 피드, 프로필 페이지까지만 포함하고 알림은 2차로 넘기겠습니다.

박지수 09:23
제가 프로필 페이지 맡겠습니다. 디자인 시안은 수요일까지 공유할게요.";
    }
}
