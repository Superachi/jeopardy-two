using System.Collections.Generic;
using System.Linq;
using Achi.Godot.Common;
using Achi.Godot.Logging;
using Godot;
using Nodes.Signals;

namespace Nodes.Netcode;

public partial class NetworkHandler : Node
{
    public static Singleton<NetworkHandler> Singleton { get; private set; } = new();
    public ServerSignals ServerSignals = new();
    public ClientSignals ClientSignals = new();

    // Server vars
    private Queue<int> _availablePeerIds = new(Enumerable.Range(0, 256));

    private Dictionary<int, ENetPacketPeer> _clientPeers = new();

    // Client vars
    private ENetPacketPeer? _serverPeer = null;

    // General netcode vars
    private ENetConnection? _connection = null;
    private bool _isServer = false;

    public ENetConnection? Connection => _connection;

    public override void _Ready()
    {
        Singleton.MarkAsSingleton(this);
        AddChild(ServerSignals);
        AddChild(ClientSignals);
    }

    public void StartServer(string ip = "127.0.0.1", int port = 42069)
    {
        _connection = new ENetConnection();
        var result = _connection.CreateHostBound(ip, port);
        if (result != Error.Ok)
        {
            LogNode.Log($"Failed to start server: {result}");
            _connection = null;
            return;
        }

        LogNode.Log($"Server started successfully on {ip}:{port}");
        _isServer = true;
    }

    public void StartClient(string ip = "127.0.0.1", int port = 42069)
    {
        _connection = new ENetConnection();
        var result = _connection.CreateHost(1);
        if (result != Error.Ok)
        {
            LogNode.Log($"Failed to start client: {result}");
            _connection = null;
            return;
        }

        LogNode.Log($"Client started successfully on {ip}:{port}");
        _isServer = false;
    }

    public override void _Process(double delta)
    {
        if (_connection == null) return;


    }

    private void HandlePackets()
    {
        var packetEvent = _connection!.Service();
        var eventType = (ENetConnection.EventType)(int)packetEvent[0];

        while (eventType != ENetConnection.EventType.None)
        {
            var peer = (ENetPacketPeer)packetEvent[1];

            switch (eventType)
            {
                case ENetConnection.EventType.Error:
                    LogNode.Log("Network error occurred.");
                    return;

                case ENetConnection.EventType.Connect:
                    if (_isServer)
                        PeerConnected(peer);
                    else
                        ConnectedToServer();
                    break;

                case ENetConnection.EventType.Disconnect:
                    if (_isServer)
                    {
                        PeerDisconnected(peer);
                    }
                    else
                    {
                        DisconnectedFromServer();
                        return;
                    }

                    break;

                case ENetConnection.EventType.Receive:
                    if (_isServer) {
                        int peerId = (int)peer.GetMeta("id");
                        var data = peer.GetPacket();
                        ServerSignals.EmitSignal(ServerSignals.SignalName.PacketReceived, peerId, data);
                    }
                    else
                    {
                        var data = peer.GetPacket();
                        ClientSignals.EmitSignal(ClientSignals.SignalName.PacketReceived, data);
                    }
                    break;
            }

            packetEvent = _connection.Service();
            eventType = (ENetConnection.EventType)(int)packetEvent[0];
        }
    }

    private void PeerConnected(ENetPacketPeer peer)
    {
        if (!_availablePeerIds.TryDequeue(out int peerId))
        {
            LogNode.Log("No peer IDs available.");
            return;
        }

        LogNode.Log("Peer connected with ID: " + peerId);
        peer.SetMeta("id", peerId);
        _clientPeers[peerId] = peer;

        ServerSignals.EmitSignal(ServerSignals.SignalName.PeerJoined, peerId);
    }

    private void PeerDisconnected(ENetPacketPeer peer)
    {
        if (!peer.HasMeta("id"))
        {
            LogNode.Log("Peer disconnected without an assigned ID.");
            return;
        }

        int peerId = (int)peer.GetMeta("id");
        _availablePeerIds.Enqueue(peerId);
        _clientPeers.Remove(peerId);

        LogNode.Log($"Successfully disconnected: {peerId} from server!");
        ServerSignals.EmitSignal(ServerSignals.SignalName.PeerLeft, peerId);
    }

    private void ConnectedToServer()
    {
        LogNode.Log("Connected to server successfully.");
        ClientSignals.EmitSignal(ClientSignals.SignalName.JoinedServer);
    }

    private void DisconnectedFromServer()
    {
        LogNode.Log("Disconnected from server.");
        _connection = null;
        ClientSignals.EmitSignal(ClientSignals.SignalName.LeftServer);
    }

    public void DisconnectClient()
    {
        if (_isServer)
        {
            LogNode.Log("Cannot disconnect client from server mode.");
            return;
        }

        _serverPeer?.PeerDisconnect();
        _serverPeer = null;
    }
}