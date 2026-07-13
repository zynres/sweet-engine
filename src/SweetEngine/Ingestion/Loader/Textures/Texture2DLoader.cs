// Copyright © 2026 Zynres.
// Licensed under the Apache-2.0 License.

using SweetLib.Collections.Unsafe.Dictionary;
using SweetEngine.Library.Resources;
using SweetEngine.Core.Enums;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace SweetEngine.Ingestion.Loader.Textures;

public unsafe struct Texture2DLoader
{
    private readonly UnsafeDictionary<TextureType, uint> default_maps;
    private readonly UnsafeDictionary<TextureType, TextureUnit> units;

    private readonly GL gl;

    public Texture2DLoader(GL gl)
    {
        this.gl = gl;

        default_maps = new(3)
        {
            [TextureType.BaseMap | TextureType.None] = CreateDefaultTexture(TextureType.BaseMap),
            [TextureType.NormalMap] = CreateDefaultTexture(TextureType.NormalMap),
            [TextureType.MetallicMap] = CreateDefaultTexture(TextureType.MetallicMap)
        };

        units = new(3)
        {
            [TextureType.BaseMap] = TextureUnit.Texture0,
            [TextureType.NormalMap] = TextureUnit.Texture1,
            [TextureType.MetallicMap] = TextureUnit.Texture2
        };
    }

    public readonly Texture2D Load(TextureType format, string path)
    {
        TextureUnit unit = units[format];

        try
        {
            using var stream = File.OpenRead(path);
            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            uint id = gl.GenTexture();

            var texture = new Texture2D(id, image.Width, image.Height, unit);

            gl.BindTexture(GLEnum.Texture2D, id);

            gl.TexImage2D<byte>(GLEnum.Texture2D, 0, (int)GLEnum.Rgba, (uint)image.Width,
                (uint)image.Height, 0, GLEnum.Rgba, GLEnum.UnsignedByte, image.Data);

            gl.GenerateMipmap(GLEnum.Texture2D);

            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);

            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.Repeat);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.Repeat);

            gl.BindTexture(GLEnum.Texture2D, 0);

            return texture;
        }
        catch (Exception)
        {
            return SetDefault(format);
        }
    }

    public readonly Texture2D Load(TextureType format, ReadOnlySpan<byte> pixels, int width, int height)
    {
        TextureUnit unit = units[format];

        try
        {
            Console.WriteLine($"width: {width} height: {height}");

            uint id = gl.GenTexture();

            var texture = new Texture2D(id, width, height, unit);

            gl.BindTexture(GLEnum.Texture2D, id);

            gl.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            gl.TexImage2D(GLEnum.Texture2D, 0, (int)InternalFormat.Rgba8, (uint)width,
                (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);

            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.ClampToEdge);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.ClampToEdge);

            gl.BindTexture(GLEnum.Texture2D, 0);

            return texture;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in load texture: {ex}");

            return SetDefault(format);
        }
    }

    public readonly Texture2D SetDefault(TextureType format)
    {
        var id = default_maps[format];

        return new Texture2D(id, 1, 1, units[format]);
    }

    private readonly uint CreateDefaultTexture(TextureType TextureType)
    {
        uint tex = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, tex);

        Span<byte> whitePixel = stackalloc byte[4];

        if (TextureType == TextureType.None || TextureType == TextureType.BaseMap)
        {
            whitePixel[0] = 255;
            whitePixel[1] = 255;
            whitePixel[2] = 255;
            whitePixel[3] = 255;
        }
        else if (TextureType == TextureType.NormalMap)
        {
            whitePixel[0] = 127;
            whitePixel[1] = 127;
            whitePixel[2] = 255;
            whitePixel[3] = 255;
        }
        else if (TextureType == TextureType.MetallicMap)
        {
            whitePixel[0] = 0;
            whitePixel[1] = 127;
            whitePixel[2] = 0;
            whitePixel[3] = 255;
        }

        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, 1, 1, 0,
            PixelFormat.Rgba, PixelType.UnsignedByte, whitePixel);

        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        gl.BindTexture(TextureTarget.Texture2D, 0);

        return tex;
    }

    public void Dispose()
    {
        default_maps.Dispose();
        units.Dispose();
    }
}