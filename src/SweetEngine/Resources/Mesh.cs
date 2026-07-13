using SweetLib.Collections.Unsafe.Array;

namespace SweetEngine.Resources;

public struct Mesh
{
    public UnsafeArray<float> Vertices;
    public UnsafeArray<uint> Indices;

    public void Dispose()
    {
        Vertices.Dispose();
        Indices.Dispose();
    }
}