using Achi.Godot.PathResolving.Attributes;
using Godot;
using JeopardyTwo.Models;

[SceneFile]
public partial class QuestionBoardButton : Button
{
    private int _buttonIndex;
    public int Index => _buttonIndex;

    public void Initialize(QuestionModel question, int buttonIndex = 0)
    {
        Name = nameof(QuestionBoardButton) + "_" + question.PointValue;

        Text = question.PointValue.ToString();
        _buttonIndex = buttonIndex;
    }

}
