using System.Collections.Generic;
using Achi.Godot.Nodes.PlayerData;
using JeopardyTwo.Nodes.JeopardyBoard;

namespace Models.Commands;
public class CSAward : CommandSpec
{
    public override string Name => "award";
    public override string Description => "Reveals the answer and gives points to a player.";
    public override List<string> Arguments => new List<string> { "playerIndex", "points" };
    public override void Execute(params string[] args)
    {
        QuestionManager.RevealAnswer();
        PlayerManager.GivePointsToPlayer(int.Parse(args[0]), int.Parse(args[1]));
    }
}