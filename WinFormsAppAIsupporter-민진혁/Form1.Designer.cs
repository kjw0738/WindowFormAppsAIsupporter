namespace WinFormsAppAIsupporter
{
    partial class Form1
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
            pnlSidebar = new Panel();
            lblTitle = new Label();
            btnMenuAIQuestion = new Button();
            btnMenuKanban = new Button();
            pnlMain = new Panel();
            pnlSidebar.SuspendLayout();
            SuspendLayout();
            // 
            // pnlSidebar
            // 
            pnlSidebar.BackColor = Color.FromArgb(45, 45, 48);
            pnlSidebar.Controls.Add(lblTitle);
            pnlSidebar.Controls.Add(btnMenuAIQuestion);
            pnlSidebar.Controls.Add(btnMenuKanban);
            pnlSidebar.Dock = DockStyle.Left;
            pnlSidebar.Location = new Point(0, 0);
            pnlSidebar.Margin = new Padding(4, 5, 4, 5);
            pnlSidebar.Name = "pnlSidebar";
            pnlSidebar.Size = new Size(286, 1142);
            pnlSidebar.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);
            lblTitle.ForeColor = Color.SkyBlue;
            lblTitle.Location = new Point(17, 33);
            lblTitle.Margin = new Padding(4, 0, 4, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(224, 32);
            lblTitle.TabIndex = 2;
            lblTitle.Text = "AI Meeting Helper";
            // 
            // btnMenuAIQuestion
            // 
            btnMenuAIQuestion.FlatAppearance.BorderSize = 0;
            btnMenuAIQuestion.FlatStyle = FlatStyle.Flat;
            btnMenuAIQuestion.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);
            btnMenuAIQuestion.ForeColor = Color.White;
            btnMenuAIQuestion.Location = new Point(0, 217);
            btnMenuAIQuestion.Margin = new Padding(4, 5, 4, 5);
            btnMenuAIQuestion.Name = "btnMenuAIQuestion";
            btnMenuAIQuestion.Size = new Size(286, 83);
            btnMenuAIQuestion.TabIndex = 1;
            btnMenuAIQuestion.Text = "🤖 AI 질문 생성";
            btnMenuAIQuestion.UseVisualStyleBackColor = true;
            btnMenuAIQuestion.Click += btnMenuAIQuestion_Click;
            // 
            // btnMenuKanban
            // 
            btnMenuKanban.FlatAppearance.BorderSize = 0;
            btnMenuKanban.FlatStyle = FlatStyle.Flat;
            btnMenuKanban.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);
            btnMenuKanban.ForeColor = Color.White;
            btnMenuKanban.Location = new Point(0, 133);
            btnMenuKanban.Margin = new Padding(4, 5, 4, 5);
            btnMenuKanban.Name = "btnMenuKanban";
            btnMenuKanban.Size = new Size(286, 83);
            btnMenuKanban.TabIndex = 0;
            btnMenuKanban.Text = "📋 To-Do 관리";
            btnMenuKanban.UseVisualStyleBackColor = true;
            btnMenuKanban.Click += btnMenuKanban_Click;
            // 
            // pnlMain
            // 
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.Location = new Point(286, 0);
            pnlMain.Margin = new Padding(4, 5, 4, 5);
            pnlMain.Name = "pnlMain";
            pnlMain.Size = new Size(1218, 1142);
            pnlMain.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1504, 1142);
            Controls.Add(pnlMain);
            Controls.Add(pnlSidebar);
            Margin = new Padding(4, 5, 4, 5);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "AI 회의록 & 과제 어시스턴트";
            pnlSidebar.ResumeLayout(false);
            pnlSidebar.PerformLayout();
            ResumeLayout(false);

        }

        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Button btnMenuKanban;
        private System.Windows.Forms.Button btnMenuAIQuestion;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.Label lblTitle;
    }
}
