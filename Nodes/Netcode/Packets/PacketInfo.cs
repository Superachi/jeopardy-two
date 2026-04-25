using Godot;
using Nodes.Netcode.PacketManagement;

namespace Nodes.Netcode.Packets;

/// <summary>
/// Inherit from this class when constructing your own PacketInfo. When setting the type, ensure you use a unique byte value that doesn't conflict with <seealso cref="BuiltInPacketTypes"/> packet types or other custom packets in your project.
/// </summary>
public abstract partial class PacketInfo : RefCounted
{
	/// <summary>
	/// Built-in packet type constants for the netcode system. Use these for library packets; user packets should use their own byte values.
	/// </summary>
	public static class BuiltInPacketTypes
	{
		/// <summary>
		/// Assigns a new peer ID and the current peer list to a client.
		/// </summary>
		public const byte IdAssignment = 0;

		/// <summary>
		/// Notifies clients that a peer ID is no longer valid (peer left).
		/// </summary>
		public const byte IdUnassignment = 1;
	}

	/// <summary>
	/// The packet type identifier for this packet. Use built-in constants for library packets, or any byte value for custom packets.
	/// </summary>
	public byte Type { get; protected set; }

	/// <summary>
	/// ENet transfer flags used when sending this packet.
	/// </summary>
	public int Flag { get; protected set; }

    /// <summary>
    /// Creates the raw bytes for this packet. The base version only writes the packet type.
    /// </summary>
    /// <returns>Byte array ready to send over the network.</returns>
	public virtual byte[] Encode()
	{
		var writer = new PacketWriter();
		writer.WriteByte(Type);
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
			NetworkHandler.DebugLog("Received empty or null packet data.");
			return;
		}

		Type = packetType;
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
	public void Send(ENetPacketPeer target)
    {
        NetworkHandler.DebugLog($"Sending packet of type {Type} with flag {Flag} to peer {target}.");
        target.Send(0, Encode(), (int)Flag);
    }

	/// <summary>
	/// Sends this packet to all peers connected to the server.
	/// </summary>
	/// <param name="server">The server connection used to broadcast.</param>
	public void Broadcast(ENetConnection server)
    {
        NetworkHandler.DebugLog($"Broadcasting packet of type {Type} with flag {Flag} to all peers.");
        server.Broadcast(0, Encode(), (int)Flag);
    }
}
