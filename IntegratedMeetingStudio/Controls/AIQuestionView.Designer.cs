namespace IntegratedMeetingStudio.Controls
{
    partial class AIQuestionView
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHistory = new System.Windows.Forms.Panel();
            this.lblHistory = new System.Windows.Forms.Label();
            this.lstHistory = new System.Windows.Forms.ListBox();
            this.pnlMainContent = new System.Windows.Forms.Panel();
            this.lblMeetingSummary = new System.Windows.Forms.Label();
            this.txtMeetingSummary = new System.Windows.Forms.TextBox();
            this.btnGenerateQuestions = new System.Windows.Forms.Button();
            this.pnlQuestions = new System.Windows.Forms.FlowLayoutPanel();
            this.lblQuestionsTitle = new System.Windows.Forms.Label();
            this.btnExtractTasks = new System.Windows.Forms.Button();
            this.lblApiKey = new System.Windows.Forms.Label();
            this.txtApiKey = new System.Windows.Forms.TextBox();
            this.pnlHistory.SuspendLayout();
            this.pnlMainContent.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHistory
            // 
            this.pnlHistory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(250)))));
            this.pnlHistory.Controls.Add(this.lblHistory);
            this.pnlHistory.Controls.Add(this.lstHistory);
            this.pnlHistory.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlHistory.Location = new System.Drawing.Point(0, 0);
            this.pnlHistory.Name = "pnlHistory";
            this.pnlHistory.Padding = new System.Windows.Forms.Padding(10);
            this.pnlHistory.Size = new System.Drawing.Size(250, 661);
            this.pnlHistory.TabIndex = 8;
            // 
            // lblHistory
            // 
            this.lblHistory.AutoSize = true;
            this.lblHistory.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblHistory.Location = new System.Drawing.Point(10, 10);
            this.lblHistory.Name = "lblHistory";
            this.lblHistory.Size = new System.Drawing.Size(95, 15);
            this.lblHistory.TabIndex = 1;
            this.lblHistory.Text = "📝 과거 분석 내역";
            // 
            // lstHistory
            // 
            this.lstHistory.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstHistory.FormattingEnabled = true;
            this.lstHistory.HorizontalScrollbar = true;
            this.lstHistory.ItemHeight = 15;
            this.lstHistory.Location = new System.Drawing.Point(10, 40);
            this.lstHistory.Name = "lstHistory";
            this.lstHistory.Size = new System.Drawing.Size(230, 600);
            this.lstHistory.TabIndex = 0;
            this.lstHistory.HorizontalExtent = 500;
            this.lstHistory.SelectedIndexChanged += new System.EventHandler(this.lstHistory_SelectedIndexChanged);

            // 
            // pnlMainContent
            // 
            this.pnlMainContent.Controls.Add(this.lblMeetingSummary);
            this.pnlMainContent.Controls.Add(this.txtMeetingSummary);
            this.pnlMainContent.Controls.Add(this.btnGenerateQuestions);
            this.pnlMainContent.Controls.Add(this.pnlQuestions);
            this.pnlMainContent.Controls.Add(this.lblQuestionsTitle);
            this.pnlMainContent.Controls.Add(this.btnExtractTasks);
            this.pnlMainContent.Controls.Add(this.lblApiKey);
            this.pnlMainContent.Controls.Add(this.txtApiKey);
            this.pnlMainContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMainContent.Location = new System.Drawing.Point(250, 0);
            this.pnlMainContent.Name = "pnlMainContent";
            this.pnlMainContent.Size = new System.Drawing.Size(534, 661);
            this.pnlMainContent.TabIndex = 9;
            // 
            // lblMeetingSummary
            // 
            this.lblMeetingSummary.AutoSize = true;
            this.lblMeetingSummary.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblMeetingSummary.Location = new System.Drawing.Point(20, 45);
            this.lblMeetingSummary.Name = "lblMeetingSummary";
            this.lblMeetingSummary.Size = new System.Drawing.Size(126, 19);
            this.lblMeetingSummary.TabIndex = 0;
            this.lblMeetingSummary.Text = "회의록 요약 (입력)";
            // 
            // txtMeetingSummary
            // 
            this.txtMeetingSummary.Location = new System.Drawing.Point(20, 75);
            this.txtMeetingSummary.Multiline = true;
            this.txtMeetingSummary.Name = "txtMeetingSummary";
            this.txtMeetingSummary.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMeetingSummary.Size = new System.Drawing.Size(560, 125);
            this.txtMeetingSummary.TabIndex = 1;
            this.txtMeetingSummary.Text = "예시: 오늘 회의에서는 JSON 데이터 스키마를 확정했습니다. 김철수 님이 내일까지 모델 클래스를 작성하기로 했고, 이영희 님은 다음주 월요일까지 UI 레이아웃" +
    " 초안을 완성하기로 했습니다.";
            // 
            // btnGenerateQuestions
            // 
            this.btnGenerateQuestions.BackColor = System.Drawing.Color.MediumSlateBlue;
            this.btnGenerateQuestions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerateQuestions.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnGenerateQuestions.ForeColor = System.Drawing.Color.White;
            this.btnGenerateQuestions.Location = new System.Drawing.Point(20, 210);
            this.btnGenerateQuestions.Name = "btnGenerateQuestions";
            this.btnGenerateQuestions.Size = new System.Drawing.Size(130, 40);
            this.btnGenerateQuestions.TabIndex = 2;
            this.btnGenerateQuestions.Text = "🤖 질문 생성";
            this.btnGenerateQuestions.UseVisualStyleBackColor = false;
            this.btnGenerateQuestions.Click += new System.EventHandler(this.btnGenerateQuestions_Click);
            // 
            // pnlQuestions
            // 
            this.pnlQuestions.AutoScroll = true;
            this.pnlQuestions.BackColor = System.Drawing.Color.White;
            this.pnlQuestions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlQuestions.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pnlQuestions.Location = new System.Drawing.Point(20, 290);
            this.pnlQuestions.Name = "pnlQuestions";
            this.pnlQuestions.Padding = new System.Windows.Forms.Padding(10);
            this.pnlQuestions.Size = new System.Drawing.Size(560, 340);
            this.pnlQuestions.TabIndex = 3;
            this.pnlQuestions.WrapContents = false;
            // 
            // lblQuestionsTitle
            // 
            this.lblQuestionsTitle.AutoSize = true;
            this.lblQuestionsTitle.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblQuestionsTitle.Location = new System.Drawing.Point(20, 265);
            this.lblQuestionsTitle.Name = "lblQuestionsTitle";
            this.lblQuestionsTitle.Size = new System.Drawing.Size(117, 19);
            this.lblQuestionsTitle.TabIndex = 4;
            this.lblQuestionsTitle.Text = "생성된 복습 질문";
            // 
            // btnExtractTasks
            // 
            this.btnExtractTasks.BackColor = System.Drawing.Color.SeaGreen;
            this.btnExtractTasks.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExtractTasks.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnExtractTasks.ForeColor = System.Drawing.Color.White;
            this.btnExtractTasks.Location = new System.Drawing.Point(160, 210);
            this.btnExtractTasks.Name = "btnExtractTasks";
            this.btnExtractTasks.Size = new System.Drawing.Size(130, 40);
            this.btnExtractTasks.TabIndex = 5;
            this.btnExtractTasks.Text = "📋 할일 추출";
            this.btnExtractTasks.UseVisualStyleBackColor = false;
            this.btnExtractTasks.Click += new System.EventHandler(this.btnExtractTasks_Click);
            // 
            // lblApiKey
            // 
            this.lblApiKey.AutoSize = true;
            this.lblApiKey.Location = new System.Drawing.Point(20, 15);
            this.lblApiKey.Name = "lblApiKey";
            this.lblApiKey.Size = new System.Drawing.Size(83, 15);
            this.lblApiKey.TabIndex = 6;
            this.lblApiKey.Text = "Groq API Key:";
            // 
            // txtApiKey
            // 
            this.txtApiKey.Location = new System.Drawing.Point(110, 12);
            this.txtApiKey.Name = "txtApiKey";
            this.txtApiKey.PasswordChar = '*';
            this.txtApiKey.Size = new System.Drawing.Size(460, 23);
            this.txtApiKey.TabIndex = 7;
            // 
            // AIQuestionView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlMainContent);
            this.Controls.Add(this.pnlHistory);
            this.Name = "AIQuestionView";
            this.Size = new System.Drawing.Size(784, 661);
            this.pnlHistory.ResumeLayout(false);
            this.pnlHistory.PerformLayout();
            this.pnlMainContent.ResumeLayout(false);
            this.pnlMainContent.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel pnlHistory;
        private System.Windows.Forms.Label lblHistory;
        private System.Windows.Forms.ListBox lstHistory;
        private System.Windows.Forms.Panel pnlMainContent;
        private System.Windows.Forms.Label lblMeetingSummary;
        private System.Windows.Forms.TextBox txtMeetingSummary;
        private System.Windows.Forms.Button btnGenerateQuestions;
        private System.Windows.Forms.FlowLayoutPanel pnlQuestions;
        private System.Windows.Forms.Label lblQuestionsTitle;
        private System.Windows.Forms.Button btnExtractTasks;
        private System.Windows.Forms.Label lblApiKey;
        private System.Windows.Forms.TextBox txtApiKey;
    }
}
