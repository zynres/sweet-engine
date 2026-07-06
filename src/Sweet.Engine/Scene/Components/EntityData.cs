namespace Sweet.Engine.Scene.Components;

public struct EntityData : IDisposable
{
    public Transform Transform;
    public MeshRenderer Renderer;

    public void Dispose()
    {
        Renderer.Dispose();
    }
}
