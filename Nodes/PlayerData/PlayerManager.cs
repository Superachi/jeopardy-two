using Achi.Godot.Common;
using Achi.Godot.Logging;
using Achi.Godot.PathResolving;
using Godot;
using Nodes.Display;

namespace Achi.Godot.Nodes.PlayerData;

public partial class PlayerManager : Node
{
    public static Singleton<PlayerManager> Singleton { get; private set; } = new();
    public int PlayerCount { get; private set; }

    public override void _Ready()
    {
        Singleton.MarkAsSingleton(this);
    }

    public void CreatePlayer(PlayerModel playerModel)
    {
        var scoreTally = SceneCreationHelper.InstantiateSceneForType<ScoreTally>();
        AddChild(scoreTally);
        scoreTally.LoadFromPlayerModel(playerModel);

        LogNode.Log($"Created player: {playerModel.Name} with title {playerModel.Title} and score {playerModel.Score}");

        PlayerCount++;
    }

    public void SetScoreTallyPositions()
    {
        var screenSize = DisplayManager.ScreenSize;
    }
}