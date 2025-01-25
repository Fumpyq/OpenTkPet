using ConsoleApp1_Pet.Shaders;
using ConsoleApp1_Pet.Textures;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Materials
{
    public class TextureMaterial : Material
    {
        public Texture mainColor;

        public TextureMaterial(Shader shader, Texture mainColor)
        {
            this.shader = shader;
            this.mainColor = mainColor;
            
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override void Use()
        {
            shader.Use();
            shader.SetTexture(mainColor);
           // mainColor.Use();
           //UseTexture
           // UseTexture(1, mainColor); // Все что дальше 1-й текстуры, требует порядковый, номер не ID !
           // int TextureLoc = GL.GetUniformLocation(this.shader.Id, "texture0");
           //for(int i = -1; i < 16;i++)
           //    UseTexture(i, mainColor);

            //UseTexture(1, mainColor);

            //UseTexture(1,mainColor);
            //UseTexture(2,mainColor);
            //UseTexture(3,mainColor);
            //UseTexture(4,mainColor);
            //UseTexture(5,mainColor);

        }
    }
}
