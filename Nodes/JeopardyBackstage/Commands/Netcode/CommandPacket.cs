using Achi.Godot.Logging;
using Godot;
using Nodes.Netcode.Packets;

namespace JeopardyTwo.Nodes.JeopardyBackstage.Commands.Netcode;

public partial class CommandPacket : PacketInfo
{
    public const byte CustomPacketType = 10;

    public string Message { get; set; } = "Undefined message";

    public CommandPacket()
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
            LogNode.Log("Failed to read message bytes from CommandPacket.");
            Message = "";
        }
    }

    public CommandPacket Create(string message)
    {
        return new CommandPacket() { Message = message };
    }
}