using System.Numerics;
using Silk.NET.OpenGL;

namespace Nova;

public struct ShaderSetter : IDisposable
{
    public uint Id { get; private set; }

    public ShaderSetter(string vertexSrc, string fragmentSrc)
    {
        var gl = GraphicStack._GL;

        uint vertex = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(vertex, vertexSrc);
        gl.CompileShader(vertex);
        CheckCompileErrors(vertex, "VERTEX");

        uint fragment = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(fragment, fragmentSrc);
        gl.CompileShader(fragment);
        CheckCompileErrors(fragment, "FRAGMENT");

        Id = gl.CreateProgram();
        gl.AttachShader(Id, vertex);
        gl.AttachShader(Id, fragment);
        gl.LinkProgram(Id);
        CheckLinkErrors(Id);

        gl.DetachShader(Id, vertex);
        gl.DetachShader(Id, fragment);
        gl.DeleteShader(vertex);
        gl.DeleteShader(fragment);

        Console.WriteLine($"Program = {Id}");
        gl.GetProgram(Id, ProgramPropertyARB.LinkStatus, out int linked);
    }

    public void Use() => GraphicStack._GL.UseProgram(Id);

    public void SetInt(string name, int value)
    {
        var gl = GraphicStack._GL;

        int loc = gl.GetUniformLocation(Id, name);
        gl.Uniform1(loc, value);

        CheckCurrentProgram(gl, name, loc);
    }

    public void SetFloat(string name, float value)
    {
        var gl = GraphicStack._GL;

        int loc = gl.GetUniformLocation(Id, name);
        gl.Uniform1(loc, value);

        CheckCurrentProgram(gl, name, loc);
    }

    public void SetVector4(string name, Vector4 vec)
    {
        var gl = GraphicStack._GL;

        int loc = gl.GetUniformLocation(Id, name);
        gl.Uniform4(loc, vec.X, vec.Y, vec.Z, vec.W);

        CheckCurrentProgram(gl, name, loc);
    }

    public void SetVector3(string name, Vector3 vec)
    {
        var gl = GraphicStack._GL;

        int loc = gl.GetUniformLocation(Id, name);
        gl.Uniform3(loc, vec.X, vec.Y, vec.Z);

        CheckCurrentProgram(gl, name, loc);
    }

    public void SetMatrix4(string name, Matrix4x4 mat, bool transpose = false)
    {
        var gl = GraphicStack._GL;

        int loc = gl.GetUniformLocation(Id, name);

        unsafe
        {
            gl.UniformMatrix4(loc, 1, transpose, (float*)&mat);
        }

        CheckCurrentProgram(gl, name, loc);
    }

    private void CheckCurrentProgram(GL gl, string name, int loc)
    {
        /* Console.WriteLine($"{name} -> {loc}");

         int currentProgram;
         gl.GetInteger(GetPName.CurrentProgram, out currentProgram);

         Console.WriteLine($"Current = {currentProgram}, Expected = {Id}");*/
    }

    private void CheckCompileErrors(uint shader, string type)
    {
        var gl = GraphicStack._GL;

        gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
        if (status == 0)
        {
            string info = gl.GetShaderInfoLog(shader);
            Console.WriteLine($"ERROR::SHADER_COMPILATION_ERROR of type: {type}\n{info}\n");
        }
    }

    private void CheckLinkErrors(uint program)
    {
        var gl = GraphicStack._GL;

        gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int status);
        if (status == 0)
        {
            string info = gl.GetProgramInfoLog(program);
            Console.WriteLine($"ERROR::PROGRAM_LINKING_ERROR\n{info}\n");
        }
    }

    public void Dispose()
    {
        var gl = GraphicStack._GL;

        if (Id != 0)
        {
            gl.DeleteProgram(Id);
            Id = 0;
        }
    }
}