using System.Collections.Generic;
using Achi.Godot.Nodes.PlayerData;

namespace Models.Commands;
public class CSSetPlayerTitle : CommandSpec
{
    public override string Name => "setplayertitle";
    public override string Description => "Sets the title of a player. Note: spaces in player titles are underscores.";
    public override List<string> Arguments => new List<string> { "playerIndex", "playerTitle" };
    public override void Execute(params string[] args)
    {
        PlayerManager.AdjustPlayerTitle(int.Parse(args[0]), args[1]);
    }
}