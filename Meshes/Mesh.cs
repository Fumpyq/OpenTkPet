
using ConsoleApp1_Pet.Materials;
using Kaitai;
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

        public event Action OnChange;

        public int id { get => VAO; }
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

            var v3Size = Unsafe.SizeOf<Vector3>();
            int v2Size = Unsafe.SizeOf<Vector2>();
            int v4Size = Unsafe.SizeOf<Vector4>();
            int relativeoffset = v3Size + v2Size;
            if (MainGameWindow.instance.APIVersion > new Version(4, 5))
            {
                // Source: https://learnopengl.com/Model-Loading/Mesh;
                GL.BindVertexArray(VAO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.VertexArrayAttribFormat(VAO, 0, v3Size, VertexAttribType.Float, false, 0);
                GL.EnableVertexArrayAttrib(VAO, 0);

                GL.VertexArrayAttribFormat(VAO, 1, v2Size, VertexAttribType.Float, false, v3Size);
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
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.size, 0);


                // Attribute 1: Texture Coordinates (2 floats)
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Vertex.size, v3Size);


                GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);

                GL.BindVertexArray(0);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            }


        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void FillBuffers()
        {
            if (isBuffersFilled) return;
            isBuffersFilled = true;
            if (MainGameWindow.instance.APIVersion > new Version(4, 5))
            {
                GL.NamedBufferData(VBO, vertices.Length * Vertex.size, vertices, BufferUsageHint.StaticDraw);

                GL.NamedBufferData(EBO, triangles.Length * sizeof(uint), triangles, BufferUsageHint.StaticDraw);
            }
            else
            {
                GL.BindVertexArray(VAO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * Vertex.size, vertices, BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.ArrayBuffer, EBO);
                GL.BufferData(BufferTarget.ArrayBuffer, triangles.Length * sizeof(uint), triangles, BufferUsageHint.StaticDraw);
            }
        }

        public void Bind()
        {
            GL.BindVertexArray(VAO);
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
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
    public enum VertexAttributeFormat
    {
        Float32,
        Float16,
        UNorm8,
        UInt16,
        // Add more formats as needed
    }

    public struct VertexAttributeDescriptor
    {
        public string Name;
        public int Stream;
        public VertexAttributeFormat Format;
        public int Dimension;
        public bool IsNormalized;
        public int Offset;
    }

    public class VertexLayout
    {
        public List<VertexAttributeDescriptor> Attributes = new();
        public int Stride;
        public int VertexCount;
        public int IndexCount;

        public VertexLayout CalculateOffsets()
        {
            int offset = 0;
            foreach (ref var attr in CollectionsMarshal.AsSpan(Attributes))
            {
                attr.Offset = offset;
                offset += GetFormatSize(attr.Format) * attr.Dimension;
            }
            Stride = offset;
            return this;
        }

        private static int GetFormatSize(VertexAttributeFormat format) => format switch
        {
            VertexAttributeFormat.Float32 => 4,
            VertexAttributeFormat.Float16 => 2,
            VertexAttributeFormat.UNorm8 => 1,
            VertexAttributeFormat.UInt16 => 2,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public class MeshData
    {
        public VertexLayout Layout;
        public byte[] VertexData;
        public int[] Indices;

        public MeshData(VertexLayout layout, int vertexCount, int indexCount)
        {
            Layout = layout.CalculateOffsets();
            Layout.VertexCount = vertexCount;
            Layout.IndexCount = indexCount;
            VertexData = new byte[Layout.Stride * vertexCount];
        }
    }

    public static class MeshSerializer
    {
        public static void Serialize(BinaryWriter writer, MeshData mesh)
        {
            // Write layout
            writer.Write(mesh.Layout.Attributes.Count);
            foreach (var attr in mesh.Layout.Attributes)
            {
                writer.Write(attr.Name);
                writer.Write(attr.Stream);
                writer.Write((byte)attr.Format);
                writer.Write((byte)attr.Dimension);
                writer.Write(attr.IsNormalized);
            }

            writer.Write(mesh.Layout.Stride);
            writer.Write(mesh.Layout.VertexCount);
            writer.Write(mesh.Layout.IndexCount);

            // Write vertex data
            writer.Write(mesh.VertexData.Length);
            writer.Write(mesh.VertexData);

            // Write indices
            WriteIndices(writer, mesh.Indices);
        }

        public static MeshData Deserialize(BinaryReader reader)
        {
            var layout = new VertexLayout();

            // Read layout
            int attrCount = reader.ReadInt32();
            for (int i = 0; i < attrCount; i++)
            {
                layout.Attributes.Add(new VertexAttributeDescriptor
                {
                    Name = reader.ReadString(),
                    Stream = reader.ReadInt32(),
                    Format = (VertexAttributeFormat)reader.ReadByte(),
                    Dimension = reader.ReadByte(),
                    IsNormalized = reader.ReadBoolean()
                });
            }

            layout.Stride = reader.ReadInt32();
            int vertexCount = reader.ReadInt32();
            int indexCount = reader.ReadInt32();

            var mesh = new MeshData(layout, vertexCount, indexCount)
            {
                // Read vertex data
                VertexData = reader.ReadBytes(reader.ReadInt32())
            };

            // Read indices
            mesh.Indices = ReadIndices(reader, indexCount);

            return mesh;
        }

        private static void WriteIndices(BinaryWriter writer, int[] indices)
        {
            writer.Write(indices.Length);

            // Compress indices using meshopt-like compression
            var bytes = MemoryMarshal.Cast<int, byte>(indices);
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        private static int[] ReadIndices(BinaryReader reader, int expectedCount)
        {
            int count = reader.ReadInt32();
            int byteLength = reader.ReadInt32();
            byte[] bytes = reader.ReadBytes(byteLength);
            return MemoryMarshal.Cast<byte, int>(bytes).ToArray();
        }
    }

    // Usage example
    public static class VertexAttributes
    {
        public static VertexAttributeDescriptor Position = new()
        {
            Name = "POSITION",
            Format = VertexAttributeFormat.Float32,
            Dimension = 3
        };

        public static VertexAttributeDescriptor Normal = new()
        {
            Name = "NORMAL",
            Format = VertexAttributeFormat.Float16,
            Dimension = 3,
            IsNormalized = true
        };

        public static VertexAttributeDescriptor UV0 = new()
        {
            Name = "TEXCOORD0",
            Format = VertexAttributeFormat.Float16,
            Dimension = 2
        };
    }
}
