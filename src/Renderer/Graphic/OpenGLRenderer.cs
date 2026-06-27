using System.Numerics;
using input;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using unsafe_maps.maps;

namespace Nova;

public unsafe struct OpenGLRenderer : IGraphicRenderer
{
    public ShaderSetter _ShaderSetter;
    public Transform CameraTransform;

    public UnsafeList<ObjectData> ObjectDatas;

    public bool IsLineRender { get; set; }

    private bool isBinding { get; set; }

    public OpenGLRenderer()
    {
        CameraTransform = new()
        {
            Position = new(0, 0, 20),
            Rotation = new(0, 0, 0)
        };

        IsLineRender = IsLineRender = false;

        _ShaderSetter = new ShaderSetter(new Shader());

        ObjectDatas = new UnsafeList<ObjectData>(100);

        isBinding = false;
    }

    public void AddObject(string path, Material mat)
    {
        var gl = GContext._GL;

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

        int modelLocation = gl.GetUniformLocation(_ShaderSetter.Handle, "uModel");
        int mvpLocation = gl.GetUniformLocation(_ShaderSetter.Handle, "uMVP");

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
    private float speed;
    private float sensitivity;

    public void Render(WindowHandle* window)
    {
        var gl = GContext._GL;
        var glfw = GContext._Glfw;

        if (gl == null || glfw == null)
            return;

        LineRenderMode();

        gl.ClearColor(0.02f, 0.02f, 0.03f, 1.0f);
        gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        float currentTime = (float)glfw.GetTime();
        float deltaTime = currentTime - lastTime;
        lastTime = currentTime;

        bool isMouseRight = false;

        if (Input.GetKeyMouse(MouseButton.Right))
        {
            isMouseRight = true;
            Vector3 vector = Vector3.Zero;

            Vector3 forward = CameraTransform.GetForward();
            Vector3 right = CameraTransform.GetRight();
            Vector3 up = CameraTransform.GetUp();

            if (Input.GetKey(Keys.W))
                vector += forward;
            if (Input.GetKey(Keys.S))
                vector -= forward;
            if (Input.GetKey(Keys.A))
                vector -= right;
            if (Input.GetKey(Keys.D))
                vector += right;

            if (Input.GetKey(Keys.Q))
                vector -= up;
            if (Input.GetKey(Keys.E))
                vector += up;

            if (Input.GetKey(Keys.ShiftLeft))
                speed += 0.1f;
            else
                speed = 25f;

            sensitivity = 3;

            CameraTransform.Rotation.Y += Input.MouseDelta.X * sensitivity * deltaTime;
            CameraTransform.Rotation.X += Input.MouseDelta.Y * sensitivity * deltaTime;

            CameraTransform.Rotation.X = Math.Clamp(
                CameraTransform.Rotation.X,
                -MathF.PI / 2 + 0.01f,
                MathF.PI / 2 - 0.01f);

            if (vector != Vector3.Zero)
                vector = Vector3.Normalize(vector);

            CameraTransform.Position += vector * speed * deltaTime;
        }

        Input.Update(isMouseRight);

        Matrix4x4 view = Matrix4x4.CreateLookAt(CameraTransform.Position, CameraTransform.Position + CameraTransform.GetForward(), Vector3.UnitY);
        Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4f, 900f / 800f, 0.1f, 200f);

        _ShaderSetter.SetVector3("uViewPos", CameraTransform.Position);

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

        ObjectDatas.Dispose();
        _ShaderSetter.Dispose();
    }
}