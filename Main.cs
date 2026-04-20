using Achi.Godot.Logging;
using Achi.Godot.PathResolving;
using Godot;
using JeopardyTwo.Nodes;
using Nodes.Display;

public partial class Main : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        AddChild(new DisplayManager());

        var Logger = new LogNode();
        AddChild(Logger);

        var questionBoard = SceneCreationHelper.InstantiateSceneForType<QuestionBoard>();
        AddChild(questionBoard);

        var questionManager = new QuestionManager();
        AddChild(questionManager);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
