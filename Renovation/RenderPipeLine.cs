using ConsoleApp1_Pet.Materials;
using ConsoleApp1_Pet.Textures;
using ConsoleApp1_Pet.Новая_папка;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Renovation
{
    public class RenderPipeLine
    {
        
    }

    public class MainPipeLine
    {
        public void Main()
        {
            PreloadRessources();
        }
        public void PreloadRessources()
        {
            var rock = Resources.Load<TextureResource>("\\Textures\\Textures\\greenishRockTexture.jpg");
            var gras = Resources.Load<TextureResource>("\\Textures\\Textures\\silk25-square-grass.jpg");
            var rock2= Resources.Load<TextureResource>("\\Textures\\Textures\\square-rock.png");

            var test=  Resources.Load<MeshResource>("F:\\Users\\malam\\Documents\\BLENDERS\\Lightv3.blend");

            var shd = Resources.Load(@"Shaders\Code\Basic3d_vert.glsl", @"Shaders\Code\SimpleTexture_frag.glsl");



            var mat = new TextureMaterial(shd.shader, rock2.texture);
            var RockMat = new TextureMaterial(shd.shader, rock.texture);
        }
        

    }

}
