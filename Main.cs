using Achi.Godot.Logging;
using Achi.Godot.Nodes.PlayerData;
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

        var playerManager = new PlayerManager();
        AddChild(playerManager);

        var pm = new PlayerModel()
        {
            Name = "Player 1",
            Title = "The First Player",
            Score = 0
        };
        playerManager.CreatePlayer(pm);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
