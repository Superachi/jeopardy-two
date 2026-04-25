using System.Collections.Generic;
using Achi.Godot.Logging;
using Godot;
using Nodes.Netcode.Packets;

namespace Nodes.Netcode;

public partial class IDAssignment : PacketInfo
{
	/// <summary>
	/// The local player ID assigned by the server.
	/// </summary>
	public byte Id { get; private set; }

	/// <summary>
	/// IDs of the other players already connected when this packet was sent.
	/// </summary>
	public List<byte> RemoteIds { get; private set; } = new();

	/// <summary>
	/// Builds a new ID assignment packet with the provided local and remote player IDs.
	/// </summary>
	/// <param name="id">The local player ID assigned to the recipient.</param>
	/// <param name="remoteIds">The list of currently connected remote player IDs.</param>
	/// <returns>A configured IDAssignment packet ready to encode and send.</returns>
	public static IDAssignment Create(byte id, IEnumerable<byte> remoteIds)
	{
		var info = new IDAssignment
		{
			Type = BuiltInPacketTypes.IdAssignment,
			Flag = (int)ENetPacketPeer.FlagReliable,
			Id = id,
			RemoteIds = new List<byte>(remoteIds),
		};

		return info;
	}

	/// <summary>
	/// Creates an ID assignment packet instance from raw network bytes.
	/// </summary>
	/// <param name="data">The incoming packet bytes.</param>
	/// <returns>A decoded IDAssignment packet.</returns>
	public static IDAssignment CreateFromData(byte[] data)
	{
		var info = new IDAssignment();
		info.Decode(data);
		return info;
	}

	/// <summary>
	/// Encodes the packet type, local ID, and all remote IDs into a byte array.
	/// </summary>
	/// <returns>Serialized packet bytes ready for network transfer.</returns>
	protected override void WritePayload(PacketWriter writer)
	{
		writer.WriteByte(Id);
		writer.WriteBytes(RemoteIds);
	}

	/// <summary>
	/// Decodes packet bytes into packet type, local ID, and remote ID list.
	/// </summary>
	/// <param name="data">Serialized packet bytes received from the network.</param>
	protected override void ReadPayload(PacketReader reader)
	{
		if (!reader.TryReadByte(out byte id))
		{
			NetworkHandler.DebugLog("IDAssignment packet is missing the assigned ID byte.");
			return;
		}

		Id = id;
		RemoteIds = new List<byte>(reader.ReadRemainingBytes());
	}
}
