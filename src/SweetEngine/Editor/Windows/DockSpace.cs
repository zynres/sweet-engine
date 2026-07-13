using SweetLib.Collections.Unsafe.List;
using SweetEngine.Core.APIs;
using ImGuiNET;
using SweetLib.Devices;

namespace SweetEngine.Editor.Windows;

public unsafe struct DockSpace : IDisposable
{
    public readonly UnsafeList<EditorWindowAPI> Windows;

    public DockSpace()
    {
        Windows = new UnsafeList<EditorWindowAPI>(10);

        Windows.Add(new EditorWindowAPI() { Draw = &SceneWindow.DrawImpl });
        Windows.Add(new EditorWindowAPI() { Draw = &GameWindow.DrawImpl });
        Windows.Add(new EditorWindowAPI() { Draw = &HierarchyWindow.DrawImpl });
        Windows.Add(new EditorWindowAPI() { Draw = &InspectorWindow.DrawImpl });
        Windows.Add(new EditorWindowAPI() { Draw = &ConsoleWindow.DrawImpl });
        Windows.Add(new EditorWindowAPI() { Draw = &DebugWindow.DrawImpl });
    }

    public void Draw()
    {
        ImGui.DockSpaceOverViewport(ImGui.GetID("main_dock_space"), ImGui.GetMainViewport());

        for (uint i = 0; i < Windows.Length; i++)
            Windows[i].Draw();
    }

    public void Dispose()
    {
        Windows.Dispose();
    }
}