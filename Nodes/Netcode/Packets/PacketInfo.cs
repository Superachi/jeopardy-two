using Achi.Godot.Logging;
using Godot;

namespace Nodes.Netcode.Packets;

public partial class PacketInfo : RefCounted
{
	public enum PacketType : byte
	{
		IdAssignment = 0,
		PlayerPosition = 10,
	}

	/// <summary>
	/// The kind of packet this instance represents.
	/// </summary>
	public PacketType Type { get; protected set; }

	/// <summary>
	/// ENet transfer flags used when sending this packet.
	/// </summary>
	public int Flag { get; set; }
    
    /// <summary>
    /// Creates the raw bytes for this packet. The base version only writes the packet type.
    /// </summary>
    /// <returns>Byte array ready to send over the network.</returns>
    public virtual byte[] Encode()
	{
		var writer = new PacketWriter();
		writer.WriteByte((byte)Type);
		WritePayload(writer);
		return writer.ToArray();
	}

	/// <summary>
	/// Reads packet data from raw bytes and updates this packet instance.
	/// </summary>
	/// <param name="data">The bytes received from the network.</param>
	public virtual void Decode(byte[] data)
	{
		var reader = PacketReader.FromPacket(data);
		if (!reader.TryReadByte(out byte packetType))
		{
			LogNode.Log("Received empty or null packet data.");
			return;
		}

		Type = (PacketType)packetType;
		ReadPayload(reader);
	}

	protected virtual void WritePayload(PacketWriter writer)
	{
	}

	protected virtual void ReadPayload(PacketReader reader)
	{
	}

	/// <summary>
	/// Sends this packet to one specific peer.
	/// </summary>
	/// <param name="target">The peer that should receive the packet.</param>
	public void Send(ENetPacketPeer target) => target.Send(0, Encode(), (int)Flag);

	/// <summary>
	/// Sends this packet to all peers connected to the server.
	/// </summary>
	/// <param name="server">The server connection used to broadcast.</param>
	public void Broadcast(ENetConnection server) => server.Broadcast(0, Encode(), (int)Flag);
}
