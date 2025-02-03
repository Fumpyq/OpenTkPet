using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Новая_папка.Resources
{
    //Base class for all manifest entries
    public abstract class ManifestEntry
    {
        [JsonProperty("key")]
        public string Key { get; set; } = null!; // Key override.
        [JsonProperty("hotReload")]
        public bool HotReload { get; set; }
    }

    //Base class for resources that require a link
    //public abstract class ManifestLinkEntry : ManifestEntry
    //{
    //    [JsonProperty("links")]
    //    public Dictionary<string, string> Links { get; set; } = new();
    //}


    public class TextureManifestEntry : ManifestEntry
    {
        [JsonProperty("path")]
        public string Path { get; set; } = null!;
    }

    public class ShaderManifestEntry : ManifestEntry
    {
        [JsonProperty("vertexPath")]
        public string VertexPath { get; set; } = null!;
        [JsonProperty("fragmentPath")]
        public string FragmentPath { get; set; } = null!;
    }

    public class MaterialManifestEntry : ManifestEntry
    {
        [JsonProperty("properties")]
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    public class Manifest
    {
        [JsonProperty("textures")]
        public Dictionary<string, TextureManifestEntry> Textures { get; set; } = new();
        [JsonProperty("shaders")]
        public Dictionary<string, ShaderManifestEntry> Shaders { get; set; } = new();
        [JsonProperty("materials")]
        public Dictionary<string, MaterialManifestEntry> Materials { get; set; } = new();

        public Dictionary<string, ManifestEntry> GetAllEntries()
        {
            var allEntries = new Dictionary<string, ManifestEntry>();
            foreach (var entry in Textures)
                allEntries[entry.Key] = entry.Value;
            foreach (var entry in Shaders)
                allEntries[entry.Key] = entry.Value;
            foreach (var entry in Materials)
                allEntries[entry.Key] = entry.Value;
            return allEntries;
        }
    }

}
