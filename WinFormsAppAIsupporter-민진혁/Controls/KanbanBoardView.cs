using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WinFormsAppAIsupporter.Models;
using WinFormsAppAIsupporter.Services;

namespace WinFormsAppAIsupporter.Controls
{
    public partial class KanbanBoardView : UserControl
    {
        private ProjectData _projectData;
        private bool _isSortEnabled = true;

        public KanbanBoardView()
        {
            InitializeComponent();

            pnlTodo.AllowDrop = true;
            pnlInProgress.AllowDrop = true;
            pnlDone.AllowDrop = true;

            pnlTodo.DragEnter += Panel_DragEnter;
            pnlInProgress.DragEnter += Panel_DragEnter;
            pnlDone.DragEnter += Panel_DragEnter;

            pnlTodo.DragDrop += Panel_DragDrop;
            pnlInProgress.DragDrop += Panel_DragDrop;
            pnlDone.DragDrop += Panel_DragDrop;
        }

        public void SetData(ProjectData data)
        {
            _projectData = data;
            RefreshBoard();
        }

        private void Panel_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(typeof(TaskCard)))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void Panel_DragDrop(object? sender, DragEventArgs e)
        {
            TaskCard? card = e.Data.GetData(typeof(TaskCard)) as TaskCard;
            FlowLayoutPanel? targetPanel = sender as FlowLayoutPanel;

            if (card == null || targetPanel == null) return;

            string newStatus = targetPanel == pnlTodo ? "Not Started" : 
                              (targetPanel == pnlInProgress ? "In Progress" : "Done");

            if (card.Parent == targetPanel)
            {
                // [Case 1] 같은 패널 내 위치 변경 (교환 방식)
                if (_isSortEnabled) return; 
                HandleReorder(card, targetPanel);
            }
            else
            {
                // [Case 2] 다른 패널로 이동 (상태 변경)
                HandleStatusChange(card, targetPanel, newStatus);
            }
        }

        private void HandleReorder(TaskCard card, FlowLayoutPanel targetPanel)
        {
            Point dropPoint = targetPanel.PointToClient(Cursor.Position);
            
            TaskCard? targetCard = null;
            for (int i = 0; i < targetPanel.Controls.Count; i++)
            {
                Control c = targetPanel.Controls[i];
                if (c == card) continue;
                // 체크: 드롭된 위치가 해당 컨트롤의 범위 내에 있는지 확인
                if (c.Bounds.Contains(dropPoint))
                {
                    targetCard = c as TaskCard;
                    break;
                }
            }

            if (targetCard != null && targetCard != card)
            {
                // 두 태스크의 Order 값을 서로 교환
                int tempOrder = card.Task.Order;
                card.Task.Order = targetCard.Task.Order;
                targetCard.Task.Order = tempOrder;

                DataManager.SaveData(_projectData);
                RefreshBoard();
            }
        }

        private void HandleStatusChange(TaskCard card, FlowLayoutPanel targetPanel, string newStatus)
        {
            card.Task.Status = newStatus;
            
            // 패널 맨 뒤에 추가
            _projectData.Tasks.Remove(card.Task);
            _projectData.Tasks.Add(card.Task); 
            
            UpdateTaskOrders();
            DataManager.SaveData(_projectData);
            RefreshBoard();
        }

        private void UpdateTaskOrders()
        {
            string[] statuses = { "Not Started", "In Progress", "Done" };
            foreach (var status in statuses)
            {
                var tasksInStatus = _projectData.Tasks.FindAll(t => t.Status == status);
                tasksInStatus.Sort((x, y) => x.Order.CompareTo(y.Order));
                for (int i = 0; i < tasksInStatus.Count; i++)
                {
                    tasksInStatus[i].Order = i;
                }
            }
        }

        public void RefreshBoard()
        {
            if (_projectData == null) return;

            pnlTodo.Controls.Clear();
            pnlInProgress.Controls.Clear();
            pnlDone.Controls.Clear();

            if (_isSortEnabled)
            {
                _projectData.Tasks.Sort((x, y) => string.Compare(x.DueDate, y.DueDate));
            }
            else
            {
                _projectData.Tasks.Sort((x, y) => x.Order.CompareTo(y.Order));
            }

            foreach (var task in _projectData.Tasks)
            {
                TaskCard card = new TaskCard(task);
                card.StatusChanged += (s, e) => { DataManager.SaveData(_projectData); RefreshBoard(); };
                card.DeleteClicked += (s, e) => { _projectData.Tasks.Remove(card.Task); DataManager.SaveData(_projectData); RefreshBoard(); };
                card.EditRequested += (s, e) => 
                {
                    using (var form = new AddTaskForm(card.Task))
                    {
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            DataManager.SaveData(_projectData);
                            RefreshBoard();
                        }
                    }
                };

                switch (task.Status)
                {
                    case "Not Started": pnlTodo.Controls.Add(card); break;
                    case "In Progress": pnlInProgress.Controls.Add(card); break;
                    case "Done": pnlDone.Controls.Add(card); break;
                }
            }

            int todoCount = 0;
            int inProgressCount = 0;
            int doneCount = 0;
            foreach (var task in _projectData.Tasks)
            {
                if (task.Status == "Not Started") todoCount++;
                else if (task.Status == "In Progress") inProgressCount++;
                else if (task.Status == "Done") doneCount++;
            }
            lblTodo.Text = $"To-Do / 미시작 ({todoCount})";
            lblInProgress.Text = $"In Progress / 진행 중 ({inProgressCount})";
            lblDone.Text = $"Done / 완료 ({doneCount})";

            UpdateOverallProgress();
        }

        private void UpdateOverallProgress()
        {
            int totalTasks = _projectData.Tasks.Count;
            int completedTasks = 0;
            foreach (var task in _projectData.Tasks)
            {
                if (task.Status == "Done") completedTasks++;
            }

            if (totalTasks > 0)
            {
                int percent = (int)((double)completedTasks / totalTasks * 100);
                pgbProgress.Value = percent;
                lblProgressPercent.Text = $"{percent}%";
            }
            else
            {
                pgbProgress.Value = 0;
                lblProgressPercent.Text = "0%";
            }
        }

        private void btnAddTask_Click(object sender, EventArgs e)
        {
            using (var form = new AddTaskForm())
            {
                if (form.ShowDialog() == DialogResult.OK && form.NewTask != null)
                {
                    _projectData.Tasks.Add(form.NewTask);
                    DataManager.SaveData(_projectData);
                    RefreshBoard();
                }
            }
        }

        private void btnSort_Click(object sender, EventArgs e)
        {
            _isSortEnabled = !_isSortEnabled;
            btnSort.BackColor = _isSortEnabled ? Color.LightGreen : Color.LightGray;
            btnSort.Text = _isSortEnabled ? "📅 정렬 ON" : "📅 정렬 OFF";
            RefreshBoard();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshBoard();
        }
    }
}
