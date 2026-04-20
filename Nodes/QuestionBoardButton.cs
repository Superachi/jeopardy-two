using Achi.Godot.PathResolving.Attributes;
using Godot;
using JeopardyTwo.Models;
using JeopardyTwo.Nodes;

[SceneFile]
public partial class QuestionBoardButton : Button
{
    private int _buttonIndex;
    public int Index => _buttonIndex;
    private QuestionModel _question = new QuestionModel();
    private CategoryModel _category = new CategoryModel();

    public void Initialize(CategoryModel category, int buttonIndex = 0)
    {
        _question = category.Questions[buttonIndex];
        _category = category;

        Name = nameof(QuestionBoardButton) + "_" + _question.PointValue;

        Text = _question.PointValue.ToString();
        _buttonIndex = buttonIndex;
    }

    public override void _Pressed() => QuestionManager.DisplayQuestionScreen(_question, _category);
}
