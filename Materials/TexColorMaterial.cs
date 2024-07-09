﻿using ConsoleApp1_Pet.Shaders;
using ConsoleApp1_Pet.Textures;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Materials
{
    public class TexColorMaterial : Material
    {
        public Texture mainColor;

        public TexColorMaterial(Shader3d shader, Texture mainColor)
        {
            this.shader = shader;
            this.mainColor = mainColor;
            
        }

        public override void Use()
        {
            shader.Use();
            UseTexture(0,mainColor);
            
        }
    }
}
