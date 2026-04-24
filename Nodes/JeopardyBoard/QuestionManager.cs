using System.Collections.Generic;
using Achi.Godot.Common;
using Achi.Godot.Logging;
using Achi.Godot.PathResolving;
using Godot;
using JeopardyTwo.Models;
using Newtonsoft.Json;

namespace JeopardyTwo.Nodes.JeopardyBoard;

public partial class QuestionManager : Node
{
    public static Singleton<QuestionManager> Singleton { get; private set; } = new();
    public static int ColumnCount { get; private set; }

    public override void _Ready()
    {
        Name = nameof(QuestionManager);
        Singleton.MarkAsSingleton(this);

        // Load the questions from a JSON file and create CategoryModels and QuestionModels
        var filePath = "res://SampleData/gameQuestions.json";

        List<CategoryModel> categories = [];
        if (FileAccess.FileExists(filePath)) {
            var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
            var jsonString = file.GetAsText();
            file.Close();

            categories = JsonConvert.DeserializeObject<List<CategoryModel>>(jsonString) ?? [];
        }

        ColumnCount = categories.Count;
        LogNode.Log($"Loaded {ColumnCount} categories from JSON file.");

        // For each CategoryModel, create a QuestionBoardColumn and add it as a child to the QuestionManager
        int i = 0;
        foreach (var category in categories)
        {
            var column = new QuestionBoardColumn();
            column.Initialize(category, i);
            AddChild(column);
            i++;
        }
    }

    public override void _Notification(int notification)
    {
        if (notification == NotificationPredelete)
        {
            Singleton.ClearSingleton();
        }
    }

    public static void DisplayQuestionScreen(QuestionModel question, CategoryModel category)
    {
        Singleton.Instance.SetColumnsVisibility(false);

        var questionScreen = SceneCreationHelper.InstantiateSceneForType<QuestionScreen>();
        Singleton.Instance.AddChild(questionScreen);

        questionScreen.Initialize(category, question);
    }

    public static void ShowQuestionBoard() => Singleton.Instance.SetColumnsVisibility(true);

    private void SetColumnsVisibility(bool showColumns)
    {
        foreach (var column in GetChildren())
        {
            if (column is QuestionBoardColumn questionBoardColumn)
            {
                questionBoardColumn.Visible = showColumns;
            }
        }
    }
}