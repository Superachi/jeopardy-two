using System.Collections.Generic;
using Achi.Godot.Logging;

public class CSAudio : CommandSpec
{
    public override string Name => "audio";
    public override string Description => "Plays audio.";
    public override List<string> Arguments => new List<string> { "audioName", };
    public override void Execute(params string[] args)
    {
        LogNode.Log($"Received audio command: {args[0]}");
    }
}