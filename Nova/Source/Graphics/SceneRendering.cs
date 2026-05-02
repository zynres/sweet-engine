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
            Renderer.Dispose();
        }
    }

    public unsafe struct SceneRendering : IDisposable
    {
        public ShaderSetter ShaderSetter { get; set; }
        public Transform CameraTransform { get; set; }

        public UnsafeList<ObjectData> ObjectDatas;

        public bool ModeLineRender { get; set; }

        private bool isBinding { get; set; }

        public void Init()
        {
            ModeLineRender = ModeLineRender = false;

            ShaderSetter = new ShaderSetter();
            ShaderSetter.SetShader(new Shader());

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

            int modelLocation = gl.GetUniformLocation(ShaderSetter.Handle, "uModel");
            int mvpLocation = gl.GetUniformLocation(ShaderSetter.Handle, "uMVP");

            gl.FrontFace(FrontFaceDirection.Ccw);
            gl.Enable(EnableCap.DepthTest);

            gl.LineWidth(1f);

            ShaderSetter.Use();

            ShaderSetter.SetInt("uBaseMap", 0);
            ShaderSetter.SetInt("uNormalMap", 1);
            ShaderSetter.SetInt("uMetallicMap", 2);

            ShaderSetter.SetVector4("uColor", mat.Color);

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

            ShaderSetter.SetVector3("uLightDir", new Vector3(0f, 1f, 1f));
            ShaderSetter.SetFloat("uLightIntensity", 2f);

            for (int i = 0; i < ObjectDatas.Length; i++)
            {
                ObjectData* _object = ObjectDatas[i]; 

                _object->Transform.Scale = new Vector3(0.5f, 0.5f, 0.5f);
                _object->Transform.Position.Y = -5;
            }
        }

        public void Rendering(WindowHandle* window)
        {
            Glfw glfw = GContext._Glfw;
            GL gl = GContext._GL;

            if (gl == null || glfw == null)
                return;

            gl.ClearColor(0.02f, 0.02f, 0.03f, 1.0f);
            gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

            float time = (float)glfw.GetTime();

            Matrix4x4 view = Matrix4x4.CreateLookAt(CameraTransform.Position, CameraTransform.Position + CameraTransform.GetForward(), Vector3.UnitY);
            Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4f, 900f / 800f, 0.1f, 200f);

            ShaderSetter.SetVector3("uViewPos", CameraTransform.Position);

            for (int i = 0; i < ObjectDatas.Length; i++)
            {
                ObjectData* _object = ObjectDatas[i];

                _object->Transform.Rotation.Y = time / 2;

                Matrix4x4 model = _object->Transform.LocalToWorldMatrix;

                Matrix4x4 mvp = model * view * proj;

                ShaderSetter.SetMatrix4("uMVP", mvp);
                ShaderSetter.SetMatrix4("uModel", model);

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

            gl.BindVertexArray(0);
            glfw.SwapBuffers(window);
        }

        public void Dispose()
        {
            for (int i = 0; i < ObjectDatas.Length; i++)
            {
                ObjectData* objectData = ObjectDatas[i];

                GContext._GL.DeleteBuffer(objectData->Renderer.vbo);
                GContext._GL.DeleteVertexArray(objectData->Renderer.vao);

                objectData->Dispose();
            }

            ObjectDatas.Dispose();
            ShaderSetter.Dispose();
        }
    }
}