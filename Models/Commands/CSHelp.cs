using System.Collections.Generic;
using Achi.Godot.Logging;
using JeopardyTwo.Helpers.Commands;

public class CSHelp : CommandSpec
{
    public override string Name => "help";
    public override string Description => "Displays a list of available commands.";
    public override List<string> Arguments => new List<string>();
    public override void Execute(params string[] args)
    {
        var commands = ClientCommandParser.Commands;
        if (commands.Count == 0)
        {
            LogNode.Log("No commands available.");
            return;
        }

        LogNode.Log("Available commands:");
        foreach (var command in commands)
        {
            LogNode.Log($"/{command.Name} - {command.Description}");
        }
    }
}