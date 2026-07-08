// Copyright © 2026 Zynres.
// Licensed under the Apache-2.0 License.

using Sweet.Engine.Renderer.Resources.Texture;
using System.Runtime.InteropServices;
using Sweet.Engine.Scene.Components;
using Sweet.Engine.Renderer.Graphic;
using Sweet.Intents.Generated;
using Sweet.Engine.Renderer;
using Sweet.Engine.Enums;
using Sweet.Engine.Core;
using System.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.GLFW;
using Sweet.Intents;
using Sweet.Devices;

namespace Sweet.Engine;

unsafe class Program
{
    private static WindowHandle* window;

    static void Main()
    {
        try
        {
            SetupDisplayBackend();

            GraphicStack.SetGlfw(Glfw.GetApi());

            var glfw = GraphicStack.Glfw;

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

            window = glfw.CreateWindow(1060, 640, "Sweet Engine", null, null);

            if (window == null)
            {
                Console.WriteLine("Failed to create window");
                return;
            }

            glfw.MakeContextCurrent(window);

            GraphicStack.SetGL(GL.GetApi(glfw.GetProcAddress));

            var gl = GraphicStack.GL;

            Texture2DLoader textureLoader = new Texture2DLoader();

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

            OpenGLRenderer rendering = new OpenGLRenderer(textureLoader);

            bool isAspect = true;

            float w = 1060f;
            float h = 640f;

            glfw.SetFramebufferSizeCallback(window, (wnd, width, height) =>
            {
                if (isAspect)
                {
                    w = width;
                    h = height;

                    gl.Viewport(0, 0, (uint)width, (uint)height);
                }
                else
                {
                    w = 1920f; h = 1080f;

                    float targetAspect = w / h;
                    float windowAspect = (float)width / height;

                    int vpX, vpY, vpW, vpH;

                    if (windowAspect > targetAspect)
                    {
                        vpH = height;
                        vpW = (int)(height * targetAspect);
                        vpX = (width - vpW) / 2;
                        vpY = 0;
                    }
                    else
                    {
                        vpW = width;
                        vpH = (int)(width / targetAspect);
                        vpX = 0;
                        vpY = (height - vpH) / 2;
                    }

                    gl.Viewport(vpX, vpY, (uint)vpW, (uint)vpH);
                }

                rendering._CameraController.Aspect = w / h;
            });

            rendering._CameraController.Aspect = w / h;

            glfw.SetWindowOpacity(window, 1f);

            Device.Init(window, glfw);
            Intent.Init(window, glfw);

            /*ITextureLoader<Texture2D> textureLoader = new Texture2DLoader();

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

            IGraphicRenderer rendering = new OpenGLRenderer(textureLoader);*/

            rendering.AddObject(AssetDirectories.Models + "/NewSakuya.obj", mat);
            rendering.AddObject(AssetDirectories.Models + "/cube.obj", cube);

            rendering.InitializeObjects();

            while (!glfw.WindowShouldClose(window))
            {
                glfw.PollEvents();

                rendering.Render(window);

                if (Intent.IsDown(EditorCameraIntents.Sprint))
                {

                }

                Thread.Sleep(6);
            }

            Device.Dispose();
            Intent.Dispose();

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