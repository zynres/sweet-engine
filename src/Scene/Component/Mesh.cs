
using unsafe_maps.maps;

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