using Silk.NET.OpenGL;
using Silk.NET.GLFW;
using System.Numerics;
using System.Runtime.InteropServices;
using input;

namespace Nova;

unsafe class Program
{
    private static WindowHandle* window;

    static void Main()
    {
        try
        {
            SetupDisplayBackend();

            GContext._Glfw = Glfw.GetApi();

            var glfw = GContext._Glfw;

            if (!glfw.Init())
            {
                Console.WriteLine("Failed to init GLFW");
                return;
            }

            _ = AssetDirectories.Root;

            glfw.WindowHint(WindowHintInt.ContextVersionMajor, 3);
            glfw.WindowHint(WindowHintInt.ContextVersionMinor, 3);
            glfw.WindowHint(WindowHintClientApi.ClientApi, ClientApi.OpenGL);
            glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);

            window = glfw.CreateWindow(900, 800, "Nova Engine", null, null);

            if (window == null)
            {
                Console.WriteLine("Failed to create window");
                return;
            }

            glfw.MakeContextCurrent(window);

            GContext._GL = GL.GetApi(glfw.GetProcAddress);

            var gl = GContext._GL;

            glfw.SetFramebufferSizeCallback(window, (wnd, width, height) =>
            {
                gl.Viewport(0, 0, (uint)width, (uint)height);
            });

            glfw.SetWindowOpacity(window, 0.95f);

            Input.Init(window, glfw);

            ITextureLoader<Texture2D> textureLoader = new Texture2DLoader();

            var _baseMap = textureLoader.Load(TextureType.BaseMap, AssetDirectories.Textures + "/sakuya-Base_Color.png");
            var _normalMap = textureLoader.Load(TextureType.NormalMap, AssetDirectories.Textures + "/sakuya-Normal.png");
            var _metallicMap = textureLoader.Load(TextureType.MetallicMap, AssetDirectories.Textures + "/sakuya-Metallic.png");

            var mat = new Material(textureLoader);

            mat.Textures.Set(0, _baseMap);
            mat.Textures.Set(1, _normalMap);
            mat.Textures.Set(2, _metallicMap);
            mat.Color = new Vector4(1, 1, 1, 1);

            var cube = new Material(textureLoader);
            cube.Color = new Vector4(1, 1, 1, 1);

            IGraphicRenderer rendering = new OpenGLRenderer(textureLoader);

            rendering.AddObject(AssetDirectories.Models + "/NewSakuya.obj", mat);
            rendering.AddObject(AssetDirectories.Models + "/cube.obj", cube);

            rendering.InitializeObjects();

            while (!glfw.WindowShouldClose(window))
            {
                glfw.PollEvents();

                rendering.Render(window); 
            }

            rendering.Dispose();

            glfw.DestroyWindow(window);
            glfw.Terminate();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    static void SetupDisplayBackend()
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