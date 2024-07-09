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
            new Vector3(-0.5f, -0.5f, -0.5f), // 4
            new Vector3(0.5f, -0.5f, -0.5f), // 5
            new Vector3(0.5f, 0.5f, -0.5f), // 6
            new Vector3(-0.5f, 0.5f, -0.5f), // 7
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
            1, 5, 6,
            6, 2, 1,

            // Left face
            0, 3, 7,
            7, 4, 0,

            // Top face
            3, 2, 6,
            6, 7, 3,

            // Bottom face
            0, 4, 5,
            5, 1, 0
        };
    
        public static Mesh Generate()
        {
            Vertex[] res = new Vertex[vertices.Length];
            for(int i = 0; i < vertices.Length; i++)
            {
                res[i] = new Vertex(vertices[i], uvs[i]);
            }
            var m = new Mesh(res, tris);
            m.CreateBuffers();
            return m;
        }
    }
}
