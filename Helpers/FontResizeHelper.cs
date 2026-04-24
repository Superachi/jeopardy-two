using System.Collections.Generic;
using Godot;
using Nodes.Display;

namespace JeopardyTwo.Helpers;

public class NodeFontResizeInfo
{
    public Node? NodeWithFontToResize { get; set; }
    public float Ratio { get; set; } = 0.045f;
}

public static class FontResizeHelper
{
    public static void ResizeNodeFonts(List<NodeFontResizeInfo> nodeFonts, int minFontSize = 6, int maxFontSize = 80)
    {
        foreach (var nodeInfo in nodeFonts)
        {
            if (nodeInfo.NodeWithFontToResize is RichTextLabel label)
                {
                    SetRichTextFontSize(label, ComputeFontSize(nodeInfo.Ratio, minFontSize, maxFontSize));
            }
            else if (nodeInfo.NodeWithFontToResize is Button button)
            {
                SetButtonFontSize(button, ComputeFontSize(nodeInfo.Ratio, minFontSize, maxFontSize));
            }
        }
    }

    public static int ComputeFontSize(float ratio, int min, int max)
    {
        var screenSize = DisplayManager.ScreenSize - new Vector2(128, 64);
        var limitingDimension = Mathf.Min(screenSize.X, screenSize.Y);
        var computedSize = Mathf.RoundToInt(limitingDimension * ratio);
        return Mathf.Clamp(computedSize, min, max);
    }

    public static void SetRichTextFontSize(RichTextLabel label, int fontSize)
    {
        label.AddThemeFontSizeOverride("normal_font_size", fontSize);
    }

    public static void SetButtonFontSize(Button button, int fontSize)
    {
        button.AddThemeFontSizeOverride("font_size", fontSize);
    }
}