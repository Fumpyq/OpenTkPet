//using ConsoleApp1_Pet.Materials;
//using ConsoleApp1_Pet.Meshes;
//using ConsoleApp1_Pet.Shaders;
//using ConsoleApp1_Pet.Textures;
//using OpenTK.Graphics.OpenGL;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ConsoleApp1_Pet.Новая_папка
//{
//    public static class Resources
//    {
//        public static Dictionary<string, IResource> resources = new Dictionary<string, IResource>();

//        public static Dictionary<Type, Func<object, object>> _loaders = new Dictionary<Type, Func<object, object>>();

//        public static void RegisterLoader<TResource>([NotNull]Func<object, TResource> loader)
//        {
//            _loaders[typeof(TResource)] = (paramsObj) => loader(paramsObj);
//        }
//        public static bool TryGetResource(string resourceKey,out IResource resource)
//        {
//            return resources.TryGetValue(resourceKey, out resource);
            
//        }
//        public static bool TryGetTexture(string resourceKey, out TextureResource resource)
//        {
//            if(TryGetResource(resourceKey,out var res))
//            {
//                if(res is TextureResource texture)
//                {
//                    resource = texture;
//                    return true;
//                }
//                else
//                {
//                    resource = null;
//                    Console.WriteLine($"Resource key:'{resourceKey}' was requested as Texture but loaded as {res.GetType()}");
//                    return false;
//                }
//            }
//            else
//            {
//                resource = null;
//                return false;
//            }
//        }

//        public static T LoadResource<T> (string key, object paramsObj) where T : IResource
//        {
//            if(_loaders.TryGetValue(typeof(T), out var loader))
//            {
//                T res = (T) loader(paramsObj);
//                if(res != null)
//                {
//                    resources.Add(key, res);
//                }
//                return res;
//            }
//            else
//            {
//                throw new KeyNotFoundException($"No loader registered for type: {typeof(T)}");
//            }
//        }

//        public static T Load<T>(string FilePath,string ResKey = null) where T: FileResource, new ()
//        {
//            var res = new T();
//            res.SetPath(FilePath);
//            if (res.Load())
//            {
//                resources.Add(ResKey ?? res.Name, res);
//                return res;
//            }
//            else
//            {
//                return null;
//            }
//        }
//        public static ShaderResource Load(string vertexPath, string fragmentPath, bool HotReload = true, string ResKey = null)
//        {
//            ShaderResource res =new  ShaderResource(vertexPath, fragmentPath, HotReload);
//            if (res.Load())
//            {
//                resources.Add(res.Name, res);
//                return res;
//            }
//            else
//            {
//                return null;
//            }
//        }
//        public static T Register<T> (T resource, string ResKey) where T: LoadAbleResource
//        {
//            resources.TryAdd(resource.Name, resource);
//            return resource;
//        }
//    }
//    public abstract class FileResource : LoadAbleResource
//    {
//        private FileSystemWatcher fileWatcher;
//        protected abstract bool SupportHotReload { get; }
//        protected virtual bool DoHotReload() { loadEx = new Exception("SupportHotReload set to true, but DoHotReload Method is not overridden"); return false; }

//        protected FileResource(string path)
//        {
//            SetPath(path);
//        }

//        protected FileResource()
//        {
//        }

//        public void SetPath(string path)
//        {
//            filePath = path;
//            Name = Path.GetFileName(filePath);

//        }

//        public string filePath { get; private set; }


       
//    }
//    public abstract class LoadAbleResource: IResource
//    {
//        public string Name { get; set; }
//        public Exception loadEx { get; protected set; }
//        public bool IsLoaded;
//        protected abstract bool LoadResource();
//        public virtual bool Load()
//        {
//            try
//            {
//                if (!LoadResource())
//                {
//                    loadEx = new Exception("no exception but LoadResource returned false");
//                    return false;
//                }
//                return true;
//            }
//            catch (Exception ex)
//            {
//                loadEx = ex;
//                Console.WriteLine(ex.ToString());
//                return false;
//            }
//        }
//    }
//    public interface IResource
//    {
//        public string Name { get; protected set; }
//    }
//    public class MaterialResource: IResource
//    {
//        public Material material;

//        string IResource.Name { get; set; }
//    }
//    public class MeshResource : FileResource
//    {
//        public Mesh mesh;

//        public MeshResource()
//        {
//        }

//        public MeshResource(string path) : base(path)
//        {
//        }

//        protected override bool SupportHotReload => true;

//        protected override bool LoadResource()
//        {

//            try
//            {
//               // mesh =  Mesh.LoadFromFile(filePath);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex);
//                return false;
//            }
//        }
//    }
//    public class ShaderResource : LoadAbleResource
//    {
//        public Shader shader;
//        private bool HotReload;
//        public ShaderResource(string vertexPath, string fragmentPath, bool HotReload = true)
//        {
//            this.shader = new Shader(vertexPath, fragmentPath);
//            this.HotReload = HotReload;
//        }

//        protected override bool LoadResource()
//        {
//            ShaderManager.CompileShader(shader);
//            return true;
//        }
//    }
//    public class TextureResource : FileResource
//    {
//        public Texture texture;

//        public TextureResource()
//        {
//        }

//        public TextureResource(string path) : base(path)
//        {
//        }

//        protected override bool SupportHotReload => true;

//        protected override bool LoadResource()
//        {
            
//            try
//            {
//                texture = new Texture(filePath);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex);
//                return false;
//            }
//        }
//    }
//}
