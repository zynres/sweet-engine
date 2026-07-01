using System.Numerics;
using input;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using unsafe_maps.maps;

namespace Nova;

public unsafe struct OpenGLRenderer : IGraphicRenderer
{
    public ShaderSetter _ShaderSetter;

    public CameraController _CameraController;
    public UnsafeList<ObjectData> ObjectDatas;

    public IGuiRenderer GuiRenderer;

    public Dictionary<string, IMeshLoader> MeshLoaders;

    public bool IsLineRender { get; set; }

    private bool isBinding { get; set; }

    public OpenGLRenderer(ITextureLoader<Texture2D> textureLoader)
    {
        IsLineRender = IsLineRender = false;

        GuiRenderer = new ImGuiRenderer(textureLoader);

        var shader = new Shader();

        _ShaderSetter = new ShaderSetter(shader.vertexSrc, shader.fragmentSrc);
        
        _CameraController = new CameraController();
        ObjectDatas = new UnsafeList<ObjectData>(100);

        MeshLoaders = new(1)
        {
            [".obj"] = new ObjLoader()
        };

        isBinding = false;

        //GContext._GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
    }

    public void AddObject(string path, Material mat)
    {
        var gl = GContext._GL;

        var mesh = MeshLoaders[Path.GetExtension(path)].Load(path);

        uint _vao = gl.GenVertexArray();
        uint _vbo = gl.GenBuffer();
        uint _ebo = gl.GenBuffer();

        gl.BindVertexArray(_vao);

        // Vertex buffer
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        gl.BufferData(
            BufferTargetARB.ArrayBuffer, (nuint)(mesh.Vertices.Length * sizeof(float)),
            mesh.Vertices.Data, BufferUsageARB.StaticDraw);


        // Index buffer       
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        gl.BufferData(
            BufferTargetARB.ElementArrayBuffer, (nuint)(mesh.Indices.Length * sizeof(uint)),
            mesh.Indices.Data, BufferUsageARB.StaticDraw);

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

        int modelLocation = gl.GetUniformLocation(_ShaderSetter.Id, "uModel");
        int mvpLocation = gl.GetUniformLocation(_ShaderSetter.Id, "uMVP");

        gl.FrontFace(FrontFaceDirection.Ccw);
        gl.Enable(EnableCap.DepthTest);

        gl.LineWidth(1f);

        _ShaderSetter.Use();

        _ShaderSetter.SetInt("uBaseMap", 0);
        _ShaderSetter.SetInt("uNormalMap", 1);
        _ShaderSetter.SetInt("uMetallicMap", 2);

        _ShaderSetter.SetVector4("uColor", mat.Color);

        var obj = new ObjectData()
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
        };

        ObjectDatas.Add(obj);
    }

    public void InitializeObjects()
    {
        _ShaderSetter.SetVector3("uLightDir", new Vector3(0f, 1f, 1f));
        _ShaderSetter.SetFloat("uLightIntensity", 2f);

        for (int i = 0; i < ObjectDatas.Length; i++)
        {
            ObjectData* _object = ObjectDatas[i];

            if (i == 0)
                _object->Transform.Scale = new Vector3(0.25f, 0.25f, 0.25f);
            else
                _object->Transform.Scale = new Vector3(1f, 1f, 1f);

            _object->Transform.Position.Y = 10 * i;
        }
    }

    private float lastTime;

    public void Render(WindowHandle* window)
    {
        var gl = GContext._GL;
        var glfw = GContext._Glfw;

        if (gl == null || glfw == null)
            return;

        LineRenderMode();

        float currentTime = (float)glfw.GetTime();
        float deltaTime = currentTime - lastTime;
        lastTime = currentTime;

        GuiRenderer.Update(deltaTime);

        gl.ClearColor(0.02f, 0.02f, 0.03f, 1.0f);
        gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        _ShaderSetter.Use();

        _CameraController.Handle(deltaTime);

        Matrix4x4 view = Matrix4x4.CreateLookAt(_CameraController.Transform->Position, _CameraController.Transform->Position + _CameraController.Transform->GetForward(), Vector3.UnitY);
        Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4f, 900f / 800f, 0.1f, 200f);

        _ShaderSetter.SetVector3("uViewPos", _CameraController.Transform->Position);

        for (int i = 0; i < ObjectDatas.Length; i++)
        {
            ObjectData* _object = ObjectDatas[i];

            _object->Transform.Rotation.Y = currentTime / 2;

            Matrix4x4 model = _object->Transform.LocalToWorldMatrix;

            Matrix4x4 mvp = model * view * proj;

            _ShaderSetter.SetMatrix4("uMVP", mvp);
            _ShaderSetter.SetMatrix4("uModel", model);

            gl.BindVertexArray(_object->Renderer.vao);

            if (IsLineRender)
            {
                if (isBinding)
                {
                    _object->Renderer.material.UnBind();

                    isBinding = false;
                }

                gl.DrawElements(
                    PrimitiveType.Lines,
                    (uint)_object->Renderer.lineIndices.Length,
                    DrawElementsType.UnsignedInt,
                    (void*)0);
            }
            else
            {
                if (!isBinding)
                    isBinding = true;

                _object->Renderer.material.Bind();

                gl.DrawElements(
                    PrimitiveType.Triangles,
                    (uint)_object->Renderer.mesh.Indices.Length,
                    DrawElementsType.UnsignedInt,
                    (void*)0);
            }
        }

        GuiRenderer.Render();

        //gl.BindVertexArray(0);
        glfw.SwapBuffers(window);
    }
    
    private void LineRenderMode()
    {
        if (Input.GetKeyDown(Keys.Number1))
        {
            if (IsLineRender)
            {
                IsLineRender = false;

                Console.WriteLine($"[Render Mode] => Fill mode");
            }
        }
        else if (Input.GetKeyDown(Keys.Number2))
        {
            if (!IsLineRender)
            {
                IsLineRender = true;

                Console.WriteLine($"[Render Mode] => LineOnly");
            }
        }
    }

    public void Dispose()
    {
        var gl = GContext._GL;

        if (gl == null)
            return;

        for (int i = 0; i < ObjectDatas.Length; i++)
        {
            ObjectData* _object = ObjectDatas[i];

            gl.DeleteBuffer(_object->Renderer.vbo);
            gl.DeleteVertexArray(_object->Renderer.vao);

            _object->Dispose();
        }

        _CameraController.Dispose();

        ObjectDatas.Dispose();
        _ShaderSetter.Dispose();
    }
}