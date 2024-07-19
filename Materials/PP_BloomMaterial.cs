using ConsoleApp1_Pet.Render;
using ConsoleApp1_Pet.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Materials
{
    public class PP_BloomMaterial:Material
    {
       
        public PP_BloomMaterial()
        {

            shader = ShaderManager.CompileShader(@"Shaders\Code\DepthTextureDisplay_vert.glsl", @"Shaders\Code\BloomFrag.glsl");
        }

        public override void Use()
        {
            shader.Use();
           
            
            shader.SetUniform("bloomThreshold",0.1f);
            shader.SetUniform("bloomIntensity", 1f);

            shader.SetTexture(Shader.ScreenTexture, Game.instance.prePostProcessingBuffer);
        }
    }
}
