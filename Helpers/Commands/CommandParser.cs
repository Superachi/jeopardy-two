using System.Collections.Generic;
using System.Linq;
using Achi.Godot.Logging;

namespace JeopardyTwo.Helpers.Commands;

public static class ClientCommandParser
{
    public const string CommandPrefix = "/";

    public const string CNHelp = "help";
    public const string CNWhisper = "whisper";
    public const string CNAudio = "audio";

    private static List<CommandSpec> _commands = new List<CommandSpec>
    {
        new CSHelp(),
        new CSWhisper(),
    };

    public static List<CommandSpec> Commands => _commands;

    public static void ParseCommand(string command)
    {
        // First, check if the command starts with a slash, indicating it's a command.
        if (!command.StartsWith("/"))
        {
            return;
        }

        // Get the command name and arguments by splitting the string.
        var parts = command.Substring(1).Split(' ');
        var commandName = parts[0];
        var arguments = parts.Skip(1).ToArray();

        // Handle different commands based on the command name.
        if (_commands.FirstOrDefault(c => c.Name.Equals(commandName, System.StringComparison.OrdinalIgnoreCase)) is CommandSpec spec)
        {
            if (arguments.Length < spec.Arguments.Count)
            {
                LogNode.Log($"{spec.Name} - {spec.Description} | Usage: /{spec.Name} {string.Join(" ", spec.Arguments)}");
                return;
            }

            spec.Execute(arguments);
            return;
        }

        LogNode.Log($"Unknown command: {commandName}");
    }
}