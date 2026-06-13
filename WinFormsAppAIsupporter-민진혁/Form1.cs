using System;
using System.Drawing;
using System.Windows.Forms;
using WinFormsAppAIsupporter.Models;
using WinFormsAppAIsupporter.Controls;
using WinFormsAppAIsupporter.Services;

namespace WinFormsAppAIsupporter
{
    public partial class Form1 : Form
    {
        private ProjectData _projectData;
        private KanbanBoardView _kanbanView;
        private AIQuestionView _aiQuestionView;

        public Form1()
        {
            InitializeComponent();

            // 데이터 로드
            _projectData = DataManager.LoadData();

            // 뷰 초기화
            _kanbanView = new KanbanBoardView();
            _kanbanView.Dock = DockStyle.Fill;
            _kanbanView.SetData(_projectData);

            _aiQuestionView = new AIQuestionView();
            _aiQuestionView.Dock = DockStyle.Fill;
            _aiQuestionView.SetData(_projectData);

            // 초기 화면 설정 (과제 관리)
            ShowView(_kanbanView);
        }

        private void ShowView(UserControl view)
        {
            pnlMain.Controls.Clear();
            pnlMain.Controls.Add(view);
            
            // 메뉴 버튼 스타일 업데이트 (선택된 버튼 강조)
            btnMenuKanban.BackColor = (view == _kanbanView) ? Color.FromArgb(60, 60, 65) : Color.Transparent;
            btnMenuAIQuestion.BackColor = (view == _aiQuestionView) ? Color.FromArgb(60, 60, 65) : Color.Transparent;
        }

        private void btnMenuKanban_Click(object sender, EventArgs e)
        {
            _kanbanView.RefreshBoard(); // 화면 전환 시 최신 데이터 반영
            ShowView(_kanbanView);
        }

        private void btnMenuAIQuestion_Click(object sender, EventArgs e)
        {
            ShowView(_aiQuestionView);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }
}
