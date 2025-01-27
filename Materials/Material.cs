using ConsoleApp1_Pet.Shaders;
using ConsoleApp1_Pet.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Materials
{
    public abstract class Material
    {
        public event Action<Material> OnUpdate;
        public Shader shader; [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public abstract void Use();
        public Material Clone()=> (Material)this.MemberwiseClone();
        /// <summary>
        /// Shader_Old will be used
        /// </summary>
        /// <param name="t"></param>
        public void UseTexture(string name,Texture t)
        {
            
            shader.SetTexture(name,t);//change to index of texture
        }
    }
}
