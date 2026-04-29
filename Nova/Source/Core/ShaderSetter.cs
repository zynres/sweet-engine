using System.Numerics;
using Silk.NET.OpenGL;

namespace Nova
{
    public struct ShaderSetter : IDisposable
    {
        private readonly GL gl;
        public uint Handle { get; private set; }

        public ShaderSetter(GL glApi, Shader shader)
        {
            gl = glApi;

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

        public void Use() => gl.UseProgram(Handle);

        public void SetInt(string name, int value)
        {
            int loc = gl.GetUniformLocation(Handle, name);
            gl.Uniform1(loc, value);
        }

        public void SetFloat(string name, float value)
        {
            int loc = gl.GetUniformLocation(Handle, name);
            gl.Uniform1(loc, value);
        }

        public void SetVector4(string name, Vector4 vec)
        {
            int loc = gl.GetUniformLocation(Handle, name);
            gl.Uniform4(loc, vec.X, vec.Y, vec.Z, vec.W);
        }

        public void SetVector3(string name, Vector3 vec)
        {
            int loc = gl.GetUniformLocation(Handle, name);
            gl.Uniform3(loc, vec.X, vec.Y, vec.Z);
        }

        public void SetMatrix4(string name, Matrix4x4 mat)
        {
            int loc = gl.GetUniformLocation(Handle, name);

            Span<float> matArray = stackalloc float[16];
            matArray[0] = mat.M11; matArray[1] = mat.M12; matArray[2] = mat.M13; matArray[3] = mat.M14;
            matArray[4] = mat.M21; matArray[5] = mat.M22; matArray[6] = mat.M23; matArray[7] = mat.M24;
            matArray[8] = mat.M31; matArray[9] = mat.M32; matArray[10] = mat.M33; matArray[11] = mat.M34;
            matArray[12] = mat.M41; matArray[13] = mat.M42; matArray[14] = mat.M43; matArray[15] = mat.M44;

            unsafe
            {
                fixed (float* ptr = matArray)
                {
                    gl.UniformMatrix4(loc, 1, false, ptr);
                }
            }
        }

        private void CheckCompileErrors(uint shader, string type)
        {
            gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
            if (status == 0)
            {
                string info = gl.GetShaderInfoLog(shader);
                Console.WriteLine($"ERROR::SHADER_COMPILATION_ERROR of type: {type}\n{info}\n");
            }
        }

        private void CheckLinkErrors(uint program)
        {
            gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int status);
            if (status == 0)
            {
                string info = gl.GetProgramInfoLog(program);
                Console.WriteLine($"ERROR::PROGRAM_LINKING_ERROR\n{info}\n");
            }
        }

        public void Dispose()
        {
            if (Handle != 0)
            {
                gl.DeleteProgram(Handle);
                Handle = 0;
            }
        }
    }
}