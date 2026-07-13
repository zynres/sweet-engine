using SweetEngine.Ingestion.Loader.Textures;

namespace SweetEngine.Library;

public struct Resource
{
    private readonly Texture2DLoader texture2DLoader;

    public Resource()
    {
        texture2DLoader = new();
    }
}