using Silk.NET.OpenGL;

namespace Nova;

public struct FrameBuffer
{
    public Texture2D Texture;

    public void Resize(int width, int height)
    {

    }

    public void Bind()
    {
        var gl = GraphicStack._GL;

        gl.ActiveTexture(Texture.Unit);
        gl.BindTexture(TextureTarget.Texture2D, Texture.Id);
    }
    
    public void UnBind()
    {
        var gl = GraphicStack._GL;

        gl.ActiveTexture(Texture.Unit);
        gl.BindTexture(TextureTarget.Texture2D, 0);
    }
}