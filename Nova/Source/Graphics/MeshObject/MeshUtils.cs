using unsafe_maps.maps;

namespace Nova
{
    public struct MeshUtils
    {
        public static UnsafeArray<uint> GenerateUniqueEdges(UnsafeArray<uint> triangleIndices)
        {
            var edges = new HashSet<(uint, uint)>();

            for (int i = 0; i < triangleIndices.Length; i += 3)
            {
                uint a = triangleIndices[i];
                uint b = triangleIndices[i + 1];
                uint c = triangleIndices[i + 2];

                AddEdge(edges, a, b);
                AddEdge(edges, b, c);
                AddEdge(edges, c, a);
            }

            var values = edges.SelectMany(e => new[] { e.Item1, e.Item2 }).ToArray();

            var indices = new UnsafeArray<uint>(values.Length);

            for (int i = 0; i < indices.Length; i++)
            {
                uint index = indices[i];

                index = values[i];
            }

            return indices;
        }

        private static void AddEdge(HashSet<(uint, uint)> edges, uint v1, uint v2)
        {
            if (v1 < v2)
                edges.Add((v1, v2));
            else
                edges.Add((v2, v1));
        }
    }
}