using Godot;

namespace Nodes.Signals;

public partial class ServerSignals : Node
{
    [Signal]
    public delegate void PeerJoinedEventHandler(int playerId);
    [Signal]
    public delegate void PeerLeftEventHandler(int playerId);
    [Signal]
    public delegate void PacketReceivedEventHandler(int playerId, byte[] data);
}