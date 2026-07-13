using SweetLib.Devices;
using Silk.NET.OpenGL;

namespace SweetEngine.Library.Resources;

public unsafe struct FrameBuffer
{
    public Texture2D Color;
    public uint Id;

    public uint Depth;

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
        var gl = GraphicContext.GL;

        // create frame buffer
        gl.GenFramebuffers(1, out uint framebuffer);
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
        Id = framebuffer;

        // create texture
        Color = new Texture2D(CreateEmptyTexture(gl, Width, Height), (int)Width, (int)Height, TextureUnit.Texture0);

        // bind texture to frame buffer
        gl.FramebufferTexture2D(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            Color.Id,
            0);

        // create render buffer (depth buffer)
        gl.GenRenderbuffers(1, out uint depth);
        gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depth);
        gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent24, Width, Height);
        Depth = depth;

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

    public uint CreateEmptyTexture(GL gl, uint width, uint height)
    {
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
        var gl = GraphicContext.GL;

        gl.BindFramebuffer(FramebufferTarget.Framebuffer, Id);
        gl.Viewport((int)X, (int)Y, Width, Height);
    }

    public void UnBind(in Window window)
    {
        var gl = GraphicContext.GL;

        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        gl.Viewport(0, 0, (uint)window.Size.X, (uint)window.Size.Y);
    }

    public void Dispose()
    {
        var gl = GraphicContext.GL;

        if (Color.Id != 0)
            gl.DeleteTexture(Color.Id);

        if (Depth != 0)
            gl.DeleteRenderbuffer(Depth);

        if (Id != 0)
            gl.DeleteFramebuffer(Id);

        Id = 0;
        Depth = 0;
        Color = default;
    }
}