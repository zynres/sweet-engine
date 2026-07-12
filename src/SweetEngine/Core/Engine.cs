using SweetEngine.Rendering;
using SweetEngine.Library;
using SweetEngine.Window;
using SweetEngine.Scene;

namespace SweetEngine.Core;

public struct Engine
{
    public World World;

    public Module Module;
    public Process Process;

    public Renderer Renderer;
    public Resource Resource;

    public Editor Editor;
    public Gui Gui;

    public void Initialize()
    {
        World = new();
        Module = new();
        Process = new();

        Renderer = new();
        Resource = new();

        Editor = new();
        Editor.Init();

        Gui = new();
    }

    public void Dispose()
    {
        
    }
}