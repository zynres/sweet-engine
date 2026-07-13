// Copyright © 2026 Zynres.
// Licensed under the Apache-2.0 License.

using System.Runtime.InteropServices;
using SweetEngine.Editor.Windows;
using SweetEngine.Core.Enums;
using SweetEngine.IO.Loaders;
using SweetEngine.Resources;
using SweetEngine.Graphics;
using SweetLib.Devices;
using SweetEngine.Core;
using System.Numerics;


namespace SweetEngine;

unsafe class Program
{
    static void Main()
    {
        try
        {
            var engine = new Engine();

            engine.Init();

            var window = GraphicContext.Window;
            var glfw = GraphicContext.Glfw;
            var gl = GraphicContext.GL;

            DebugWindow.Window = &engine.Device.Window;
            DebugWindow.Mouse = &engine.Device.Mouse;

            var textureLoader = new Texture2DLoader();
            var rendering = new OpenGLRenderer(&engine.Device, in textureLoader);

            var _baseMap = textureLoader.Load(TextureType.BaseMap, AssetDirectories.Textures + "/sakuya-Base_Color.png");
            var _normalMap = textureLoader.Load(TextureType.NormalMap, AssetDirectories.Textures + "/sakuya-Normal.png");
            var _metallicMap = textureLoader.Load(TextureType.MetallicMap, AssetDirectories.Textures + "/sakuya-Metallic.png");

            Material* mat = (Material*)NativeMemory.Alloc((nuint)sizeof(Material));
            *mat = new Material(in textureLoader);

            mat->Textures.Set(0, _baseMap);
            mat->Textures.Set(1, _normalMap);
            mat->Textures.Set(2, _metallicMap);
            mat->Color = new Vector4(1, 1, 1, 1);

            rendering.AddObject(AssetDirectories.Models + "/NewSakuya.obj", mat);
            //rendering.AddObject(AssetDirectories.Models + "/cube.obj", mat);
            rendering.InitializeObjects();

            while (!glfw.WindowShouldClose(window))
            {
                glfw.PollEvents();

                rendering.Render(in engine.Intent);

                engine.Intent.KickBackInvoke();
                Thread.Sleep(6);
            }

            engine.Dispose();
            rendering.Dispose();
            textureLoader.Dispose();

            glfw.DestroyWindow(window);
            glfw.Terminate();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}