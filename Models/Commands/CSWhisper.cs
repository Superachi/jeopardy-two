using System.Collections.Generic;
using Achi.Godot.Logging;

public class CSWhisper : CommandSpec
{
    public override string Name => "whisper";
    public override string Description => "Sends a private message to another player.";
    public override List<string> Arguments => new List<string> { "message" };
    public override void Execute(params string[] args)
    {
        var message = string.Join(' ', args);
        LogNode.Log($"Received message: {message}");
    }
}