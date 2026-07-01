using Silk.NET.OpenGL;

namespace Nova;

public interface ITextureLoader<T> where T : unmanaged
{
    T Load(TextureType format, string path);
    T Load(TextureType format, ReadOnlySpan<byte> pixels, int width, int height);
    T SetDefault(TextureType format);
}