using Achi.Godot.Logging;
using Achi.Godot.PathResolving.Attributes;
using Godot;
using JeopardyTwo.Models;
using Nodes.Display;

[SceneFile]
public partial class QuestionScreen : Control
{
    private RichTextLabel _categoryLabel = null!;
    private RichTextLabel _questionLabel = null!;
    private RichTextLabel _answerLabel = null!;
    private Button _revealButton = null!;

    private CategoryModel _category = new CategoryModel();
    private QuestionModel _question = new QuestionModel();


	public override void _Ready()
	{
        Name = nameof(QuestionScreen);
        
        _categoryLabel = GetNode<RichTextLabel>("CategoryLabel");
        _questionLabel = GetNode<RichTextLabel>("QuestionLabel");
        _answerLabel = GetNode<RichTextLabel>("AnswerLabel");
        _revealButton = GetNode<Button>("RevealButton");

        _revealButton.Pressed += OnRevealButtonPressed;
        DisplayManager.DisplaySignals.WindowResize += OnWindowResize;
    }

    public void Initialize(CategoryModel category, QuestionModel question)
    {
        _category = category;
        _question = question;

        _categoryLabel.Text = category.Name;
        _questionLabel.Text = question.Question;
        _answerLabel.Text = question.Answer;
        _answerLabel.Hide();

        SetPositions();
    }

    private void OnRevealButtonPressed()
    {
        _answerLabel.Show();
        _revealButton.Hide();
    }

    private void SetPositions()
    {
        AdjustLabelSettings(_categoryLabel, 0.1f);
        AdjustLabelSettings(_questionLabel, 0.3f);

        var answerVOffsetRatio = 0.7f;
        AdjustLabelSettings(_answerLabel, answerVOffsetRatio);

        var screenSize = DisplayManager.ScreenSize;
        var buttonSize = _revealButton.Size;
        _revealButton.Position = new Vector2(screenSize.X / 2 - buttonSize.X / 2, screenSize.Y * answerVOffsetRatio - buttonSize.Y / 2);
    }

    private void OnWindowResize(Vector2 screenSize) => SetPositions();

    private void AdjustLabelSettings(RichTextLabel label, float vOffsetRatio)
    {
        var screenSize = DisplayManager.ScreenSize;

        label.Size = screenSize;
        label.PivotOffset = label.Size / 2;
        label.GlobalPosition = new Vector2(screenSize.X / 2, screenSize.Y * vOffsetRatio) - label.Size / 2;
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;
    }
}
