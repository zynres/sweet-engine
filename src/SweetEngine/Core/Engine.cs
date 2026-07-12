using SweetEngine.Rendering;
using SweetEngine.Library;
using SweetEngine.Window;
using SweetEngine.Scene;
using SweetLib.Intents;
using SweetLib.Devices;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;

namespace SweetEngine.Core;

public unsafe struct Engine
{
    public World World;

    public Module Module;
    public Process Process;

    public Renderer Renderer;
    public Resource Resource;

    public Intent Intent;
    public Device Device;
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
        Intent = new();
        Device = new();

        Device.Init(Editor.Window, Editor.Glfw);
        Intent.Init(Editor.Window, Editor.Glfw);
    }

    public void Dispose()
    {
        Intent.Dispose();
    }
}