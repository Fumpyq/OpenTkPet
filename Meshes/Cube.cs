using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Meshes
{
    public class Cube
    {
        static readonly Vector3[] vertices = new Vector3[]
 {
            // Front face
            new Vector3(-0.5f, -0.5f, 0.5f), // 0
            new Vector3(0.5f, -0.5f, 0.5f), // 1
            new Vector3(0.5f, 0.5f, 0.5f), // 2
            new Vector3(-0.5f, 0.5f, 0.5f), // 3

            // Back face
            new Vector3(-0.5f, -0.5f, -0.5f),   // 4
            new Vector3(0.5f, -0.5f, -0.5f),    // 5
            new Vector3(0.5f, 0.5f, -0.5f),     // 6
            new Vector3(-0.5f, 0.5f, -0.5f),    // 7

            // Right face
            new Vector3(0.5f, -0.5f, 0.5f),     // 8 (shared with Front)
            new Vector3(0.5f, -0.5f, -0.5f),    // 9 (shared with Back)
            new Vector3(0.5f, 0.5f, -0.5f),     // 10 (shared with Back)
            new Vector3(0.5f, 0.5f, 0.5f),      // 11 (shared with Front)

            // Left face
            new Vector3(-0.5f, -0.5f, 0.5f),    // 12 (shared with Front)
            new Vector3(-0.5f, -0.5f, -0.5f),   // 13 (shared with Back)
            new Vector3(-0.5f, 0.5f, -0.5f),    // 14 (shared with Back)
            new Vector3(-0.5f, 0.5f, 0.5f),     // 15 (shared with Front)

            // Top face
            new Vector3(-0.5f, 0.5f, 0.5f), // 16 (shared with Front)
            new Vector3(0.5f, 0.5f, 0.5f),  // 17 (shared with Front)
            new Vector3(0.5f, 0.5f, -0.5f), // 18 (shared with Back)
            new Vector3(-0.5f, 0.5f, -0.5f), // 19 (shared with Back)

            // Bottom face
            new Vector3(-0.5f, -0.5f, 0.5f),    // 20 (shared with Front)
            new Vector3(0.5f, -0.5f, 0.5f),     // 21 (shared with Front)
            new Vector3(0.5f, -0.5f, -0.5f),    // 22 (shared with Back)
            new Vector3(-0.5f, -0.5f, -0.5f),   // 23 (shared with Back)
        };

        // UVs (texture coordinates)
        static readonly Vector2[] uvs = new Vector2[]
{
            // Front face
            new Vector2(0f, 0f), // 0
            new Vector2(1f, 0f), // 1
            new Vector2(1f, 1f), // 2
            new Vector2(0f, 1f), // 3

            // Back face
            new Vector2(0f, 0f), // 4
            new Vector2(1f, 0f), // 5
            new Vector2(1f, 1f), // 6
            new Vector2(0f, 1f), // 7

            // Right face
            new Vector2(0f, 0f), // 8
            new Vector2(1f, 0f), // 9
            new Vector2(1f, 1f), // 10
            new Vector2(0f, 1f), // 11

            // Left face
            new Vector2(0f, 0f), // 12
            new Vector2(1f, 0f), // 13
            new Vector2(1f, 1f), // 14
            new Vector2(0f, 1f), // 15

            // Top face
            new Vector2(0f, 0f), // 16
            new Vector2(1f, 0f), // 17
            new Vector2(1f, 1f), // 18
            new Vector2(0f, 1f), // 19

            // Bottom face
            new Vector2(0f, 0f), // 20
            new Vector2(1f, 0f), // 21
            new Vector2(1f, 1f), // 22
            new Vector2(0f, 1f), // 23
        };


        // Triangles (indices)
        static readonly uint[] tris = new uint[]
{
            // Front face
            0, 1, 2,
            2, 3, 0,

            // Back face
            4, 5, 6,
            6, 7, 4,

            // Right face
            8, 9, 10,
            10, 11, 8,

            // Left face
            12, 13, 14,
            14, 15, 12,

            // Top face
            16, 17, 18,
            18, 19, 16,

            // Bottom face
            20, 21, 22,
            22, 23, 20
        };

        public static Mesh Generate(bool GenerateBuffers = true)
        {
            Vertex[] res = new Vertex[vertices.Length];
            for(int i = 0; i < vertices.Length; i++)
            {
                res[i] = new Vertex(vertices[i], uvs[i]);
            }
            var m = new Mesh(res, tris);
            if(GenerateBuffers) m.CreateBuffers();
            return m;
        }
        public static Mesh Generate(Vector3 pos, uint trisOffest, bool GenerateBuffers = true)
        {
            Vector3[] vertices = new Vector3[]
 {
            // Front face
            pos+new Vector3(-0.5f, -0.5f, 0.5f), // 0
            pos+new Vector3(0.5f, -0.5f, 0.5f), // 1
            pos+new Vector3(0.5f, 0.5f, 0.5f), // 2
            pos+new Vector3(-0.5f, 0.5f, 0.5f), // 3

            // Back face
            pos+new Vector3(-0.5f, -0.5f, -0.5f), // 4
            pos+new Vector3(0.5f, -0.5f, -0.5f), // 5
            pos+new Vector3(0.5f, 0.5f, -0.5f), // 6
            pos+new Vector3(-0.5f, 0.5f, -0.5f), // 7

            // Right face
            pos+new Vector3(0.5f, -0.5f, 0.5f), // 8 (shared with Front)
            pos+new Vector3(0.5f, -0.5f, -0.5f), // 9 (shared with Back)
            pos+new Vector3(0.5f, 0.5f, -0.5f), // 10 (shared with Back)
            pos+new Vector3(0.5f, 0.5f, 0.5f), // 11 (shared with Front)

            // Left face
            pos+new Vector3(-0.5f, -0.5f, 0.5f), // 12 (shared with Front)
            pos+new Vector3(-0.5f, -0.5f, -0.5f), // 13 (shared with Back)
            pos+new Vector3(-0.5f, 0.5f, -0.5f), // 14 (shared with Back)
            pos+new Vector3(-0.5f, 0.5f, 0.5f), // 15 (shared with Front)

            // Top face
            pos+new Vector3(-0.5f, 0.5f, 0.5f), // 16 (shared with Front)
            pos+new Vector3(0.5f, 0.5f, 0.5f), // 17 (shared with Front)
            pos+new Vector3(0.5f, 0.5f, -0.5f), // 18 (shared with Back)
            pos+new Vector3(-0.5f, 0.5f, -0.5f), // 19 (shared with Back)

            // Bottom face
            pos+new Vector3(-0.5f, -0.5f, 0.5f), // 20 (shared with Front)
            pos+new Vector3(0.5f, -0.5f, 0.5f), // 21 (shared with Front)
            pos+new Vector3(0.5f, -0.5f, -0.5f), // 22 (shared with Back)
            pos+new Vector3(-0.5f, -0.5f, -0.5f), // 23 (shared with Back)
        };
            uint[] tris = new uint[]
{
            // Front face
            trisOffest+0, trisOffest+1, trisOffest+2,
            trisOffest+2, trisOffest+3, trisOffest+0,

            // Back face
            trisOffest+4, trisOffest+5, trisOffest+6,
            trisOffest+6, trisOffest+7, trisOffest+4,

            // Right face
            trisOffest+8, trisOffest+9, trisOffest+10,
            trisOffest+10, trisOffest+11, trisOffest+8,

            // Left face
            trisOffest+12, trisOffest+13, trisOffest+14,
            trisOffest+14, trisOffest+15, trisOffest+12,

            // Top face
            trisOffest+16, trisOffest+17, trisOffest+18,
            trisOffest+18, trisOffest+19, trisOffest+16,

            // Bottom face
            trisOffest+20, trisOffest+21, trisOffest+22,
            trisOffest+22, trisOffest+23, trisOffest+20
       };
            Vertex[] res = new Vertex[vertices.Length];



            for (int i = 0; i < vertices.Length; i++)
            {
                res[i] = new Vertex(vertices[i], uvs[i]);
            }
            var m = new Mesh(res, tris);
            if (GenerateBuffers) m.CreateBuffers();
            return m;
        }
    }
}
