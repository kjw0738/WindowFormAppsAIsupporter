namespace IntegratedMeetingStudio.Controls
{
    partial class TaskCard
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
            this.pnlPriority = new System.Windows.Forms.Panel();
            this.lblContent = new System.Windows.Forms.Label();
            this.btnDelete = new System.Windows.Forms.Button();
            this.lblAssignee = new System.Windows.Forms.Label();
            this.lblDueDate = new System.Windows.Forms.Label();
            this.btnNextStatus = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // pnlPriority
            // 
            this.pnlPriority.BackColor = System.Drawing.Color.Gray;
            this.pnlPriority.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlPriority.Location = new System.Drawing.Point(0, 0);
            this.pnlPriority.Name = "pnlPriority";
            this.pnlPriority.Size = new System.Drawing.Size(10, 130);
            this.pnlPriority.TabIndex = 0;
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.BackColor = System.Drawing.Color.MistyRose;
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("맑은 고딕", 7F, System.Drawing.FontStyle.Bold);
            this.btnDelete.Location = new System.Drawing.Point(210, 5);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(20, 20);
            this.btnDelete.TabIndex = 1;
            this.btnDelete.Text = "X";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // lblContent
            // 
            this.lblContent.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.lblContent.Location = new System.Drawing.Point(15, 8);
            this.lblContent.Name = "lblContent";
            this.lblContent.Size = new System.Drawing.Size(185, 40);
            this.lblContent.TabIndex = 2;
            this.lblContent.Text = "과제 내용";
            this.lblContent.DoubleClick += new System.EventHandler(this.TaskCard_DoubleClick);
            this.lblContent.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TaskCard_MouseDown);
            this.lblContent.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TaskCard_MouseMove);
            // 
            // lblAssignee
            // 
            this.lblAssignee.AutoSize = true;
            this.lblAssignee.Font = new System.Drawing.Font("맑은 고딕", 8F);
            this.lblAssignee.Location = new System.Drawing.Point(15, 48);
            this.lblAssignee.Name = "lblAssignee";
            this.lblAssignee.Size = new System.Drawing.Size(40, 13);
            this.lblAssignee.TabIndex = 3;
            this.lblAssignee.Text = "담당자";
            this.lblAssignee.DoubleClick += new System.EventHandler(this.TaskCard_DoubleClick);
            this.lblAssignee.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TaskCard_MouseDown);
            this.lblAssignee.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TaskCard_MouseMove);
            // 
            // lblDueDate
            // 
            this.lblDueDate.AutoSize = true;
            this.lblDueDate.Font = new System.Drawing.Font("맑은 고딕", 8F);
            this.lblDueDate.Location = new System.Drawing.Point(15, 68);
            this.lblDueDate.Name = "lblDueDate";
            this.lblDueDate.Size = new System.Drawing.Size(40, 13);
            this.lblDueDate.TabIndex = 4;
            this.lblDueDate.Text = "마감일";
            this.lblDueDate.DoubleClick += new System.EventHandler(this.TaskCard_DoubleClick);
            this.lblDueDate.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TaskCard_MouseDown);
            this.lblDueDate.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TaskCard_MouseMove);
            // 
            // btnNextStatus
            // 
            this.btnNextStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            // btnNextStatus
            // 
            this.btnNextStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNextStatus.Font = new System.Drawing.Font("맑은 고딕", 8F);
            this.btnNextStatus.Location = new System.Drawing.Point(145, 95);
            this.btnNextStatus.Name = "btnNextStatus";
            this.btnNextStatus.Size = new System.Drawing.Size(85, 25);
            this.btnNextStatus.TabIndex = 5;
            this.btnNextStatus.Text = "진행 ->";
            this.btnNextStatus.UseVisualStyleBackColor = true;
            this.btnNextStatus.Click += new System.EventHandler(this.btnNextStatus_Click);

            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.btnNextStatus);
            this.Controls.Add(this.lblDueDate);
            this.Controls.Add(this.lblAssignee);
            this.Controls.Add(this.lblContent);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.pnlPriority);
            this.Name = "TaskCard";
            this.Size = new System.Drawing.Size(240, 130);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Panel pnlPriority;
        private System.Windows.Forms.Label lblContent;
        private System.Windows.Forms.Label lblAssignee;
        private System.Windows.Forms.Label lblDueDate;
        private System.Windows.Forms.Button btnNextStatus;
        private System.Windows.Forms.Button btnDelete;
    }
}
