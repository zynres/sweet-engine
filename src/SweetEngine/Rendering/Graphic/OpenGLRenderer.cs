// Copyright © 2026 Zynres.
// Licensed under the Apache-2.0 License.

using SweetEngine.Library.Resources.Shaders;
using SweetEngine.Ingestion.Loader.Textures;
using SweetEngine.Ingestion.Loader.Meshs;
using System.Runtime.InteropServices;
using SweetEngine.Library.Resources;
using SweetLib.Collections.Unsafe.List;
using SweetEngine.Scene.Components;
using SweetEngine.Rendering.UI;
using SweetEngine.Controllers;
using SweetEngine.Window;
using System.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.GLFW;
using SweetLib.Intents;
using SweetLib.Devices;

namespace SweetEngine.Rendering.Graphic;

public unsafe struct OpenGLRenderer
{
    public ShaderSetter _ShaderSetter;

    public CameraController* _CameraController;
    public UnsafeList<EntityData> EntityDatas;

    public ImGuiRenderer GuiRenderer;

    public Dictionary<string, ObjLoader> MeshLoaders;

    private readonly Device* device;

    private readonly Glfw glfw;
    private readonly GL gl;

    public bool IsLineRender { get; set; }

    private bool isBinding { get; set; }

    public OpenGLRenderer(GL gl, Glfw glfw, Device* device, in Texture2DLoader textureLoader)
    {
        IsLineRender = IsLineRender = false;

        GuiRenderer = new ImGuiRenderer(textureLoader, gl, glfw);
        this.device = device;
        this.glfw = glfw;
        this.gl = gl;

        var shader = new Library.Resources.Shaders.Shader();

        _ShaderSetter = new ShaderSetter(gl, shader.vertexSrc, shader.fragmentSrc);

        _CameraController =(CameraController*)NativeMemory.Alloc((nuint)sizeof(CameraController)); 
        *_CameraController = new CameraController();

        EntityDatas = new UnsafeList<EntityData>(100);

        MeshLoaders = new(1)
        {
            [".obj"] = new ObjLoader()
        };

        isBinding = false;

        //GraphicStack.GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
    }

    public void AddObject(string path, Material* mat)
    {
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

        var obj = new EntityData()
        {
            Transform = new Transform()
            {
                ModelLoc = modelLocation,
                MvpLoc = mvpLocation
            },
            Renderer = new MeshRenderer()
            {
                lineIndices = MeshUtils.GenerateUniqueEdges(in mesh.Indices),
                material = mat,
                mesh = mesh,
                vao = _vao,
                vbo = _vbo,
                ebo = _ebo
            }
        };

        EntityDatas.Add(obj);
    }

    private FrameBuffer* frameBuffer;

    public void InitializeObjects()
    {
        gl.FrontFace(FrontFaceDirection.Ccw);
        gl.Enable(EnableCap.DepthTest);

        gl.LineWidth(1f);

        _ShaderSetter.Use();

        _ShaderSetter.SetInt("uBaseMap", 0);
        _ShaderSetter.SetInt("uNormalMap", 1);
        _ShaderSetter.SetInt("uMetallicMap", 2);

        _ShaderSetter.SetVector3("uLightDir", new Vector3(0f, 1f, 1f));
        _ShaderSetter.SetFloat("uLightIntensity", 2f);

        frameBuffer = (FrameBuffer*)NativeMemory.Alloc((nuint)sizeof(FrameBuffer)); 
        *frameBuffer = new FrameBuffer(gl, 640, 320);

        SceneWindow.Depends(frameBuffer, _CameraController);

        for (uint i = 0; i < EntityDatas.Length; i++)
        {
            ref EntityData _object = ref EntityDatas[i];

            _object.Transform.Scale = new Vector3(0.25f, 0.25f, 0.25f);

            _object.Direction = new Vector3(
                Random.Shared.NextSingle() * 2 - 1, 
                0, 
                Random.Shared.NextSingle() * 2 - 1);

            //_object.Transform.Position.Y = 10 * i;
        }
    }

    public void Render(WindowHandle* window, in Intent intent)
    {
        LineRenderMode(intent);

        device->Update(glfw, intent);

        GuiRenderer.Update(window, device->Time.Delta);

        frameBuffer->Bind(gl);

        gl.ClearColor(0.02f, 0.02f, 0.03f, 1.0f);
        gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        _CameraController->Handle(device->Time.Delta);

        Matrix4x4 view = Matrix4x4.CreateLookAt(_CameraController->Transform.Position, _CameraController->Transform.Position + _CameraController->Transform.GetForward(), Vector3.UnitY);
        Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4f, _CameraController->Aspect, 0.1f, 1000f);

        _ShaderSetter.Use();

        _ShaderSetter.SetVector3("uViewPos", _CameraController->Transform.Position);

        for (uint i = 0; i < EntityDatas.Length; i++)
        {
            ref EntityData _object = ref EntityDatas[i];
            
            _ShaderSetter.SetVector4("uColor", _object.Renderer.material->Color);

            _object.Transform.Rotation.Y = device->Time.Current / 2;
            
            _object.Transform.Position += _object.Direction * 2 * device->Time.Delta;

            Matrix4x4 model = _object.Transform.LocalToWorldMatrix;

            Matrix4x4 mvp = model * view * proj;

            _ShaderSetter.SetMatrix4("uMVP", mvp);
            _ShaderSetter.SetMatrix4("uModel", model);

            gl.BindVertexArray(_object.Renderer.vao);

            if (IsLineRender)
            {
                if (isBinding)
                {
                    _object.Renderer.material->UnBind(gl);

                    isBinding = false;
                }

                gl.DrawElements(
                    PrimitiveType.Lines,
                    _object.Renderer.lineIndices.Length,
                    DrawElementsType.UnsignedInt,
                    (void*)0);
            }
            else
            {
                if (!isBinding)
                    isBinding = true;

                _object.Renderer.material->Bind(gl);

                gl.DrawElements(
                    PrimitiveType.Triangles,
                    _object.Renderer.mesh.Indices.Length,
                    DrawElementsType.UnsignedInt,
                    (void*)0);
            }
        }

        frameBuffer->UnBind(gl, in device->Window);

        GuiRenderer.Render();

        gl.BindVertexArray(0);
        glfw.SwapBuffers(window);
    }

    private void LineRenderMode(Intent intent)
    {
        if (intent.IsHeld(Keys.Number1))
        {
            if (IsLineRender)
            {
                IsLineRender = false;

                Console.WriteLine($"[Render Mode] => Fill mode");
            }
        }
        else if (intent.IsHeld(Keys.Number2))
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
        if (gl == null)
            return;

        GuiRenderer.Dispose();

        for (uint i = 0; i < EntityDatas.Length; i++)
        {
            ref EntityData _object = ref EntityDatas[i];

            gl.DeleteBuffer(_object.Renderer.vbo);
            gl.DeleteVertexArray(_object.Renderer.vao);

            _object.Dispose();
        }

        _CameraController->Dispose();
        frameBuffer->Dispose(gl);

        NativeMemory.Free(_CameraController);
        NativeMemory.Free(frameBuffer);

        EntityDatas.Dispose();
        _ShaderSetter.Dispose();
    }
}