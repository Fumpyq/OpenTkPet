using ConsoleApp1_Pet.Shaders;
using ConsoleApp1_Pet.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Materials
{
    public abstract class Material
    {

        public Shader shader;
        public abstract void Use();
        public Material Clone()=> (Material)this.MemberwiseClone();
        /// <summary>
        /// Shader will be used
        /// </summary>
        /// <param name="t"></param>
        public void UseTexture(int layout,Texture t)
        {
            shader.SetTexture(layout, 0);//change to index of texture
        }
    }
}
