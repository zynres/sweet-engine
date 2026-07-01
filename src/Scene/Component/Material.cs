using System.Numerics;
using Silk.NET.OpenGL;
using unsafe_maps.maps;

namespace Nova;

public unsafe struct Material : IDisposable
{
    public Vector4 Color;

    public UnsafeArray<Texture2D> Textures;

    public Material(ITextureLoader<Texture2D>* loader)
    {
        Textures = new UnsafeArray<Texture2D>(3);

        Textures.Set(0, loader->SetDefault(TextureType.BaseMap));
        Textures.Set(1, loader->SetDefault(TextureType.NormalMap));
        Textures.Set(2, loader->SetDefault(TextureType.MetallicMap));
    }

    public readonly void Bind()
    {
        var gl = GContext._GL;

        if (gl != null)
        {
            for (int i = 0; i < Textures.Length; i++)
            {
                Texture2D* texture = Textures[i];

                gl.ActiveTexture(texture->Unit);
                gl.BindTexture(GLEnum.Texture2D, texture->Id);
            }
        }
    }

    public readonly void UnBind()
    {
        var gl = GContext._GL;

        if (gl != null)
        {
            for (int i = 0; i < Textures.Length; i++)
            {
                Texture2D* texture = Textures[i];

                gl.ActiveTexture(texture->Unit);
                gl.BindTexture(GLEnum.Texture2D, 0);
            }
        }
    }

    public void Dispose()
    {
        Textures.Dispose();
    }
}
