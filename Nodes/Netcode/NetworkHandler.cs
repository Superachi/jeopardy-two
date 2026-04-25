using System;
using System.Collections.Generic;
using System.Linq;
using Achi.Godot.Common;
using Achi.Godot.Logging;
using Godot;
using Nodes.Netcode.PacketManagement;
using Nodes.Netcode.Packets;
using Nodes.Netcode.PeerManagement;
using Nodes.Signals;

namespace Nodes.Netcode;

public partial class NetworkHandler : Node
{
    public static Singleton<NetworkHandler> Singleton { get; private set; } = new();
    public ServerPeerManager ServerPeerManager { get; } = new();
    public ServerPacketListener ServerPacketListener { get; } = new();
    public ClientPeerManager ClientPeerManager { get; } = new();
    public ClientPacketListener ClientPacketListener { get; } = new();
    public ServerSignals ServerSignals { get; } = new();
    public ClientSignals ClientSignals { get; } = new();
    public GeneralNetworkSignals GeneralNetworkSignals { get; } = new();

    /// <summary>
    /// The maximum number of peers allowed in the session. Configurable at construction.
    /// </summary>
    public int MaxPeerCount { get; set; } = 256;

    // Server vars
    private Queue<int> _availablePeerIds = new();

    private Dictionary<int, ENetPacketPeer> _clientPeers = new();

    // Client vars
    private ENetPacketPeer? _serverPeer = null;

    // General netcode vars
    private ENetConnection? _connection = null;
    public bool IsServer { get; private set; } = false;

    public ENetConnection? Connection => _connection;

    /// <summary>
    /// Enables or disables debug logging for all netcode components.
    /// </summary>
    public static bool DebugLoggingEnabled { get; set; } = true;

    /// <summary>
    /// Wrapper for LogNode.Log that checks DebugLoggingEnabled.
    /// </summary>
    public static void DebugLog(string message)
    {
        if (!DebugLoggingEnabled) return;

        string finalMessage = "";
        var handler = NetworkHandler.Singleton.Instance;
        if (handler.IsServer)
        {
            finalMessage += "[🟢SERVER] ";
        }
        else
        {
            var clientId = handler.ClientPeerManager?.LocalPeerId;
            if (clientId != null)
                finalMessage += $"[🟣CLIENT #{clientId}] ";
            else
                finalMessage += "[🟣CLIENT (ID UNKNOWN)] ";
        }
        finalMessage += message;
        LogNode.Log(finalMessage);
    }

    /// <summary>
    /// Initializes the NetworkHandler and its child nodes. Sets up peer ID pool.
    /// </summary>
    public override void _Ready()
    {
        Singleton.MarkAsSingleton(this);
        if (MaxPeerCount <= 0 || MaxPeerCount > 256)
        {
            MaxPeerCount = 256;
            DebugLog("MaxPeerCount was out of range. Reset to 256.");
        }
        _availablePeerIds = new Queue<int>(Enumerable.Range(0, MaxPeerCount));

        AddChild(ServerPeerManager);
        AddChild(ServerPacketListener);
        AddChild(ClientPeerManager);
        AddChild(ClientPacketListener);
        AddChild(ServerSignals);
        AddChild(ClientSignals);
        AddChild(GeneralNetworkSignals);
    }

    private void StartNetServer(string ip = "127.0.0.1", int port = 42069)
    {
        _connection = new ENetConnection();
        var result = _connection.CreateHostBound(ip, port);
        if (result != Error.Ok)
        {
            DebugLog($"Failed to start server: {result}");
            _connection = null;
            return;
        }

        DebugLog($"Server started successfully on {ip}:{port}");
        IsServer = true;
    }

    private void StartNetClient(string ip = "127.0.0.1", int port = 42069)
    {
        _connection = new ENetConnection();
        var result = _connection.CreateHost(1);
        if (result != Error.Ok)
        {
            DebugLog($"Failed to start client: {result}");
            _connection = null;
            return;
        }

        _connection.ConnectToHost(ip, port);
        DebugLog($"Client started and connecting to {ip}:{port}");

        IsServer = false;
    }

    public override void _Process(double delta)
    {
        if (_connection == null) return;
        HandlePackets();
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
                    DebugLog("Network error occurred.");
                    return;

                case ENetConnection.EventType.Connect:
                    if (IsServer)
                        PeerConnected(peer);
                    else
                        ConnectedToServer();
                    break;

                case ENetConnection.EventType.Disconnect:
                    if (IsServer)
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
                    if (IsServer) {
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
            DebugLog("No peer IDs available.");
            return;
        }

        DebugLog("Peer connected with ID: " + peerId);
        peer.SetMeta("id", peerId);
        _clientPeers[peerId] = peer;

        ServerSignals.EmitSignal(ServerSignals.SignalName.PeerJoined, peerId);
    }

    private void PeerDisconnected(ENetPacketPeer peer)
    {
        if (!peer.HasMeta("id"))
        {
            DebugLog("Peer disconnected without an assigned ID.");
            return;
        }

        int peerId = (int)peer.GetMeta("id");
        _availablePeerIds.Enqueue(peerId);
        _clientPeers.Remove(peerId);

        DebugLog($"Successfully disconnected: {peerId} from server!");
        ServerSignals.EmitSignal(ServerSignals.SignalName.PeerLeft, peerId);
    }

    private void ConnectedToServer()
    {
        DebugLog("Connected to server successfully.");
        ClientSignals.EmitSignal(ClientSignals.SignalName.JoinedServer);
    }

    private void DisconnectedFromServer()
    {
        DebugLog("Disconnected from server.");
        _connection = null;
        ClientSignals.EmitSignal(ClientSignals.SignalName.LeftServer);
    }

    public void DisconnectClient()
    {
        if (IsServer)
        {
            DebugLog("Cannot disconnect client from server mode.");
            return;
        }

        _serverPeer?.PeerDisconnect();
        _serverPeer = null;
    }

    # region Public API

    private static string UninitializedErrorMessage() => $"NetworkHandler singleton instance is not initialized.";

    public static void StartServer(string ip = "127.0.0.1", int port = 42069)
    {
        var handler = Singleton.Instance ?? throw new Exception(UninitializedErrorMessage());
        handler.StartNetServer(ip, port);
    }

    public static void StartClient(string ip = "127.0.0.1", int port = 42069)
    {
        var handler = Singleton.Instance ?? throw new Exception(UninitializedErrorMessage());
        handler.StartNetClient(ip, port);
    }

    public static void RegisterServerPacketHandler(IServerPacketHandler handler)
    {
        var networkHandler = Singleton.Instance ?? throw new Exception(UninitializedErrorMessage());
        networkHandler.ServerPacketListener.RegisterPacketHandler(handler);
    }

    public static void RegisterClientPacketHandler(IClientPacketHandler handler)
    {
        var networkHandler = Singleton.Instance ?? throw new Exception(UninitializedErrorMessage());
        networkHandler.ClientPacketListener.RegisterPacketHandler(handler);
    }

    public static bool TryGetConnection(out ENetConnection? connection)
    {
        var handler = Singleton.Instance ?? throw new Exception(UninitializedErrorMessage());
        connection = handler.Connection;
        return connection != null;
    }

    public static void BroadcastPacket(PacketInfo packet)
    {
        var handler = Singleton.Instance ?? throw new Exception(UninitializedErrorMessage());
        if (handler.Connection == null)
        {
            throw new Exception("NetworkHandler connection returned null.");
        }

        packet.Broadcast(handler.Connection);
    }

    # endregion Public API
}