
namespace Sweet.Engine.Core;

public struct AssetDirectories
{
    public static readonly string Root;
    public static readonly string Models;
    public static readonly string Textures;
    public static readonly string Shaders;
    public static readonly string Scenes;

    static AssetDirectories()
    {
        Root = Path.GetFullPath(Path.Combine("D:", "sweet-engine", "assets"));
        Models = Path.Combine(Root, "Models");
        Textures = Path.Combine(Root, "Textures");
        Shaders = Path.Combine(Root, "Shaders");
        Scenes = Path.Combine(Root, "Scenes");

        EnsureExists(Root);
        EnsureExists(Models);
        EnsureExists(Textures);
        EnsureExists(Shaders);
        EnsureExists(Scenes);
    }

    private static void EnsureExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Console.WriteLine($"[AssetDirs] Created folder: {path}");
        }
    }
}