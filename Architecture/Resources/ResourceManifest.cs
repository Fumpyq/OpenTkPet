using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Architecture.Resources
{
    public class ResourceManifest
    {
        public List<ResourceEntry> Resources { get; set; }
    }

    public class ResourceEntry
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Shader { get; set; }
        public string Mesh { get; set; }
        public List<string> Materials { get; set; }

        // Texture-specific
        public string Role { get; set; }
        public TextureFormat Format { get; set; }

        // Material-specific
        public MaterialProperties Properties { get; set; }

        // Mesh-specific
        public MeshImportSettings ImportSettings { get; set; }
    }

    public class TextureFormat
    {
        public bool SRgb { get; set; }
        public bool GenerateMipmaps { get; set; }
    }

    public class MaterialProperties
    {
        public Dictionary<string, string> Textures { get; set; }
        public Dictionary<string, Color4> Colors { get; set; }
        public Dictionary<string, float> Floats { get; set; }
    }

    public class MeshImportSettings
    {
        public bool CalculateNormals { get; set; }
        public bool GenerateTangents { get; set; }
    }
}
