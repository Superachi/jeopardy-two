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
    private float _screenEdgePadding = 32;

    public override void _Ready()
    {
        Singleton.MarkAsSingleton(this);
        DisplayManager.DisplaySignals.WindowResize += OnWindowResize;
    }

    public void CreatePlayer(PlayerModel playerModel)
    {
        var scoreTally = SceneCreationHelper.InstantiateSceneForType<ScoreTally>();
        AddChild(scoreTally);
        scoreTally.LoadFromPlayerModel(playerModel);

        LogNode.Log($"Created player: {playerModel.Name} with title {playerModel.Title} and score {playerModel.Score}");
        UpdatePlayerIndexes();

        PlayerCount++;
        SetScoreTallyPositions();
    }

    public void SetScoreTallyPositions()
    {
        var screenSize = DisplayManager.ScreenSize;

        var i = 0;
        var hPad = 8;
        foreach (var child in GetChildren())
        {
            if (child is ScoreTally scoreTally)
            {
                var distancePerTally = (screenSize.X - _screenEdgePadding * 2) / Mathf.Max(PlayerCount, 1);
                scoreTally.Size = new Vector2(distancePerTally - hPad, screenSize.Y / 10);
                var yPosition = screenSize.Y - scoreTally.Size.Y - _screenEdgePadding;
                scoreTally.Position = new Vector2(_screenEdgePadding + distancePerTally * (i) + hPad / 2, yPosition);
                scoreTally.ReAlignText();
                i++;
            }
        }
    }

    private void OnWindowResize(Vector2 screenSize)
    {
        SetScoreTallyPositions();
    }

    private void UpdatePlayerIndexes()
    {
        var index = 0;
        foreach (var child in GetChildren())
        {
            if (child is ScoreTally scoreTally)
            {
                scoreTally.SetIndex(index);
                index++;
            }
        }
    }

    #region Command executions

    public static void GivePointsToPlayer(int playerIndex, int points)
    {
        Singleton.Instance.AddScoreForPlayer(playerIndex, points);
    }

    private void AddScoreForPlayer(int playerIndex, int points)
    {
        foreach (var child in GetChildren())
        {
            if (child is ScoreTally scoreTally && scoreTally.PlayerIndex == playerIndex)
            {
                scoreTally.AddScore(points);
                return;
            }
        }

        LogNode.Log($"Tried to give points to player with index {playerIndex}, but no such player was found.");
    }

    public static void AdjustPlayerName(int playerIndex, string playerName)
    {
        Singleton.Instance.SetPlayerName(playerIndex, playerName);
    }

    private void SetPlayerName(int playerIndex, string playerName)
    {
        foreach (var child in GetChildren())
        {
            if (child is ScoreTally scoreTally && scoreTally.PlayerIndex == playerIndex)
            {
                scoreTally.SetPlayerName(playerName);
                return;
            }
        }

        LogNode.Log($"Tried to set player name for player with index {playerIndex}, but no such player was found.");
    }

    public static void AdjustPlayerTitle(int playerIndex, string playerTitle)
    {
        Singleton.Instance.SetPlayerTitle(playerIndex, playerTitle);
    }

    private void SetPlayerTitle(int playerIndex, string playerTitle)
    {
        foreach (var child in GetChildren())
        {
            if (child is ScoreTally scoreTally && scoreTally.PlayerIndex == playerIndex)
            {
                scoreTally.SetPlayerTitle(playerTitle);
                return;
            }
        }

        LogNode.Log($"Tried to set player title for player with index {playerIndex}, but no such player was found.");
    }

    #endregion Command executions
}