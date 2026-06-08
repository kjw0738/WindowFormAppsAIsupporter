using ReviewQuizApp.Controls;

namespace WinFormsAppAIsupporter;

public partial class MainForm : Form
{
    private ReviewQuizModuleControl? _quizModule;

    public MainForm()
    {
        InitializeComponent();
        SetupViews();
        ShowView(panelMeetingView);
    }

    private void SetupViews()
    {
        panelMeetingView.Controls.Clear();
        panelTaskView.Controls.Clear();
        panelQuizView.Controls.Clear();

        panelMeetingView.BackColor = Color.AliceBlue;
        panelTaskView.BackColor = Color.Honeydew;
        panelQuizView.BackColor = Color.White;

        panelMeetingView.Controls.Add(CreatePlaceholderLabel("AI 회의록 화면"));
        panelTaskView.Controls.Add(CreatePlaceholderLabel("과제 관리 화면"));
    }

    private void btnMeeting_Click(object sender, EventArgs e)
    {
        ResetMenuButtons();
        btnMeeting.BackColor = Color.LightSkyBlue;
        ShowView(panelMeetingView);
    }

    private void btnTask_Click(object sender, EventArgs e)
    {
        ResetMenuButtons();
        btnTask.BackColor = Color.LightGreen;
        ShowView(panelTaskView);
    }

    private void btnQuiz_Click(object sender, EventArgs e)
    {
        ResetMenuButtons();
        btnQuiz.BackColor = Color.Khaki;
        EnsureQuizModuleLoaded();
        ShowView(panelQuizView);
    }

    private void EnsureQuizModuleLoaded()
    {
        if (_quizModule != null)
        {
            return;
        }

        _quizModule = new ReviewQuizModuleControl
        {
            Dock = DockStyle.Fill
        };

        panelQuizView.Controls.Add(_quizModule);
    }

    private void ShowView(Panel targetPanel)
    {
        panelMeetingView.Visible = false;
        panelTaskView.Visible = false;
        panelQuizView.Visible = false;

        targetPanel.Visible = true;
        targetPanel.BringToFront();
    }

    private void ResetMenuButtons()
    {
        btnMeeting.BackColor = Color.Gainsboro;
        btnTask.BackColor = Color.Gainsboro;
        btnQuiz.BackColor = Color.Gainsboro;
    }

    private static Label CreatePlaceholderLabel(string text)
    {
        return new Label
        {
            AutoSize = true,
            Font = new Font("맑은 고딕", 18F, FontStyle.Bold),
            Location = new Point(32, 32),
            Text = text
        };
    }
}
