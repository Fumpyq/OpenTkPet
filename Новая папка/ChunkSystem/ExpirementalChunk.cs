using BepuPhysics.Collidables;
using ConsoleApp1_Pet.Render;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Новая_папка.ChunkSystem
{
    public class ExpirementalChunk
    {
         
        public static void Run()
        {
            int size = 2;
            for (int x = -size; x <= size; x++)
            {
                for (int z = -size; z <= size; z++)
                {
                    var c = MainGameWindow.instance.cg.GenerateChunk(new OpenTK.Mathematics.Vector2i(x, z));
                    var mesh = ChunkMeshGen.GenerateMesh(c);
                    mesh.CreateBuffers();
                    var resMat = MainGameWindow.instance.RockMaterial;
                    var rr3 = new RenderComponent(mesh, resMat);
                    rr3.WithSelfGamobject();
                    rr3.gameObject.transform.position = new Vector3(x,0,z)*Chunk.Width;
                  //  MainGameWindow.instance.renderer.AddToRender(rr3);
                }
            }
           
        }
    }
}
