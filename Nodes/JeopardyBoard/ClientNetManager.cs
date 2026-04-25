using Achi.Godot.Logging;
using Godot;
using JeopardyTwo.Nodes.JeopardyBackstage;
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
    public byte PacketType => TestPacket.CustomPacketType;

    public void HandlePacket(byte[] data)
    {
        var packet = new TestPacket();
        packet.Decode(data);
        LogNode.Log($"Received TestPacket with message: {packet.Message}");
    }
}