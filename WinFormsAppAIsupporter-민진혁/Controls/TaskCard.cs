using System;
using System.Drawing;
using System.Windows.Forms;
using WinFormsAppAIsupporter.Models;

namespace WinFormsAppAIsupporter.Controls
{
    public partial class TaskCard : UserControl
    {
        private TaskItem _task;

        public event EventHandler? StatusChanged;
        public event EventHandler? DeleteClicked;
        public event EventHandler? EditRequested;

        public TaskCard(TaskItem task)
        {
            InitializeComponent();
            _task = task;
            
            // 이벤트 강제 재연결
            this.DoubleClick += TaskCard_DoubleClick;
            this.MouseDown += TaskCard_MouseDown;
            this.MouseMove += TaskCard_MouseMove;

            // 하위 컨트롤에도 이벤트 전파
            this.lblContent.DoubleClick += TaskCard_DoubleClick;
            this.lblContent.MouseDown += TaskCard_MouseDown;
            this.lblContent.MouseMove += TaskCard_MouseMove;
            
            this.AutoSize = false;
            
            UpdateDisplay();
        }

        public TaskItem Task => _task;

        private void TaskCard_DoubleClick(object? sender, EventArgs e)
        {
            EditRequested?.Invoke(this, EventArgs.Empty);
        }

        private Point _dragStartPoint;

        private void TaskCard_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) _dragStartPoint = e.Location;
        }

        private void TaskCard_MouseMove(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (Math.Abs(e.X - _dragStartPoint.X) > 5 || Math.Abs(e.Y - _dragStartPoint.Y) > 5)
                {
                    this.DoDragDrop(this, DragDropEffects.Move);
                }
            }
        }

        private void UpdateDisplay()
        {
            lblContent.Text = _task.Content;
            lblAssignee.Text = $"담당자: {_task.Assignee}";
            lblDueDate.Text = $"마감일: {_task.DueDate}";

            // 제목 영역
            lblContent.AutoSize = true;
            lblContent.MaximumSize = new Size(230, 0); 
            lblContent.Location = new Point(15, 8);
            
            // 정보 레이블 위치 (제목 아래 간격 8px)
            lblAssignee.Top = lblContent.Bottom + 8;
            lblDueDate.Top = lblAssignee.Bottom + 8;
            
            // [수정] 진행 버튼은 Anchor로 하단 우측 고정 (Designer 참조)
            
            // 우선순위 색상 설정
            switch (_task.Priority)
            {
                case "긴급": pnlPriority.BackColor = Color.Red; break;
                case "보통": pnlPriority.BackColor = Color.Gold; break;
                case "낮음": pnlPriority.BackColor = Color.LimeGreen; break;
                default: pnlPriority.BackColor = Color.Gray; break;
            }

            // 마감일 임박 체크
            if (_task.DueDate.Contains(DateTime.Now.ToString("yyyy-MM-dd")))
            {
                lblDueDate.ForeColor = Color.Red;
                lblDueDate.Font = new Font(lblDueDate.Font, FontStyle.Bold);
            }
            else
            {
                lblDueDate.ForeColor = SystemColors.ControlText;
                lblDueDate.Font = new Font(lblDueDate.Font, FontStyle.Regular);
            }

            // 상태에 따른 버튼 텍스트 변경
            if (_task.Status == "Done")
            {
                btnNextStatus.Visible = false;
            }
            else
            {
                btnNextStatus.Visible = true;
                btnNextStatus.Text = _task.Status == "Not Started" ? "시작하기 ->" : "완료하기 ->";
            }
            
            // [수정] 카드 높이를 마감일 아래 버튼 여백을 포함하여 동적 계산 (최소 130px)
            this.Height = Math.Max(130, lblDueDate.Bottom + 40);
            pnlPriority.Height = this.Height;
        }

        private void btnNextStatus_Click(object sender, EventArgs e)
        {
            if (_task.Status == "Not Started") _task.Status = "In Progress";
            else if (_task.Status == "In Progress") _task.Status = "Done";

            UpdateDisplay();
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("삭제하시겠습니까?", "삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DeleteClicked?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
