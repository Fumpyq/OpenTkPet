using ConsoleApp1_Pet.Materials;
using ConsoleApp1_Pet.Meshes;
using ConsoleApp1_Pet.Shaders;
using ConsoleApp1_Pet.Textures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Новая_папка.Resources
{
    //public abstract class ResourceLoaderBase<T>
    //{
    //    public abstract T Load(object paramsObj);
    //    public abstract void OnUnhandledException(object paramsObj);
    //}
    public interface IResource
    {
        public string Name { get; protected set; }
    }
    public abstract class FileResource : LoadAbleResource
    {
        private FileSystemWatcher fileWatcher;
        protected abstract bool SupportHotReload { get; }
        protected virtual bool DoHotReload() { loadEx = new Exception("SupportHotReload set to true, but DoHotReload Method is not overridden"); return false; }

        protected FileResource(string path)
        {
            SetPath(path);
        }

        protected FileResource()
        {
        }

        public void SetPath(string path)
        {
            filePath = path;
            Name = Path.GetFileName(filePath);

        }

        public string filePath { get; private set; }



    }
    public abstract class LoadAbleResource : IResource
    {
        public string Name { get; set; }
        public Exception loadEx { get; protected set; }
        public bool IsLoaded;
        protected abstract bool LoadResource();
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
    }

    public class MaterialResource : LoadAbleResource
    {
        public Material material;

        public MaterialResource(object materialParams)
        {
            
        }

        protected override bool LoadResource()
        {
            
        }
    }
    
    public class MeshResource : FileResource
    {
        public Mesh mesh;

        public MeshResource()
        {
        }

        public MeshResource(string path) : base(path)
        {
        }

        protected override bool SupportHotReload => true;

        protected override bool LoadResource()
        {

            try
            {
                // mesh =  Mesh.LoadFromFile(filePath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }
    }
    public record ShaderLoadParams(string VertexPath, string FragmentPath);
    public record MaterialLoadParams(Type materialType,);
    public class ShaderResource : LoadAbleResource
    {
        public Shader shader;
        private bool HotReload;
        public ShaderResource(ShaderLoadParams shdParams)
        {
            this.shader = new Shader(shdParams.VertexPath, shdParams.FragmentPath);
            this.HotReload = HotReload;
        }

        protected override bool LoadResource()
        {
            ShaderManager.CompileShader(shader);
            return true;
        }
    }
    public class TextureResource : FileResource
    {
        public Texture texture;

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
    public class ResourceManager
    {
        private readonly Dictionary<string, IResource> _loadedResources = new();
        private readonly Dictionary<Type, Func<object, IResource>> _loaders = new();
        private FileSystemWatcher? _fileWatcher; // for hot reload
        private string _manifestPath = null!;
        private Manifest _manifest = null!;


        public ResourceManager(Func<Exception, object> exceptionHandler)
        {
            
        }
        public ResourceManager(Func<Exception, object> exceptionHandler, string manifestPath)
        {
            _manifestPath = manifestPath;
            _fileWatcher = CreateWatcher();
        }

        private FileSystemWatcher? CreateWatcher()
        {
            if (string.IsNullOrEmpty(_manifestPath) || !File.Exists(_manifestPath))
                return null;

            string directory = Path.GetDirectoryName(_manifestPath) ?? ".";
            string fileName = Path.GetFileName(_manifestPath);

            FileSystemWatcher fileWatcher = new(directory, fileName);
            fileWatcher.Changed += OnManifestChanged;
            fileWatcher.EnableRaisingEvents = true;
            return fileWatcher;
        }

        private void OnManifestChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
                LoadManifest(_manifestPath);
        }


        public void RegisterLoader<TResource>(Func<object, TResource> loader) where TResource : IResource
        {
            _loaders[typeof(TResource)] = (paramsObj) => loader(paramsObj);
        }

        public ResourceManager LoadManifest(string manifestPath)
        {
            _manifestPath = manifestPath;
            _manifest = LoadManifestFromJson(manifestPath);
            LoadResourcesFromManifest(_manifest);

            // Ensure the watcher is in place (in case it wasn't before)
            if (_fileWatcher == null)
            {
                var watcher = CreateWatcher();
                if (watcher != null)
                {
                    Console.WriteLine($"Hot Reload for manifest enabled");
                    _fileWatcher = watcher;
                }
            }
            return this;
        }

        private Manifest LoadManifestFromJson(string manifestPath)
        {
            if (string.IsNullOrEmpty(manifestPath) || !File.Exists(manifestPath))
                throw new FileNotFoundException($"Manifest file not found at path: {manifestPath}");

            string json = File.ReadAllText(manifestPath);
            return JsonConvert.DeserializeObject<Manifest>(json) ?? throw new InvalidOperationException($"Failed to Deserialize Manifest: {manifestPath}");
        }


        private void LoadResourcesFromManifest(Manifest manifest)
        {
            var allEntries = manifest.GetAllEntries();
            foreach (var entry in allEntries)
            {
                var resourceKey = entry.Value.Key ?? entry.Key; //Use the key overwrite if defined or the initial key

                try
                {
                    LoadEntry(resourceKey,entry.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString() );
                }

            }
        }

        private void LoadEntry(string key,ManifestEntry entry)
        {
            switch (entry)
            {
                case TextureManifestEntry T:
                        LoadResource<TextureResource>(key, T.Path); 
                        break;
                case ShaderManifestEntry S:
                    LoadResource<ShaderResource>(key, new ShaderLoadParams(S.VertexPath,S.FragmentPath));
                    break;
                case MaterialManifestEntry M: break;
            }
        }


        private TResource LoadResource<TResource>(string key, object loadParams) where TResource : IResource
        {
                if (_loaders.TryGetValue(typeof(TResource), out var loader))
                {
                    TResource loadedResource = (TResource)loader(loadParams);
                    if (loadedResource != null)
                        _loadedResources[key] = loadedResource;
                    
                    return loadedResource;
                }
                else
                {
                    throw new KeyNotFoundException($"No loader registered for type {typeof(TResource).Name}");
                }
        }


        public TResource GetResource<TResource>(string key) where TResource : IResource
        {
            if (_loadedResources.TryGetValue(key, out var resource) && resource is TResource typedResource)
                return typedResource;

            return default!;
        }

        public bool TryGetResource<TResource>(string key, object loadParams, TResource res) where TResource : IResource
        {
            if (!_loadedResources.TryGetValue(key,out var loaded))
            {
                if(loaded is TResource typedResource)
                {
                    res = typedResource;
                    return true;
                }
                else
                {
                    throw new InvalidOperationException($"Resource with given key ({key}) is already loaded with different type(LOADED: '{loaded.GetType()}' REQUESTED: '{typeof(TResource)}')");
                }

            }
            else
            {
                res = default(TResource);
                return false ;
            }
                

          
        }


        public bool ResourceExists(string key)
        {
            return _loadedResources.ContainsKey(key);
        }

        public bool IsResourceLoaded(string key)
        {
            return _loadedResources.ContainsKey(key);
        }

        public bool IsResourceLoaded<TResource>(string key) where TResource : IResource
        {
            if (_loadedResources.TryGetValue(key, out var resource))
                return resource is TResource;
            return false;
        }
    }
}
