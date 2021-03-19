using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial
{
    /// <summary>
    /// https://github.com/opentk/LearnOpenTK/blob/c654d2e2d22a060e84a800af2abe37704410fa5f/Common/Shader.cs
    /// </summary>
    public class Shader
    {
        public static readonly int BUFFER_SIZE = 2048;

        public readonly uint Handle;

        private readonly Dictionary<string, int> _uniformLocations;

        // This is how you create a simple shader.
        // Shaders are written in GLSL, which is a language very similar to C in its semantics.
        // The GLSL source is compiled *at runtime*, so it can optimize itself for the graphics card it's currently being used on.
        // A commented example of GLSL can be found in shader.vert
        public Shader(string vertPath, string fragPath)
        {
            // There are several different types of shaders, but the only two you need for basic rendering are the vertex and fragment shaders.
            // The vertex shader is responsible for moving around vertices, and uploading that data to the fragment shader.
            //   The vertex shader won't be too important here, but they'll be more important later.
            // The fragment shader is responsible for then converting the vertices to "fragments", which represent all the data OpenGL needs to draw a pixel.
            //   The fragment shader is what we'll be using the most here.

            // Load vertex shader and compile
            var shaderSource = File.ReadAllText(vertPath);

            // GL.CreateShader will create an empty shader (obviously). The ShaderType enum denotes which type of shader will be created.
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);

            // Now, bind the GLSL source code
            GL.ShaderSource(vertexShader, shaderSource);

            // And then compile
            CompileShader(vertexShader);

            // We do the same for the fragment shader
            shaderSource = File.ReadAllText(fragPath);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            // These two shaders must then be merged into a shader program, which can then be used by OpenGL.
            // To do this, create a program...
            Handle = GL.CreateProgram();

            // Attach both shaders...
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            // And then link them together.
            LinkProgram(Handle);

            // When the shader program is linked, it no longer needs the individual shaders attacked to it; the compiled code is copied into the shader program.
            // Detach them, and then delete them.
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            // The shader is now ready to go, but first, we're going to cache all the shader uniform locations.
            // Querying this from the shader is very slow, so we do it once on initialization and reuse those values
            // later.

            // First, we have to get the number of active uniforms in the shader.
            // The following line from the tutorial didn't work: GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);
            // So instead, we use:
            int numberOfUniforms = 0;
            GL.GetProgramiv(Handle, ProgramPropertyARB.ActiveUniforms, ref numberOfUniforms);

            // Next, allocate the dictionary to hold the locations.
            _uniformLocations = new Dictionary<string, int>();

            // Loop over all the uniforms,
            for (uint i = 0; i < numberOfUniforms; i++)
            {
                // get the name of this uniform,
                // The following line from the tutorial didn't work: var key = GL.GetActiveUniform(Handle, i, out _, out _);
                // So instead, we use:
                int lengthDiscard = 0;
                int sizeDiscard = 0;
                UniformType typeDiscard = UniformType.Bool;
                var key = GL.GetActiveUniform(Handle, i, BUFFER_SIZE, ref lengthDiscard, ref sizeDiscard, ref typeDiscard);
                // Note: The above call is under active development by the opentk team


                // get the location,
                var location = GL.GetUniformLocation(Handle, key);

                // and then add it to the dictionary.
                _uniformLocations.Add(key, location);
            }
        }

        private static void CompileShader(uint shader)
        {
            // Try to compile the shader
            GL.CompileShader(shader);

            // Check for compilation errors
            int code = 0;
            // The following from the tutorial doesn't work: GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            // So instead, we write:
            GL.GetShaderiv(shader, ShaderParameterName.CompileStatus, ref code);

            if (code != (int)All.True)
            {
                // We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
                // The following from the tutorial doesn't work: var infoLog = GL.GetShaderInfoLog(shader);
                // So instead, w write:
                string infoLog = "";
                int length = 0;
                GL.GetShaderInfoLog(shader, BUFFER_SIZE, ref length, out infoLog);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(uint program)
        {
            // We link the program
            GL.LinkProgram(program);

            // Check for linking errors
            // The following from the tutorial doesn't work: GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            // So instead we write:
            int code = 0;
            GL.GetProgramiv(program, ProgramPropertyARB.LinkStatus, ref code);
            if (code != (int)All.True)
            {
                // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
                throw new Exception($"Error occurred whilst linking Program({program})");
            }
        }

        // A wrapper function that enables the shader program.
        public void Use()
        {
            GL.UseProgram(Handle);
        }

        // The shader sources provided with this project use hardcoded layout(location)-s. If you want to do it dynamically,
        // you can omit the layout(location=X) lines in the vertex shader, and use this in VertexAttribPointer instead of the hardcoded values.
        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        // Uniform setters
        // Uniforms are variables that can be set by user code, instead of reading them from the VBO.
        // You use VBOs for vertex-related data, and uniforms for almost everything else.

        // Setting a uniform is almost always the exact same, so I'll explain it here once, instead of in every method:
        //     1. Bind the program you want to set the uniform on
        //     2. Get a handle to the location of the uniform with GL.GetUniformLocation.
        //     3. Use the appropriate GL.Uniform* function to set the uniform.

        /// <summary>
        /// Set a uniform int on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform</param>
        /// <param name="data">The data to set</param>
        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            // The following from the tutorial doesn't work: GL.Uniform1(_uniformLocations[name], data);
            // So instead, we write:
            GL.Uniform1i(_uniformLocations[name], data);
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
            // The following from the tutorial doesn't work: GL.UniformMatrix4(_uniformLocations[name], true, ref data);
            // So instead, we write:
            Span<float> matrixSpan = new float[]
            {
                data.M11, data.M12, data.M13, data.M14,
                data.M21, data.M22, data.M23, data.M24,
                data.M31, data.M32, data.M33, data.M34,
                data.M41, data.M42, data.M43, data.M44
            };
            GL.UniformMatrix4fv(_uniformLocations[name], true, matrixSpan);
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
    }
}
