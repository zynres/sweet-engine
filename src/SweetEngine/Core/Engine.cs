using SweetEngine.Resources;
using SweetEngine.Graphics;
using SweetEngine.Editor;
using SweetLib.Intents;
using SweetLib.Devices;
using SweetEngine.MDI;

namespace SweetEngine.Core;

public unsafe struct Engine
{
    public Device Device;
    public Intent Intent;

    public KitchenManager Kitchen;
    public MixerManager Mixer;

    public RenderPipeline Renderer;
    public ResourceManager Resource;

    public EditorManager Editor;
    public GuiSystem Gui;

    public void Init()
    {
        Device = new();
        Device.Init();
        
        Intent = new();
        Intent.Init(GraphicContext.Window, GraphicContext.Glfw);

        Kitchen = new();
        Mixer = new();

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