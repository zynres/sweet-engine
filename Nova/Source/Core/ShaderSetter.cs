using System.Numerics;
using Silk.NET.OpenGL;

namespace Nova
{
    public struct ShaderSetter : IDisposable
    {
        public uint Handle { get; private set; }

        public void SetShader(Shader shader)
        {
            GL gl = GContext._GL;

            uint vertex = gl.CreateShader(ShaderType.VertexShader);
            gl.ShaderSource(vertex, shader.vertexSrc);
            gl.CompileShader(vertex);
            CheckCompileErrors(vertex, "VERTEX");

            uint fragment = gl.CreateShader(ShaderType.FragmentShader);
            gl.ShaderSource(fragment, shader.fragmentSrc);
            gl.CompileShader(fragment);
            CheckCompileErrors(fragment, "FRAGMENT");

            Handle = gl.CreateProgram();
            gl.AttachShader(Handle, vertex);
            gl.AttachShader(Handle, fragment);
            gl.LinkProgram(Handle);
            CheckLinkErrors(Handle);

            gl.DetachShader(Handle, vertex);
            gl.DetachShader(Handle, fragment);
            gl.DeleteShader(vertex);
            gl.DeleteShader(fragment);
        }

        public readonly void Use() => GContext._GL.UseProgram(Handle);

        public readonly void SetInt(string name, int value)
        {
            GL gl = GContext._GL;

            int loc = gl.GetUniformLocation(Handle, name);
            gl.Uniform1(loc, value);
        }

        public readonly void SetFloat(string name, float value)
        {
            GL gl = GContext._GL;

            int loc = gl.GetUniformLocation(Handle, name);
            gl.Uniform1(loc, value);
        }

        public readonly void SetVector4(string name, Vector4 vec)
        {
            GL gl = GContext._GL;

            int loc = gl.GetUniformLocation(Handle, name);
            gl.Uniform4(loc, vec.X, vec.Y, vec.Z, vec.W);
        }

        public readonly void SetVector3(string name, Vector3 vec)
        {
            GL gl = GContext._GL;

            int loc = gl.GetUniformLocation(Handle, name);
            gl.Uniform3(loc, vec.X, vec.Y, vec.Z);
        }

        public readonly void SetMatrix4(string name, Matrix4x4 mat)
        {
            GL gl = GContext._GL;

            int loc = gl.GetUniformLocation(Handle, name);
                
            unsafe
            {
               
                gl.UniformMatrix4(loc, 1, false, (float*)&mat);
        
            }
        }

        private readonly void CheckCompileErrors(uint shader, string type)
        {
            GL gl = GContext._GL;

            gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
            if (status == 0)
            {
                string info = gl.GetShaderInfoLog(shader);
                Console.WriteLine($"ERROR::SHADER_COMPILATION_ERROR of type: {type}\n{info}\n");
            }
        }

        private readonly void CheckLinkErrors(uint program)
        {
            GL gl = GContext._GL;

            gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int status);
            if (status == 0)
            {
                string info = gl.GetProgramInfoLog(program);
                Console.WriteLine($"ERROR::PROGRAM_LINKING_ERROR\n{info}\n");
            }
        }

        public void Dispose()
        {
            GL gl = GContext._GL;

            if (Handle != 0)
            {
                gl.DeleteProgram(Handle);
                Handle = 0;
            }
        }
    }
}