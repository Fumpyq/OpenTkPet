﻿using ConsoleApp1_Pet.Shaders;
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
        public static long IdCounter;
        public long id { get; private set; }
        public event Action<Material> OnUpdate;
        public Shader shader;

        protected Material()
        {
            id  = Interlocked.Increment(ref IdCounter);
        }

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
