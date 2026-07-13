using SweetEngine.Rendering;
using SweetEngine.Library;
using SweetEngine.Windows;
using SweetEngine.Scene;
using SweetLib.Intents;
using SweetLib.Devices;

namespace SweetEngine.Core;

public unsafe struct Engine
{
    public Device Device;
    public Intent Intent;

    public World World;

    public Module Module;
    public Process Process;

    public Renderer Renderer;
    public Resource Resource;

    public Editor Editor;
    public Gui Gui;

    public void Init()
    {
        Device = new();
        Device.Init();
        
        Intent = new();
        Intent.Init(GraphicContext.Window, GraphicContext.Glfw);

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
        Intent.Dispose();
    }
}