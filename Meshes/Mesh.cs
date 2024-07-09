using ConsoleApp1_Pet.Materials;
using OpenTK.Graphics.Egl;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Meshes
{
    public class Mesh
    {
        public Vertex[] vertices;
        public uint[] triangles;

        public int VAO, VBO, EBO;
        private bool isBuffersFilled;
        public Mesh()
        {
        }

        public Mesh(Vertex[] vertices, uint[] triangles)
        {
            this.vertices = vertices;
            this.triangles = triangles;
        }

        public void CreateBuffers()
        {
            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();
            EBO = GL.GenBuffer();

            var v3Size = Unsafe.SizeOf<Vector3>();
            // Source: https://learnopengl.com/Model-Loading/Mesh;
            if (Game.instance.APIVersion>new Version(4,5))
            {

                GL.BindVertexArray(VAO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.VertexArrayAttribFormat(VAO, 0, v3Size, VertexAttribType.Float, false, 0);
                GL.EnableVertexArrayAttrib(VAO, 0);
                GL.VertexArrayAttribFormat(VAO, 1, Unsafe.SizeOf<Vector2>(), VertexAttribType.Float, false, v3Size);
                GL.EnableVertexArrayAttrib(VAO, 1);

                GL.VertexArrayVertexBuffer(VAO, 0, VBO, 0, Vertex.size);
                GL.VertexArrayAttribBinding(VAO, 0, 0);
                GL.VertexArrayAttribBinding(VAO, 1, 0);
                GL.VertexArrayElementBuffer(VAO, EBO);

                GL.BindVertexArray(0);
            }
            else
            {
                GL.BindVertexArray(VAO);

                // Bind vertex buffer (VBO)
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false,Vertex.size, 0);
             

                // Attribute 1: Texture Coordinates (2 floats)
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Vertex.size, v3Size);
                

                // Bind element array buffer (EBO)
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);

                // Unbind VAO, VBO, and EBO 
                GL.BindVertexArray(0);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            }


        }
        public void FillBuffers()
        {
            if (isBuffersFilled) return;
            isBuffersFilled = true;
            if (Game.instance.APIVersion > new Version(4, 5))
            {
                GL.NamedBufferData(VBO, vertices.Length * Vertex.size, vertices, BufferUsageHint.StaticDraw);

                GL.NamedBufferData(EBO, triangles.Length * sizeof(uint), triangles, BufferUsageHint.StaticDraw);
            }
            else
            {
                GL.BindVertexArray(VAO);
                GL.BindBuffer(BufferTarget.ArrayBuffer,VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * Vertex.size, vertices, BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.ArrayBuffer, EBO);
                GL.BufferData(BufferTarget.ArrayBuffer, triangles.Length * sizeof(uint), triangles, BufferUsageHint.StaticDraw);
            }
        }
    
    }
    public struct Vertex
    {
        public static readonly int size = Unsafe.SizeOf<Vertex>();
        public Vector3 Position;
        public Vector2 Uv;

        public Vertex(Vector3 position) : this()
        {
            Position = position;
        }

        public Vertex(Vector3 position, Vector2 uv)
        {
            Position = position;
            Uv = uv;
        }
    }
}
