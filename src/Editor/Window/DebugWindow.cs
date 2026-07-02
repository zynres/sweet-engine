using ImGuiNET;

namespace Nova;

public static class DebugWindow
{
    public static void DrawImpl()
    {
        ImGui.Begin("Debug");

        ImGui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)", 1000f / ImGui.GetIO().Framerate, ImGui.GetIO().Framerate));
        ImGui.End();
    }
}
