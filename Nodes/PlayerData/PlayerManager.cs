using Achi.Godot.Common;
using Achi.Godot.PathResolving;
using Godot;
using Nodes.Display;

namespace Achi.Godot.Nodes.PlayerData;

public partial class PlayerManager : Node
{
    public static Singleton<PlayerManager> Singleton { get; private set; } = null!;
    public int PlayerCount { get; private set; }

    public override void _Ready()
    {
        Singleton.MarkAsSingleton(this);
    }

    public void CreatePlayer(PlayerModel playerModel)
    {
        var scoreTally = SceneCreationHelper.InstantiateSceneForType<ScoreTally>();
        scoreTally.LoadFromPlayerModel(playerModel);
        AddChild(scoreTally);

        PlayerCount++;
    }

    public void SetScoreTallyPositions()
    {
        var screenSize = DisplayManager.ScreenSize;
        
    }
}