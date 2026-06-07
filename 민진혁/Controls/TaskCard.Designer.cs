namespace WinFormsAppAIsupporter.Controls
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
            this.lblContent = new System.Windows.Forms.Label();
            this.lblAssignee = new System.Windows.Forms.Label();
            this.lblDueDate = new System.Windows.Forms.Label();
            this.btnNextStatus = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.pnlPriority = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // lblContent
            // 
            this.lblContent.AutoSize = true;
            this.lblContent.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblContent.Location = new System.Drawing.Point(15, 10);
            this.lblContent.Name = "lblContent";
            this.lblContent.Size = new System.Drawing.Size(72, 19);
            this.lblContent.TabIndex = 0;
            this.lblContent.Text = "과제 내용";
            this.lblContent.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TaskCard_MouseDown);
            this.lblContent.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TaskCard_MouseMove);
            this.lblContent.DoubleClick += new System.EventHandler(this.TaskCard_DoubleClick);
            // 
            // lblAssignee
            // 
            this.lblAssignee.AutoSize = true;
            this.lblAssignee.Location = new System.Drawing.Point(15, 40);
            this.lblAssignee.Name = "lblAssignee";
            this.lblAssignee.Size = new System.Drawing.Size(43, 15);
            this.lblAssignee.TabIndex = 1;
            this.lblAssignee.Text = "담당자";
            this.lblAssignee.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TaskCard_MouseDown);
            this.lblAssignee.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TaskCard_MouseMove);
            this.lblAssignee.DoubleClick += new System.EventHandler(this.TaskCard_DoubleClick);
            // 
            // lblDueDate
            // 
            this.lblDueDate.AutoSize = true;
            this.lblDueDate.Location = new System.Drawing.Point(15, 65);
            this.lblDueDate.Name = "lblDueDate";
            this.lblDueDate.Size = new System.Drawing.Size(43, 15);
            this.lblDueDate.TabIndex = 2;
            this.lblDueDate.Text = "마감일";
            this.lblDueDate.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TaskCard_MouseDown);
            this.lblDueDate.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TaskCard_MouseMove);
            this.lblDueDate.DoubleClick += new System.EventHandler(this.TaskCard_DoubleClick);
            // 
            // btnNextStatus
            // 
            this.btnNextStatus.Location = new System.Drawing.Point(150, 60);
            this.btnNextStatus.Name = "btnNextStatus";
            this.btnNextStatus.Size = new System.Drawing.Size(90, 25);
            this.btnNextStatus.TabIndex = 3;
            this.btnNextStatus.Text = "진행 ->";
            this.btnNextStatus.UseVisualStyleBackColor = true;
            this.btnNextStatus.Click += new System.EventHandler(this.btnNextStatus_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.MistyRose;
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("맑은 고딕", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnDelete.Location = new System.Drawing.Point(225, 5);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(20, 20);
            this.btnDelete.TabIndex = 4;
            this.btnDelete.Text = "X";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // pnlPriority
            // 
            this.pnlPriority.BackColor = System.Drawing.Color.Gray;
            this.pnlPriority.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlPriority.Location = new System.Drawing.Point(5, 5);
            this.pnlPriority.Name = "pnlPriority";
            this.pnlPriority.Size = new System.Drawing.Size(5, 90);
            this.pnlPriority.TabIndex = 5;
            this.pnlPriority.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TaskCard_MouseDown);
            this.pnlPriority.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TaskCard_MouseMove);
            this.pnlPriority.DoubleClick += new System.EventHandler(this.TaskCard_DoubleClick);
            // 
            // TaskCard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.pnlPriority);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnNextStatus);
            this.Controls.Add(this.lblDueDate);
            this.Controls.Add(this.lblAssignee);
            this.Controls.Add(this.lblContent);
            this.Name = "TaskCard";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.Size = new System.Drawing.Size(250, 100);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TaskCard_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TaskCard_MouseMove);
            this.DoubleClick += new System.EventHandler(this.TaskCard_DoubleClick);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblContent;
        private System.Windows.Forms.Label lblAssignee;
        private System.Windows.Forms.Label lblDueDate;
        private System.Windows.Forms.Button btnNextStatus;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Panel pnlPriority;
    }
}
