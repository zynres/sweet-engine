using Silk.NET.OpenGL;
using StbImageSharp;
using System;

namespace Nova
{
    public enum TexFormat
    {
        None,
        BaseMap,
        NormalMap,
        MetallicMap,
    }

    public struct Texture2D
    {
        public uint Handle { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Texture2D(TexFormat texFormat = TexFormat.None, string path = "")
        {
            var gl = GContext._GL;

            try
            {
                using var stream = File.OpenRead(path);
                ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

                Width = image.Width;
                Height = image.Height;

                Handle = gl.GenTexture();
                gl.BindTexture(GLEnum.Texture2D, Handle);

                gl.TexImage2D<byte>(GLEnum.Texture2D, 0, (int)GLEnum.Rgba, (uint)image.Width,
                    (uint)image.Height, 0, GLEnum.Rgba, GLEnum.UnsignedByte, image.Data);

                gl.GenerateMipmap(GLEnum.Texture2D);

                int filter = (int)GLEnum.LinearMipmapLinear;

                gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, ref filter);
                filter = (int)GLEnum.Linear;

                gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, ref filter);
                filter = (int)GLEnum.Repeat;

                gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, ref filter);
                filter = (int)GLEnum.Repeat;

                gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, ref filter);

                gl.BindTexture(GLEnum.Texture2D, 0);
            }
            catch (Exception)
            {
                uint whiteTex = CreateWhiteTexture(texFormat);
                gl.BindTexture(GLEnum.Texture2D, whiteTex);
            }
        }

        public readonly void Bind(TextureUnit unit)
        {
            var gl = GContext._GL;

            if (gl != null)
            {
                gl.ActiveTexture(unit);
                gl.BindTexture(GLEnum.Texture2D, Handle);
            }
        }

        public readonly void UnBind(TextureUnit unit)
        {
            var gl = GContext._GL;

            if (gl != null)
            {
                gl.ActiveTexture(unit);
                gl.BindTexture(GLEnum.Texture2D, 0);
            }
        }

        public readonly uint CreateWhiteTexture(TexFormat texFormat)
        {
            var gl = GContext._GL;

            uint tex = gl.GenTexture();
            gl.BindTexture(TextureTarget.Texture2D, tex);

            byte[] whitePixel;

            if (texFormat == TexFormat.None || texFormat == TexFormat.BaseMap)
            {
                whitePixel = [255, 255, 255, 255]; 
            }
            else if (texFormat == TexFormat.NormalMap)
            {
                whitePixel = [127, 127, 255, 255];
            }
            else if (texFormat == TexFormat.MetallicMap)
            {
                whitePixel = [0, 127, 0, 255]; 
            }

            unsafe
            {
                fixed (byte* p = whitePixel)
                {
                    gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, 1, 1, 0,
                                  PixelFormat.Rgba, PixelType.UnsignedByte, p);
                }
            }

            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            gl.BindTexture(TextureTarget.Texture2D, 0);

            return tex;
        }
    }
}