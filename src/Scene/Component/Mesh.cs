using Sweet.Collections.Unsafe.Array;

namespace Nova;

public struct Mesh : IDisposable
{
    public UnsafeArray<float> Vertices;
    public UnsafeArray<uint> Indices;
    public readonly uint VertexCount => Vertices.Length / 8;

    public void Dispose()
    {
        Vertices.Dispose();
        Indices.Dispose();
    }
}