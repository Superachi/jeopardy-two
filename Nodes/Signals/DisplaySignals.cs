using Godot;

namespace Nodes.Signals;

public partial class DisplaySignals : Node
{
    [Signal]
    public delegate void WindowResizeEventHandler(Vector2 newSize);
}