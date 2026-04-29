namespace Nova
{
    public static class MeshUtils
    {
        public static uint[] GenerateUniqueEdges(uint[] triangleIndices)
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

            return edges.SelectMany(e => new[] { e.Item1, e.Item2 }).ToArray(); ;
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