using Silk.NET.GLFW;

namespace Nova;

public unsafe interface IGraphicRenderer : IDisposable
{
    void AddObject(string path, Material mat);
    void InitializeObjects();
    void Render(WindowHandle* window);
}