
namespace Nova;

public struct GuiShader()
{
    public string vertexSrc = @"
        #version 330 core

        layout(location = 0) in vec2 pos;
        layout(location = 1) in vec2 uv;
        layout(location = 2) in vec4 color;

        uniform mat4 uProj;

        out vec2 vUV;
        out vec4 vColor;

        void main()
        {
            vUV = uv;
            vColor = color;
            gl_Position = uProj * vec4(pos.xy, 0, 1);
        }";
    public string fragmentSrc = @"
        #version 330 core

        in vec2 vUV;
        in vec4 vColor;

        uniform sampler2D uTexture;

        out vec4 FragColor;

        void main()
        {
            FragColor = vColor * texture(uTexture, vUV);
        }";
}