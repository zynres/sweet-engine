using Silk.NET.OpenGL;
using Silk.NET.GLFW;

namespace Sweet.Engine.Renderer;

public static class GraphicStack
{
    public static GL GL { get; private set; }
    public static Glfw Glfw { get; private set; }

    internal static void SetGL(GL gl) => GL = gl;
    internal static void SetGlfw(Glfw glfw) => Glfw = glfw;

}