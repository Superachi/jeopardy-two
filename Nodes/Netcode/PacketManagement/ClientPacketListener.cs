using System.Collections.Generic;
using Achi.Godot.Logging;
using Godot;
using Nodes.Netcode.Packets;
using Nodes.Netcode.PeerManagement;
using Nodes.Signals;

namespace Nodes.Netcode.PacketManagement;

/// <summary>
/// Listens for client packet events and dispatches them to registered handlers or the peer manager.
/// </summary>
public partial class ClientPacketListener : Node
{
    private ClientSignals? _clientSignals;
    private ClientPeerManager? _peerManager;
    private readonly Dictionary<byte, IClientPacketHandler> _packetHandlers = new();

    /// <summary>
    /// Registers a packet handler for its declared packet type.
    /// </summary>
    /// <param name="handler">The handler instance to register.</param>
    /// <param name="replaceExisting">If true, replaces an existing handler for the same packet type.</param>
    /// <returns>True if the handler was registered; otherwise false.</returns>
    public bool RegisterPacketHandler(IClientPacketHandler handler, bool replaceExisting = false)
    {
        if (handler == null)
        {
            NetworkHandler.DebugLog("Cannot register a null client packet handler.");
            return false;
        }
        if (_packetHandlers.ContainsKey(handler.PacketType) && !replaceExisting)
        {
            NetworkHandler.DebugLog($"A handler is already registered for client packet type {(int)handler.PacketType}.");
            return false;
        }
        NetworkHandler.DebugLog($"Registered client packet handler for type {(int)handler.PacketType}.");
        _packetHandlers[handler.PacketType] = handler;
        return true;
    }

    /// <summary>
    /// Unregisters a packet handler for the given packet type.
    /// </summary>
    /// <param name="packetType">The packet type to remove a handler for.</param>
    /// <returns>True if a handler was removed; otherwise false.</returns>
    public bool UnregisterPacketHandler(byte packetType)
    {
        return _packetHandlers.Remove(packetType);
    }

    public override void _Ready()
    {
        var networkHandler = NetworkHandler.Singleton.Instance;
        if (networkHandler == null)
        {
            NetworkHandler.DebugLog("ClientPacketListener could not find NetworkHandler.");
            return;
        }

        _clientSignals = networkHandler.ClientSignals;
        _peerManager = networkHandler.ClientPeerManager;
        if (_clientSignals == null || _peerManager == null)
        {
            NetworkHandler.DebugLog("ClientPacketListener could not find required signal or peer manager.");
            return;
        }

        _clientSignals.PacketReceived += OnClientPacket;
    }

    public override void _ExitTree()
    {
        if (_clientSignals != null)
        {
            _clientSignals.PacketReceived -= OnClientPacket;
        }
    }

    /// <summary>
    /// Reads incoming packet bytes and routes each packet to the right handler or the peer manager.
    /// </summary>
    /// <param name="data">Raw packet bytes received from the network.</param>
    private void OnClientPacket(byte[] data)
    {
        if (data == null || data.Length == 0)
        {
            NetworkHandler.DebugLog("Received empty client packet.");
            return;
        }

        var packetType = data[0];

        if (_packetHandlers.TryGetValue(packetType, out var handler))
        {
            handler.HandlePacket(data);
            return;
        }

        switch (packetType)
        {
            case PacketInfo.BuiltInPacketTypes.IdAssignment:
                _peerManager?.ManageIds(IDAssignment.CreateFromData(data));
                break;
            case PacketInfo.BuiltInPacketTypes.IdUnassignment:
                // TODO: Implement client-side removal of peer IDs if needed
                break;
            default:
                NetworkHandler.DebugLog($"Unhandled client packet type index {data[0]}. Ensure a handler is registered for this type if it's expected.");
                break;
        }
    }
}
