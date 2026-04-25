using System.Collections.Generic;
using Achi.Godot.Logging;
using Godot;
using Nodes.Signals;

namespace Nodes.Netcode.PeerRegistry;

/// <summary>
/// Tracks and manages the IDs of peers currently connected to the server.
/// Handles peer join/leave events and broadcasts ID assignments.
/// </summary>
public partial class ServerPeerManager : Node
{
    /// <summary>
    /// Stores the IDs of peers that are currently connected to the server.
    /// </summary>
    public List<byte> PeerIds { get; private set; } = new();
    private ServerSignals? _serverSignals;

    public override void _Ready()
    {
        var networkHandler = NetworkHandler.Singleton.Instance;
        if (networkHandler == null)
        {
            NetworkHandler.DebugLog("ServerPeerManager could not find NetworkHandler.");
            return;
        }

        _serverSignals = networkHandler.ServerSignals;
        _serverSignals.PeerJoined += OnPeerConnected;
        _serverSignals.PeerLeft += OnPeerDisconnected;
    }

    public override void _ExitTree()
    {
        if (_serverSignals != null)
        {
            _serverSignals.PeerJoined -= OnPeerConnected;
            _serverSignals.PeerLeft -= OnPeerDisconnected;
        }
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
            NetworkHandler.DebugLog("Cannot broadcast ID assignment because server connection is null.");
            return;
        }

        IDAssignment.Create(peerIdByte, PeerIds).Broadcast(networkHandler.Connection);
    }

    /// <summary>
    /// Removes a disconnected peer ID from the active peer list.
    /// </summary>
    /// <param name="peerId">The ID of the peer that disconnected.</param>
    /// <summary>
    /// Handles a peer disconnecting and broadcasts an ID unassignment packet to all peers.
    /// </summary>
    /// <param name="peerId">The ID of the peer that disconnected.</param>
    private void OnPeerDisconnected(int peerId)
    {
        var peerIdByte = (byte)peerId;
        PeerIds.Remove(peerIdByte);

        var networkHandler = NetworkHandler.Singleton.Instance;
        if (networkHandler?.Connection == null)
        {
            NetworkHandler.DebugLog("Cannot broadcast ID unassignment because server connection is null.");
            return;
        }

        // Broadcast ID unassignment to all peers
        IDUnassignment.Create(peerIdByte).Broadcast(networkHandler.Connection);
    }
}
