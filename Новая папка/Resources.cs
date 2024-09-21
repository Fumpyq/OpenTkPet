using ConsoleApp1_Pet.Textures;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Новая_папка
{
    public static class Resources
    {
        public static Dictionary<string, Resource> resources = new Dictionary<string, Resource>();

        public static T Load<T>(string FilePath) where T: Resource,new ()
        {
            var res = new T();
            res.SetPath(FilePath);
            if (res.Load())
            {
                resources.Add(res.Name, res);
                return res;
            }
            else
            {
                return null;
            }
        }
    }

    public abstract class Resource
    {
        private FileSystemWatcher fileWatcher;
        protected abstract bool SupportHotReload { get; }
        protected virtual bool DoHotReload() { loadEx = new Exception("SupportHotReload set to true, but DoHotReload Method is not overridden"); return false;  }

        protected Resource(string path)
        {
            SetPath(path);
        }

        protected Resource()
        {
        }

        public void SetPath(string path)
        {
            filePath = path;
            Name = Path.GetFileName(filePath);

        }
        public string Name { get; private set; } 
        public string filePath { get; private set; }
        public Exception loadEx { get; private set; }
        public virtual bool Load()
        {
            try
            {
                if (!LoadResource())
                {
                    loadEx = new Exception("no exception but LoadResource returned false");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                loadEx = ex;
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
        protected abstract bool LoadResource();
    }
    public class TextureResource : Resource
    {
        public Texture texture;

        public TextureResource()
        {
        }

        public TextureResource(string path) : base(path)
        {
        }

        protected override bool SupportHotReload => true;

        protected override bool LoadResource()
        {
            
            try
            {
                texture = new Texture(filePath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }
    }
}
