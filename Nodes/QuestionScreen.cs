using Achi.Godot.PathResolving.Attributes;
using Godot;
using JeopardyTwo.Models;
using JeopardyTwo.Nodes;
using Nodes.Display;
using JeopardyTwo.Helpers;
using System.Collections.Generic;

[SceneFile]
public partial class QuestionScreen : Control
{
    private RichTextLabel _categoryLabel = null!;
    private RichTextLabel _questionLabel = null!;
    private RichTextLabel _answerLabel = null!;
    private Button _revealButton = null!;
    private Button _backButton = null!;

    private CategoryModel _category = new();
    private QuestionModel _question = new();


	public override void _Ready()
	{
        Name = nameof(QuestionScreen);
        
        _categoryLabel = GetNode<RichTextLabel>("CategoryLabel");
        _questionLabel = GetNode<RichTextLabel>("QuestionLabel");
        _answerLabel = GetNode<RichTextLabel>("AnswerLabel");
        _revealButton = GetNode<Button>("RevealButton");
        _backButton = GetNode<Button>("BackButton");

        _revealButton.Pressed += OnRevealButtonPressed;
        _backButton.Pressed += OnBackButtonPressed;
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

    private void OnBackButtonPressed()
    {
        QuestionManager.ShowQuestionBoard();
        QueueFree();
    }

    private void SetPositions()
    {
        var nodeFonts = new List<NodeFontResizeInfo>
        {
            new() {
                NodeWithFontToResize = _categoryLabel,
                Ratio = 0.1f,
            },
            new() {
                NodeWithFontToResize = _questionLabel,
                Ratio = 0.05f,
            },
            new() {
                NodeWithFontToResize = _answerLabel,
                Ratio = 0.1f,
            }
        };
        FontResizeHelper.ResizeNodeFonts(nodeFonts);

        AdjustLabelSettings(_categoryLabel, 0.15f);
        AdjustLabelSettings(_questionLabel, 0.4f);

        var answerVOffsetRatio = 0.65f;
        AdjustLabelSettings(_answerLabel, answerVOffsetRatio);

        var screenSize = DisplayManager.ScreenSize;
        var revealButtonSize = _revealButton.Size;
        _revealButton.Position = new Vector2(screenSize.X / 2 - revealButtonSize.X / 2, screenSize.Y * answerVOffsetRatio - revealButtonSize.Y / 2);

        _backButton.Position = new Vector2(32, 32);
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
