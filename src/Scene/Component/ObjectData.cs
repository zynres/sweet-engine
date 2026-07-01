namespace Nova;

public struct ObjectData : IDisposable
{
    public Transform Transform;
    public MeshRenderer Renderer;

    public void Dispose()
    {
        Renderer.Dispose();
    }
}
