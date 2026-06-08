namespace ReviewQuizApp.Controls;

public sealed class ReviewQuizModuleControl : UserControl
{
    private readonly Form1 _quizForm;

    public ReviewQuizModuleControl()
    {
        Dock = DockStyle.Fill;

        _quizForm = new Form1
        {
            TopLevel = false,
            FormBorderStyle = FormBorderStyle.None,
            Dock = DockStyle.Fill
        };

        Controls.Add(_quizForm);
        _quizForm.Show();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _quizForm.Dispose();
        }

        base.Dispose(disposing);
    }
}
