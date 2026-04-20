using Achi.Godot.PathResolving.Attributes;
using Godot;
using Nodes.Display;

namespace JeopardyTwo.Nodes;

[SceneFile]
public partial class QuestionBoard : Panel
{
    private float _padding = 16;

	public override void _Ready()
    {
        DisplayManager.DisplaySignals.WindowResize += OnWindowResize;
        SetPanelSize();
    }

    public override void _Process(double delta)
	{
	}

    private void OnWindowResize(Vector2 screenSize)
    {
        SetPanelSize();
    }

    private void SetPanelSize()
    {
        var screenSize = DisplayManager.ScreenSize;
        Position = new Vector2(_padding, _padding);
        Size = screenSize - new Vector2(_padding * 2, _padding * 2);
    }
}
