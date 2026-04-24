using Achi.Godot.Common;
using Achi.Godot.Logging;
using Achi.Godot.Nodes.PlayerData;
using Achi.Godot.PathResolving;
using Godot;
using JeopardyTwo.Nodes;
using Nodes.Display;

public partial class Main : Node2D
{
    public static Singleton<Main> Singleton { get; private set; } = new ();

	public override void _Ready()
    {
        Singleton.MarkAsSingleton(this);

        AddChild(new DisplayManager());

        var Logger = new LogNode();
        AddChild(Logger);

        var startingScreen = SceneCreationHelper.InstantiateSceneForType<StartingScreen>();
        AddChild(startingScreen);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
