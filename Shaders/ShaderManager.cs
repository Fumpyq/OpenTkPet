using ConsoleApp1_Pet.Meshes;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Shaders
{
    public static class ShaderManager
    {
        private static Dictionary<string,int> CompiledVertex = new Dictionary<string, int>(2);
        private static ConcurrentQueue<Shader> InvalidateShaders = new ConcurrentQueue<Shader>();
        private static ConcurrentQueue<Shader> RecompileShaders = new ConcurrentQueue<Shader>();
        private static List<ShaderHotReloadTracker> Trackers = new List<ShaderHotReloadTracker> (2);

        public static void InvalidateShader(Shader shader)
        {
            if (CompiledVertex.Remove(shader.VertexPath)) { }
        }
        public static void InvalidateShader_FromAsync(Shader shader)
        {
            InvalidateShaders.Enqueue(shader);
        }
        public static void RecompileShader_FromAsync(Shader shader)
        {
            RecompileShaders.Enqueue(shader);
        }
         
        public static void RecompileShader(Shader shader)
        {
            
            Task<string> vert = null;
            //Task<string> frag = File.ReadAllTextAsync(shader.FragmentPath);
            Task<string> frag = null;
            var vertText = string.Empty;
            var fragText = string.Empty; 
            if (!CompiledVertex.TryGetValue(shader.VertexPath,out int VertexId))
            {
                //vert = File.ReadAllTextAsync(shader.VertexPath);
                vertText = File.ReadAllText(shader.VertexPath);
            }
            if (shader.Id > 0)
            {
                GL.DeleteProgram(shader.Id);
            }
            if (vert != null || !string.IsNullOrEmpty(vertText))
            {
                VertexId = GL.CreateShader(ShaderType.VertexShader);
                //vertText = vert.GetAwaiter().GetResult();
                GL.ShaderSource(VertexId, vertText);
                GL.CompileShader(VertexId);
            }
            var FragId = GL.CreateShader(ShaderType.FragmentShader);
            //fragText = frag.GetAwaiter().GetResult();
            fragText = File.ReadAllText(shader.FragmentPath);
            GL.ShaderSource(FragId, fragText);

            var sid= GL.CreateProgram();
            shader.Id = sid;


            GL.GetShader(VertexId, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(VertexId);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"VERTEX ERROR:\n{vertText}\n{infoLog}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                //Console.WriteLine($" ->>> VERT (points) ===\n{vertText}");
            }
            try
            {
                GL.CompileShader(FragId);
            }
            catch(Exception ex)
            {
                Thread.Sleep(125);
                GL.CompileShader(FragId);

            }

            GL.GetShader(FragId, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(FragId);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"VERTEX ERROR:\n{fragText}\n{infoLog}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
               // Console.WriteLine($" ->>> FRAG (surface) ===\n{fragText}");
            }

            GL.AttachShader(sid, VertexId);
            GL.AttachShader(sid, FragId);

            GL.LinkProgram(sid);

            GL.GetProgram(sid, GetProgramParameterName.LinkStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(sid);
                Console.WriteLine(infoLog);
            }
            GL.DetachShader(sid, VertexId);
            GL.DetachShader(sid, FragId);
            GL.DeleteShader(FragId);
            shader.OnCompiled();
            //GL.DeleteShader(VertexShader);
        }

        public static void OnFrameStart()
        {
            while (InvalidateShaders.Count > 0)
            {
                Shader ss = null;
                while (!InvalidateShaders.TryDequeue(out ss)) ;
                InvalidateShader(ss);

            }
            while (RecompileShaders.Count > 0)
            {
                Shader ss = null;
                while (!RecompileShaders.TryDequeue(out ss)) ;
                RecompileShader(ss);

            }
        }

        public static Shader CompileShader(string vertexPath,string fragmentPath,bool HotReload = true)
        {
            var res = new Shader(fragmentPath,vertexPath);
            RecompileShader(res);
            if (HotReload) Trackers.Add(new ShaderHotReloadTracker(res));
            return res;
        }
        public class ShaderHotReloadTracker
        {
#if DEBUG
            public static readonly string SourcePath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName;
#else
public static readonly string SourcePath = AppDomain.CurrentDomain.BaseDirectory;
#endif
            public Shader ObservedShader;
            public FileSystemWatcher VertexWatcher;
            public FileSystemWatcher FragmentWatcher;
            public const NotifyFilters NotifFilter = NotifyFilters.CreationTime
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Size;
            public ShaderHotReloadTracker(Shader shader)
            {
                ObservedShader = shader;

                VertexWatcher = new FileSystemWatcher(SourcePath + "\\" + Path.GetDirectoryName(shader.VertexPath), Path.GetFileName(shader.VertexPath));

                VertexWatcher.NotifyFilter = NotifFilter;
                VertexWatcher.Changed += OnVertexChanged;
                VertexWatcher.EnableRaisingEvents = true;

                FragmentWatcher = new FileSystemWatcher(SourcePath + "\\" + Path.GetDirectoryName(shader.FragmentPath), Path.GetFileName(shader.FragmentPath));
                FragmentWatcher.Path = SourcePath + "\\" + Path.GetDirectoryName(shader.FragmentPath);
                FragmentWatcher.Filter = Path.GetFileName(shader.FragmentPath);
                FragmentWatcher.NotifyFilter = NotifFilter;
                FragmentWatcher.Changed += OnFragmentChanged;
                FragmentWatcher.EnableRaisingEvents = true;
            }
            private void OnFragmentChanged(object sender, FileSystemEventArgs e)
            {
                Console.WriteLine("FRAG RECOMPILE");
#if DEBUG
                int RetryCount = 10;
                while (RetryCount > 0)
                {
                    try
                {
                   
                        File.WriteAllText(ObservedShader.FragmentPath, File.ReadAllText(SourcePath + "\\" + ObservedShader.FragmentPath));
                        Thread.Sleep(250);
                        RetryCount = 0;
                   
                }
                catch(Exception ex)
                {
                    RetryCount--;
                    Thread.Sleep(125);
                    if (RetryCount <= 0)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                }
#endif
                ShaderManager.RecompileShader_FromAsync(ObservedShader);
            }
            private void OnVertexChanged(object sender, FileSystemEventArgs e)
            {
                Console.WriteLine("VERT RECOMPILE");
#if DEBUG
                int RetryCount = 10;
                while (RetryCount > 0)
                {
                    try
                    {
                   
                            File.WriteAllText(ObservedShader.VertexPath, File.ReadAllText(SourcePath + "\\" + ObservedShader.VertexPath));
                            Thread.Sleep(250);
                            RetryCount = 0;
                   
                    }
                    catch (Exception ex)
                    {
                        RetryCount--;
                        Thread.Sleep(125);
                        if (RetryCount <= 0)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }

#endif
                ShaderManager.InvalidateShader_FromAsync(ObservedShader);
                ShaderManager.RecompileShader_FromAsync(ObservedShader);
            }
        }
    }
}
