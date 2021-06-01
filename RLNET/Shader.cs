using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RLNET
{
    /// <summary>
    /// Simple shader class informed by a <see href="https://opentk.net/learn/chapter1/2-hello-triangle.html">tutorial.</see>
    /// </summary>
    public class Shader : IDisposable
    {
        public uint Handle { get; set; }

        public static readonly int BUFFER_SIZE = 2048;

        private readonly Dictionary<string, int> _uniformLocations;

        public Shader(string vertexPath, string fragmentPath)
        {
            uint VertexShader;
            uint FragmentShader;
            string VertexShaderSource;

            using (StreamReader reader = new StreamReader(vertexPath, Encoding.UTF8))
            {
                VertexShaderSource = reader.ReadToEnd();
            }

            string FragmentShaderSource;

            using (StreamReader reader = new StreamReader(fragmentPath, Encoding.UTF8))
            {
                FragmentShaderSource = reader.ReadToEnd();
            }

            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);

            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);

            GL.CompileShader(VertexShader);

            GL.GetShaderInfoLog(VertexShader, out string infoLogVert);
            if (infoLogVert != System.String.Empty)
                System.Console.WriteLine(infoLogVert);

            GL.CompileShader(FragmentShader);

            GL.GetShaderInfoLog(FragmentShader, out string infoLogFrag);

            if (infoLogFrag != System.String.Empty)
                System.Console.WriteLine(infoLogFrag);

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            GL.LinkProgram(Handle);

            // Cleanup:
            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);

            // Calculate Uniforms
            int numberOfUniforms = 0;
            GL.GetProgrami(Handle, ProgramPropertyARB.ActiveUniforms, ref numberOfUniforms);

            _uniformLocations = new Dictionary<string, int>();

            for (uint i = 0; i < numberOfUniforms; i++)
            {
                // The following line from the tutorial didn't work: var key = GL.GetActiveUniform(Handle, i, out _, out _);
                // So instead, we use:
                int lengthDiscard = 0;
                int sizeDiscard = 0;
                UniformType typeDiscard = UniformType.Bool;
                string key = GL.GetActiveUniform(Handle, i, BUFFER_SIZE, ref lengthDiscard, ref sizeDiscard, ref typeDiscard);
                int location = GL.GetUniformLocation(Handle, key);
                _uniformLocations.Add(key, location);
            }
        }

        private void CalculateUniformLocations()
        {

        }

        /// <summary> Use the shader. </summary>
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        /// <summary>
        /// Set a uniform float on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            // The following from the tutorial doesn't work: GL.Uniform1(_uniformLocations[name], data);
            // So instead, we write:
            GL.Uniform1f(_uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform Matrix4 on this shader
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        /// <remarks>
        ///   <para>
        ///   The matrix is transposed before being sent to the shader.
        ///   </para>
        /// </remarks>
        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            // Terrible, miserable way of doing it because
            // openTK 5 prerelease dropped UniformMatrix4f using
            // Matrix4 type:
            float[] matrixSpan = new float[]
            {
                data.M11, data.M12, data.M13, data.M14,
                data.M21, data.M22, data.M23, data.M24,
                data.M31, data.M32, data.M33, data.M34,
                data.M41, data.M42, data.M43, data.M44
            };
            GL.UniformMatrix4f(_uniformLocations[name], 1, true, matrixSpan);
            // This single line means the entire project has to be compiled as "unsafe".
            // Worth keeping around as proof of my pain.
            /*unsafe
            { 
                fixed (float* arrayHandle = matrixSpan)
                {
                    GL.UniformMatrix4fv(_uniformLocations[name], 1, 0, arrayHandle);
                }
            }*/
        }

        /// <summary>
        /// Set a uniform Vector3 on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            // The following from the tutorial doesn't work: GL.Uniform3(_uniformLocations[name], data);
            // So instead, we write:
            GL.Uniform3f(_uniformLocations[name], data.X, data.Y, data.Z);
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Handle);

                disposedValue = true;
            }
        }

        ~Shader()
        {
            GL.DeleteProgram(Handle);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
