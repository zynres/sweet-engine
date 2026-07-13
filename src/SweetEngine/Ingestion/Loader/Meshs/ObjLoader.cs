// Copyright © 2026 Zynres.
// Licensed under the Apache-2.0 License.

using SweetLib.Collections.Unsafe.Dictionary;
using SweetLib.Collections.Unsafe.Array;
using SweetLib.Collections.Unsafe.List;
using SweetEngine.Library.Resources;
using System.Globalization;
using System.Numerics;

namespace SweetEngine.Ingestion.Loader.Meshs;

public unsafe struct ObjLoader
{
    public Mesh Load(string path)
    {
        /*Stopwatch stopwatch = new();
        stopwatch.Start();*/

        UnsafeList<Vector3> positions = new(128);
        UnsafeList<Vector3> normals = new(128);
        UnsafeList<Vector2> uvs = new(128);

        UnsafeList<float> finalVerts = new(128);
        UnsafeList<uint> indices = new(128);
        UnsafeDictionary<VertexKey, uint> vertexMap = new(128);

        UnsafeList<Vector3> vertexPositions = new(128);
        UnsafeList<Vector2> vertexUVs = new(128);
        UnsafeList<Vector3> tan1 = new(128);

        foreach (var line in File.ReadLines(path))
        {
            if (line.StartsWith("v "))
            {
                var p = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                positions.Add(new Vector3(
                    float.Parse(p[1], CultureInfo.InvariantCulture),
                    float.Parse(p[2], CultureInfo.InvariantCulture),
                    float.Parse(p[3], CultureInfo.InvariantCulture)
                ));
            }
            else if (line.StartsWith("vt "))
            {
                var p = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                uvs.Add(new Vector2(
                    float.Parse(p[1], CultureInfo.InvariantCulture),
                    float.Parse(p[2], CultureInfo.InvariantCulture)
                ));
            }
            else if (line.StartsWith("vn "))
            {
                var p = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                normals.Add(new Vector3(
                    float.Parse(p[1], CultureInfo.InvariantCulture),
                    float.Parse(p[2], CultureInfo.InvariantCulture),
                    float.Parse(p[3], CultureInfo.InvariantCulture)
                ));
            }
            else if (line.StartsWith("f "))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).AsSpan(1);

                if (parts.Length < 3) continue;

                var faceIndices = new UnsafeArray<uint>((uint)parts.Length);

                for (uint j = 0; j < parts.Length; j++)
                {
                    var idx = parts[(int)j].Split('/');

                    int vIndex = int.Parse(idx[0]) - 1;
                    int tIndex = (idx.Length > 1 && idx[1] != "") ? int.Parse(idx[1]) - 1 : -1;
                    int nIndex = (idx.Length > 2 && idx[2] != "") ? int.Parse(idx[2]) - 1 : -1;

                    var key = new VertexKey(vIndex, tIndex, nIndex);

                    if (!vertexMap.TryGetValue(key, out uint index))
                    {
                        var pos = positions[(uint)vIndex];
                        var uv = (tIndex >= 0 && tIndex < uvs.Length) ? uvs[(uint)tIndex] : new Vector2();
                        var norm = (nIndex >= 0 && nIndex < normals.Length) ? normals[(uint)nIndex] : new Vector3();

                        var values = new UnsafeArray<float>(11);

                        values.Data[0] = pos.X;
                        values.Data[1] = pos.Y;
                        values.Data[2] = pos.Z;

                        values.Data[3] = norm.X;
                        values.Data[4] = norm.Y;
                        values.Data[5] = norm.Z;

                        values.Data[6] = uv.X;
                        values.Data[7] = uv.Y;

                        values.Data[8] = 0; // tangents
                        values.Data[9] = 0;
                        values.Data[10] = 0;

                        finalVerts.AddRange(values);

                        index = finalVerts.Length / 11 - 1;
                        vertexMap[key] = index;

                        vertexPositions.Add(pos);
                        vertexUVs.Add(uv);
                        tan1.Add(Vector3.Zero);
                        
                        values.Dispose();
                    }

                    faceIndices.Set(j, index);
                }

                for (uint j = 1; j < faceIndices.Length - 1; j++)
                {
                    indices.Add(faceIndices[0]);
                    indices.Add(faceIndices[j]);
                    indices.Add(faceIndices[j + 1]);

                    uint i0 = faceIndices[0];
                    uint i1 = faceIndices[j];
                    uint i2 = faceIndices[j + 1];

                    Vector3 v0 = vertexPositions[i0];
                    Vector3 v1 = vertexPositions[i1];
                    Vector3 v2 = vertexPositions[i2];

                    Vector2 uv0 = vertexUVs[i0];
                    Vector2 uv1 = vertexUVs[i1];
                    Vector2 uv2 = vertexUVs[i2];

                    var deltaPos1 = v1 - v0;
                    var deltaPos2 = v2 - v0;
                    var deltaUV1 = uv1 - uv0;
                    var deltaUV2 = uv2 - uv0;

                    float r = deltaUV1.X * deltaUV2.Y - deltaUV1.Y * deltaUV2.X;

                    if (Math.Abs(r) < 1e-6f)
                        continue;

                    r = 1.0f / r;

                    Vector3 tangent = (deltaPos1 * deltaUV2.Y - deltaPos2 * deltaUV1.Y) * r;

                    tan1[i0] += tangent;
                    tan1[i1] += tangent;
                    tan1[i2] += tangent;
                }

                faceIndices.Dispose();
            }
        }

        for (uint i = 0; i < tan1.Length; i++)
        {
            Vector3 t = Vector3.Normalize(tan1[i]);

            uint baseIndex = i * 11 + 8;

            finalVerts[baseIndex + 0] = t.X;
            finalVerts[baseIndex + 1] = t.Y;
            finalVerts[baseIndex + 2] = t.Z;
        }

        var mesh = new Mesh
        {
            Vertices = new UnsafeArray<float>(finalVerts.Length),
            Indices = new UnsafeArray<uint>(indices.Length)
        };

        finalVerts.CopyTo(&mesh.Vertices);
        indices.CopyTo(&mesh.Indices);

        positions.Dispose();
        normals.Dispose();
        uvs.Dispose();

        finalVerts.Dispose();
        indices.Dispose();

        vertexMap.Dispose();

        vertexPositions.Dispose();
        vertexUVs.Dispose();
        tan1.Dispose();

        /*Console.WriteLine($"Vertices: {mesh.Vertices.Length}");
        Console.WriteLine($"Indices: {mesh.Indices.Length}");

        stopwatch.Stop();

        Console.WriteLine($"process ms: {stopwatch.ElapsedMilliseconds}");*/

        return mesh;
    }
}