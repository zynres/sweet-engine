using System.Numerics;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;

namespace Nova
{
    public struct ObjectData
    {
        public Transform Transform;
        public MeshRenderer Renderer;
    }

    public unsafe struct SceneRendering
    {
        public ShaderSetter Shader { get; set; }
        public Transform CameraTransform { get; set; }

        private List<ObjectData> objectAllDatas { get; set; }
        public ObjectData[] ObjectDatas { get; set; }

        public bool ModeLineRender { get; set; }

        private Glfw glfw;
        private GL gl;

        private bool isBinding { get; set; }

        public void Init(Glfw glfW, GL gL)
        {
            ModeLineRender = ModeLineRender = false;

            glfw = glfW;
            gl = gL;

            Shader = new ShaderSetter(gl, new Shader());

            objectAllDatas = [];

            isBinding = false;
        }

        public void AddObject(string path, Material mat)
        {
            var mesh = MeshLoader.Load(path);
            
            uint _vao = gl.GenVertexArray();
            uint _vbo = gl.GenBuffer();
            uint _ebo = gl.GenBuffer();

            gl.BindVertexArray(_vao);

            // Vertex buffer
            fixed (float* v = &mesh.Vertices[0])
            {
                gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(mesh.Vertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
            }

            // Index buffer
            fixed (uint* i = &mesh.Indices[0])
            {
                gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(mesh.Indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);
            }

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

            objectAllDatas.Add(new ObjectData()
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

            ObjectDatas = objectAllDatas.ToArray();

            if (ObjectDatas.Length == 0)
                return;

            fixed (ObjectData* path = &ObjectDatas[0])
            {
                for (int i = 0; i < ObjectDatas.Length; i++)
                {
                    ObjectData* _object = path + i;

                    _object->Transform.Scale = new Vector3(0.5f, 0.5f, 0.5f);
                    _object->Transform.Position.Y = -5;
                }
            }
        }

        public void Rendering(WindowHandle* window)
        {
            float time = (float)glfw.GetTime();

            Matrix4x4 view = Matrix4x4.CreateLookAt(CameraTransform.Position, CameraTransform.Position + CameraTransform.GetForward(), Vector3.UnitY);
            Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4f, 900f / 800f, 0.1f, 200f);

            Shader.SetVector3("uViewPos", CameraTransform.Position);
            
            fixed (ObjectData* path = &ObjectDatas[0])
            {
                for (int i = 0; i < ObjectDatas.Length; i++)
                {
                    ObjectData* _object = path + i;

                    _object->Transform.Rotation.Y = time / 2;

                    Matrix4x4 model = _object->Transform.LocalToWorldMatrix;

                    Matrix4x4 mvp = model * view * proj;

                    gl.UniformMatrix4(_object->Transform.ModelLoc, 1, false, &model.M11);
                    gl.UniformMatrix4(_object->Transform.MvpLoc, 1, false, &mvp.M11);

                    Shader.SetMatrix4("uMVP", mvp);
                    Shader.SetMatrix4("uModel", model);

                    gl.BindVertexArray(_object->Renderer.vao);

                    if (ModeLineRender)
                    {
                        if (isBinding)
                        {
                            _object->Renderer.material.BaseMap.UnBind(TextureUnit.Texture0);
                            _object->Renderer.material.NormalMap.UnBind(TextureUnit.Texture1);
                            _object->Renderer.material.MetallicMap.UnBind(TextureUnit.Texture2);

                            isBinding = false;
                        }

                        gl.DrawElements(PrimitiveType.Lines, (uint)_object->Renderer.lineIndices.Length, DrawElementsType.UnsignedInt, (void*)0);
                    }
                    else
                    {
                        if (!isBinding)
                        {
                            _object->Renderer.material.BaseMap.Bind(TextureUnit.Texture0);
                            _object->Renderer.material.NormalMap.Bind(TextureUnit.Texture1);
                            _object->Renderer.material.MetallicMap.Bind(TextureUnit.Texture2);

                            isBinding = true;
                        }

                        gl.DrawElements(PrimitiveType.Triangles, (uint)_object->Renderer.mesh.Indices.Length, DrawElementsType.UnsignedInt, (void*)0);
                    }
                }
            }

            gl.BindVertexArray(0);
            glfw.SwapBuffers(window);

            gl.ClearColor(0.02f, 0.02f, 0.03f, 1.0f);
            gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        }
    }
}