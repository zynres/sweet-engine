using SweetEngine.IO.Loaders;

namespace SweetEngine.Resources;

public struct ResourceManager
{
    private readonly Texture2DLoader texture2DLoader;

    public ResourceManager()
    {
        texture2DLoader = new();
    }
}