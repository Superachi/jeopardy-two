using System.Collections.Generic;
using Achi.Godot.Logging;
using Nodes.Netcode.Packets;
using Nodes.Signals;
using Godot;

namespace Nodes.Netcode.PeerRegistry;

/// <summary>
/// Listens for server packet events and dispatches them to registered IPacketHandler implementations.
/// </summary>
public partial class ServerPacketListener : Node
{
    private readonly Dictionary<PacketInfo.PacketType, IPacketHandler> _packetHandlers = new();
    private ServerSignals? _serverSignals;

    /// <summary>
    /// Registers a packet handler for its declared packet type.
    /// </summary>
    public bool RegisterPacketHandler(IPacketHandler handler, bool replaceExisting = false)
    {
        if (handler == null)
        {
            LogNode.Log("Cannot register a null packet handler.");
            return false;
        }
        if (_packetHandlers.ContainsKey(handler.PacketType) && !replaceExisting)
        {
            LogNode.Log($"A handler is already registered for packet type {(int)handler.PacketType}.");
            return false;
        }
        _packetHandlers[handler.PacketType] = handler;
        return true;
    }

    /// <summary>
    /// Unregisters a packet handler for the given packet type.
    /// </summary>
    public bool UnregisterPacketHandler(PacketInfo.PacketType packetType)
    {
        return _packetHandlers.Remove(packetType);
    }

    public override void _Ready()
    {
        var networkHandler = NetworkHandler.Singleton.Instance;
        if (networkHandler == null)
        {
            LogNode.Log("ServerPacketListener could not find NetworkHandler.");
            return;
        }
        _serverSignals = networkHandler.ServerSignals;
        _serverSignals.PacketReceived += OnServerPacket;
    }

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
    private void OnServerPacket(int peerId, byte[] data)
    {
        if (data == null || data.Length == 0)
        {
            LogNode.Log($"Received empty server packet from peer {peerId}.");
            return;
        }
        var packetType = (PacketInfo.PacketType)data[0];
        if (_packetHandlers.TryGetValue(packetType, out var handler))
        {
            handler.HandlePacket(peerId, data);
            return;
        }
        LogNode.Log($"Unhandled server packet type {(int)packetType} from peer {peerId}. Register an IPacketHandler to process it.");
    }
}
