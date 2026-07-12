using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using SweetEngine.Core;
using SweetEngine.Rendering;

namespace SweetEngine.Window;

public unsafe struct Editor
{
    public WindowHandle* Window { get; private set; }
    public Glfw Glfw { get; private set; }
    public GL GL { get; private set; }

    public void Init()
    {
        CreateWindow(1060, 640);
    }
    
    private void CreateWindow(short sizeX, short sizeY)
    {
        SetupDisplayBackend();

        Glfw = Glfw.GetApi();

        if (!Glfw.Init())
            throw new Exception("Failed to init GLFW");

        _ = AssetDirectories.Root;

        Glfw.WindowHint(WindowHintInt.ContextVersionMajor, 3);
        Glfw.WindowHint(WindowHintInt.ContextVersionMinor, 3);
        Glfw.WindowHint(WindowHintClientApi.ClientApi, ClientApi.OpenGL);
        Glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);

        WindowHandle* window = Glfw.CreateWindow(sizeX, sizeY, "Sweet Engine", null, null);

        if (window == null)
            throw new Exception("Failed to create window");

        Glfw.MakeContextCurrent(window);
        Glfw.SetWindowOpacity(window, 1f);

        GL = GL.GetApi(Glfw.GetProcAddress);
        Window = window;
    }

    private void SetupDisplayBackend()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.WriteLine("Running on Windows - no display setup needed");
            return;
        }

        string waylandDisplay = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
        string x11Display = Environment.GetEnvironmentVariable("DISPLAY");

        if (!string.IsNullOrEmpty(waylandDisplay))
        {
            Console.WriteLine($"Using Wayland display: {waylandDisplay}");
            Environment.SetEnvironmentVariable("XDG_SESSION_TYPE", "wayland");
            Environment.SetEnvironmentVariable("GDK_BACKEND", "wayland");
        }
        else if (!string.IsNullOrEmpty(x11Display))
        {
            Console.WriteLine($"Using X11 display: {x11Display}");
            Environment.SetEnvironmentVariable("XDG_SESSION_TYPE", "x11");
            Environment.SetEnvironmentVariable("GDK_BACKEND", "x11");
        }
        else
        {
            Console.WriteLine("No display found, defaulting to X11");
            Environment.SetEnvironmentVariable("DISPLAY", ":1");
            Environment.SetEnvironmentVariable("XDG_SESSION_TYPE", "x11");
            Environment.SetEnvironmentVariable("GDK_BACKEND", "x11");
        }
    }
}