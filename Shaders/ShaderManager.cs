using BepuPhysics.Collidables;
using ConsoleApp1_Pet.Architecture.Resources;
using ConsoleApp1_Pet.Meshes;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;




namespace ConsoleApp1_Pet.Shaders
{
    public static class ShaderManager
    {
        private class ShaderCacheInfo
        {
            public int id;
            public string CompiledCode;

            public ShaderCacheInfo(int id, string compiledCode)
            {
                this.id = id;
                CompiledCode = compiledCode;
            }
        }

        private static Dictionary<string, ShaderCacheInfo> CompiledVertex = new Dictionary<string, ShaderCacheInfo>(2);
        private static Dictionary<string, ShaderCacheInfo> CompiledFragments = new Dictionary<string, ShaderCacheInfo>(2);


        public static void RecompileShader(ShaderResource sr)
        {
            if (!sr.IsLoaded) sr.Load();
            RecompileShader(sr.shader, sr.VertexPath, sr.FragmentPath, sr.VertexCode , sr.FragmentCode);
        }
        public static void RecompileShader(Shader sh, string VertexName,string FragmentName,string VertexCode,string FragmentCode)
        {
            void LogCopileErrors(int shaderId, string prefix,string shaderCode)
            {
                GL.GetShader(shaderId, ShaderParameter.CompileStatus, out int success);
                if (success == 0)
                {
                    string infoLog = GL.GetShaderInfoLog(shaderId);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{prefix} Shader compile error:\n{FragmentCode}\n{infoLog}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

            if (!CompiledVertex.TryGetValue(VertexName, out var vertex) || vertex.CompiledCode != VertexCode)
            {
                if (vertex != null)
                {
                    GL.DeleteShader(vertex.id);
                    vertex.id = GL.CreateShader(ShaderType.VertexShader);
                    vertex.CompiledCode = VertexCode;
                }
                else
                {
                    vertex = new ShaderCacheInfo(GL.CreateShader(ShaderType.VertexShader), VertexName);
                    CompiledVertex[VertexName] = vertex;
                }
                var vertexId = vertex.id;
                GL.ShaderSource(vertexId, VertexCode);
                GL.CompileShader(vertexId);
                LogCopileErrors(vertexId, "VERTEX", VertexCode);
            }
            if (!CompiledFragments.TryGetValue(FragmentName, out var frag) || frag.CompiledCode !=FragmentCode)
            {
                if (frag != null)
                {
                    GL.DeleteShader(frag.id);
                    frag.id = GL.CreateShader(ShaderType.FragmentShader);
                    frag.CompiledCode = FragmentCode;
                }
                else
                {
                    frag = new ShaderCacheInfo(GL.CreateShader(ShaderType.FragmentShader), FragmentName);
                    CompiledFragments[FragmentName] = frag;
                }
                var fragId =frag.id;
                GL.ShaderSource(fragId, FragmentCode);
                GL.CompileShader(fragId);
                LogCopileErrors(fragId, "FRAGMENT", FragmentCode);
            }

            if (sh.Id > 0)
            {
                GL.DeleteProgram(sh.Id);
            }

            var sid = GL.CreateProgram();
            sh.Id = sid;

            GL.AttachShader(sid, vertex.id);
            GL.AttachShader(sid, frag.id);

            GL.LinkProgram(sid);
            GL.GetProgram(sid, GetProgramParameterName.LinkStatus, out int lingSuccess);
            if (lingSuccess == 0)
            {
                string infoLog = GL.GetProgramInfoLog(sid);
                Console.WriteLine(infoLog);
            }
            sh.OnCompiled();
        }




        public static void OnFrameStart()
        {
           
        }
      
    }
}
