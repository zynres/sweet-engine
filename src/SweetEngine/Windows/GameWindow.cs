using SweetLib.Devices;
using ImGuiNET;

namespace SweetEngine.Windows;

public static class GameWindow
{
    public static void DrawImpl(ref GraphicContext context)
    {
        ImGui.Begin("Game");

        ImGui.End();
    }
}