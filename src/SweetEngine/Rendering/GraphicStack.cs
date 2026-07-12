using Silk.NET.OpenGL;
using Silk.NET.GLFW;

namespace SweetEngine.Rendering;

public static class GraphicStack
{
    public static GL GL { get; private set; }
    public static Glfw Glfw { get; private set; }

    internal static GL SetGL(GL gl) => GL = gl;
    internal static Glfw SetGlfw(Glfw glfw) => Glfw = glfw;
}