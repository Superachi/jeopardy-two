using Godot;
using JeopardyTwo.Nodes.JeopardyBackstage.Commands.Netcode;
using Nodes.Netcode;

namespace JeopardyTwo.Nodes.JeopardyBackstage;

public partial class ServerNetManager : Control
{
    public override void _Ready()
    {
        // Register the handler (on the server):
        NetworkHandler.StartServer();
    }
}