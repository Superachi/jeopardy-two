using Achi.Godot.Logging;
using Achi.Godot.PathResolving.Attributes;
using Godot;
using Nodes.Netcode;
using Nodes.Netcode.PacketManagement;
using Nodes.Netcode.Packets;

namespace JeopardyTwo.Nodes.JeopardyBackstage;

public partial class BackstageManager : Control
{
    private Button _packetTestButton = new();

    public override void _Ready()
    {
        _packetTestButton.Text = "Test Packet";
        _packetTestButton.Pressed += OnPacketTestButtonPressed;
        AddChild(_packetTestButton);

        // Register the handler (on the server):
        NetworkHandler.StartServer();
        NetworkHandler.RegisterServerPacketHandler(new ServerTestPacketHandler());
    }

    private void OnPacketTestButtonPressed()
    {
        var packet = new TestPacket() { Message = "Hello from the server!" };
        var connection = NetworkHandler.Singleton.Instance.Connection;
        packet.Broadcast(connection);
        LogNode.Log("Packet test button pressed.");
    }
}

public partial class TestPacket : PacketInfo
{
    public const byte CustomPacketType = 10;

    public string Message { get; set; } = "Undefined message";

    public TestPacket()
    {
        Type = CustomPacketType;
        Flag = (int)ENetPacketPeer.FlagReliable;
    }

    protected override void WritePayload(PacketWriter writer)
    {
        var byteData = System.Text.Encoding.UTF8.GetBytes(Message);
        writer.WriteBytes(byteData);
    }

    protected override void ReadPayload(PacketReader reader)
    {
        if (reader.ReadRemainingBytes() is byte[] byteData && byteData.Length > 0)
        {
            Message = System.Text.Encoding.UTF8.GetString(byteData);
        }
        else
        {
            LogNode.Log("Failed to read message bytes from TestPacket.");
            Message = "";
        }
    }
}

public class ServerTestPacketHandler : IServerPacketHandler
{
    public byte PacketType => TestPacket.CustomPacketType;

    public void HandlePacket(int peerId, byte[] data)
    {
        var packet = new TestPacket();
        packet.Decode(data);
        LogNode.Log($"Received TestPacket from peer {peerId} with message: {packet.Message}");
    }
}