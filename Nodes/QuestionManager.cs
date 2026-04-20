using System.Collections.Generic;
using Achi.Godot.Logging;
using Godot;
using JeopardyTwo.Models;
using Newtonsoft.Json;

namespace JeopardyTwo.Nodes;

public partial class QuestionManager : Node
{
    public override void _Ready()
    {
        Name = nameof(QuestionManager);

        // Load the questions from a JSON file and create CategoryModels and QuestionModels
        var filePath = "res://SampleData/questions.json";

        List<CategoryModel> categories = [];
        if (FileAccess.FileExists(filePath)) {
            var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
            var jsonString = file.GetAsText();
            file.Close();

            categories = JsonConvert.DeserializeObject<List<CategoryModel>>(jsonString) ?? [];
        }

        LogNode.Log($"Loaded {categories.Count} categories from JSON file.");

        // For each CategoryModel, create a QuestionBoardColumn and add it as a child to the QuestionManager
        int i = 0;
        foreach (var category in categories) {
            var column = new QuestionBoardColumn();
            column.Initialize(category, i);
            column.Position = new Vector2(i * 200, 32); // Example positioning, adjust as needed
            AddChild(column);
            i++;
        }
    }
}