using unsafe_maps.src;

namespace Nova;

public struct MeshRenderer : IDisposable
{
    public Mesh mesh;
    public Material material;
    public uint vao, vbo, ebo;
    public UnsafeArray<uint> lineIndices;

    public void Dispose()
    {
        lineIndices.Dispose();
        material.Dispose();
        mesh.Dispose();
    }
}