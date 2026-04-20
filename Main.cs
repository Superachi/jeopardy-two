using Achi.Godot.Logging;
using Godot;
using JeopardyTwo.Nodes;
using System;

public partial class Main : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        var Logger = new LogNode();
        AddChild(Logger);

        LogNode.Log("Hello");

        var questionManager = new QuestionManager();
        AddChild(questionManager);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
