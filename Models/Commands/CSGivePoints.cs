using System.Collections.Generic;
using Achi.Godot.Nodes.PlayerData;

namespace Models.Commands;
public class CSGivePoints : CommandSpec
{
    public override string Name => "give";
    public override string Description => "Gives points to a player. Note: spaces in player names are underscores.";
    public override List<string> Arguments => new List<string> { "playerName", "points" };
    public override void Execute(params string[] args)
    {
        PlayerManager.GivePointsToPlayer(int.Parse(args[0]), int.Parse(args[1]));
    }
}