using Achi.Godot.Logging;
using Godot;
using Nodes.Netcode.Packets;

namespace Nodes.Netcode;

/// <summary>
/// Packet sent by the server to notify clients that a peer has disconnected and its ID is no longer valid.
/// </summary>
public partial class IDUnassignment : PacketInfo
{
    /// <summary>
    /// The player ID that has been removed from the session.
    /// </summary>
    public byte Id { get; private set; }

    /// <summary>
    /// Builds a new ID unassignment packet for the given peer ID.
    /// </summary>
    /// <param name="id">The player ID to unassign.</param>
    /// <returns>A configured IDUnassignment packet ready to encode and send.</returns>
    public static IDUnassignment Create(byte id)
    {
        var info = new IDUnassignment
        {
            Type = BuiltInPacketTypes.IdUnassignment,
            Flag = (int)ENetPacketPeer.FlagReliable,
            Id = id,
        };
        return info;
    }

    /// <summary>
    /// Creates an ID unassignment packet instance from raw network bytes.
    /// </summary>
    /// <param name="data">The incoming packet bytes.</param>
    /// <returns>A decoded IDUnassignment packet.</returns>
    public static IDUnassignment CreateFromData(byte[] data)
    {
        var info = new IDUnassignment();
        info.Decode(data);
        return info;
    }

    /// <summary>
    /// Encodes the packet type and the removed ID into a byte array.
    /// </summary>
    /// <returns>Serialized packet bytes ready for network transfer.</returns>
    protected override void WritePayload(PacketWriter writer)
    {
        writer.WriteByte(Id);
    }

    /// <summary>
    /// Decodes packet bytes into packet type and removed ID.
    /// </summary>
    /// <param name="reader">PacketReader for the received data.</param>
    protected override void ReadPayload(PacketReader reader)
    {
        if (!reader.TryReadByte(out byte id))
        {
            NetworkHandler.DebugLog("IDUnassignment packet is missing the ID byte.");
            return;
        }
        Id = id;
    }
}
