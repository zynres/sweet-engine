using Sweet.Engine.Renderer;
using Silk.NET.OpenGL;
using Sweet.Devices;

namespace Sweet.Engine.Scene.Components;

public unsafe struct FrameBuffer : IDisposable
{
    public uint Id;
    public Texture2D ColorTexture;

    public uint DepthBuffer;

    public uint Width;
    public uint Height;
    public uint X, Y;

    public FrameBuffer(uint width, uint height)
    {
        Width = width;
        Height = height;

        CreateAttachment();

    }

    public void Resize(uint x, uint y, uint width, uint height)
    {
        Width = width;
        Height = height;
        X = x;
        Y = y;

        Dispose();
        CreateAttachment();
    }

    private void CreateAttachment()
    {
        var gl = GraphicStack.GL;

        // create frame buffer
        gl.GenFramebuffers(1, out uint framebuffer);
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
        Id = framebuffer;

        // create texture
        ColorTexture = new Texture2D(CreateEmptyTexture(Width, Height), (int)Width, (int)Height, TextureUnit.Texture0);

        // bind texture to frame buffer
        gl.FramebufferTexture2D(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            ColorTexture.Id,
            0);

        // create render buffer (depth buffer)
        gl.GenRenderbuffers(1, out uint depth);
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depth);
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent24, Width, Height);
        DepthBuffer = depth;

        //bind depth buffer to frame buffer
        gl.FramebufferRenderbuffer(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.DepthAttachment,
            RenderbufferTarget.Renderbuffer,
            depth);

        if (gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
        {
            throw new Exception("frameBuffer inComplete");
        }

        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public uint CreateEmptyTexture(uint width, uint height)
    {
        var gl = GraphicStack.GL;

        var tex = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, tex);

        gl.TexImage2D(
            TextureTarget.Texture2D, 0, InternalFormat.Rgba, width, height,
            0, PixelFormat.Rgba, PixelType.UnsignedByte, null);

        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);

        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);

        gl.BindTexture(TextureTarget.Texture2D, 0);

        return tex;
    }

    public void Bind()
    {
        var gl = GraphicStack.GL;

        gl.BindFramebuffer(FramebufferTarget.Framebuffer, Id);
        gl.Viewport((int)X, (int)Y, Width, Height);
    }

    public void UnBind()
    {
        var gl = GraphicStack.GL;

        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        gl.Viewport(0, 0, (uint)Device.Window->Size.X, (uint)Device.Window->Size.Y);
    }

    public void Dispose()
    {
        var gl = GraphicStack.GL;

        if (ColorTexture.Id != 0)
            gl.DeleteTexture(ColorTexture.Id);

        if (DepthBuffer != 0)
            gl.DeleteRenderbuffer(DepthBuffer);

        if (Id != 0)
            gl.DeleteFramebuffer(Id);

        Id = 0;
        DepthBuffer = 0;
        ColorTexture = default;
    }
}