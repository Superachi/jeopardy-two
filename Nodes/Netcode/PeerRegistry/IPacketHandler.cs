using Nodes.Netcode.Packets;

namespace Nodes.Netcode.PeerRegistry;

/// <summary>
/// Handles a specific packet type received by the server peer registry.
/// </summary>
public interface IPacketHandler
{
	/// <summary>
	/// The packet type this handler is responsible for.
	/// </summary>
	PacketInfo.PacketType PacketType { get; }

	/// <summary>
	/// Handles packet bytes received from a specific peer.
	/// </summary>
	/// <param name="peerId">The sending peer ID.</param>
	/// <param name="data">Raw packet bytes.</param>
	void HandlePacket(int peerId, byte[] data);
}
