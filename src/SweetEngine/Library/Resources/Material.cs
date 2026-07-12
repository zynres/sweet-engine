using SweetEngine.Ingestion.Loader.Textures;
using Sweet.Collections.Unsafe.Array;
using SweetEngine.Rendering;
using SweetEngine.Core.Enums;
using System.Numerics;
using Silk.NET.OpenGL;

namespace SweetEngine.Library.Resources;

public unsafe struct Material
{
    public Vector4 Color;

    public bool IsFree;

    public UnsafeArray<Texture2D> Textures;

    public Material(in Texture2DLoader loader)
    {
        Textures = new UnsafeArray<Texture2D>(3);

        Textures.Set(0, loader.SetDefault(TextureType.BaseMap));
        Textures.Set(1, loader.SetDefault(TextureType.NormalMap));
        Textures.Set(2, loader.SetDefault(TextureType.MetallicMap));
    }

    public readonly void Bind(GL gl)
    {
        for (uint i = 0; i < Textures.Length; i++)
        {
            Texture2D texture = Textures[i];

            gl.ActiveTexture(texture.Unit);
            gl.BindTexture(TextureTarget.Texture2D, texture.Id);
        }
    }

    public readonly void UnBind(GL gl)
    {
        for (uint i = 0; i < Textures.Length; i++)
        {
            Texture2D texture = Textures[i];

            gl.ActiveTexture(texture.Unit);
            gl.BindTexture(TextureTarget.Texture2D, 0);
        }
    }

    public void Dispose()
    {
        Textures.Dispose();
    }
}
