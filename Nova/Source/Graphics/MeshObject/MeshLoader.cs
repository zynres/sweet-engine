using System.Globalization;
using System.Numerics;
using unsafe_maps.maps;

namespace Nova
{
    public unsafe struct MeshLoader
    {
        public static Mesh Load(string path)
        {
            var lines = File.ReadAllLines(path);

            using UnsafeList<Vector3> positions = new(50);
            using UnsafeList<Vector3> normals = new(50);
            using UnsafeList<Vector2> uvs = new(50);

            using UnsafeList<float> finalVerts = new(50);
            using UnsafeList<uint> indices = new(50);
            Dictionary<VertexKey, uint> vertexMap = new();

            using UnsafeList<Vector3> vertexPositions = new(50);
            using UnsafeList<Vector2> vertexUVs = new(50);
            using UnsafeList<Vector3> tan1 = new(50);

            foreach (var line in lines)
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

                    using var faceIndices = new UnsafeArray<uint>(parts.Length);

                    for (int j = 0; j < parts.Length; j++)
                    {
                        var idx = parts[j].Split('/');

                        int vIndex = int.Parse(idx[0]) - 1;
                        int tIndex = (idx.Length > 1 && idx[1] != "") ? int.Parse(idx[1]) - 1 : -1;
                        int nIndex = (idx.Length > 2 && idx[2] != "") ? int.Parse(idx[2]) - 1 : -1;

                        var key = new VertexKey(vIndex, tIndex, nIndex);

                        if (!vertexMap.TryGetValue(key, out uint index))
                        {
                            var pos = *positions[vIndex];
                            var uv = (tIndex >= 0 && tIndex < uvs.Length) ? *uvs[tIndex] : new Vector2();
                            var norm = (nIndex >= 0 && nIndex < normals.Length) ? *normals[nIndex] : new Vector3();

                            using var values = new UnsafeArray<float>(11);

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

                            values.SetLength(11);

                            finalVerts.AddRange(values);

                            index = (uint)(finalVerts.Length / 11 - 1);
                            vertexMap[key] = index;

                            vertexPositions.Add(pos);
                            vertexUVs.Add(uv);
                            tan1.Add(Vector3.Zero);
                        }

                        faceIndices.Set(j, index);
                    }

                    for (int j = 1; j < faceIndices.Length - 1; j++)
                    {
                        indices.Add(*faceIndices[0]);
                        indices.Add(*faceIndices[j]);
                        indices.Add(*faceIndices[j + 1]);

                        uint i0 = *faceIndices[0];
                        uint i1 = *faceIndices[j];
                        uint i2 = *faceIndices[j + 1];

                        Vector3* v0 = vertexPositions[(int)i0];
                        Vector3* v1 = vertexPositions[(int)i1];
                        Vector3* v2 = vertexPositions[(int)i2];

                        Vector2* uv0 = vertexUVs[(int)i0];
                        Vector2* uv1 = vertexUVs[(int)i1];
                        Vector2* uv2 = vertexUVs[(int)i2];

                        var deltaPos1 = *v1 - *v0;
                        var deltaPos2 = *v2 - *v0;
                        var deltaUV1 = *uv1 - *uv0;
                        var deltaUV2 = *uv2 - *uv0;

                        float r = deltaUV1.X * deltaUV2.Y - deltaUV1.Y * deltaUV2.X;

                        if (Math.Abs(r) < 1e-6f)
                            continue;

                        r = 1.0f / r;

                        Vector3 tangent = (deltaPos1 * deltaUV2.Y - deltaPos2 * deltaUV1.Y) * r;

                        *tan1[(int)i0] += tangent;
                        *tan1[(int)i1] += tangent;
                        *tan1[(int)i2] += tangent;
                    }
                }
            }

            for (int i = 0; i < tan1.Length; i++)
            {
                Vector3 t = Vector3.Normalize(*tan1[i]);

                int baseIndex = i * 11 + 8;

                *finalVerts[baseIndex + 0] = t.X;
                *finalVerts[baseIndex + 1] = t.Y;
                *finalVerts[baseIndex + 2] = t.Z;
            }

            var mesh = new Mesh
            {
                Vertices = new UnsafeArray<float>(finalVerts.Length),
                Indices = new UnsafeArray<uint>(indices.Length)
            };

            finalVerts.CopyTo(&mesh.Vertices);
            indices.CopyTo(&mesh.Indices);

            Console.WriteLine($"Vertices: {mesh.Vertices.Length}");
            Console.WriteLine($"Indices: {mesh.Indices.Length}");

            return mesh;
        }
    }
}