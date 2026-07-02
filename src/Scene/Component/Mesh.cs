
using unsafe_maps.src;

namespace Nova;

public struct Mesh : IDisposable
{
    public UnsafeArray<float> Vertices;
    public UnsafeArray<uint> Indices;
    public readonly int VertexCount => Vertices.Length / 8;

    public void Dispose()
    {
        Vertices.Dispose();
        Indices.Dispose();
    }
}