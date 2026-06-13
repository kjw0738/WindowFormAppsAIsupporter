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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            navPanel = new Panel();
            btnNavInput = new Button();
            btnNavSwot = new Button();
            btnNavRoles = new Button();
            panelInput = new Panel();
            lblTitle = new Label();
            btnAttachVoice = new Button();
            lblFilePath = new Label();
            lblParticipants = new Label();
            numParticipants = new NumericUpDown();
            btnStartAnalysis = new Button();
            lblStatus = new Label();
            panelSwot = new Panel();
            lblSwotTitle = new Label();
            txtS = new TextBox();
            txtW = new TextBox();
            txtO = new TextBox();
            txtT = new TextBox();
            lblS = new Label();
            lblW = new Label();
            lblO = new Label();
            lblT = new Label();
            panelRoles = new Panel();
            lblRolesTitle = new Label();
            flowLayoutPanelRoles = new FlowLayoutPanel();
            navPanel.SuspendLayout();
            panelInput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numParticipants).BeginInit();
            panelSwot.SuspendLayout();
            panelRoles.SuspendLayout();
            SuspendLayout();
            // 
            // navPanel
            // 
            navPanel.BackColor = Color.LightGray;
            navPanel.Controls.Add(btnNavInput);
            navPanel.Controls.Add(btnNavSwot);
            navPanel.Controls.Add(btnNavRoles);
            navPanel.Dock = DockStyle.Top;
            navPanel.Location = new Point(0, 0);
            navPanel.Margin = new Padding(4, 4, 4, 4);
            navPanel.Name = "navPanel";
            navPanel.Size = new Size(1029, 67);
            navPanel.TabIndex = 3;
            // 
            // btnNavInput
            // 
            btnNavInput.Location = new Point(15, 13);
            btnNavInput.Margin = new Padding(4, 4, 4, 4);
            btnNavInput.Name = "btnNavInput";
            btnNavInput.Size = new Size(154, 40);
            btnNavInput.TabIndex = 0;
            btnNavInput.Text = "음성 입력";
            btnNavInput.Click += BtnNav_Click;
            // 
            // btnNavSwot
            // 
            btnNavSwot.Location = new Point(177, 13);
            btnNavSwot.Margin = new Padding(4, 4, 4, 4);
            btnNavSwot.Name = "btnNavSwot";
            btnNavSwot.Size = new Size(154, 40);
            btnNavSwot.TabIndex = 1;
            btnNavSwot.Text = "SWOT 분석";
            btnNavSwot.Click += BtnNav_Click;
            // 
            // btnNavRoles
            // 
            btnNavRoles.Location = new Point(339, 13);
            btnNavRoles.Margin = new Padding(4, 4, 4, 4);
            btnNavRoles.Name = "btnNavRoles";
            btnNavRoles.Size = new Size(154, 40);
            btnNavRoles.TabIndex = 2;
            btnNavRoles.Text = "역할 분배";
            btnNavRoles.Click += BtnNav_Click;
            // 
            // panelInput
            // 
            panelInput.Controls.Add(lblTitle);
            panelInput.Controls.Add(btnAttachVoice);
            panelInput.Controls.Add(lblFilePath);
            panelInput.Controls.Add(lblParticipants);
            panelInput.Controls.Add(numParticipants);
            panelInput.Controls.Add(btnStartAnalysis);
            panelInput.Controls.Add(lblStatus);
            panelInput.Dock = DockStyle.Fill;
            panelInput.Location = new Point(0, 67);
            panelInput.Margin = new Padding(4, 4, 4, 4);
            panelInput.Name = "panelInput";
            panelInput.Size = new Size(1029, 533);
            panelInput.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("맑은 고딕", 16F, FontStyle.Bold);
            lblTitle.Location = new Point(0, 40);
            lblTitle.Margin = new Padding(4, 0, 4, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(1029, 53);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "AI 회의록 분석 시스템";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnAttachVoice
            // 
            btnAttachVoice.Location = new Point(386, 133);
            btnAttachVoice.Margin = new Padding(4, 4, 4, 4);
            btnAttachVoice.Name = "btnAttachVoice";
            btnAttachVoice.Size = new Size(257, 53);
            btnAttachVoice.TabIndex = 1;
            btnAttachVoice.Text = "음성 파일 첨부 (.mp3, .m4a, .wav)";
            btnAttachVoice.Click += BtnAttachVoice_Click;
            // 
            // lblFilePath
            // 
            lblFilePath.Location = new Point(0, 200);
            lblFilePath.Margin = new Padding(4, 0, 4, 0);
            lblFilePath.Name = "lblFilePath";
            lblFilePath.Size = new Size(1029, 27);
            lblFilePath.TabIndex = 2;
            lblFilePath.Text = "선택된 파일 없음";
            lblFilePath.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblParticipants
            // 
            lblParticipants.Location = new Point(411, 253);
            lblParticipants.Margin = new Padding(4, 0, 4, 0);
            lblParticipants.Name = "lblParticipants";
            lblParticipants.Size = new Size(103, 33);
            lblParticipants.TabIndex = 3;
            lblParticipants.Text = "참여 인원수:";
            lblParticipants.TextAlign = ContentAlignment.MiddleRight;
            // 
            // numParticipants
            // 
            numParticipants.Location = new Point(527, 256);
            numParticipants.Margin = new Padding(4, 4, 4, 4);
            numParticipants.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numParticipants.Name = "numParticipants";
            numParticipants.Size = new Size(154, 27);
            numParticipants.TabIndex = 4;
            numParticipants.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // btnStartAnalysis
            // 
            btnStartAnalysis.BackColor = Color.SkyBlue;
            btnStartAnalysis.FlatStyle = FlatStyle.Flat;
            btnStartAnalysis.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);
            btnStartAnalysis.Location = new Point(386, 333);
            btnStartAnalysis.Margin = new Padding(4, 4, 4, 4);
            btnStartAnalysis.Name = "btnStartAnalysis";
            btnStartAnalysis.Size = new Size(257, 67);
            btnStartAnalysis.TabIndex = 5;
            btnStartAnalysis.Text = "회의 분석 시작";
            btnStartAnalysis.UseVisualStyleBackColor = false;
            btnStartAnalysis.Click += BtnStartAnalysis_Click;
            // 
            // lblStatus
            // 
            lblStatus.Location = new Point(0, 413);
            lblStatus.Margin = new Padding(4, 0, 4, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1029, 33);
            lblStatus.TabIndex = 6;
            lblStatus.Text = "준비 완료";
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelSwot
            // 
            panelSwot.Controls.Add(lblSwotTitle);
            panelSwot.Controls.Add(txtS);
            panelSwot.Controls.Add(txtW);
            panelSwot.Controls.Add(txtO);
            panelSwot.Controls.Add(txtT);
            panelSwot.Controls.Add(lblS);
            panelSwot.Controls.Add(lblW);
            panelSwot.Controls.Add(lblO);
            panelSwot.Controls.Add(lblT);
            panelSwot.Dock = DockStyle.Fill;
            panelSwot.Location = new Point(0, 67);
            panelSwot.Margin = new Padding(4, 4, 4, 4);
            panelSwot.Name = "panelSwot";
            panelSwot.Size = new Size(1029, 533);
            panelSwot.TabIndex = 1;
            panelSwot.Visible = false;
            // 
            // lblSwotTitle
            // 
            lblSwotTitle.Font = new Font("맑은 고딕", 14F, FontStyle.Bold);
            lblSwotTitle.Location = new Point(26, 13);
            lblSwotTitle.Margin = new Padding(4, 0, 4, 0);
            lblSwotTitle.Name = "lblSwotTitle";
            lblSwotTitle.Size = new Size(257, 40);
            lblSwotTitle.TabIndex = 0;
            lblSwotTitle.Text = "SWOT 분석 결과";
            // 
            // txtS
            // 
            txtS.Location = new Point(26, 87);
            txtS.Margin = new Padding(4, 4, 4, 4);
            txtS.Multiline = true;
            txtS.Name = "txtS";
            txtS.ReadOnly = true;
            txtS.ScrollBars = ScrollBars.Vertical;
            txtS.Size = new Size(481, 185);
            txtS.TabIndex = 1;
            // 
            // txtW
            // 
            txtW.Location = new Point(521, 87);
            txtW.Margin = new Padding(4, 4, 4, 4);
            txtW.Multiline = true;
            txtW.Name = "txtW";
            txtW.ReadOnly = true;
            txtW.ScrollBars = ScrollBars.Vertical;
            txtW.Size = new Size(481, 185);
            txtW.TabIndex = 2;
            // 
            // txtO
            // 
            txtO.Location = new Point(26, 313);
            txtO.Margin = new Padding(4, 4, 4, 4);
            txtO.Multiline = true;
            txtO.Name = "txtO";
            txtO.ReadOnly = true;
            txtO.ScrollBars = ScrollBars.Vertical;
            txtO.Size = new Size(481, 185);
            txtO.TabIndex = 3;
            // 
            // txtT
            // 
            txtT.Location = new Point(521, 313);
            txtT.Margin = new Padding(4, 4, 4, 4);
            txtT.Multiline = true;
            txtT.Name = "txtT";
            txtT.ReadOnly = true;
            txtT.ScrollBars = ScrollBars.Vertical;
            txtT.Size = new Size(481, 185);
            txtT.TabIndex = 4;
            // 
            // lblS
            // 
            lblS.AutoSize = true;
            lblS.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
            lblS.Location = new Point(26, 60);
            lblS.Margin = new Padding(4, 0, 4, 0);
            lblS.Name = "lblS";
            lblS.Size = new Size(122, 20);
            lblS.TabIndex = 5;
            lblS.Text = "Strengths (강점)";
            // 
            // lblW
            // 
            lblW.AutoSize = true;
            lblW.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
            lblW.Location = new Point(521, 60);
            lblW.Margin = new Padding(4, 0, 4, 0);
            lblW.Name = "lblW";
            lblW.Size = new Size(139, 20);
            lblW.TabIndex = 6;
            lblW.Text = "Weaknesses (약점)";
            // 
            // lblO
            // 
            lblO.AutoSize = true;
            lblO.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
            lblO.Location = new Point(26, 287);
            lblO.Margin = new Padding(4, 0, 4, 0);
            lblO.Name = "lblO";
            lblO.Size = new Size(152, 20);
            lblO.TabIndex = 7;
            lblO.Text = "Opportunities (기회)";
            // 
            // lblT
            // 
            lblT.AutoSize = true;
            lblT.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
            lblT.Location = new Point(521, 287);
            lblT.Margin = new Padding(4, 0, 4, 0);
            lblT.Name = "lblT";
            lblT.Size = new Size(107, 20);
            lblT.TabIndex = 8;
            lblT.Text = "Threats (위협)";
            // 
            // panelRoles
            // 
            panelRoles.Controls.Add(lblRolesTitle);
            panelRoles.Controls.Add(flowLayoutPanelRoles);
            panelRoles.Dock = DockStyle.Fill;
            panelRoles.Location = new Point(0, 67);
            panelRoles.Margin = new Padding(4, 4, 4, 4);
            panelRoles.Name = "panelRoles";
            panelRoles.Size = new Size(1029, 533);
            panelRoles.TabIndex = 2;
            panelRoles.Visible = false;
            // 
            // lblRolesTitle
            // 
            lblRolesTitle.Font = new Font("맑은 고딕", 14F, FontStyle.Bold);
            lblRolesTitle.Location = new Point(26, 13);
            lblRolesTitle.Margin = new Padding(4, 0, 4, 0);
            lblRolesTitle.Name = "lblRolesTitle";
            lblRolesTitle.Size = new Size(257, 40);
            lblRolesTitle.TabIndex = 0;
            lblRolesTitle.Text = "지능형 역할 분배";
            // 
            // flowLayoutPanelRoles
            // 
            flowLayoutPanelRoles.AutoScroll = true;
            flowLayoutPanelRoles.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanelRoles.Location = new Point(26, 67);
            flowLayoutPanelRoles.Margin = new Padding(4, 4, 4, 4);
            flowLayoutPanelRoles.Name = "flowLayoutPanelRoles";
            flowLayoutPanelRoles.Size = new Size(977, 440);
            flowLayoutPanelRoles.TabIndex = 1;
            flowLayoutPanelRoles.WrapContents = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1029, 600);
            Controls.Add(panelInput);
            Controls.Add(panelSwot);
            Controls.Add(panelRoles);
            Controls.Add(navPanel);
            Margin = new Padding(4, 4, 4, 4);
            Name = "Form1";
            Text = "AI Meeting Assistant";
            navPanel.ResumeLayout(false);
            panelInput.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numParticipants).EndInit();
            panelSwot.ResumeLayout(false);
            panelSwot.PerformLayout();
            panelRoles.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel navPanel;
        private System.Windows.Forms.Button btnNavInput;
        private System.Windows.Forms.Button btnNavSwot;
        private System.Windows.Forms.Button btnNavRoles;
        
        private System.Windows.Forms.Panel panelInput;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnAttachVoice;
        private System.Windows.Forms.Label lblFilePath;
        private System.Windows.Forms.Label lblParticipants;
        private System.Windows.Forms.NumericUpDown numParticipants;
        private System.Windows.Forms.Button btnStartAnalysis;
        private System.Windows.Forms.Label lblStatus;

        private System.Windows.Forms.Panel panelSwot;
        private System.Windows.Forms.Label lblSwotTitle;
        private System.Windows.Forms.TextBox txtS;
        private System.Windows.Forms.TextBox txtW;
        private System.Windows.Forms.TextBox txtO;
        private System.Windows.Forms.TextBox txtT;
        private System.Windows.Forms.Label lblS;
        private System.Windows.Forms.Label lblW;
        private System.Windows.Forms.Label lblO;
        private System.Windows.Forms.Label lblT;

        private System.Windows.Forms.Panel panelRoles;
        private System.Windows.Forms.Label lblRolesTitle;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelRoles;
    }
}
