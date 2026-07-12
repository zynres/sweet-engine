using SweetEngine.Core;
using Silk.NET.OpenGL;
using Silk.NET.GLFW;

namespace SweetEngine.Windows;

public unsafe struct Editor
{
    public void Init()
    {
        _ = AssetDirectories.Root;
    }
}