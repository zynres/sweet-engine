using System.Numerics;
using Silk.NET.OpenGL;

namespace Nova
{
    public struct ShaderSetter : IDisposable
    {
        public uint Handle { get; private set; }

        public ShaderSetter(Shader shader)
        {
            var gl = GContext._GL;

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

        public void Use() => GContext._GL.UseProgram(Handle);

        public void SetInt(string name, int value)
        {
            var gl = GContext._GL;

            int loc = gl.GetUniformLocation(Handle, name);
            gl.Uniform1(loc, value);
        }

        public void SetFloat(string name, float value)
        {
            var gl = GContext._GL;

            int loc = gl.GetUniformLocation(Handle, name);
            gl.Uniform1(loc, value);
        }

        public void SetVector4(string name, Vector4 vec)
        {
            var gl = GContext._GL;

            int loc = gl.GetUniformLocation(Handle, name);
            gl.Uniform4(loc, vec.X, vec.Y, vec.Z, vec.W);
        }

        public void SetVector3(string name, Vector3 vec)
        {
            var gl = GContext._GL;

            int loc = gl.GetUniformLocation(Handle, name);
            gl.Uniform3(loc, vec.X, vec.Y, vec.Z);
        }

        public void SetMatrix4(string name, Matrix4x4 mat)
        {
            var gl = GContext._GL;

            int loc = gl.GetUniformLocation(Handle, name);

            unsafe
            {
                gl.UniformMatrix4(loc, 1, false, (float*)&mat);
            }
        }

        private void CheckCompileErrors(uint shader, string type)
        {
            var gl = GContext._GL;

            gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
            if (status == 0)
            {
                string info = gl.GetShaderInfoLog(shader);
                Console.WriteLine($"ERROR::SHADER_COMPILATION_ERROR of type: {type}\n{info}\n");
            }
        }

        private void CheckLinkErrors(uint program)
        {
            var gl = GContext._GL;

            gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int status);
            if (status == 0)
            {
                string info = gl.GetProgramInfoLog(program);
                Console.WriteLine($"ERROR::PROGRAM_LINKING_ERROR\n{info}\n");
            }
        }

        public void Dispose()
        {
            var gl = GContext._GL;

            if (Handle != 0)
            {
                gl.DeleteProgram(Handle);
                Handle = 0;
            }
        }
    }
}