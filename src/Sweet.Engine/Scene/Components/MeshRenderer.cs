using System.Runtime.InteropServices;
using Sweet.Collections.Unsafe.Array;

namespace Sweet.Engine.Scene.Components;

public unsafe struct MeshRenderer : IDisposable
{
    public Mesh mesh;
    public Material* material;
    public uint vao, vbo, ebo;
    public UnsafeArray<uint> lineIndices;

    public void Dispose()
    {
        lineIndices.Dispose();

        if (!material->IsFree)
        {
            material->IsFree = true;
            
            material->Dispose();
            NativeMemory.Free(material);
        }

        mesh.Dispose();
    }
}