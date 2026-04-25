using Achi.Godot.Logging;
using Godot;
using JeopardyTwo.Nodes.JeopardyBackstage.Commands.Netcode;
using Nodes.Netcode;
using Nodes.Netcode.PacketManagement;

public partial class ClientNetManager : Node
{
    public override void _Ready()
    {
        NetworkHandler.StartClient();
        NetworkHandler.RegisterClientPacketHandler(new ClientTestPacketHandler());
    }
}

public class ClientTestPacketHandler : IClientPacketHandler
{
    public byte PacketType => CommandPacket.CustomPacketType;

    public void HandlePacket(byte[] data)
    {
        var packet = new CommandPacket();
        packet.Decode(data);
        LogNode.Log($"Received TestPacket with message: {packet.Message}");
    }
}