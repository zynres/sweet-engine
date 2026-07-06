using Sweet.Collections.Unsafe.Array;

namespace Sweet.Engine.Scene.Components;

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