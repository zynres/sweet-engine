// Copyright © 2026 Zynres.
// Licensed under the Apache-2.0 License.

using SweetEngine.Ingestion.Loader.Textures;
using System.Runtime.InteropServices;
using SweetEngine.Rendering.Graphic;
using SweetEngine.Library.Resources;
using SweetEngine.Core.Enums;
using SweetEngine.Core;
using System.Numerics;
using Silk.NET.GLFW;


namespace SweetEngine;

unsafe class Program
{
    static void Main()
    {
        try
        {
            var engine = new Engine();

            engine.Initialize();

            WindowHandle* window = engine.Editor.Window;

            var glfw = engine.Editor.Glfw;
            var gl = engine.Editor.GL;

            var textureLoader = new Texture2DLoader(gl);
            var rendering = new OpenGLRenderer(gl, glfw, &engine.Device, in textureLoader);

            var _baseMap = textureLoader.Load(TextureType.BaseMap, AssetDirectories.Textures + "/sakuya-Base_Color.png");
            var _normalMap = textureLoader.Load(TextureType.NormalMap, AssetDirectories.Textures + "/sakuya-Normal.png");
            var _metallicMap = textureLoader.Load(TextureType.MetallicMap, AssetDirectories.Textures + "/sakuya-Metallic.png");

            Material* mat = (Material*)NativeMemory.Alloc((nuint)sizeof(Material));
            *mat = new Material(in textureLoader);

            mat->Textures.Set(0, _baseMap);
            mat->Textures.Set(1, _normalMap);
            mat->Textures.Set(2, _metallicMap);
            mat->Color = new Vector4(
                Random.Shared.NextSingle(),
                Random.Shared.NextSingle(),
                Random.Shared.NextSingle(),
                Random.Shared.NextSingle());
            
            rendering.AddObject(AssetDirectories.Models + "/NewSakuya.obj", mat);
            rendering.AddObject(AssetDirectories.Models + "/cube.obj", mat);
            rendering.InitializeObjects();

            while (!glfw.WindowShouldClose(window))
            {
                glfw.PollEvents();

                rendering.Render(window, in engine.Intent);

                engine.Intent.KickBackInvoke();
                Thread.Sleep(6);
            }

            engine.Dispose();

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