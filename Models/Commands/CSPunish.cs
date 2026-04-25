using System.Collections.Generic;
using Achi.Godot.Nodes.PlayerData;
using JeopardyTwo.Nodes.JeopardyBoard;

namespace Models.Commands;

public class CSPunish : CommandSpec
{
    public override string Name => "punish";
    public override string Description => "Reveals the answer and deducts points from a player.";
    public override List<string> Arguments => new List<string> { "playerIndex" };
    public override void Execute(params string[] args)
    {
        PlayerManager.TakeQuestionPointsFromPlayer(int.Parse(args[0]));
    }
}