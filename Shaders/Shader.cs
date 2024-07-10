using ConsoleApp1_Pet.Textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common.Input;
using SixLabors.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApp1_Pet.Shaders
{
    public class Shader
    {
        public int Id;
        private int VertexShaderId;

        public const string GlslVersion = "#version 460 core";
        private const string VertPrefix = @$"{GlslVersion}
layout (location = 0) in
vec3 aPos;
";
        private const string VertMain = @$"
void main()
{{
    gl_Position = vec4(aPos, 1.0);
}}";
        protected virtual string VertexPrefix { get => VertPrefix; }
        protected virtual string VertexMain { get => VertMain; }

        private const string FragmentPrefix = @$"{GlslVersion}
out vec4 FragColor;
";
        private const string FragmentMain = @$"
void main()
{{
    FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
}}
";
        /// <summary>
        /// Should include GlslVersion if base not used
        /// </summary>
        protected virtual string FragPrefix { get => FragmentPrefix; }
        /// <summary>
        /// Should include main(){}
        /// </summary>
        protected virtual string FragMain {get => FragmentMain; }
        public string GetVertex() => $"{VertexPrefix} \n {VertexMain}";
        public string GetFragment() => $"{FragPrefix} \n {FragMain}"; 
        public void Compile()
        {
            var VertexShader = GL.CreateShader(ShaderType.VertexShader);
            var vertText = GetVertex();
            GL.ShaderSource(VertexShader, vertText);
            var fragText = GetFragment();
            var FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, fragText);

            GL.CompileShader(VertexShader);

            GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(VertexShader);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{vertText}\n{infoLog}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{vertText}");
                Console.ForegroundColor = ConsoleColor.White;
            }

            GL.CompileShader(FragmentShader);

            GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(FragmentShader);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{fragText}\n{infoLog}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{fragText}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            Id = GL.CreateProgram();

            GL.AttachShader(Id, VertexShader);
            GL.AttachShader(Id, FragmentShader);

            GL.LinkProgram(Id);

            GL.GetProgram(Id, GetProgramParameterName.LinkStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(Id);
                Console.WriteLine(infoLog);
            }
            GL.DetachShader(Id, VertexShader);
            GL.DetachShader(Id, FragmentShader);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);
        }
        public void Use()
        {
            GL.UseProgram(Id);
        }
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                GL.DeleteProgram(Id);

                disposedValue = true;
            }
        }



        public int GetAttribLocation(string attribName)
        {
            var res = GL.GetAttribLocation(Id, attribName);
            //if (res < 0) throw new  Exception("Attrib location ex");
            return res;
        }


        /// <summary> Don't forget to Use() shader before any SetCalls </summary>
        public void SetMatrix(string name,Matrix4 mat)
        {
           // Use();
            GL.UniformMatrix4(GetAttribLocation(name),true,ref mat);
        }
        public void SetTexture(int attribLocation, Texture tex)
        {
            //Use();
            GL.Uniform1(0, tex.id);
        }
        /// <summary> Don't forget to Use() shader before any SetCalls </summary>
        public void SetMatrix(int attribLocation, Matrix4 mat)
        {
            //Use();
            GL.UniformMatrix4(attribLocation, true, ref mat);
        }
        ~Shader()
        {
            if (disposedValue == false)
            {
                Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
