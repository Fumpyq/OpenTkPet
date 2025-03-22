using BepuPhysics.Collidables;
using ConsoleApp1_Pet.Materials;
using ConsoleApp1_Pet.Shaders;
using ConsoleApp1_Pet.Textures;
using OpenTK.Graphics.ES20;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Architecture
{
    public abstract class Resource : IDisposable
    {
        public string Name { get; }
        public bool IsLoaded { get; protected set; }
        public event Action<Resource> OnReloaded;

        protected Resource(string name) => Name = name;

        public abstract void Load();
        public abstract void Reload();
        public abstract void Dispose();

        protected void NotifyReloaded() => OnReloaded?.Invoke(this);
    }

    public sealed class ResourceManager : IDisposable
    {
        public readonly ConcurrentDictionary<string, Resource> _resources = new();
        private readonly ConcurrentDictionary<string, List<string>> _fileDependencies = new();
        private readonly ConcurrentDictionary<string, List<string>> _resourceDependencies = new();
        private readonly FileSystemWatcher _fileWatcher;
        private readonly Timer _reloadTimer;
        private readonly ConcurrentQueue<string> _changedFiles = new();
        private readonly string _resourceRoot;
        private readonly object _lock = new();

        private const string ShaderPreffix = "Shader::";
        private const string TexturePreffix = "Texture::";
        private const string MaterialPreffix = "Mat::";


        public ResourceManager(string resourceRoot)
        {
            _resourceRoot = Path.GetFullPath(resourceRoot);
            Directory.CreateDirectory(_resourceRoot);

            _fileWatcher = new FileSystemWatcher(_resourceRoot)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                EnableRaisingEvents = true
            };

            _fileWatcher.Changed += OnFileChanged;
            _fileWatcher.Created += OnFileChanged;
            _fileWatcher.Deleted += OnFileChanged;
            _fileWatcher.Renamed += OnFileRenamed;

            _reloadTimer = new Timer(_ => ProcessChanges(), null, 500, 500);
        }
        public MaterialResource RegisterMaterial(string name, Material mat)
        {
           return RegisterResource<MaterialResource>(MaterialPreffix+name, new MaterialResource(name,mat));
        }
        public TextureResource RegisterTexture(string name, Texture tex)
        {
            return RegisterResource<TextureResource>(TexturePreffix + name, new TextureResource(name, tex));
        }
        private T RegisterResource <T>(string name,T res) where T: Resource
        {
            return (T)_resources.GetOrAdd(name, key =>
            {
                return res;
            });
        }
        public T Get<T>(string name) where T : Resource => (T)_resources[name];
        public TextureResource GetTexture(string name) => (TextureResource)_resources[TexturePreffix+name];
        public MaterialResource GetMaterial(string name) => (MaterialResource)_resources[MaterialPreffix+name];
        public bool TryGet<T>(string name, out T resource) where T : Resource
        {
            if(_resources.TryGetValue(name, out var _res)){
                if(_res is T right)
                {
                    resource = right;
                    return true;
                }
                else
                {
                    throw new InvalidOperationException($"Requested resource {name} expected {typeof(T).FullName} actual {_res.GetType().FullName}");
                }
            }
            else
            {
                resource = null;
                return false;
            }

        }
        public TextureResource CreateTexture(string name, string path)
        {

            path = FullPath(path);
            return CreateResource<TextureResource>(TexturePreffix + name,
                () => new TextureResource(path,name),
                files: new[] { path });
        }
        public TextureResource CreateTexture( string path)
        {
     
            return CreateTexture(Path.GetFileNameWithoutExtension(path),path);
        }
        public ShaderResource CreateShader(string name, string vertPath, string fragPath)
        {
            var fullVert = FullPath(vertPath);
            var fullFrag = FullPath(fragPath);

            return CreateResource<ShaderResource>(ShaderPreffix + name,
                () => new ShaderResource(name, fullVert, fullFrag),
                files: new[] { fullVert, fullFrag });
        }

        public ShaderResource CreateShader(string name, string baseName)
        {
            var basePath = FullPath(baseName);
            return CreateShader(name, $"{basePath}.vert", $"{basePath}.frag");
        }

        public ModelResource CreateModel(string name, string meshPath, params string[] materialNames)
        {
            var fullMeshPath = FullPath(meshPath);
            var mesh = CreateResource<MeshResource>(
                $"Mesh:{fullMeshPath}",
                () => MeshResource.LoadFromFile(fullMeshPath),
                files: new[] { fullMeshPath });

            var materials = materialNames.Select(n => Get<MaterialResource>(n)).ToArray();

            return CreateResource<ModelResource>(name,
                () => new ModelResource(name, mesh, materials),
                dependencies: materialNames.Prepend(mesh.Name));
        }

        //public MaterialResource CreateMaterial(string name, string shaderName)
        //{
        //    return CreateResource<MaterialResource>(name,
        //        () => new MaterialResource(name, Get<ShaderResource>(shaderName)),
        //        dependencies: new[] { shaderName });
        //}

        private T CreateResource<T>(
            string name,
            Func<T> factory,
            IEnumerable<string>? files = null,
            IEnumerable<string>? dependencies = null) where T : Resource
        {
            return (T)_resources.GetOrAdd(name, key =>
            {
                var resource = factory();
                resource.OnReloaded += OnResourceReloaded;

                foreach (var file in files ?? Enumerable.Empty<string>())
                {
                    _fileDependencies.AddOrUpdate(file,
                        new List<string> { key },
                        (_, list) => { list.Add(key); return list; });
                }

                foreach (var dep in dependencies ?? Enumerable.Empty<string>())
                {
                    _resourceDependencies.AddOrUpdate(dep,
                        new List<string> { key },
                        (_, list) => { list.Add(key); return list; });
                }

                resource.Load();
                return resource;
            });
        }

        private void OnResourceReloaded(Resource resource)
        {
            if (_resourceDependencies.TryGetValue(resource.Name, out var dependents))
            {
                foreach (var dependent in dependents)
                {
                    if (_resources.TryGetValue(dependent, out var dependentResource))
                    {
                        dependentResource.Reload();
                    }
                }
            }
        }

        private void ProcessChanges()
        {
            var processed = new HashSet<string>();
            while (_changedFiles.TryDequeue(out var file))
            {
                if (!processed.Add(file)) continue;

                if (_fileDependencies.TryGetValue(file, out var resources))
                {
                    foreach (var resourceName in resources)
                    {
                        if (_resources.TryGetValue(resourceName, out var resource))
                        {
                            resource.Reload();
                        }
                    }
                }
            }
        }

        private string FullPath(string path) => Path.Combine(_resourceRoot, path);

        private void OnFileChanged(object sender, FileSystemEventArgs e) =>
            _changedFiles.Enqueue(e.FullPath);

        private void OnFileRenamed(object sender, RenamedEventArgs e) =>
            _changedFiles.Enqueue(e.FullPath);

        public void Dispose()
        {
            _fileWatcher?.Dispose();
            _reloadTimer?.Dispose();
            foreach (var resource in _resources.Values)
            {
                resource.Dispose();
            }
        }
    }

    // Example Resource Implementations
    public sealed class ShaderResource : Resource
    {
        public string VertexPath { get; }
        public string FragmentPath { get; }
        public string VertexCode { get; private set; }
        public string FragmentCode { get; private set; }
        public Shader shader;
        public static implicit operator Shader(ShaderResource sr) => sr.shader;
        public ShaderResource(string name, string vertPath, string fragPath) : base(name)
        {
            VertexPath = vertPath;
            FragmentPath = fragPath;
        }

        public override void Load()
        {
            VertexCode = File.ReadAllText(VertexPath);
            FragmentCode = File.ReadAllText(FragmentPath);
            if (shader == null)
            {
                shader = new Shader();
            }
            IsLoaded = true;
            ShaderManager.RecompileShader(this);
         
        }

        public override void Reload()
        {
            Dispose();
            Load();
            NotifyReloaded();
        }

        public override void Dispose()
        {
            VertexCode = FragmentCode = null;
            IsLoaded = false;
        }
    }

    public sealed class MeshResource : Resource
    {
        public static MeshResource LoadFromFile(string path) => new(path);

        public MeshData Data { get; private set; }

        private MeshResource(string path) : base(Path.GetFileName(path)) { }

        public override void Load()
        {
            Data = ParseMesh(File.ReadAllBytes(Name));
            IsLoaded = true;
        }

        private MeshData ParseMesh(byte[] data) => new(); // Implementation omitted

        public override void Reload()
        {
            Dispose();
            Load();
            NotifyReloaded();
        }

        public override void Dispose() => Data = null;
    }

    public sealed class TextureResource : Resource
    {
        public string TexturePath;
        public Texture texture;
        public static implicit operator Texture(TextureResource tr) => tr.texture;
        public TextureResource(string filePath) : base(Path.GetFileNameWithoutExtension(filePath))
        {
            TexturePath= filePath;
        }
        public TextureResource(string filePath,string name) : base(name)
        {
            TexturePath = filePath;
        }
        public TextureResource(string name, Texture texture) : base(name)
        {
            this.texture = texture;
        }
        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override void Load()
        {
            if (texture == null)
            {
                texture = TextureLoaderExtensions.CreateTextureFromFile(TexturePath, TexturePreset.Auto);
            }
            else
            {

                texture.LoadFromFile(TexturePath);
            }
        }

        public override void Reload()
        {
            Load();
        }
    }

    public sealed class MaterialResource : Resource
    {
        public Material mat;
       // public ShaderResource Shader { get; }

        //public MaterialResource(string name, ShaderResource shader) : base(name) => Shader = shader;
        public MaterialResource(string name, Material material) : base(name) => mat = material;
        public static implicit operator Material(MaterialResource sr) => sr.mat;
        public override void Load() => IsLoaded = true; // Materials are runtime-constructed
        public override void Reload() => NotifyReloaded(); // Propagate changes to dependent models
        public override void Dispose() { }
    }

    public sealed class ModelResource : Resource
    {
        public MeshResource Mesh { get; }
        public MaterialResource[] Materials { get; }

        public ModelResource(string name, MeshResource mesh, MaterialResource[] materials)
            : base(name)
        {
            Mesh = mesh;
            Materials = materials;
        }

        public override void Load() => IsLoaded = true;
        public override void Reload() => NotifyReloaded(); // Let render system handle updates
        public override void Dispose() { }
    }

    public class MeshData { } // Placeholder
}
