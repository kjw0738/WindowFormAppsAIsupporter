namespace WinFormsAppAIsupporter.Controls
{
    partial class KanbanBoardView
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
            pnlHeader = new Panel();
            lblProgressTitle = new Label();
            lblProgressPercent = new Label();
            pgbProgress = new ProgressBar();
            lblSearch = new Label();
            txtSearch = new TextBox();
            btnSort = new Button();
            btnAddTask = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            pnlTodo = new FlowLayoutPanel();
            pnlInProgress = new FlowLayoutPanel();
            pnlDone = new FlowLayoutPanel();
            lblTodo = new Label();
            lblInProgress = new Label();
            lblDone = new Label();
            pnlHeader.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.Controls.Add(lblProgressTitle);
            pnlHeader.Controls.Add(lblProgressPercent);
            pnlHeader.Controls.Add(pgbProgress);
            pnlHeader.Controls.Add(lblSearch);
            pnlHeader.Controls.Add(txtSearch);
            pnlHeader.Controls.Add(btnSort);
            pnlHeader.Controls.Add(btnAddTask);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Margin = new Padding(4, 5, 4, 5);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1397, 150);
            pnlHeader.TabIndex = 1;
            // 
            // lblProgressTitle
            // 
            lblProgressTitle.AutoSize = true;
            lblProgressTitle.Location = new Point(14, 92);
            lblProgressTitle.Margin = new Padding(4, 0, 4, 0);
            lblProgressTitle.Name = "lblProgressTitle";
            lblProgressTitle.Size = new Size(150, 25);
            lblProgressTitle.TabIndex = 6;
            lblProgressTitle.Text = "전체 수행 진행률";
            // 
            // lblProgressPercent
            // 
            lblProgressPercent.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblProgressPercent.AutoSize = true;
            lblProgressPercent.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
            lblProgressPercent.Location = new Point(1340, 92);
            lblProgressPercent.Margin = new Padding(4, 0, 4, 0);
            lblProgressPercent.Name = "lblProgressPercent";
            lblProgressPercent.Size = new Size(38, 25);
            lblProgressPercent.TabIndex = 5;
            lblProgressPercent.Text = "0%";
            // 
            // pgbProgress
            // 
            pgbProgress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pgbProgress.Location = new Point(200, 92);
            pgbProgress.Margin = new Padding(4, 5, 4, 5);
            pgbProgress.Name = "pgbProgress";
            pgbProgress.Size = new Size(1125, 25);
            pgbProgress.TabIndex = 4;
            // 
            // lblSearch
            // 
            lblSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(968, 32);
            lblSearch.Margin = new Padding(4, 0, 4, 0);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(58, 25);
            lblSearch.TabIndex = 3;
            lblSearch.Text = "검색 :";
            // 
            // txtSearch
            // 
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtSearch.Location = new Point(1040, 25);
            txtSearch.Margin = new Padding(4, 5, 4, 5);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "검색어 입력 (내용/담당자)";
            txtSearch.Size = new Size(341, 31);
            txtSearch.TabIndex = 2;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // btnSort
            // 
            btnSort.BackColor = Color.LightGreen;
            btnSort.FlatStyle = FlatStyle.Flat;
            btnSort.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
            btnSort.Location = new Point(200, 17);
            btnSort.Margin = new Padding(4, 5, 4, 5);
            btnSort.Name = "btnSort";
            btnSort.Size = new Size(143, 50);
            btnSort.TabIndex = 1;
            btnSort.Text = "📅 정렬 ON";
            btnSort.UseVisualStyleBackColor = false;
            btnSort.Click += btnSort_Click;
            // 
            // btnAddTask
            // 
            btnAddTask.BackColor = Color.LightSkyBlue;
            btnAddTask.FlatStyle = FlatStyle.Flat;
            btnAddTask.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
            btnAddTask.Location = new Point(14, 17);
            btnAddTask.Margin = new Padding(4, 5, 4, 5);
            btnAddTask.Name = "btnAddTask";
            btnAddTask.Size = new Size(171, 50);
            btnAddTask.TabIndex = 0;
            btnAddTask.Text = "+ 새 과제 추가";
            btnAddTask.UseVisualStyleBackColor = false;
            btnAddTask.Click += btnAddTask_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel1.Controls.Add(pnlTodo, 0, 1);
            tableLayoutPanel1.Controls.Add(pnlInProgress, 1, 1);
            tableLayoutPanel1.Controls.Add(pnlDone, 2, 1);
            tableLayoutPanel1.Controls.Add(lblTodo, 0, 0);
            tableLayoutPanel1.Controls.Add(lblInProgress, 1, 0);
            tableLayoutPanel1.Controls.Add(lblDone, 2, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 150);
            tableLayoutPanel1.Margin = new Padding(4, 5, 4, 5);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 67F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(1397, 850);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // pnlTodo
            // 
            pnlTodo.AutoScroll = true;
            pnlTodo.BackColor = Color.FromArgb(240, 240, 240);
            pnlTodo.Dock = DockStyle.Fill;
            pnlTodo.FlowDirection = FlowDirection.TopDown;
            pnlTodo.Location = new Point(4, 72);
            pnlTodo.Margin = new Padding(4, 5, 4, 5);
            pnlTodo.Name = "pnlTodo";
            pnlTodo.Padding = new Padding(14, 17, 14, 17);
            pnlTodo.Size = new Size(457, 773);
            pnlTodo.TabIndex = 0;
            pnlTodo.WrapContents = false;
            // 
            // pnlInProgress
            // 
            pnlInProgress.AutoScroll = true;
            pnlInProgress.BackColor = Color.FromArgb(230, 240, 255);
            pnlInProgress.Dock = DockStyle.Fill;
            pnlInProgress.FlowDirection = FlowDirection.TopDown;
            pnlInProgress.Location = new Point(469, 72);
            pnlInProgress.Margin = new Padding(4, 5, 4, 5);
            pnlInProgress.Name = "pnlInProgress";
            pnlInProgress.Padding = new Padding(14, 17, 14, 17);
            pnlInProgress.Size = new Size(457, 773);
            pnlInProgress.TabIndex = 1;
            pnlInProgress.WrapContents = false;
            // 
            // pnlDone
            // 
            pnlDone.AutoScroll = true;
            pnlDone.BackColor = Color.FromArgb(230, 255, 230);
            pnlDone.Dock = DockStyle.Fill;
            pnlDone.FlowDirection = FlowDirection.TopDown;
            pnlDone.Location = new Point(934, 72);
            pnlDone.Margin = new Padding(4, 5, 4, 5);
            pnlDone.Name = "pnlDone";
            pnlDone.Padding = new Padding(14, 17, 14, 17);
            pnlDone.Size = new Size(459, 773);
            pnlDone.TabIndex = 2;
            pnlDone.WrapContents = false;
            // 
            // lblTodo
            // 
            lblTodo.AutoSize = true;
            lblTodo.BackColor = Color.FromArgb(220, 220, 220);
            lblTodo.Dock = DockStyle.Fill;
            lblTodo.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);
            lblTodo.Location = new Point(4, 0);
            lblTodo.Margin = new Padding(4, 0, 4, 0);
            lblTodo.Name = "lblTodo";
            lblTodo.Size = new Size(457, 67);
            lblTodo.TabIndex = 3;
            lblTodo.Text = "To-Do / 미시작 (0)";
            lblTodo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblInProgress
            // 
            lblInProgress.AutoSize = true;
            lblInProgress.BackColor = Color.FromArgb(200, 220, 255);
            lblInProgress.Dock = DockStyle.Fill;
            lblInProgress.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);
            lblInProgress.Location = new Point(469, 0);
            lblInProgress.Margin = new Padding(4, 0, 4, 0);
            lblInProgress.Name = "lblInProgress";
            lblInProgress.Size = new Size(457, 67);
            lblInProgress.TabIndex = 4;
            lblInProgress.Text = "In Progress / 진행 중 (0)";
            lblInProgress.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblDone
            // 
            lblDone.AutoSize = true;
            lblDone.BackColor = Color.FromArgb(200, 240, 200);
            lblDone.Dock = DockStyle.Fill;
            lblDone.Font = new Font("맑은 고딕", 12F, FontStyle.Bold);
            lblDone.Location = new Point(934, 0);
            lblDone.Margin = new Padding(4, 0, 4, 0);
            lblDone.Name = "lblDone";
            lblDone.Size = new Size(459, 67);
            lblDone.TabIndex = 5;
            lblDone.Text = "Done / 완료 (0)";
            lblDone.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // KanbanBoardView
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            Controls.Add(pnlHeader);
            Margin = new Padding(4, 5, 4, 5);
            Name = "KanbanBoardView";
            Size = new Size(1397, 1000);
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);

        }

        private System.Windows.Forms.Panel pnlHeader;
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
