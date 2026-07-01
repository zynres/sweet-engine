using System.Numerics;
using Silk.NET.OpenGL;
using ImGuiNET;
using input;
using Silk.NET.GLFW;

namespace Nova;

public unsafe struct ImGuiRenderer : IGuiRenderer
{
    public ShaderSetter _ShaderSetter;
    private Texture2D _fontTexture;

    private uint _vao;
    private uint _vbo;
    private uint _ebo;

    private float f = 0.0f;
    private Vector3 clear_color = new Vector3(114f / 255f, 144f / 255f, 154f / 255f);
    private byte[] _textBuffer = new byte[100];

    public ImGuiRenderer(ITextureLoader<Texture2D> textureLoader)
    {
        var guiShader = new GuiShader();

        _ShaderSetter = new ShaderSetter(guiShader.vertexSrc, guiShader.fragmentSrc);

        ImGui.CreateContext();
        ImGui.StyleColorsDark();

        var io = ImGui.GetIO();

        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

        SetupFonts(io, textureLoader);
        SetupBuffers();
    }

    public void Update(float deltaTime)
    {
        var gl = GContext._GL;

        if (gl == null)
            return;

        ImGuiIOPtr io = ImGui.GetIO();

        io.DeltaTime = deltaTime;
        io.DisplaySize = new Vector2(Input.Width, Input.Height);
        io.DisplayFramebufferScale = new Vector2(1f, 1f);

        io.AddMousePosEvent(Input.MousePosition.X, Input.MousePosition.Y);

        io.AddMouseButtonEvent(0, Input.GetKeyMouse(MouseButton.Left));
        io.AddMouseButtonEvent(1, Input.GetKeyMouse(MouseButton.Right));
        io.AddMouseButtonEvent(2, Input.GetKeyMouse(MouseButton.Middle));

        io.AddKeyEvent(ImGuiKey.Space, Input.GetKey(Keys.Space));
        io.AddKeyEvent(ImGuiKey.A, Input.GetKey(Keys.A));
        io.AddKeyEvent(ImGuiKey.W, Input.GetKey(Keys.W));
        io.AddKeyEvent(ImGuiKey.S, Input.GetKey(Keys.S));
        io.AddKeyEvent(ImGuiKey.D, Input.GetKey(Keys.D));

        ImGui.NewFrame();

        ImGui.Begin("Debug");

        ImGui.SliderFloat("float", ref f, 0.0f, 1.0f, string.Empty);
        ImGui.ColorEdit3("clear color", ref clear_color);
        ImGui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)", 1000f / ImGui.GetIO().Framerate, ImGui.GetIO().Framerate));

        ImGui.InputText("Text input", _textBuffer, 100);

        ImGui.Text("Texture sample");
        ImGui.Image((nint)_fontTexture.Id, new Vector2(300, 150), Vector2.Zero, Vector2.One, Vector4.One, Vector4.One);
        ImGui.End();
    }

    public void Render()
    {
        var gl = GContext._GL;

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
            0, Input.Width,
            Input.Height, 0,
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
        var gl = GContext._GL;

        if (drawData.NativePtr == null)
            return;

        gl.BindVertexArray(_vao);

        int vtxOffset = 0;
        int idxOffset = 0;

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

                drawData.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

                gl.Scissor(
                    (int)clip.X,
                    (int)(Input.Height - clip.W),
                    (uint)(clip.Z - clip.X),
                    (uint)(clip.W - clip.Y)
                );

                gl.DrawElementsBaseVertex(
                    PrimitiveType.Triangles,
                    cmd.ElemCount,
                    DrawElementsType.UnsignedShort,
                    (void*)((cmd.IdxOffset + idxOffset) * sizeof(ushort)),
                    (int)(cmd.VtxOffset + vtxOffset)
                );
            }

            vtxOffset += cmdList.VtxBuffer.Size;
            idxOffset += cmdList.IdxBuffer.Size;
        }

        gl.BindVertexArray(0);
    }

    private void SetupFonts(ImGuiIOPtr io, ITextureLoader<Texture2D> textureLoader)
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
        var gl = GContext._GL;

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
