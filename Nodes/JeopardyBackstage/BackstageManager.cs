using Achi.Godot.Common;
using Achi.Godot.PathResolving.Attributes;
using Godot;
using JeopardyTwo.Nodes.JeopardyBackstage.Commands.Netcode;
using Nodes.Netcode;

[SceneFile]
public partial class BackstageManager : Control
{
    private LineEdit _commandLineEdit = null!;
    public override void _Ready()
    {
        _commandLineEdit = this.GetPrivateNode<LineEdit>(nameof(_commandLineEdit));
        _commandLineEdit.Text = "Type a command here...";
        _commandLineEdit.TextSubmitted += OnCommandSubmitted;
    }

    private void OnCommandSubmitted(string command)
    {
        var packet = new CommandPacket().Create(command);
        NetworkHandler.BroadcastPacket(packet);
    }
}