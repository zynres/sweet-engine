namespace Nova
{
    public struct MeshRenderer
    {
        public Mesh mesh;
        public Material material;
        public uint vao, vbo, ebo;
        public uint[] lineIndices;
    }
}