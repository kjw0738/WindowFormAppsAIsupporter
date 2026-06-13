using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsAppAIsupporter
{
    public partial class Form1 : Form
    {
        private readonly AIService _aiService;
        private string _selectedAudioPath = string.Empty;

        public Form1()
        {
            InitializeComponent();
            _aiService = new AIService();
        }

        /// <summary>
        /// 상단 내비게이션 버튼 클릭 이벤트
        /// </summary>
        private void BtnNav_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            
            // 모든 패널을 숨김
            panelInput.Visible = false;
            panelSwot.Visible = false;
            panelRoles.Visible = false;

            // 클릭된 버튼에 따라 해당 패널만 표시
            if (btn == btnNavInput) panelInput.Visible = true;
            else if (btn == btnNavSwot) panelSwot.Visible = true;
            else if (btn == btnNavRoles) panelRoles.Visible = true;
        }

        /// <summary>
        /// 음성 파일 첨부 버튼 클릭 이벤트
        /// </summary>
        private void BtnAttachVoice_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Audio Files|*.mp3;*.m4a;*.wav;*.mp4";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _selectedAudioPath = openFileDialog.FileName;
                    lblFilePath.Text = Path.GetFileName(_selectedAudioPath);
                }
            }
        }

        /// <summary>
        /// 회의 분석 시작 버튼 클릭 이벤트
        /// </summary>
        private async void BtnStartAnalysis_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedAudioPath))
            {
                MessageBox.Show("먼저 음성 파일을 첨부해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                SetLoadingState(true);
                lblStatus.Text = "음성 텍스트 변환 중 (STT)...";

                // 1. 음성 -> 텍스트 변환
                string meetingText = await _aiService.TranscribeAudioAsync(_selectedAudioPath);

                lblStatus.Text = "SWOT 분석 및 역할 분배 중...";

                // 2. SWOT 분석과 역할 분배를 동시에 진행
                int participantCount = (int)numParticipants.Value;
                
                var swotTask = _aiService.AnalyzeSwotAsync(meetingText);
                var rolesTask = _aiService.DistributeRolesAsync(meetingText, participantCount);

                await Task.WhenAll(swotTask, rolesTask);

                // SWOT 결과 파싱 및 표시
                ParseAndDisplaySwot(swotTask.Result);
                
                // 역할 분배 결과 파싱 및 동적 UI 생성
                ParseAndDisplayRoles(rolesTask.Result, participantCount);

                lblStatus.Text = "분석 완료!";
                MessageBox.Show("분석이 완료되었습니다. 결과 창으로 이동합니다.", "성공");

                // 분석 완료 후 SWOT 화면으로 이동
                BtnNav_Click(btnNavSwot, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류 발생: {ex.Message}", "에러", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "분석 실패";
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        /// <summary>
        /// AI의 역할 분배 응답을 파싱하여 인원수만큼 UI 컨트롤을 생성합니다.
        /// </summary>
        private void ParseAndDisplayRoles(string response, int count)
        {
            // 기존 컨트롤 제거
            flowLayoutPanelRoles.Controls.Clear();

            // AI 답변에서 \n 등을 실제 줄바꿈으로 변환
            response = response.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);

            for (int i = 1; i <= count; i++)
            {
                int participantIndex = i; // 클로저 이슈 해결을 위해 로컬 변수 사용
                string tag = $"[P{i}]";
                string nextTag = $"[P{i + 1}]";
                string roleContent = "";

                // 태그 사이의 내용 추출
                int start = response.IndexOf(tag);
                if (start != -1)
                {
                    start += tag.Length;
                    int end = response.IndexOf(nextTag, start);
                    roleContent = end != -1 ? response.Substring(start, end - start) : response.Substring(start);
                }

                // 한 줄(Row)을 담을 패널 생성 (너비를 750으로 확장)
                Panel rowPanel = new Panel { Size = new System.Drawing.Size(750, 80), Margin = new Padding(0, 0, 0, 10) };

                // 1. 라벨
                Label lbl = new Label { Text = $"참여자 {participantIndex}", Location = new System.Drawing.Point(0, 10), Size = new System.Drawing.Size(70, 20), AutoSize = true, Font = new System.Drawing.Font("맑은 고딕", 9, System.Drawing.FontStyle.Bold) };

                // 2. 텍스트 박스 (너비를 580으로 확장)
                TextBox txt = new TextBox { Text = roleContent.Trim(), Location = new System.Drawing.Point(80, 5), Size = new System.Drawing.Size(580, 70), Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical };

                // 3. 수정 버튼 (위치를 665로 이동하여 오른쪽 끝으로 배치)
                Button btn = new Button { Text = "수정", Location = new System.Drawing.Point(665, 5), Size = new System.Drawing.Size(80, 30) };
                btn.Click += (s, e) => {
                    if (txt.ReadOnly) {
                        txt.ReadOnly = false;
                        btn.Text = "저장";
                        txt.Focus();
                    } else {
                        txt.ReadOnly = true;
                        btn.Text = "수정";
                        MessageBox.Show($"참여자 {participantIndex}의 업무가 저장되었습니다.", "알림");
                    }
                };

                rowPanel.Controls.Add(lbl);
                rowPanel.Controls.Add(txt);
                rowPanel.Controls.Add(btn);

                flowLayoutPanelRoles.Controls.Add(rowPanel);
            }
        }

        /// <summary>
        /// AI의 SWOT 응답을 [S], [W], [O], [T] 태그 기준으로 잘라 각 칸에 넣습니다.
        /// </summary>
        private void ParseAndDisplaySwot(string response)
        {
            // AI 답변에서 \n 등을 실제 줄바꿈으로 변환
            response = response.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);

            string s = "", w = "", o = "", t = "";
            
            try
            {
                // [S], [W], [O], [T] 기준으로 분리
                string[] parts = response.Split(new[] { "[S]", "[W]", "[O]", "[T]" }, StringSplitOptions.None);
                
                if (parts.Length >= 5)
                {
                    s = parts[1].Trim();
                    w = parts[2].Trim();
                    o = parts[3].Trim();
                    t = parts[4].Trim();
                }
            }
            catch { /* 파싱 실패 시 원문 무시 */ }

            txtS.Text = s;
            txtW.Text = w;
            txtO.Text = o;
            txtT.Text = t;
        }

        /// <summary>
        /// 분석 중 UI 비활성화 처리
        /// </summary>
        private void SetLoadingState(bool isLoading)
        {
            btnStartAnalysis.Enabled = !isLoading;
            btnAttachVoice.Enabled = !isLoading;
            numParticipants.Enabled = !isLoading;
            navPanel.Enabled = !isLoading;
            
            this.Cursor = isLoading ? Cursors.WaitCursor : Cursors.Default;
        }
    }
}
