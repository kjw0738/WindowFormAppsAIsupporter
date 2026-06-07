namespace WinFormsAppAIsupporter
{
    partial class Form1
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pnlTodo = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlInProgress = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlDone = new System.Windows.Forms.FlowLayoutPanel();
            this.lblTodo = new System.Windows.Forms.Label();
            this.lblInProgress = new System.Windows.Forms.Label();
            this.lblDone = new System.Windows.Forms.Label();
            this.btnAddTask = new System.Windows.Forms.Button();
            this.btnSort = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.pgbProgress = new System.Windows.Forms.ProgressBar();
            this.lblProgressPercent = new System.Windows.Forms.Label();
            this.lblProgressTitle = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Controls.Add(this.pnlTodo, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.pnlInProgress, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.pnlDone, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblTodo, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblInProgress, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblDone, 2, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 80);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(900, 520);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // pnlTodo
            // 
            this.pnlTodo.AutoScroll = true;
            this.pnlTodo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.pnlTodo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTodo.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pnlTodo.Location = new System.Drawing.Point(3, 43);
            this.pnlTodo.Name = "pnlTodo";
            this.pnlTodo.Padding = new System.Windows.Forms.Padding(10);
            this.pnlTodo.Size = new System.Drawing.Size(294, 474);
            this.pnlTodo.TabIndex = 0;
            this.pnlTodo.WrapContents = false;
            // 
            // pnlInProgress
            // 
            this.pnlInProgress.AutoScroll = true;
            this.pnlInProgress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(240)))), ((int)(((byte)(255)))));
            this.pnlInProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlInProgress.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pnlInProgress.Location = new System.Drawing.Point(303, 43);
            this.pnlInProgress.Name = "pnlInProgress";
            this.pnlInProgress.Padding = new System.Windows.Forms.Padding(10);
            this.pnlInProgress.Size = new System.Drawing.Size(294, 474);
            this.pnlInProgress.TabIndex = 1;
            this.pnlInProgress.WrapContents = false;
            // 
            // pnlDone
            // 
            this.pnlDone.AutoScroll = true;
            this.pnlDone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(255)))), ((int)(((byte)(230)))));
            this.pnlDone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDone.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.pnlDone.Location = new System.Drawing.Point(603, 43);
            this.pnlDone.Name = "pnlDone";
            this.pnlDone.Padding = new System.Windows.Forms.Padding(10);
            this.pnlDone.Size = new System.Drawing.Size(294, 474);
            this.pnlDone.TabIndex = 2;
            this.pnlDone.WrapContents = false;
            // 
            // lblTodo
            // 
            this.lblTodo.AutoSize = true;
            this.lblTodo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblTodo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTodo.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTodo.Location = new System.Drawing.Point(3, 0);
            this.lblTodo.Name = "lblTodo";
            this.lblTodo.Size = new System.Drawing.Size(294, 40);
            this.lblTodo.TabIndex = 3;
            this.lblTodo.Text = "To-Do / 미시작 (0)";
            this.lblTodo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblInProgress
            // 
            this.lblInProgress.AutoSize = true;
            this.lblInProgress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(220)))), ((int)(((byte)(255)))));
            this.lblInProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblInProgress.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblInProgress.Location = new System.Drawing.Point(303, 0);
            this.lblInProgress.Name = "lblInProgress";
            this.lblInProgress.Size = new System.Drawing.Size(294, 40);
            this.lblInProgress.TabIndex = 4;
            this.lblInProgress.Text = "In Progress / 진행 중 (0)";
            this.lblInProgress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDone
            // 
            this.lblDone.AutoSize = true;
            this.lblDone.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(240)))), ((int)(((byte)(200)))));
            this.lblDone.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDone.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblDone.Location = new System.Drawing.Point(603, 0);
            this.lblDone.Name = "lblDone";
            this.lblDone.Size = new System.Drawing.Size(294, 40);
            this.lblDone.TabIndex = 5;
            this.lblDone.Text = "Done / 완료 (0)";
            this.lblDone.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnAddTask
            // 
            this.btnAddTask.BackColor = System.Drawing.Color.LightSkyBlue;
            this.btnAddTask.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddTask.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnAddTask.Location = new System.Drawing.Point(10, 10);
            this.btnAddTask.Name = "btnAddTask";
            this.btnAddTask.Size = new System.Drawing.Size(120, 30);
            this.btnAddTask.TabIndex = 1;
            this.btnAddTask.Text = "+ 새 과제 추가";
            this.btnAddTask.UseVisualStyleBackColor = false;
            this.btnAddTask.Click += new System.EventHandler(this.btnAddTask_Click);
            // 
            // btnSort
            // 
            this.btnSort.BackColor = System.Drawing.Color.LightGreen;
            this.btnSort.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSort.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnSort.Location = new System.Drawing.Point(140, 10);
            this.btnSort.Name = "btnSort";
            this.btnSort.Size = new System.Drawing.Size(130, 30);
            this.btnSort.TabIndex = 7;
            this.btnSort.Text = "📅 정렬 ON";
            this.btnSort.UseVisualStyleBackColor = false;
            this.btnSort.Click += new System.EventHandler(this.btnSort_Click);
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(650, 15);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.PlaceholderText = "검색어 입력 (내용/담당자)";
            this.txtSearch.Size = new System.Drawing.Size(230, 23);
            this.txtSearch.TabIndex = 2;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(600, 19);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(43, 15);
            this.lblSearch.TabIndex = 3;
            this.lblSearch.Text = "검색 :";
            // 
            // pgbProgress
            // 
            this.pgbProgress.Location = new System.Drawing.Point(150, 48);
            this.pgbProgress.Name = "pgbProgress";
            this.pgbProgress.Size = new System.Drawing.Size(650, 15);
            this.pgbProgress.TabIndex = 4;
            // 
            // lblProgressPercent
            // 
            this.lblProgressPercent.AutoSize = true;
            this.lblProgressPercent.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblProgressPercent.Location = new System.Drawing.Point(810, 48);
            this.lblProgressPercent.Name = "lblProgressPercent";
            this.lblProgressPercent.Size = new System.Drawing.Size(25, 15);
            this.lblProgressPercent.TabIndex = 5;
            this.lblProgressPercent.Text = "0%";
            // 
            // lblProgressTitle
            // 
            this.lblProgressTitle.AutoSize = true;
            this.lblProgressTitle.Location = new System.Drawing.Point(10, 48);
            this.lblProgressTitle.Name = "lblProgressTitle";
            this.lblProgressTitle.Size = new System.Drawing.Size(126, 15);
            this.lblProgressTitle.TabIndex = 6;
            this.lblProgressTitle.Text = "전체 과제 수행 진행률";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Controls.Add(this.btnSort);
            this.Controls.Add(this.lblProgressTitle);
            this.Controls.Add(this.lblProgressPercent);
            this.Controls.Add(this.pgbProgress);
            this.Controls.Add(this.lblSearch);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.btnAddTask);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AI 회의록 & 과제 도우미";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel pnlTodo;
        private System.Windows.Forms.FlowLayoutPanel pnlInProgress;
        private System.Windows.Forms.FlowLayoutPanel pnlDone;
        private System.Windows.Forms.Label lblTodo;
        private System.Windows.Forms.Label lblInProgress;
        private System.Windows.Forms.Label lblDone;
        private System.Windows.Forms.Button btnAddTask;
        private System.Windows.Forms.Button btnSort;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.ProgressBar pgbProgress;
        private System.Windows.Forms.Label lblProgressPercent;
        private System.Windows.Forms.Label lblProgressTitle;
    }
}
