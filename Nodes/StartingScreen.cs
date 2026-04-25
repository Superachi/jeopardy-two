using Achi.Godot.Common;
using Achi.Godot.Nodes.PlayerData;
using Achi.Godot.PathResolving;
using Achi.Godot.PathResolving.Attributes;
using Godot;
using JeopardyTwo.Nodes.JeopardyBackstage;
using JeopardyTwo.Nodes.JeopardyBoard;
using Nodes.Netcode;

namespace JeopardyTwo.Nodes;

[SceneFile]
public partial class StartingScreen : Control
{
    private Button _presentationButton = null!;
    private Button _backstageButton = null!;

	public override void _Ready()
	{
        _presentationButton = this.GetPrivateNode<Button>(nameof(_presentationButton));
        _backstageButton = this.GetPrivateNode<Button>(nameof(_backstageButton));
        _presentationButton.Pressed += OnPresentationButtonPressed;
        _backstageButton.Pressed += OnBackstageButtonPressed;
	}

	public override void _Process(double delta)
	{
	}

    private void OnPresentationButtonPressed()
    {
        var questionBoard = SceneCreationHelper.InstantiateSceneForType<QuestionBoard>();
        Main.Singleton.Instance.AddChild(questionBoard);

        var questionManager = new QuestionManager();
        Main.Singleton.Instance.AddChild(questionManager);

        var playerManager = new PlayerManager();
        Main.Singleton.Instance.AddChild(playerManager);

        var pm = new PlayerModel()
        {
            Name = "Player 1",
            Title = "The First Player",
            Score = 0
        };
        playerManager.CreatePlayer(pm);
        playerManager.CreatePlayer(pm);
        playerManager.CreatePlayer(pm);
        playerManager.CreatePlayer(pm);
        playerManager.CreatePlayer(pm);
        playerManager.CreatePlayer(pm);
        playerManager.CreatePlayer(pm);

        Main.Singleton.Instance.AddChild(new ClientNetManager());
        QueueFree();
    }

    private void OnBackstageButtonPressed()
    {
        Main.Singleton.Instance.AddChild(new BackstageManager());
        QueueFree();
    }
}
