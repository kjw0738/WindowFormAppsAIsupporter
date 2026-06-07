namespace WinFormsAppAIsupporter
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panelHeader = new Panel();
            lblTitle = new Label();
            panelSidebar = new Panel();
            btnQuiz = new Button();
            btnTask = new Button();
            btnMeeting = new Button();
            panelContent = new Panel();
            panelQuizView = new Panel();
            panelTaskView = new Panel();
            panelMeetingView = new Panel();
            panelHeader.SuspendLayout();
            panelSidebar.SuspendLayout();
            panelContent.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.Navy;
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1084, 70);
            panelHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Font = new Font("맑은 고딕", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 20);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(325, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "AI 회의록 & 팀 과제 정리 도우미";
            // 
            // panelSidebar
            // 
            panelSidebar.BackColor = Color.Gainsboro;
            panelSidebar.Controls.Add(btnQuiz);
            panelSidebar.Controls.Add(btnTask);
            panelSidebar.Controls.Add(btnMeeting);
            panelSidebar.Dock = DockStyle.Left;
            panelSidebar.Location = new Point(0, 70);
            panelSidebar.Name = "panelSidebar";
            panelSidebar.Size = new Size(220, 591);
            panelSidebar.TabIndex = 1;
            // 
            // btnQuiz
            // 
            btnQuiz.FlatStyle = FlatStyle.Flat;
            btnQuiz.Font = new Font("맑은 고딕", 11F);
            btnQuiz.Location = new Point(20, 170);
            btnQuiz.Name = "btnQuiz";
            btnQuiz.Size = new Size(180, 50);
            btnQuiz.TabIndex = 2;
            btnQuiz.Text = "복습 퀴즈";
            btnQuiz.UseVisualStyleBackColor = true;
            btnQuiz.Click += btnQuiz_Click;
            // 
            // btnTask
            // 
            btnTask.FlatStyle = FlatStyle.Flat;
            btnTask.Font = new Font("맑은 고딕", 11F);
            btnTask.Location = new Point(20, 100);
            btnTask.Name = "btnTask";
            btnTask.Size = new Size(180, 50);
            btnTask.TabIndex = 1;
            btnTask.Text = "과제 관리";
            btnTask.UseVisualStyleBackColor = true;
            btnTask.Click += btnTask_Click;
            // 
            // btnMeeting
            // 
            btnMeeting.Anchor = AnchorStyles.Top;
            btnMeeting.FlatStyle = FlatStyle.Flat;
            btnMeeting.Font = new Font("맑은 고딕", 11F);
            btnMeeting.Location = new Point(20, 30);
            btnMeeting.Name = "btnMeeting";
            btnMeeting.Size = new Size(180, 50);
            btnMeeting.TabIndex = 0;
            btnMeeting.Text = "AI 회의록";
            btnMeeting.UseVisualStyleBackColor = true;
            btnMeeting.Click += btnMeeting_Click;
            // 
            // panelContent
            // 
            panelContent.BackColor = Color.White;
            panelContent.Controls.Add(panelQuizView);
            panelContent.Controls.Add(panelTaskView);
            panelContent.Controls.Add(panelMeetingView);
            panelContent.Dock = DockStyle.Fill;
            panelContent.Location = new Point(220, 70);
            panelContent.Name = "panelContent";
            panelContent.Size = new Size(864, 591);
            panelContent.TabIndex = 2;
            // 
            // panelQuizView
            // 
            panelQuizView.BackColor = Color.LightYellow;
            panelQuizView.Dock = DockStyle.Fill;
            panelQuizView.Location = new Point(0, 0);
            panelQuizView.Name = "panelQuizView";
            panelQuizView.Size = new Size(864, 591);
            panelQuizView.TabIndex = 2;
            panelQuizView.Visible = false;
            // 
            // panelTaskView
            // 
            panelTaskView.BackColor = Color.LightGreen;
            panelTaskView.Dock = DockStyle.Fill;
            panelTaskView.Location = new Point(0, 0);
            panelTaskView.Name = "panelTaskView";
            panelTaskView.Size = new Size(864, 591);
            panelTaskView.TabIndex = 1;
            panelTaskView.Visible = false;
            // 
            // panelMeetingView
            // 
            panelMeetingView.BackColor = Color.LightBlue;
            panelMeetingView.Dock = DockStyle.Fill;
            panelMeetingView.Location = new Point(0, 0);
            panelMeetingView.Name = "panelMeetingView";
            panelMeetingView.Size = new Size(864, 591);
            panelMeetingView.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1084, 661);
            Controls.Add(panelContent);
            Controls.Add(panelSidebar);
            Controls.Add(panelHeader);
            MinimumSize = new Size(1100, 700);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "AI 회의록 & 팀 과제 정리 도우미";
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            panelSidebar.ResumeLayout(false);
            panelContent.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panelHeader;
        private Label lblTitle;
        private Panel panelSidebar;
        private Panel panelContent;
        private Button btnQuiz;
        private Button btnTask;
        private Button btnMeeting;
        private Panel panelQuizView;
        private Panel panelTaskView;
        private Panel panelMeetingView;
    }
}
