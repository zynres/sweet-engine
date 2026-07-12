using SweetLib.Devices;
using ImGuiNET;

namespace SweetEngine.Window;

public static unsafe class DebugWindow
{
    public static void DrawImpl()
    {
        ImGui.Begin("Debug");

        ImGui.Text($"Application average {1000f / ImGui.GetIO().Framerate:F3} ms/frame ({ImGui.GetIO().Framerate:F1} FPS)");
        ImGui.Text($"Cursor pos: {Device.Mouse->Position}");
        ImGui.Text($"Window size: {Device.Window->Size}");
        ImGui.End();
    }
}
