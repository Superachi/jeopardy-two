using Achi.Godot.Logging;
using Godot;
using Nodes.Netcode.Packets;
using Nodes.Signals;

namespace Nodes.Netcode.PeerRegistry;

/// <summary>
/// Listens for client packet events and dispatches them to the appropriate handler(s).
/// </summary>
public partial class ClientPacketListener : Node
{
    private ClientSignals? _clientSignals;
    private ClientPeerManager? _peerManager;

    public override void _Ready()
    {
        var networkHandler = NetworkHandler.Singleton.Instance;
        if (networkHandler == null)
        {
            LogNode.Log("ClientPacketListener could not find NetworkHandler.");
            return;
        }

        _clientSignals = networkHandler.ClientSignals;
        _peerManager = networkHandler.ClientPeerManager;
        if (_clientSignals == null || _peerManager == null)
        {
            LogNode.Log("ClientPacketListener could not find required signal or peer manager.");
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
    /// Reads incoming packet bytes and routes each packet to the right handler.
    /// </summary>
    /// <param name="data">Raw packet bytes received from the network.</param>
    private void OnClientPacket(byte[] data)
    {
        if (data == null || data.Length == 0)
        {
            LogNode.Log("Received empty client packet.");
            return;
        }

        var packetType = (PacketInfo.PacketType)data[0];

        switch (packetType)
        {
            case PacketInfo.PacketType.IdAssignment:
                _peerManager?.ManageIds(IDAssignment.CreateFromData(data));
                break;

            default:
                LogNode.Log($"Unhandled packet type index {data[0]}.");
                break;
        }
    }
}
