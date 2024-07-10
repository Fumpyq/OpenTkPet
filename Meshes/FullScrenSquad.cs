using ConsoleApp1_Pet.Materials;
using ConsoleApp1_Pet.Shaders;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Meshes
{
    public static class FullScreenSquad
    {
        private static int _vao;
        private static int _vbo;

        public static void Initialize()
        {
            // ... other initialization ...

            // Create a VAO
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            // Create a VBO
            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            // Vertex data for the quad (two triangles)
            float[] vertices = {
        -1.0f, -1.0f, 0.0f, 0.0f, 0.0f,
        1.0f, -1.0f, 0.0f, 1.0f, 0.0f,
        1.0f, 1.0f, 0.0f, 1.0f, 1.0f,
        -1.0f, 1.0f, 0.0f, 0.0f, 1.0f
    };

            // Upload the vertex data to the VBO
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Set up vertex attribute pointers
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // Unbind VBO and VAO
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }
        public static void Render(TextureMaterial mat)
        {
            mat.Use();
            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4); // Or PrimitiveType.TriangleStrip
           // GL.BindVertexArray(0);
        }
    }
}
