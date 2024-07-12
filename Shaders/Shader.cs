using ConsoleApp1_Pet.Textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApp1_Pet.Shaders
{
    public class ShaderTextureInfo
    {
        public int uniformLayout;
        public int textureUnit;
    }
    public class Shader
    {

        public const string CameraDepth = "_camDepth";

        public int Id;
        public string Name;
        public string VertexPath;
        public string FragmentPath;
        public Dictionary<string, int> UniformsLayout = new Dictionary<string, int>();
        public Dictionary<string, int> TexturesLayout = new Dictionary<string, int>();
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
        public void OnCompiled()
        {
            UniformsLayout.Clear();
            TexturesLayout.Clear();
            FetchUniforms();
            
        }
        public bool IsHaveUniform(string name) => UniformsLayout.ContainsKey(name);
        public bool IsHaveTexture(string name) => TexturesLayout.ContainsKey(name);

        private void FetchUniforms()
        {
            this.Use();
            GL.GetProgram(Id, GetProgramParameterName.ActiveUniforms,out int numUniforms);
            
            int TexUnit = 0;
            for (int i = 0; i < numUniforms; ++i)
            {
                var name = GL.GetActiveUniform(Id, i, out int size, out ActiveUniformType type);
               // string name = GL.GetActiveUniformName(Id, i);

                //if (name == null)
                //{
                //    Console.WriteLine("Warning: Unable to retrieve uniform name.");
                //    continue;
                //}
               
                int location = GL.GetUniformLocation(Id, name);

                if (location == -1)
                {
                    Console.WriteLine($"Warning: Uniform '{name}' not found.");
                    continue;
                }
                switch(type)
                {
                    case ActiveUniformType.Sampler2D: TexturesLayout.Add(name, TexUnit); GL.Uniform1(location, TexUnit);  TexUnit++; break;
                    default: UniformsLayout.Add(name, location); break;
                }
               
            }
            
        }

        public void Use()
        {
            GL.UseProgram(Id);
        }
        private bool disposedValue = false;
        protected int GetTextureUnit(string uniformName)
        {
            var res = 0;
            if(!TexturesLayout.TryGetValue(uniformName , out res))
            {
                var TexUnit = TexturesLayout.Count;
                var loc=  GetAttribLocation(uniformName);
                GL.Uniform1(loc, TexUnit);
                res = TexUnit;
            }
            return res;
        }
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
        public int GetUniformLocation(string uniName)
        {
            var res = GL.GetUniformLocation(Id, uniName);
            return res;
        }


        /// <summary> Don't forget to Use() shader before any SetCalls </summary>
        public void SetMatrix(string name, Matrix4 mat)
        {
            if(UniformsLayout.TryGetValue(name, out var res))
                GL.UniformMatrix4(res, true, ref mat);
            // Use();

        }
        public void SetTexture(Texture tex)
        {
            //Use();
            tex.Use();
            // GL.Uniform1(attribLocation, );
        }
        public void SetTexture(string name, Texture tex)
        {
            //Use();
            tex.Use(GetTextureUnit(name));
            // GL.Uniform1(attribLocation, );
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
        // Method 1: Set uniform by name
        public void SetUniform(string name, int value)
        {
            if (UniformsLayout.TryGetValue(name, out var res))
                GL.Uniform1(res, value);
            //if (UniformsLayout.TryGetValue(name, out int location))
            //{
            //    GL.Uniform1(location, value);
            //}
            //else
            //{
            //    location = GL.GetUniformLocation(Id, name);
            //    UniformsLayout[name] = location;
            //    GL.Uniform1(location, value);
            //}
        }

        // Method 2: Set uniform by name (overloaded for different data types)
        public void SetUniform(string name, float value)
        {
            if (UniformsLayout.TryGetValue(name, out var res))
                GL.Uniform1(res, value);
        }

        // Method 3: Set uniform by name (for vectors)
        public void SetUniform(string name, Vector3 value)
        {
            if (UniformsLayout.TryGetValue(name, out var res))
                GL.Uniform3(res, value);
        }

        // Method 4: Set uniform by name (for matrices)
        public void SetUniform(string name, Matrix4 value)
        {
            if (UniformsLayout.TryGetValue(name, out var res))
                GL.UniformMatrix4(res, true, ref value);
        }

        //// Method 5: Set uniform by name (for textures)
        //public void SetUniform(string name, Texture texture)
        //{
        //    if (UniformsLayout.TryGetValue(name, out int location))
        //    {
        //        GL.Uniform1(location, texture.id);
        //    }
        //    else
        //    {
        //        location = GL.GetUniformLocation(Id, name);
        //        UniformsLayout[name] = location;
        //        GL.Uniform1(location, textureUnit);
        //    }
        //}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
