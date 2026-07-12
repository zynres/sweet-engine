using Silk.NET.OpenGL;

namespace SweetEngine.Library.Resources;

public struct Texture2D
{
    public uint Id { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public TextureUnit Unit { get; private set; }

    public Texture2D(uint id, int width, int height, TextureUnit unit)
    {
        Id = id;
        Unit = unit;
        Width = width;
        Height = height;
    }
}
