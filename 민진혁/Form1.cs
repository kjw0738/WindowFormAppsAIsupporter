using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WinFormsAppAIsupporter.Models;
using WinFormsAppAIsupporter.Controls;
using WinFormsAppAIsupporter.Services;

namespace WinFormsAppAIsupporter
{
    public partial class Form1 : Form
    {
        private ProjectData _projectData;
        private bool _isSortEnabled = true; // 기본값 활성화

        public Form1()
        {
            InitializeComponent();
            
            // 드래그 앤 드롭 설정
            pnlTodo.AllowDrop = true;
            pnlInProgress.AllowDrop = true;
            pnlDone.AllowDrop = true;

            pnlTodo.DragEnter += Panel_DragEnter;
            pnlInProgress.DragEnter += Panel_DragEnter;
            pnlDone.DragEnter += Panel_DragEnter;

            pnlTodo.DragDrop += Panel_DragDrop;
            pnlInProgress.DragDrop += Panel_DragDrop;
            pnlDone.DragDrop += Panel_DragDrop;

            // JSON 데이터 불러오기
            _projectData = DataManager.LoadData();
            
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
            if (e.Data != null && e.Data.GetDataPresent(typeof(TaskCard)))
            {
                TaskCard? card = e.Data.GetData(typeof(TaskCard)) as TaskCard;
                FlowLayoutPanel? targetPanel = sender as FlowLayoutPanel;

                if (card != null && targetPanel != null)
                {
                    // 1. 상태 변경 확인
                    string newStatus = "";
                    if (targetPanel == pnlTodo) newStatus = "Not Started";
                    else if (targetPanel == pnlInProgress) newStatus = "In Progress";
                    else if (targetPanel == pnlDone) newStatus = "Done";

                    // 2. 드롭 위치의 인덱스 계산 (보다 명확한 로직)
                    Point dropPoint = targetPanel.PointToClient(new Point(e.X, e.Y));
                    int targetIndex = targetPanel.Controls.Count; // 기본값: 맨 뒤

                    for (int i = 0; i < targetPanel.Controls.Count; i++)
                    {
                        Control c = targetPanel.Controls[i];
                        if (c == card) continue; // 드래그 중인 카드 제외

                        // 카드의 중심점 Y 좌표와 비교
                        if (dropPoint.Y < c.Top + c.Height / 2)
                        {
                            targetIndex = i;
                            break;
                        }
                    }

                    // 맨 마지막으로 드롭될 때 인덱스 보정 (기존 로직의 side effect 제거)
                    // FlowLayoutPanel은 컨트롤의 위치를 자동 조정하므로, 단순히 인덱스만 맞추면 됩니다.
                    
                    // 3. 카드 이동 및 데이터 업데이트
                    card.Task.Status = newStatus;
                    
                    _projectData.Tasks.Remove(card.Task);
                    
                    // targetIndex가 현재 컨트롤 개수보다 크지 않도록 제한
                    _projectData.Tasks.Insert(Math.Min(targetIndex, _projectData.Tasks.Count), card.Task);

                    // 순서 재할당
                    int index = 0;
                    foreach (var task in _projectData.Tasks)
                    {
                        task.Order = index++;
                    }

                    DataManager.SaveData(_projectData);
                    RefreshBoard();
                }
            }
        }

        private void RefreshBoard()
        {
            pnlTodo.Controls.Clear();
            pnlInProgress.Controls.Clear();
            pnlDone.Controls.Clear();

            int todoCount = 0;
            int inProgressCount = 0;
            int doneCount = 0;

            // [수정] 정렬 로직: 순서(Order) 우선, 그 다음 마감일(DueDate)
            _projectData.Tasks.Sort((x, y) =>
            {
                if (x.Status != y.Status) return x.Status.CompareTo(y.Status); // Status별로 묶음
                if (_isSortEnabled) return string.Compare(x.DueDate, y.DueDate);
                return x.Order.CompareTo(y.Order);
            });

            string searchText = txtSearch.Text.ToLower();

            foreach (var task in _projectData.Tasks)
            {
                // 필터링 로직: 검색어가 비어있거나, 내용/담당자에 검색어가 포함된 경우만 표시
                if (string.IsNullOrEmpty(searchText) || 
                    task.Content.ToLower().Contains(searchText) || 
                    task.Assignee.ToLower().Contains(searchText))
                {
                    TaskCard card = new TaskCard(task);
                    card.StatusChanged += Card_StatusChanged;
                    card.DeleteClicked += Card_DeleteClicked;
                    card.EditRequested += Card_EditRequested;

                    switch (task.Status)
                    {
                        case "Not Started":
                            pnlTodo.Controls.Add(card);
                            todoCount++;
                            break;
                        case "In Progress":
                            pnlInProgress.Controls.Add(card);
                            inProgressCount++;
                            break;
                        case "Done":
                            pnlDone.Controls.Add(card);
                            doneCount++;
                            break;
                    }
                }
            }

            lblTodo.Text = $"To-Do / 미시작 ({todoCount})";
            lblInProgress.Text = $"In Progress / 진행 중 ({inProgressCount})";
            lblDone.Text = $"Done / 완료 ({doneCount})";

            // 진행률 계산 및 업데이트
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

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshBoard();
        }

        private void Card_StatusChanged(object? sender, EventArgs e)
        {
            // 상태가 변경될 때마다 JSON 파일에 저장
            DataManager.SaveData(_projectData);
            
            // 화면 갱신
            RefreshBoard();
        }

        private void Card_DeleteClicked(object? sender, EventArgs e)
        {
            if (sender is TaskCard card)
            {
                _projectData.Tasks.Remove(card.Task);
                DataManager.SaveData(_projectData);
                RefreshBoard();
            }
        }

        private void Card_EditRequested(object? sender, EventArgs e)
        {
            if (sender is TaskCard card)
            {
                using (var form = new AddTaskForm(card.Task))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        DataManager.SaveData(_projectData);
                        RefreshBoard();
                    }
                }
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
            
            if (_isSortEnabled)
            {
                btnSort.BackColor = Color.LightGreen;
                btnSort.Text = "📅 정렬 ON";
            }
            else
            {
                btnSort.BackColor = Color.LightGray;
                btnSort.Text = "📅 정렬 OFF";
            }

            RefreshBoard();
        }
    }
}
