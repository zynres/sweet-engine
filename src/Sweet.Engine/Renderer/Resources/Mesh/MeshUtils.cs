using Sweet.Collections.Unsafe.HashSet;
using Sweet.Collections.Unsafe.Array;

namespace Sweet.Engine.Renderer.Resources.Mesh;

public unsafe struct MeshUtils
{
    public static UnsafeArray<uint> GenerateUniqueEdges(in UnsafeArray<uint> triangleIndices)
    {
        var edges = new UnsafeHashSet<(uint, uint)>(triangleIndices.Length * 2);

        for (uint i = 0; i < triangleIndices.Length; i += 3)
        {
            uint a = triangleIndices[i];
            uint b = triangleIndices[i + 1];
            uint c = triangleIndices[i + 2];

            AddEdge(ref edges, a, b);
            AddEdge(ref edges, b, c);
            AddEdge(ref edges, c, a);
        }

        var indices = new UnsafeArray<uint>(edges.Length * 2);

        uint index = 0;

        for (uint i = 0; i < edges.Length; i++)
        {
            var (a, b) = edges[i];

            indices.Set(index++, a);
            indices.Set(index++, b);
        }

        edges.Dispose();

        return indices;
    }

    private static void AddEdge(ref UnsafeHashSet<(uint, uint)> edges, uint v1, uint v2)
    {
        if (v1 < v2)
            edges.Add((v1, v2));
        else
            edges.Add((v2, v1));
    }
}