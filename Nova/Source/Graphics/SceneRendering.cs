using System.Numerics;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using unsafe_maps.maps;

namespace Nova
{
    public struct ObjectData : IDisposable
    {
        public Transform Transform;
        public MeshRenderer Renderer;

        public void Dispose()
        {
            Renderer.lineIndices.Dispose();
        }
    }

    public unsafe struct SceneRendering : IDisposable
    {
        public ShaderSetter Shader { get; set; }
        public Transform CameraTransform { get; set; }

        public UnsafeList<ObjectData> ObjectDatas;

        public bool ModeLineRender { get; set; }

        private bool isBinding { get; set; }

        public void Init()
        {
            ModeLineRender = ModeLineRender = false;

            Shader = new ShaderSetter(GContext._GL, new Shader());

            ObjectDatas = new UnsafeList<ObjectData>(100);

            isBinding = false;
        }

        public void AddObject(string path, Material mat)
        {
            GL gl = GContext._GL;

            if (gl == null)
                return;

            var mesh = MeshLoader.Load(path);
            
            uint _vao = gl.GenVertexArray();
            uint _vbo = gl.GenBuffer();
            uint _ebo = gl.GenBuffer();

            gl.BindVertexArray(_vao);

            // Vertex buffer
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(mesh.Vertices.Length * sizeof(float)), mesh.Vertices.Data, BufferUsageARB.StaticDraw);
            

            // Index buffer
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(mesh.Indices.Length * sizeof(uint)), mesh.Indices.Data, BufferUsageARB.StaticDraw);
            

            uint stride = 11 * sizeof(float);

            // position
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, (void*)0);
            gl.EnableVertexAttribArray(0);

            // normal
            gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float))); 
            gl.EnableVertexAttribArray(1);

            // uv
            gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, (void*)(6 * sizeof(float))); 
            gl.EnableVertexAttribArray(2);

            // tangent
            gl.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, stride, (void*)(8 * sizeof(float))); 
            gl.EnableVertexAttribArray(3);

            gl.BindVertexArray(0);

            gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

            int modelLocation = gl.GetUniformLocation(Shader.Handle, "uModel");
            int mvpLocation = gl.GetUniformLocation(Shader.Handle, "uMVP");

            gl.FrontFace(FrontFaceDirection.Ccw);
            gl.Enable(EnableCap.DepthTest);

            gl.LineWidth(1f);

            Shader.Use();

            Shader.SetInt("uBaseMap", 0);
            Shader.SetInt("uNormalMap", 1);
            Shader.SetInt("uMetallicMap", 2);

            Shader.SetVector4("uColor", mat.Color);

            ObjectDatas.Add(new ObjectData()
            {
                Transform = new Transform()
                {
                    ModelLoc = modelLocation,
                    MvpLoc = mvpLocation
                },
                Renderer = new MeshRenderer()
                {
                    lineIndices = MeshUtils.GenerateUniqueEdges(mesh.Indices),
                    material = mat,
                    mesh = mesh,
                    vao = _vao,
                    vbo = _vbo,
                    ebo = _ebo
                }
            });
        }

        public void InitializeObject()
        {
            CameraTransform = new() { Position = new(0, 0, 35f) };

            Shader.SetVector3("uLightDir", new Vector3(0f, 1f, 1f));
            Shader.SetFloat("uLightIntensity", 2f);

            if (ObjectDatas.Length == 0)
                return;

            for (int i = 0; i < ObjectDatas.Length; i++)
            {
                ObjectData _object = ObjectDatas[i];

                _object.Transform.Scale = new Vector3(0.5f, 0.5f, 0.5f);
                _object.Transform.Position.Y = -5;
            }
        }

        public void Rendering(WindowHandle* window)
        {
            Glfw glfw = GContext._Glfw;
            GL gl = GContext._GL;

            if (gl == null || glfw == null)
                return;

            float time = (float)glfw.GetTime();

            Matrix4x4 view = Matrix4x4.CreateLookAt(CameraTransform.Position, CameraTransform.Position + CameraTransform.GetForward(), Vector3.UnitY);
            Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4f, 900f / 800f, 0.1f, 200f);

            Shader.SetVector3("uViewPos", CameraTransform.Position);

            for (int i = 0; i < ObjectDatas.Length; i++)
            {
                ObjectData _object = ObjectDatas[i];

                _object.Transform.Rotation.Y = time / 2;

                Matrix4x4 model = _object.Transform.LocalToWorldMatrix;

                Matrix4x4 mvp = model * view * proj;

                gl.UniformMatrix4(_object.Transform.ModelLoc, 1, false, &model.M11);
                gl.UniformMatrix4(_object.Transform.MvpLoc, 1, false, &mvp.M11);

                Shader.SetMatrix4("uMVP", mvp);
                Shader.SetMatrix4("uModel", model);

                gl.BindVertexArray(_object.Renderer.vao);

                if (ModeLineRender)
                {
                    if (isBinding)
                    {
                        _object.Renderer.material.BaseMap.UnBind(TextureUnit.Texture0);
                        _object.Renderer.material.NormalMap.UnBind(TextureUnit.Texture1);
                        _object.Renderer.material.MetallicMap.UnBind(TextureUnit.Texture2);

                        isBinding = false;
                    }

                    gl.DrawElements(PrimitiveType.Lines, (uint)_object.Renderer.lineIndices.Length, DrawElementsType.UnsignedInt, (void*)0);
                }
                else
                {
                    if (!isBinding)
                    {
                        _object.Renderer.material.BaseMap.Bind(TextureUnit.Texture0);
                        _object.Renderer.material.NormalMap.Bind(TextureUnit.Texture1);
                        _object.Renderer.material.MetallicMap.Bind(TextureUnit.Texture2);

                        isBinding = true;
                    }

                    gl.DrawElements(PrimitiveType.Triangles, (uint)_object.Renderer.mesh.Indices.Length, DrawElementsType.UnsignedInt, (void*)0);
                }
            }

            gl.BindVertexArray(0);
            glfw.SwapBuffers(window);

            gl.ClearColor(0.02f, 0.02f, 0.03f, 1.0f);
            gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        }

        public void Dispose()
        {
            for (int i = 0; i < ObjectDatas.Length; i++)
            {
                ObjectData objectData = ObjectDatas[i];

                objectData.Dispose();
            }

            ObjectDatas.Dispose();
            Shader.Dispose();
        }
    }
}