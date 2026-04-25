using JeopardyTwo.Helpers.Commands;
using Nodes.Netcode.PacketManagement;

namespace JeopardyTwo.Nodes.JeopardyBackstage.Commands.Netcode;

public class CommandClientPacketHandler : IClientPacketHandler
{
    public byte PacketType => CommandPacket.CustomPacketType;

    public void HandlePacket(byte[] data)
    {
        var packet = new CommandPacket();
        packet.Decode(data);
        ClientCommandParser.ParseCommand(packet.Message);
    }
}