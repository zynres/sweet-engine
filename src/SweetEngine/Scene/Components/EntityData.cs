using System.Numerics;

namespace SweetEngine.Scene.Components;

public struct EntityData : IDisposable
{
    public Transform Transform;
    public MeshRenderer Renderer;

    public Vector3 Direction;

    public void Dispose()
    {
        Renderer.Dispose();
    }
}
