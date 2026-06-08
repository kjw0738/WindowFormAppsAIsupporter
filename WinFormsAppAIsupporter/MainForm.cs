namespace WinFormsAppAIsupporter
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            SetupViews();
            ShowView(panelMeetingView);
        }

        private void SetupViews()
        {
            panelMeetingView.Controls.Clear();
            panelTaskView.Controls.Clear();
            panelQuizView.Controls.Clear();

            panelMeetingView.BackColor = Color.LightBlue;
            panelTaskView.BackColor = Color.LightGreen;
            panelQuizView.BackColor = Color.LightYellow;

            SetupMeetingView();
            SetupTaskView();
            SetupQuizView();
        }

        private void SetupMeetingView()
        {
            Label lblInput = new Label();
            lblInput.Text = "회의 내용 입력";
            lblInput.Location = new Point(30, 30);
            lblInput.Size = new Size(200, 30);
            lblInput.Font = new Font("맑은 고딕", 12, FontStyle.Bold);

            RichTextBox rtbInput = new RichTextBox();
            rtbInput.Name = "rtbMeetingInput";
            rtbInput.Location = new Point(30, 70);
            rtbInput.Size = new Size(760, 160);

            Button btnSummary = new Button();
            btnSummary.Name = "btnSummary";
            btnSummary.Text = "요약하기";
            btnSummary.Location = new Point(30, 250);
            btnSummary.Size = new Size(120, 40);
            btnSummary.Click += btnSummary_Click;

            Label lblResult = new Label();
            lblResult.Text = "요약 결과";
            lblResult.Location = new Point(30, 310);
            lblResult.Size = new Size(200, 30);
            lblResult.Font = new Font("맑은 고딕", 12, FontStyle.Bold);

            RichTextBox rtbResult = new RichTextBox();
            rtbResult.Name = "rtbMeetingResult";
            rtbResult.Location = new Point(30, 350);
            rtbResult.Size = new Size(760, 160);

            panelMeetingView.Controls.Add(lblInput);
            panelMeetingView.Controls.Add(rtbInput);
            panelMeetingView.Controls.Add(btnSummary);
            panelMeetingView.Controls.Add(lblResult);
            panelMeetingView.Controls.Add(rtbResult);
        }

        private void SetupTaskView()
        {
            Label lblTitle = new Label();
            lblTitle.Text = "과제 관리";
            lblTitle.Location = new Point(30, 30);
            lblTitle.Size = new Size(200, 30);
            lblTitle.Font = new Font("맑은 고딕", 12, FontStyle.Bold);

            DataGridView dgvTasks = new DataGridView();
            dgvTasks.Name = "dgvTasks";
            dgvTasks.Location = new Point(30, 70);
            dgvTasks.Size = new Size(760, 360);
            dgvTasks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvTasks.AllowUserToAddRows = true;
            dgvTasks.BackgroundColor = Color.White;
            dgvTasks.DefaultCellStyle.BackColor = Color.White;
            dgvTasks.RowHeadersVisible = false;

            dgvTasks.Columns.Add("Assignee", "팀원");
            dgvTasks.Columns.Add("Task", "할 일");
            dgvTasks.Columns.Add("DueDate", "마감일");

            DataGridViewComboBoxColumn statusColumn = new DataGridViewComboBoxColumn();
            statusColumn.Name = "Status";
            statusColumn.HeaderText = "상태";
            statusColumn.Items.Add("미시작");
            statusColumn.Items.Add("진행 중");
            statusColumn.Items.Add("완료");

            dgvTasks.Columns.Add(statusColumn);

            Button btnAdd = new Button();
            btnAdd.Name = "btnAddTask";
            btnAdd.Text = "과제 추가";
            btnAdd.Location = new Point(30, 450);
            btnAdd.Size = new Size(120, 40);
            btnAdd.Click += btnAddTask_Click;

            Button btnDelete = new Button();
            btnDelete.Name = "btnDeleteTask";
            btnDelete.Text = "과제 삭제";
            btnDelete.Location = new Point(170, 450);
            btnDelete.Size = new Size(120, 40);
            btnDelete.Click += btnDeleteTask_Click;

            panelTaskView.Controls.Add(lblTitle);
            panelTaskView.Controls.Add(dgvTasks);
            panelTaskView.Controls.Add(btnAdd);
            panelTaskView.Controls.Add(btnDelete);
        }

        private void SetupQuizView()
        {
            Label lblTitle = new Label();
            lblTitle.Text = "복습 퀴즈";
            lblTitle.Location = new Point(30, 30);
            lblTitle.Size = new Size(200, 30);
            lblTitle.Font = new Font("맑은 고딕", 12, FontStyle.Bold);

            Label lblQuestion = new Label();
            lblQuestion.Text = "문제: 프로젝트 데이터 저장 형식은 무엇인가요?";
            lblQuestion.Location = new Point(30, 80);
            lblQuestion.Size = new Size(760, 40);
            lblQuestion.Font = new Font("맑은 고딕", 11);

            TextBox txtAnswer = new TextBox();
            txtAnswer.Name = "txtAnswer";
            txtAnswer.Location = new Point(30, 140);
            txtAnswer.Size = new Size(400, 30);

            Button btnCheck = new Button();
            btnCheck.Name = "btnCheckAnswer";
            btnCheck.Text = "정답 확인";
            btnCheck.Location = new Point(450, 138);
            btnCheck.Size = new Size(120, 35);
            btnCheck.Click += btnCheckAnswer_Click;

            Label lblResult = new Label();
            lblResult.Name = "lblQuizResult";
            lblResult.Text = "결과 표시 영역";
            lblResult.Location = new Point(30, 200);
            lblResult.Size = new Size(760, 40);
            lblResult.Font = new Font("맑은 고딕", 11);
            lblResult.BorderStyle = BorderStyle.FixedSingle;
            lblResult.BackColor = Color.White;
            lblResult.TextAlign = ContentAlignment.MiddleLeft;

            panelQuizView.Controls.Add(lblTitle);
            panelQuizView.Controls.Add(lblQuestion);
            panelQuizView.Controls.Add(txtAnswer);
            panelQuizView.Controls.Add(btnCheck);
            panelQuizView.Controls.Add(lblResult);
        }

        private void ResetMenuButtons()
        {
            btnMeeting.BackColor = Color.Gainsboro;
            btnTask.BackColor = Color.Gainsboro;
            btnQuiz.BackColor = Color.Gainsboro;
        }

        private void ShowView(Panel targetPanel)
        {
            panelMeetingView.Visible = false;
            panelTaskView.Visible = false;
            panelQuizView.Visible = false;

            targetPanel.Visible = true;
            targetPanel.BringToFront();
        }

        private void btnMeeting_Click(object sender, EventArgs e)
        {
            ResetMenuButtons();
            btnMeeting.BackColor = Color.LightSkyBlue;
            ShowView(panelMeetingView);
        }

        private void btnTask_Click(object sender, EventArgs e)
        {
            ResetMenuButtons();
            btnTask.BackColor = Color.LightGreen;
            ShowView(panelTaskView);
        }

        private void btnQuiz_Click(object sender, EventArgs e)
        {
            ResetMenuButtons();
            btnQuiz.BackColor = Color.Khaki;
            ShowView(panelQuizView);
        }

        private void btnSummary_Click(object sender, EventArgs e)
        {
            RichTextBox input =
                (RichTextBox)panelMeetingView.Controls["rtbMeetingInput"];

            RichTextBox result =
                (RichTextBox)panelMeetingView.Controls["rtbMeetingResult"];

            if (string.IsNullOrWhiteSpace(input.Text))
            {
                result.Text = "회의 내용을 먼저 입력해주세요.";
                return;
            }

            result.Text =
                "회의 요약\n\n" +
                "- 주요 내용: " +
                input.Text.Substring(0, Math.Min(50, input.Text.Length));
        }

        private void btnAddTask_Click(object sender, EventArgs e)
        {
            DataGridView dgvTasks =
                (DataGridView)panelTaskView.Controls["dgvTasks"];

            dgvTasks.Rows.Add("", "", "", "미시작");
        }

        private void btnDeleteTask_Click(object sender, EventArgs e)
        {
            DataGridView dgvTasks =
                (DataGridView)panelTaskView.Controls["dgvTasks"];

            if (dgvTasks.CurrentRow != null && !dgvTasks.CurrentRow.IsNewRow)
            {
                dgvTasks.Rows.Remove(dgvTasks.CurrentRow);
            }
        }

        private void btnCheckAnswer_Click(object sender, EventArgs e)
        {
            TextBox txtAnswer =
                (TextBox)panelQuizView.Controls["txtAnswer"];

            Label lblResult =
                (Label)panelQuizView.Controls["lblQuizResult"];

            string answer = txtAnswer.Text.Trim().ToLower();

            if (answer == "json" || answer == "제이슨")
            {
                lblResult.Text = "정답입니다!";
                lblResult.ForeColor = Color.Green;
            }
            else
            {
                lblResult.Text = "오답입니다. 정답은 JSON입니다.";
                lblResult.ForeColor = Color.Red;
            }
        }
    }
}