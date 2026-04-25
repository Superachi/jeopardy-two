using System.Collections.Generic;
using Achi.Godot.Nodes.PlayerData;

namespace Models.Commands;
public class CSSetPlayerName : CommandSpec
{
    public override string Name => "setplayername";
    public override string Description => "Sets the name of a player. Note: spaces in player names are underscores.";
    public override List<string> Arguments => new List<string> { "playerIndex", "playerName" };
    public override void Execute(params string[] args)
    {
        PlayerManager.AdjustPlayerName(int.Parse(args[0]), args[1]);
    }
}