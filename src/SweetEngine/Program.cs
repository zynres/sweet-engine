// Copyright © 2026 Zynres.
// Licensed under the Apache-2.0 License.

using SweetEngine.Ingestion.Loader.Textures;
using System.Runtime.InteropServices;
using SweetEngine.Rendering.Graphic;
using SweetEngine.Library.Resources;
using SweetEngine.Rendering;
using SweetEngine.Core;
using System.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.GLFW;
using Sweet.Intents;
using Sweet.Devices;

namespace SweetEngine;

unsafe class Program
{
    static void Main()
    {
        try
        {
            Engine engine = new Engine();

            engine.Initialize();

            WindowHandle* window = engine.Editor.CreateWindow(1060, 640);

            var glfw = GraphicStack.SetGlfw(engine.Editor.Context.Glfw);
            var gl = GraphicStack.SetGL(engine.Editor.Context.GL);

            Texture2DLoader textureLoader = new Texture2DLoader();
            OpenGLRenderer rendering = new OpenGLRenderer(textureLoader);

            /*var _baseMap = textureLoader.Load(TextureType.BaseMap, AssetDirectories.Textures + "/sakuya-Base_Color.png");
            var _normalMap = textureLoader.Load(TextureType.NormalMap, AssetDirectories.Textures + "/sakuya-Normal.png");
            var _metallicMap = textureLoader.Load(TextureType.MetallicMap, AssetDirectories.Textures + "/sakuya-Metallic.png");

            /*Material* mat = (Material*)NativeMemory.Alloc((nuint)sizeof(Material));
            *mat = new Material(in textureLoader);

            mat->Textures.Set(0, _baseMap);
            mat->Textures.Set(1, _normalMap);
            mat->Textures.Set(2, _metallicMap);
            mat->Color = new Vector4(1, 1, 1, 1);*/

            Material* cube = (Material*)NativeMemory.Alloc((nuint)sizeof(Material));
            *cube = new Material(in textureLoader);
            cube->Color = new Vector4(
                Random.Shared.NextSingle(),
                Random.Shared.NextSingle(),
                Random.Shared.NextSingle(),
                Random.Shared.NextSingle());

            //rendering.AddObject(AssetDirectories.Models + "/NewSakuya.obj", mat);
            rendering.AddObject(AssetDirectories.Models + "/cube.obj", cube);

            Device.Init(window, glfw);
            Intent.Init(window, glfw);

            rendering.InitializeObjects();

            while (!glfw.WindowShouldClose(window))
            {
                glfw.PollEvents();

                rendering.Render(window);

                Intent.KickBackInvoke();
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
}