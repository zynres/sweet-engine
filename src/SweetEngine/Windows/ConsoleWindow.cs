using ImGuiNET;
using SweetLib.Devices;

namespace SweetEngine.Windows;

public static class ConsoleWindow
{
    public static void DrawImpl(ref GraphicContext context)
    {
        ImGui.Begin("Console");

        ImGui.End();
    }
}