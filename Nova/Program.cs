using Silk.NET.OpenGL;
using Silk.NET.GLFW;
using System.Numerics;

namespace Nova
{
    unsafe class Program
    {
        private static WindowHandle* window;
        private static Glfw glfw;
        private static GL gl;

        static void Main()
        {
            SetupDisplayBackend();

            glfw = Glfw.GetApi();

            if (!glfw.Init())
            {
                Console.WriteLine("Failed to init GLFW");
                return;
            }

            _ = AssetDirectories.Root;

            glfw.WindowHint(WindowHintInt.ContextVersionMajor, 3);
            glfw.WindowHint(WindowHintInt.ContextVersionMajor, 3);
            glfw.WindowHint(WindowHintClientApi.ClientApi, ClientApi.OpenGL);

            window = glfw.CreateWindow(900, 700, "Nova Engine", null, null);

            if (window == null)
            {
                Console.WriteLine("Failed to create window");
                return;
            }

            glfw.MakeContextCurrent(window);

            gl = GL.GetApi(glfw.GetProcAddress);

            glfw.SetFramebufferSizeCallback(window, (wnd, width, height) =>
            {
                gl.Viewport(0, 0, (uint)width, (uint)height);
            });

            gl.Viewport(0, 0, 900, 700);

            var _baseMap = new Texture2D(gl, TexFormat.BaseMap, AssetDirectories.Textures + "/sakuya-Base_Color.png");
            var _normalMap = new Texture2D(gl, TexFormat.NormalMap, AssetDirectories.Textures + "/sakuya-Normal.png");
            var _metallicMap = new Texture2D(gl, TexFormat.MetallicMap, AssetDirectories.Textures + "/sakuya-Metallic.png");

            var mat = new Material() { Color = new Vector4(1, 1, 1, 1), BaseMap = _baseMap, NormalMap = _normalMap, MetallicMap = _metallicMap};

            SceneRendering rendering = new();

            rendering.Init(glfw, gl);
            rendering.AddObject(AssetDirectories.Models + "/NewSakuya.obj", mat);
            rendering.InitializeObject();

            while (!glfw.WindowShouldClose(window))
            {
                glfw.PollEvents();

                if (InputAction.Press == (InputAction)glfw.GetKey(window, Keys.Number1))
                {
                    if (rendering.ModeLineRender)
                    {
                        rendering.ModeLineRender = false;

                        Console.WriteLine($"[Render Mode] => Fill mode");
                    }
                }
                else if (InputAction.Press == (InputAction)glfw.GetKey(window, Keys.Number2))
                {
                    if (!rendering.ModeLineRender)
                    {
                        rendering.ModeLineRender = true;

                        Console.WriteLine($"[Render Mode] => LineOnly");
                    }
                }

                rendering.Rendering(window);
            }

            if (rendering.ObjectDatas.Length > 0)
            {
                fixed (ObjectData* path = &rendering.ObjectDatas[0])
                {
                    for (int i = 0; i < rendering.ObjectDatas.Length; i++)
                    {
                        ObjectData* objectData = path + i;

                        gl.DeleteBuffer(objectData->Renderer.vbo);
                        gl.DeleteVertexArray(objectData->Renderer.vao);
                    }
                }
            }

            rendering.Shader.Dispose();

            glfw.DestroyWindow(window);
            glfw.Terminate();
        }

        static void SetupDisplayBackend()
        {
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
}