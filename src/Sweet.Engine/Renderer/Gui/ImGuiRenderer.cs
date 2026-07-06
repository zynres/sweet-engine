using Sweet.Engine.Renderer.Resources.Texture;
using Sweet.Engine.Renderer.Resources.Shader;
using Sweet.Engine.Scene.Components;
using Sweet.Engine.Editor.Window;
using Sweet.Engine.Enums;
using System.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.GLFW;
using Sweet.Intents;
using ImGuiNET;

namespace Sweet.Engine.Renderer.Gui;

public unsafe struct ImGuiRenderer
{
    public ShaderSetter _ShaderSetter;
    private Texture2D _fontTexture;

    private DockSpace dockSpace;

    private uint _vao;
    private uint _vbo;
    private uint _ebo;

    public ImGuiRenderer(Texture2DLoader textureLoader)
    {
        var guiShader = new GuiShader();

        _ShaderSetter = new ShaderSetter(guiShader.vertexSrc, guiShader.fragmentSrc);

        dockSpace = new DockSpace();

        ImGui.CreateContext();
        ImGui.StyleColorsDark();

        var io = ImGui.GetIO();

        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        SetupFonts(io, textureLoader);
        SetupBuffers();
    }

    public void Update(WindowHandle* window, float deltaTime)
    {
        UpdateIO(window, deltaTime);

        ImGui.NewFrame();

        //dockSpace.Draw();
    }

    public void Render()
    {
        var gl = GraphicStack._GL;

        _ShaderSetter.Use();

        gl.Disable(EnableCap.StencilTest);
        gl.Disable(EnableCap.DepthTest);
        gl.Disable(EnableCap.CullFace);

        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        gl.Enable(EnableCap.ScissorTest);
        gl.Enable(EnableCap.Blend);
        gl.DepthMask(false);

        _ShaderSetter.SetInt("uTexture", 0);

        Matrix4x4 ortho = Matrix4x4.CreateOrthographicOffCenter(
            0, Intent.Width,
            Intent.Height, 0,
            -1, 1
        );

        _ShaderSetter.SetMatrix4("uProj", ortho, false);

        ImGui.Render();

        RenderDrawData(ImGui.GetDrawData());

        gl.Enable(EnableCap.StencilTest);
        gl.Enable(EnableCap.DepthTest);
        gl.Enable(EnableCap.CullFace);

        gl.Disable(EnableCap.ScissorTest);
        gl.Disable(EnableCap.Blend);
        gl.DepthMask(true);
    }

    private void RenderDrawData(ImDrawDataPtr drawData)
    {
        var gl = GraphicStack._GL;

        if (drawData.NativePtr == null)
            return;

        drawData.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

        gl.BindVertexArray(_vao);

        for (int i = 0; i < drawData.CmdListsCount; i++)
        {
            var cmdList = drawData.CmdLists[i];

            gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            gl.BufferData(BufferTargetARB.ArrayBuffer,
                (nuint)(cmdList.VtxBuffer.Size * sizeof(ImDrawVert)),
                (void*)cmdList.VtxBuffer.Data,
                BufferUsageARB.StreamDraw);

            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
            gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                (nuint)(cmdList.IdxBuffer.Size * sizeof(ushort)),
                (void*)cmdList.IdxBuffer.Data,
                BufferUsageARB.StreamDraw);

            for (int cmdI = 0; cmdI < cmdList.CmdBuffer.Size; cmdI++)
            {
                var cmd = cmdList.CmdBuffer[cmdI];

                gl.ActiveTexture(TextureUnit.Texture0);
                gl.BindTexture(TextureTarget.Texture2D, (uint)cmd.TextureId);

                var clip = cmd.ClipRect;

                gl.Scissor(
                    (int)clip.X,
                    (int)(Intent.Height - clip.W),
                    (uint)(clip.Z - clip.X),
                    (uint)(clip.W - clip.Y)
                );

                gl.DrawElementsBaseVertex(
                    PrimitiveType.Triangles,
                    cmd.ElemCount,
                    DrawElementsType.UnsignedShort,
                    (void*)(cmd.IdxOffset * sizeof(ushort)),
                    (int)cmd.VtxOffset
                );
            }
        }

        gl.BindVertexArray(0);
    }

    private void UpdateIO(WindowHandle* window, float deltaTime)
    {
        ImGuiIOPtr io = ImGui.GetIO();

        io.DeltaTime = deltaTime;
        io.DisplaySize = new Vector2(Intent.Width, Intent.Height);
        io.DisplayFramebufferScale = new Vector2(1f, 1f);

        io.AddMousePosEvent(Intent.MousePosition.X, Intent.MousePosition.Y);

        io.AddMouseButtonEvent(0, Intent.GetKeyMouse(MouseButton.Left));
        io.AddMouseButtonEvent(1, Intent.GetKeyMouse(MouseButton.Right));
        io.AddMouseButtonEvent(2, Intent.GetKeyMouse(MouseButton.Middle));

        io.AddKeyEvent(ImGuiKey.Delete, Intent.GetKey(Keys.Delete));
        io.AddKeyEvent(ImGuiKey.Space, Intent.GetKey(Keys.Space));
        io.AddKeyEvent(ImGuiKey.A, Intent.GetKey(Keys.A));
        io.AddKeyEvent(ImGuiKey.W, Intent.GetKey(Keys.W));
        io.AddKeyEvent(ImGuiKey.S, Intent.GetKey(Keys.S));
        io.AddKeyEvent(ImGuiKey.D, Intent.GetKey(Keys.D));

        GraphicStack._Glfw.SetCharCallback(window, (wnd, codepoint) =>
        {
            io.AddInputCharacter(codepoint);
        });
    }

    private void SetupFonts(ImGuiIOPtr io, Texture2DLoader textureLoader)
    {
        io.Fonts.AddFontDefault();
        io.Fonts.Build();

        io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height, out int per_pixel);

        int length = width * height * per_pixel;

        ReadOnlySpan<byte> bytes = new(pixels, length);

        _fontTexture = textureLoader.Load(TextureType.BaseMap, bytes, width, height);

        io.Fonts.SetTexID((IntPtr)_fontTexture.Id);
        io.Fonts.ClearTexData();
    }

    private void SetupBuffers()
    {
        var gl = GraphicStack._GL;

        _vao = gl.GenVertexArray();
        _vbo = gl.GenBuffer();
        _ebo = gl.GenBuffer();

        gl.BindVertexArray(_vao);

        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        gl.BufferData(
            BufferTargetARB.ArrayBuffer, (nuint)(10000 * sizeof(ImDrawVert)),
            null, BufferUsageARB.DynamicDraw);

        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        gl.BufferData(
            BufferTargetARB.ElementArrayBuffer, (nuint)(20000 * sizeof(ushort)),
            null, BufferUsageARB.DynamicDraw);

        int stride = sizeof(ImDrawVert);

        // pos (vec2)
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, (uint)stride, (void*)0);

        // uv (vec2)
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)stride, (void*)8);

        // color (u32)
        gl.EnableVertexAttribArray(2);
        gl.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, (uint)stride, (void*)16);

        gl.BindVertexArray(0);
    }
}
