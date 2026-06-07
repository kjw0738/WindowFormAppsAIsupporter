using System;
using System.Windows.Forms;
using WinFormsAppAIsupporter.Models;

namespace WinFormsAppAIsupporter
{
    public partial class AddTaskForm : Form
    {
        public TaskItem? NewTask { get; private set; }
        private bool _isEditMode = false;

        public AddTaskForm()
        {
            InitializeComponent();
            dtpDueDate.Value = DateTime.Now;
            cmbPriority.SelectedIndex = 1; // "보통" 기본값
            this.Text = "새 과제 추가";
        }

        public AddTaskForm(TaskItem existingTask) : this()
        {
            _isEditMode = true;
            this.Text = "과제 수정";
            txtContent.Text = existingTask.Content;
            txtAssignee.Text = existingTask.Assignee;
            if (DateTime.TryParse(existingTask.DueDate, out DateTime dueDate))
            {
                dtpDueDate.Value = dueDate;
            }
            cmbPriority.SelectedItem = existingTask.Priority;
            NewTask = existingTask;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtContent.Text) || string.IsNullOrWhiteSpace(txtAssignee.Text))
            {
                MessageBox.Show("과제 내용과 담당자를 입력해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedPriority = cmbPriority.SelectedItem?.ToString() ?? "보통";

            if (_isEditMode && NewTask != null)
            {
                NewTask.Content = txtContent.Text;
                NewTask.Assignee = txtAssignee.Text;
                NewTask.DueDate = dtpDueDate.Value.ToString("yyyy-MM-dd");
                NewTask.Priority = selectedPriority;
            }
            else
            {
                NewTask = new TaskItem(
                    $"TASK-{DateTime.Now:yyyyMMddHHmmss}",
                    txtAssignee.Text,
                    txtContent.Text,
                    dtpDueDate.Value.ToString("yyyy-MM-dd"),
                    "Not Started",
                    selectedPriority
                );
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
