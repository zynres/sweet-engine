using System.Runtime.InteropServices;
using SweetLib.Collections.Unsafe.Array;
using SweetEngine.Library.Resources;

namespace SweetEngine.Scene.Components;

public unsafe struct MeshRenderer
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