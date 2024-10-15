using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Modding
{
    public struct ResourceDefinition
    {
        public string? type;
        /// <summary>
        /// if not specified use path instead
        /// </summary>
        public string? name;
        public string path;
        public string? icon;
        public bool isHidden;

        public ResourceDefinition(string path) : this()
        {
            this.path = path ?? throw new ArgumentNullException(nameof(path));
        }
    }
    public abstract class Mod
    {
        public abstract string Name { get; }
        public abstract string Version { get; }
        public abstract string Description { get; }
        public abstract string Title { get; }
        public abstract string Author { get; }
        public abstract string Target_Game_Version { get; }
        public abstract string[]? Dependecies { get; }
        public abstract List<ResourceDefinition> Data { get; }
        public  virtual void Load() { }
        public  virtual void OnModLoaded() { }
        public  virtual void OnAllModsLoaded() { }
    }


    public class CoreMod : Mod
    {
        public override string Name { get => "Core"; }
        public override string Version { get => "0.0.0.0.0.0.0.1";  }
        public override string Description { get => "Core component of game"; }
        public override string Title { get => "Core"; }
        public override string Author { get => "Unknown";  }
        public override string Target_Game_Version { get => ""; }
        public override string[]? Dependecies { get => null; }
        public override List<ResourceDefinition> Data { get => new List<ResourceDefinition>()
        {
            new ResourceDefinition("")
        };
        }
    }
}
