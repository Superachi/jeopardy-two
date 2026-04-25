using System.Collections.Generic;
using Achi.Godot.Logging;
using Godot;

namespace Nodes.Netcode;

public partial class ServerNetworkGlobals : Node
{
	/// <summary>
	/// Stores the IDs of peers that are currently connected to the server.
	/// </summary>
	public List<byte> PeerIds { get; private set; } = new();

	/// <summary>
	/// Connects this node to server network events when the node is ready.
	/// </summary>
	public override void _Ready()
	{
		var networkHandler = NetworkHandler.Singleton.Instance;
		if (networkHandler == null)
		{
			LogNode.Log("ServerNetworkGlobals could not find NetworkHandler.");
			return;
		}

		networkHandler.ServerSignals.PeerJoined += OnPeerConnected;
		networkHandler.ServerSignals.PeerLeft += OnPeerDisconnected;
		networkHandler.ServerSignals.PacketReceived += OnServerPacket;
	}

	/// <summary>
	/// Handles a newly connected peer and broadcasts updated ID assignment data.
	/// </summary>
	/// <param name="peerId">The ID of the peer that just connected.</param>
	private void OnPeerConnected(int peerId)
	{
		var peerIdByte = (byte)peerId;
		if (!PeerIds.Contains(peerIdByte))
		{
			PeerIds.Add(peerIdByte);
		}

		var networkHandler = NetworkHandler.Singleton.Instance;
		if (networkHandler?.Connection == null)
		{
			LogNode.Log("Cannot broadcast ID assignment because server connection is null.");
			return;
		}

		IDAssignment.Create(peerIdByte, PeerIds).Broadcast(networkHandler.Connection);
	}

	/// <summary>
	/// Removes a disconnected peer ID from the active peer list.
	/// </summary>
	/// <param name="peerId">The ID of the peer that disconnected.</param>
	private void OnPeerDisconnected(int peerId)
	{
		PeerIds.Remove((byte)peerId);

		// TODO: Create and broadcast an ID unassignment packet to connected peers.
	}

	/// <summary>
	/// Processes packets received by the server and logs unhandled packet types.
	/// </summary>
	/// <param name="peerId">The sending peer's ID.</param>
	/// <param name="data">Raw packet bytes received from that peer.</param>
	private void OnServerPacket(int peerId, byte[] data)
	{
		if (data == null || data.Length == 0)
		{
			LogNode.Log($"Received empty server packet from peer {peerId}.");
			return;
		}

		var packetType = (PacketInfo.PacketType)data[0];
		LogNode.Log($"Unhandled server packet type {(int)packetType} from peer {peerId}.");
	}
}
