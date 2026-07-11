// Copyright © 2026 Zynres.
// Licensed under the Apache-2.0 License.

using Sweet.Engine.Renderer.Resources.Texture;
using Sweet.Engine.Renderer.Resources.Shader;
using Sweet.Engine.Renderer.Resources.Mesh;
using Sweet.Engine.Editor.Controllers;
using Sweet.Collections.Unsafe.List;
using Sweet.Engine.Scene.Components;
using Sweet.Engine.Renderer.Gui;
using System.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.GLFW;
using Sweet.Intents;
using Sweet.Devices;
using Sweet.Engine.Editor.Window;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Sweet.Engine.Renderer.Graphic;

public unsafe struct OpenGLRenderer
{
    public ShaderSetter _ShaderSetter;

    public CameraController* _CameraController;
    public UnsafeList<EntityData> EntityDatas;

    public ImGuiRenderer GuiRenderer;

    public Dictionary<string, ObjLoader> MeshLoaders;

    public bool IsLineRender { get; set; }

    private bool isBinding { get; set; }

    public OpenGLRenderer(Texture2DLoader textureLoader)
    {
        IsLineRender = IsLineRender = false;

        GuiRenderer = new ImGuiRenderer(textureLoader);

        var shader = new Resources.Shader.Shader();

        _ShaderSetter = new ShaderSetter(shader.vertexSrc, shader.fragmentSrc);

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
        var gl = GraphicStack.GL;

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

        _ShaderSetter.SetVector4("uColor", mat->Color);

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
        _ShaderSetter.SetVector3("uLightDir", new Vector3(0f, 1f, 1f));
        _ShaderSetter.SetFloat("uLightIntensity", 2f);

        frameBuffer = (FrameBuffer*)NativeMemory.Alloc((nuint)sizeof(FrameBuffer)); 
        *frameBuffer = new FrameBuffer(640, 320);

        SceneWindow.Depends(frameBuffer, _CameraController);

        for (uint i = 0; i < EntityDatas.Length; i++)
        {
            ref EntityData _object = ref EntityDatas[i];

            _object.Transform.Scale = new Vector3(0.25f, 0.25f, 0.25f);

            _object.Direction = new Vector3(
                (float)Random.Shared.NextDouble() * 2 - 1, 
                0, 
                (float)Random.Shared.NextDouble() * 2 - 1);

            //_object.Transform.Position.Y = 10 * i;
        }
    }

    public void Render(WindowHandle* window)
    {
        var gl = GraphicStack.GL;
        var glfw = GraphicStack.Glfw;

        LineRenderMode();

        Device.Update();

        GuiRenderer.Update(window, Device.Time->Delta);

        frameBuffer->Bind();

        gl.ClearColor(0.02f, 0.02f, 0.03f, 1.0f);
        gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        _CameraController->Handle(Device.Time->Delta);

        Matrix4x4 view = Matrix4x4.CreateLookAt(_CameraController->Transform->Position, _CameraController->Transform->Position + _CameraController->Transform->GetForward(), Vector3.UnitY);
        Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4f, _CameraController->Aspect, 0.1f, 1000f);

        _ShaderSetter.Use();
        _ShaderSetter.SetVector3("uViewPos", _CameraController->Transform->Position);

        for (uint i = 0; i < EntityDatas.Length; i++)
        {
            ref EntityData _object = ref EntityDatas[i];

            _object.Transform.Rotation.Y = Device.Time->Current / 2;
            
            _object.Transform.Position += _object.Direction * 2 * Device.Time->Delta;

            Matrix4x4 model = _object.Transform.LocalToWorldMatrix;

            Matrix4x4 mvp = model * view * proj;

            _ShaderSetter.SetMatrix4("uMVP", mvp);
            _ShaderSetter.SetMatrix4("uModel", model);

            gl.BindVertexArray(_object.Renderer.vao);

            if (IsLineRender)
            {
                if (isBinding)
                {
                    _object.Renderer.material->UnBind();

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

                _object.Renderer.material->Bind();

                gl.DrawElements(
                    PrimitiveType.Triangles,
                    _object.Renderer.mesh.Indices.Length,
                    DrawElementsType.UnsignedInt,
                    (void*)0);
            }
        }

        frameBuffer->UnBind();

        GuiRenderer.Render();

        gl.BindVertexArray(0);
        glfw.SwapBuffers(window);
    }

    private void LineRenderMode()
    {
        if (Intent.IsHeld(Keys.Number1))
        {
            if (IsLineRender)
            {
                IsLineRender = false;

                Console.WriteLine($"[Render Mode] => Fill mode");
            }
        }
        else if (Intent.IsHeld(Keys.Number2))
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
        var gl = GraphicStack.GL;

        if (gl == null)
            return;

        for (uint i = 0; i < EntityDatas.Length; i++)
        {
            ref EntityData _object = ref EntityDatas[i];

            gl.DeleteBuffer(_object.Renderer.vbo);
            gl.DeleteVertexArray(_object.Renderer.vao);

            _object.Dispose();
        }

        _CameraController->Dispose();
        frameBuffer->Dispose();

        NativeMemory.Free(_CameraController);
        NativeMemory.Free(frameBuffer);

        EntityDatas.Dispose();
        _ShaderSetter.Dispose();
    }
}