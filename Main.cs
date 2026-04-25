using System.Collections.Generic;
using Achi.Godot.Audio;
using Achi.Godot.Audio.Models;
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

        // Audio
        var audioManager = new AudioManager() { Name = nameof(AudioManager) };
        var bus = new AudioBusModel()
        {
            Index = 0,
            Name = "SFX",
            VolumeDb = 0f,
        };
        audioManager.ConfigureBuses(new List<AudioBusModel>() { bus });
        AddChild(audioManager);

        var startingScreen = SceneCreationHelper.InstantiateSceneForType<StartingScreen>();
        AddChild(startingScreen);

        NetworkHandler.RegisterClientPacketHandler(new CommandClientPacketHandler());
    }

    public override void _Process(double delta)
	{
	}
}
