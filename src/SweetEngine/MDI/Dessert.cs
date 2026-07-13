using SweetEngine.MDI.Ingredients;
using System.Numerics;

namespace SweetEngine.MDI;

public struct Dessert
{
    public Transform Transform;
    public MeshRenderer Renderer;

    public Vector3 Direction;

    public void Dispose()
    {
        Renderer.Dispose();
    }
}
