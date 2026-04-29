using System.Globalization;
using System.Numerics;

namespace Nova
{
    public static class MeshLoader
    {
        public static Mesh Load(string path)
        {
            var lines = File.ReadAllLines(path);

            List<Vector3> positions = new();
            List<Vector3> normals = new();
            List<Vector2> uvs = new();

            List<float> finalVerts = new();
            List<uint> indices = new();
            Dictionary<VertexKey, uint> vertexMap = new();

            List<Vector3> vertexPositions = new();
            List<Vector2> vertexUVs = new();
            List<Vector3> tan1 = new();

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

                    uint[] faceIndices = new uint[parts.Length];

                    for (int j = 0; j < parts.Length; j++)
                    {
                        var idx = parts[j].Split('/');

                        int vIndex = int.Parse(idx[0]) - 1;
                        int tIndex = (idx.Length > 1 && idx[1] != "") ? int.Parse(idx[1]) - 1 : -1;
                        int nIndex = (idx.Length > 2 && idx[2] != "") ? int.Parse(idx[2]) - 1 : -1;

                        var key = new VertexKey(vIndex, tIndex, nIndex);

                        if (!vertexMap.TryGetValue(key, out uint index))
                        {
                            var pos = positions[vIndex];
                            var uv = (tIndex >= 0 && tIndex < uvs.Count) ? uvs[tIndex] : new Vector2();
                            var norm = (nIndex >= 0 && nIndex < normals.Count) ? normals[nIndex] : new Vector3();

                            finalVerts.AddRange(new float[]
                            {
                                pos.X, pos.Y, pos.Z,
                                norm.X, norm.Y, norm.Z,
                                uv.X, uv.Y,
                                0, 0, 0 // tangent
                            });

                            index = (uint)(finalVerts.Count / 11 - 1);
                            vertexMap[key] = index;

                            vertexPositions.Add(pos);
                            vertexUVs.Add(uv);
                            tan1.Add(Vector3.Zero);
                        }

                        faceIndices[j] = index;
                    }

                    for (int j = 1; j < faceIndices.Length - 1; j++)
                    {
                        indices.Add(faceIndices[0]);
                        indices.Add(faceIndices[j]);
                        indices.Add(faceIndices[j + 1]);

                        uint i0 = faceIndices[0];
                        uint i1 = faceIndices[j];
                        uint i2 = faceIndices[j + 1];

                        var v0 = vertexPositions[(int)i0];
                        var v1 = vertexPositions[(int)i1];
                        var v2 = vertexPositions[(int)i2];

                        var uv0 = vertexUVs[(int)i0];
                        var uv1 = vertexUVs[(int)i1];
                        var uv2 = vertexUVs[(int)i2];

                        var deltaPos1 = v1 - v0;
                        var deltaPos2 = v2 - v0;
                        var deltaUV1 = uv1 - uv0;
                        var deltaUV2 = uv2 - uv0;

                        float r = (deltaUV1.X * deltaUV2.Y - deltaUV1.Y * deltaUV2.X);
                        if (Math.Abs(r) < 1e-6f) continue;
                        r = 1.0f / r;


                        Vector3 tangent = (deltaPos1 * deltaUV2.Y - deltaPos2 * deltaUV1.Y) * r;

                        tan1[(int)i0] += tangent;
                        tan1[(int)i1] += tangent;
                        tan1[(int)i2] += tangent;
                    }
                }
            }

            for (int i = 0; i < tan1.Count; i++)
            {
                Vector3 t = Vector3.Normalize(tan1[i]);
                int baseIndex = i * 11 + 8;
                finalVerts[baseIndex + 0] = t.X;
                finalVerts[baseIndex + 1] = t.Y;
                finalVerts[baseIndex + 2] = t.Z;
            }

            return new Mesh
            {
                Vertices = finalVerts.ToArray(),
                Indices = indices.ToArray()
            };
        }

        private static void ComputeTangents(Mesh mesh)
        {
            var vertices = mesh.Vertices;
            var indices = mesh.Indices;

            int vertexCount = vertices.Length / 8;
            Vector3[] pos = new Vector3[vertexCount];
            Vector3[] norm = new Vector3[vertexCount];
            Vector2[] uv = new Vector2[vertexCount];
            Vector3[] tan = new Vector3[vertexCount];

            for (int i = 0; i < vertexCount; i++)
            {
                int b = i * 8;
                pos[i] = new Vector3(vertices[b + 0], vertices[b + 1], vertices[b + 2]);
                norm[i] = new Vector3(vertices[b + 3], vertices[b + 4], vertices[b + 5]);
                uv[i] = new Vector2(vertices[b + 6], vertices[b + 7]);
            }

            for (int i = 0; i < indices.Length; i += 3)
            {
                int i0 = (int)indices[i + 0];
                int i1 = (int)indices[i + 1];
                int i2 = (int)indices[i + 2];

                var p0 = pos[i0];
                var p1 = pos[i1];
                var p2 = pos[i2];
                var uv0 = uv[i0];
                var uv1 = uv[i1];
                var uv2 = uv[i2];

                var dp1 = p1 - p0;
                var dp2 = p2 - p0;
                var duv1 = uv1 - uv0;
                var duv2 = uv2 - uv0;

                float r = 1.0f / (duv1.X * duv2.Y - duv1.Y * duv2.X);
                var tangent = (dp1 * duv2.Y - dp2 * duv1.Y) * r;

                tan[i0] += tangent;
                tan[i1] += tangent;
                tan[i2] += tangent;
            }

            for (int i = 0; i < vertexCount; i++)
                tan[i] = Vector3.Normalize(tan[i]);

            List<float> newVerts = new(vertexCount * 11);
            for (int i = 0; i < vertexCount; i++)
            {
                int b = i * 8;
                
                newVerts.AddRange(
                [
                    vertices[b + 0], vertices[b + 1], vertices[b + 2], // pos
                    vertices[b + 3], vertices[b + 4], vertices[b + 5], // normal
                    vertices[b + 6], vertices[b + 7],                   // uv
                    tan[i].X, tan[i].Y, tan[i].Z                        // tangent
                ]);
            }

            mesh.Vertices = newVerts.ToArray();
        }
    }
}