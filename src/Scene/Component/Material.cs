using System.Numerics;
using Silk.NET.OpenGL;
using unsafe_maps.src;

namespace Nova;

public unsafe struct Material : IDisposable
{
    public Vector4 Color;

    public UnsafeArray<Texture2D> Textures;

    public Material(Texture2DLoader loader)
    {
        Textures = new UnsafeArray<Texture2D>(3);

        Textures.Set(0, loader.SetDefault(TextureType.BaseMap));
        Textures.Set(1, loader.SetDefault(TextureType.NormalMap));
        Textures.Set(2, loader.SetDefault(TextureType.MetallicMap));
    }

    public readonly void Bind()
    {
        var gl = GraphicStack._GL;

        for (int i = 0; i < Textures.Length; i++)
        {
            Texture2D* texture = Textures[i];

            gl.ActiveTexture(texture->Unit);
            gl.BindTexture(TextureTarget.Texture2D, texture->Id);
        }
    }

    public readonly void UnBind()
    {
        var gl = GraphicStack._GL;

        for (int i = 0; i < Textures.Length; i++)
        {
            Texture2D* texture = Textures[i];

            gl.ActiveTexture(texture->Unit);
            gl.BindTexture(TextureTarget.Texture2D, 0);
        }
    }

    public void Dispose()
    {
        Textures.Dispose();
    }
}
