using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Shaders
{
    public class Shader
    {
        public int Id;
        public string Name;
        public string VertexPath;
        public string FragmentPath;
        public Shader(string Fragment,string Vertex) {
            VertexPath = Vertex;
                FragmentPath = Fragment;
            Name = Path.GetFileNameWithoutExtension(Fragment);
            //var shaderCode = File.ReadAllText(FilePath);

            //var VertexShader = GL.CreateShader(ShaderType.VertexShader);
            //var vertText = GetVertex();
            //GL.ShaderSource(VertexShader, vertText);
            //var fragText = GetFragment();
            //var FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            //GL.ShaderSource(FragmentShader, fragText);


            //GL.CompileShader(VertexShader);

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
        public void SetMatrix(string name, Matrix4 mat)
        {
            // Use();
            GL.UniformMatrix4(GetAttribLocation(name), true, ref mat);
        }
        public void SetTexture(int attribLocation, int samplerNumber)
        {
            //Use();
            GL.Uniform1(attribLocation, samplerNumber);
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
