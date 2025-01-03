
using ConsoleApp1_Pet.Materials;
using Kaitai;
using OpenTK.Graphics.Egl;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp.PixelFormats;
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

        public int VAO, VBO, EBO, IBO;
        private bool isMeshBuffersFilled;

        public event Action OnChange;

        public Mesh()
        {  

        }

        public Mesh(Vertex[] vertices, uint[] triangles)
        {
            this.vertices = vertices;
            this.triangles = triangles;
        }
        //public static Mesh LoadFromFile(string fileName)
        //{
        //    //KaitaiStream kaitaiStream = new KaitaiStream(fileName);
        //    //kaitaiStream.

        //    //var res = scene.RootNode.GetEntity<Mesh>();
        //    //if (res != null)
        //    //{

        //    //}
        //    //return res;
        //}

        public void CreateBuffers()
        {
            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();
            EBO = GL.GenBuffer();
            IBO = GL.GenBuffer();

            var v3Size = Unsafe.SizeOf<Vector3>();
            int v2Size = Unsafe.SizeOf<Vector2>();
            int v4Size = Unsafe.SizeOf<Vector4>();
            int relativeoffset = v3Size + v2Size;
            if (MainGameWindow.instance.APIVersion>new Version(4,5))
            {
                // Source: https://learnopengl.com/Model-Loading/Mesh;
                GL.BindVertexArray(VAO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.VertexArrayAttribFormat(VAO, 0, v3Size, VertexAttribType.Float, false, 0);
                GL.EnableVertexArrayAttrib(VAO, 0);

                GL.VertexArrayAttribFormat(VAO, 2, v2Size, VertexAttribType.Float, false, v3Size);
                GL.EnableVertexArrayAttrib(VAO, 2);

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
                // Attribute 0: Position (3 float)
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false,Vertex.size, 0);
                // Attribute 1: Texture Coordinates (2 floats)
                GL.EnableVertexAttribArray(2);
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Vertex.size, v3Size);
                // Bind element array buffer (EBO) to VAO
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);

                // Unbind VAO, VBO, and EBO 
                GL.BindVertexArray(0);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            }


        }
        Matrix4[] PrevBatch;
        public unsafe void FillBuffers(Span<Matrix4> transformInstances)
        {
            PrevBatch= transformInstances.ToArray();
            PrevBatch[0] = Matrix4.Identity;
            var v3Size = Unsafe.SizeOf<Vector3>();
            int v2Size = Unsafe.SizeOf<Vector2>();
            int v4Size = Unsafe.SizeOf<Vector4>();

            GL.BindVertexArray(VAO);
            fixed (Matrix4* bp = &MemoryMarshal.GetReference(transformInstances))
            {
                if (MainGameWindow.instance.APIVersion > new Version(4, 5))
                {
                    GL.NamedBufferData(IBO, transformInstances.Length * Unsafe.SizeOf<Matrix4>(), (nint)bp, BufferUsageHint.StaticDraw);
                    GL.VertexArrayAttribIFormat(VAO, 2, 4 * v4Size, VertexAttribType.Float, 0);
                    GL.EnableVertexArrayAttrib(VAO, 2);
                    GL.VertexArrayAttribIFormat(VAO, 3, 4 * v4Size, VertexAttribType.Float, v4Size);
                    GL.EnableVertexArrayAttrib(VAO, 3);
                    GL.VertexArrayAttribIFormat(VAO, 4, 4 * v4Size, VertexAttribType.Float, v4Size * 2);
                    GL.EnableVertexArrayAttrib(VAO, 4);
                    GL.VertexArrayAttribIFormat(VAO, 5, 4 * v4Size, VertexAttribType.Float, v4Size * 3);
                    GL.EnableVertexArrayAttrib(VAO, 5);
                    GL.VertexArrayBindingDivisor(VAO, 2, 1);
                    GL.VertexArrayBindingDivisor(VAO, 3, 1);
                    GL.VertexArrayBindingDivisor(VAO, 4, 1);
                    GL.VertexArrayBindingDivisor(VAO, 5, 1);
                   

                    if (isMeshBuffersFilled) return;
                    isMeshBuffersFilled = true;
                    GL.NamedBufferData(VBO, vertices.Length * Vertex.size, vertices, BufferUsageHint.StaticDraw);
                    GL.NamedBufferData(EBO, triangles.Length * sizeof(uint), triangles, BufferUsageHint.StaticDraw);
                }
                else
                {

                    var s = Unsafe.SizeOf<Matrix4>();
                    
                    GL.BindBuffer(BufferTarget.ArrayBuffer, IBO);
                    GL.BufferData(BufferTarget.ArrayBuffer, transformInstances.Length * s, PrevBatch, BufferUsageHint.StaticDraw);
                 
                    GL.EnableVertexAttribArray(3);
                    GL.VertexAttribPointer(3, v4Size, VertexAttribPointerType.Float, false, 4 * v4Size, 0);
                    GL.EnableVertexAttribArray(4);
                    GL.VertexAttribPointer(4, v4Size, VertexAttribPointerType.Float, false, 4 * v4Size, v4Size);
                    GL.EnableVertexAttribArray(5);
                    GL.VertexAttribPointer(5, v4Size, VertexAttribPointerType.Float, false, 4 * v4Size, v4Size * 2);
                    GL.EnableVertexAttribArray(6);
                    GL.VertexAttribPointer(6, v4Size, VertexAttribPointerType.Float, false, 4 * v4Size, v4Size * 3);
                    GL.VertexAttribDivisor(3, 1);
                    GL.VertexAttribDivisor(4, 1);
                    GL.VertexAttribDivisor(5, 1);
                    GL.VertexAttribDivisor(6, 1);

                    if (isMeshBuffersFilled) return;
                    isMeshBuffersFilled = true;
                    GL.BindBuffer(BufferTarget.ArrayBuffer,VBO);
                        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * Vertex.size, vertices, BufferUsageHint.StaticDraw);
                        
                        GL.BindBuffer(BufferTarget.ArrayBuffer, EBO);
                        GL.BufferData(BufferTarget.ArrayBuffer, triangles.Length * sizeof(uint), triangles, BufferUsageHint.StaticDraw);
                        
                }
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
