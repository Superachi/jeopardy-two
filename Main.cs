using Achi.Godot.Common;
using Achi.Godot.Logging;
using Achi.Godot.PathResolving;
using Godot;
using JeopardyTwo.Nodes;
using JeopardyTwo.Nodes.JeopardyBackstage.Commands.Netcode;
using Nodes.Display;
using Nodes.Netcode;

public partial class Main : Node2D
{
    public static Singleton<Main> Singleton { get; private set; } = new ();

	public override void _Ready()
    {
        OS.SetEnvironment("ProjectFileAttribute_ProjectName", "jeopardy-two");

        Singleton.MarkAsSingleton(this);

        AddChild(new DisplayManager());

        var Logger = new LogNode();
        AddChild(Logger);

        var startingScreen = SceneCreationHelper.InstantiateSceneForType<StartingScreen>();
        AddChild(startingScreen);

        NetworkHandler.RegisterClientPacketHandler(new CommandClientPacketHandler());
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
