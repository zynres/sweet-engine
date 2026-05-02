using unsafe_maps.maps;

namespace Nova
{
    public struct MeshRenderer : IDisposable
    {
        public Mesh mesh;
        public Material material;
        public uint vao, vbo, ebo;
        public UnsafeArray<uint> lineIndices;

        public void Dispose()
        {
            lineIndices.Dispose();
            mesh.Dispose();
        }
    }
}