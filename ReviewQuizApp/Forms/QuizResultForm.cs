using ReviewQuizApp.Models;
using ReviewQuizApp.Services;

namespace ReviewQuizApp.Forms;

public sealed class QuizResultForm : Form
{
    public QuizResultForm(IReadOnlyList<QuizQuestion> questions, IReadOnlyDictionary<string, string> answers)
    {
        Text = "퀴즈 결과";
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(680, 460);
        Size = new Size(840, 600);

        var correctCount = questions.Count(question =>
            answers.TryGetValue(question.Id, out var answer)
            && QuizDataLoader.SameAnswer(answer, question.CorrectAnswer));
        var score = questions.Count == 0 ? 0 : correctCount * 100 / questions.Count;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(20),
            BackColor = Color.White
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var summary = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            Margin = new Padding(0, 0, 0, 16),
            Text = $"점수: {correctCount} / {questions.Count} ({score}점)"
        };

        var list = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true
        };
        list.Columns.Add("문항", 80);
        list.Columns.Add("결과", 80);
        list.Columns.Add("내 답", 180);
        list.Columns.Add("정답", 180);
        list.Columns.Add("해설", 320);

        for (var i = 0; i < questions.Count; i++)
        {
            var question = questions[i];
            answers.TryGetValue(question.Id, out var answer);
            var isCorrect = QuizDataLoader.SameAnswer(answer ?? string.Empty, question.CorrectAnswer);
            var item = new ListViewItem($"{i + 1}번");
            item.SubItems.Add(isCorrect ? "정답" : "오답");
            item.SubItems.Add(string.IsNullOrWhiteSpace(answer) ? "(미응답)" : answer);
            item.SubItems.Add(question.CorrectAnswer);
            item.SubItems.Add(question.Explanation);
            item.BackColor = isCorrect ? Color.FromArgb(236, 253, 245) : Color.FromArgb(254, 242, 242);
            list.Items.Add(item);
        }

        var closeButton = new Button
        {
            Anchor = AnchorStyles.Right,
            AutoSize = true,
            MinimumSize = new Size(96, 36),
            Text = "닫기"
        };
        closeButton.Click += (_, _) => Close();

        root.Controls.Add(summary, 0, 0);
        root.Controls.Add(list, 0, 1);
        root.Controls.Add(closeButton, 0, 2);
        Controls.Add(root);
    }
}
