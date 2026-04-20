using Achi.Godot.Common;
using Godot;
using Nodes.Signals;

namespace Nodes.Display;

public partial class DisplayManager : Node
{
    public static DisplaySignals DisplaySignals { get; private set; } = new DisplaySignals();
    private Vector2 _viewportSize;

    public static Singleton<DisplayManager> Singleton { get; private set; } = new();

    public override void _Ready()
    {
        Singleton.MarkAsSingleton(this);
        AddChild(DisplaySignals);
    }

    public override void _Process(double delta)
    {
        var oldViewportSize = _viewportSize;
        _viewportSize = GetViewport().GetVisibleRect().Size;

        if (oldViewportSize != _viewportSize)
        {
            DisplaySignals.EmitSignal(nameof(Signals.DisplaySignals.WindowResize), _viewportSize);
        }
    }

    public override void _Notification(int notification)
    {
        if (notification == NotificationPredelete)
        {
            Singleton.ClearSingleton();
        }
    }

    public static Vector2 ScreenSize => Singleton.Instance.GetViewport().GetVisibleRect().Size;
}