using SweetLib.Devices;
using ImGuiNET;

namespace SweetEngine.Editor.Windows;

public static unsafe class DebugWindow
{
    public static Mouse* Mouse;
    public static Window* Window;
    
    public static void DrawImpl()
    {
        ImGui.Begin("Debug");

        ImGui.Text($"Application average {1000f / ImGui.GetIO().Framerate:F3} ms/frame ({ImGui.GetIO().Framerate:F1} FPS)");
        ImGui.Text($"Cursor pos: {Mouse->Position}");
        ImGui.Text($"Window size: {Window->Size}");
        ImGui.End();
    }
}
