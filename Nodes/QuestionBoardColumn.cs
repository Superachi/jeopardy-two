using Achi.Godot.Logging;
using Achi.Godot.PathResolving;
using Godot;
using JeopardyTwo.Models;
using JeopardyTwo.Nodes;
using Nodes.Display;

public partial class QuestionBoardColumn : Node2D
{
    private Label _categoryLabel = new Label();

    public CategoryModel Category { get; set; } = new CategoryModel();
    private int _columnIndex;

    public override void _Ready()
    {
        DisplayManager.DisplaySignals.WindowResize += OnWindowResize;
    }

    public void Initialize(CategoryModel category, int columnIndex)
    {
        Name = nameof(QuestionBoardColumn) + "_" + columnIndex;

        Category = category;
        AddChild(_categoryLabel);
        _categoryLabel.Text = category.Name;
        _categoryLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _categoryLabel.VerticalAlignment = VerticalAlignment.Center;

        _columnIndex = columnIndex;

        SetPosition();
        CreateButtons();
    }

    private void CreateButtons()
    {
        var i = 0;
        
        foreach (var question in Category.Questions) {
            var button = SceneCreationHelper.InstantiateSceneForType<QuestionBoardButton>();
            button.Initialize(Category, i);
            AddChild(button);
            i++;
        }

        SetButtonPositions();
    }

    private void SetPosition()
    {
        var columnWidth = GetColumnWidth();
        Position = new Vector2(100 + _columnIndex * columnWidth, 32);

        SetButtonPositions();
    }

    private void SetButtonPositions()
    {
        var columnWidth = GetColumnWidth();

        var screenSize = DisplayManager.ScreenSize;
        var buttonCount = Category.Questions.Count;
        var buttonHeight = screenSize.Y * 0.6f / buttonCount;
        var hPadding = columnWidth / 10;
        var vPadding = buttonHeight / 10;

        foreach (var child in GetChildren()) {
            if (child is QuestionBoardButton button) {
                var index = button.Index;
                button.Size = new Vector2(columnWidth - hPadding, buttonHeight - vPadding);
                button.Position = new Vector2(0, index * buttonHeight + 64);
            }
        }
    }

    private float GetColumnWidth()
    {
        var screenSize = DisplayManager.ScreenSize;
        var columnCount = QuestionManager.ColumnCount;
        return (screenSize.X - 200) / columnCount;
    }

    private void OnWindowResize(Vector2 newSize) => SetPosition();
}