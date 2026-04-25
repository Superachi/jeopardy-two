using System.Collections.Generic;
using Achi.Godot.Logging;
using Nodes.Netcode.Packets;
using Nodes.Signals;
using Godot;

namespace Nodes.Netcode.PacketManagement;

/// <summary>
/// Listens for server packet events and dispatches them to registered IServerPacketHandler implementations.
/// </summary>
public partial class ServerPacketListener : Node
{
    private readonly Dictionary<byte, IServerPacketHandler> _packetHandlers = new();
    private ServerSignals? _serverSignals;

    /// <summary>
    /// Registers a packet handler for its declared packet type.
    /// </summary>
    /// <param name="handler">The handler instance to register.</param>
    /// <param name="replaceExisting">If true, replaces an existing handler for the same packet type.</param>
    /// <returns>True if the handler was registered; otherwise false.</returns>
    public bool RegisterPacketHandler(IServerPacketHandler handler, bool replaceExisting = false)
    {
        if (handler == null)
        {
            NetworkHandler.DebugLog("Cannot register a null packet handler.");
            return false;
        }
        if (_packetHandlers.ContainsKey(handler.PacketType) && !replaceExisting)
        {
            NetworkHandler.DebugLog($"A handler is already registered for packet type {(int)handler.PacketType}.");
            return false;
        }
        NetworkHandler.DebugLog($"Registered server packet handler for type {(int)handler.PacketType}.");
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

    /// <summary>
    /// Initializes the packet listener and subscribes to server packet events.
    /// </summary>
    public override void _Ready()
    {
        var networkHandler = NetworkHandler.Singleton.Instance;
        if (networkHandler == null)
        {
            NetworkHandler.DebugLog("ServerPacketListener could not find NetworkHandler.");
            return;
        }
        _serverSignals = networkHandler.ServerSignals;
        _serverSignals.PacketReceived += OnServerPacket;
    }

    /// <summary>
    /// Unsubscribes from server packet events when the node exits the tree.
    /// </summary>
    public override void _ExitTree()
    {
        if (_serverSignals != null)
        {
            _serverSignals.PacketReceived -= OnServerPacket;
        }
    }

    /// <summary>
    /// Processes packets received by the server and dispatches them to registered handlers.
    /// </summary>
    /// <param name="peerId">The sending peer's ID.</param>
    /// <param name="data">Raw packet bytes received from that peer.</param>
    private void OnServerPacket(int peerId, byte[] data)
    {
        if (data == null || data.Length == 0)
        {
            NetworkHandler.DebugLog($"Received empty server packet from peer {peerId}.");
            return;
        }
        var packetType = data[0];
        if (_packetHandlers.TryGetValue(packetType, out var handler))
        {
            handler.HandlePacket(peerId, data);
            return;
        }
        NetworkHandler.DebugLog($"Unhandled server packet type {(int)packetType} from peer {peerId}. Register an IServerPacketHandler to process it.");
    }
}
