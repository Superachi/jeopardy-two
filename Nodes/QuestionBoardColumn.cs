using Achi.Godot.PathResolving;
using Godot;
using JeopardyTwo.Models;

public partial class QuestionBoardColumn : Node2D
{
    private int _columnIndex;
    public CategoryModel Category { get; set; } = new CategoryModel();

    public void Initialize(CategoryModel category, int columnIndex)
    {
        Name = nameof(QuestionBoardColumn) + "_" + columnIndex;

        Category = category;
        _columnIndex = columnIndex;

        CreateButtons();
    }

    private void CreateButtons()
    {
        var i = 0;
        
        foreach (var question in Category.Questions) {
            var button = SceneCreationHelper.InstantiateSceneForType<QuestionBoardButton>();
            button.Initialize(question, i);
            button.Position = new Vector2(Position.X, Position.Y + i * 100);
            AddChild(button);
            i++;
        }
    }
}