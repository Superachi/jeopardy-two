using Godot;

namespace Nodes.Signals;

public partial class ClientSignals : Node
{
    [Signal]
    public delegate void JoinedServerEventHandler();
    [Signal]
    public delegate void LeftServerEventHandler();
    [Signal]
    public delegate void PacketReceivedEventHandler(byte[] data);
}