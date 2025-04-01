
using ConsoleApp1_Pet.Meshes;
using MessagePack.Formatters;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Новая_папка.Resources
{
    public static class ResourcePacker
    {
        
        public static unsafe void PackMesh(string resPath, Mesh mesh)
        {

            Stopwatch sw = Stopwatch.StartNew();
            var path = Path.GetDirectoryName(resPath);
            if(!string.IsNullOrEmpty(path)) Directory.CreateDirectory(path);
            using (var fileStream = File.Create(resPath))
            using (var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal)) // Or CompressionLevel.Fastest
            using (var writer = new BinaryWriter(gzipStream))
            {
                int vertexDataSize = mesh.vertices.Length * sizeof(Vertex);

                // Allocate a byte array to hold the vertex data
                byte[] vertexData = new byte[vertexDataSize];

                Buffer.BlockCopy(mesh.vertices, 0, vertexData, 0, vertexData.Length);

                writer.Write(vertexData);

                byte[] trisData = new byte[sizeof(uint) * mesh.triangles.Length];


                Buffer.BlockCopy(mesh.triangles, 0, trisData, 0, trisData.Length);


                writer.Write(mesh.vertices.Length);
                writer.Write(mesh.triangles.Length);
                writer.Write(vertexData);
                writer.Write(trisData);

                var test = mesh.vertices.Where(x => x.Position.Length > 3).ToList();
                var av = 1;
            }
            sw.Stop();
            var el = sw.Elapsed;
            var asdds = 123;
        }
        public static unsafe void UnPackMesh(string resPath, Mesh mesh)
        {
            Stopwatch sw = Stopwatch.StartNew();
            using (var fileStream = File.OpenRead(resPath))
            using (var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress)) // Or CompressionLevel.Fastest
            using (var reader = new BinaryReader(gzipStream))
            {
                int vertexDataSize = mesh.vertices.Length * sizeof(Vertex);

                // Allocate a byte array to hold the vertex data
               // byte[] vertexData = new byte[vertexDataSize];

                byte[] vertexData = reader.ReadBytes(vertexDataSize);

                int vertexStride = 20;
                int vertCount= reader.ReadInt32();
                 int trisCount= reader.ReadInt32();


             
                Span<byte> byteSpan = vertexData.AsSpan(0, vertexDataSize);
                Span<Vertex> vertexSpan = MemoryMarshal.Cast<byte, Vertex>(byteSpan);
                Vertex[] vertices = vertexSpan.ToArray();
                byte[] trisData = new byte[sizeof(uint) * mesh.triangles.Length];


                uint[] indices = new uint[trisCount];
                byte[] indexBytes = reader.ReadBytes(trisCount * 4);
                Buffer.BlockCopy(indexBytes, 0, indices, 0, indexBytes.Length);
                var test = mesh.vertices.Where(x => x.Position.Length > 3).ToList();
                var av = 1;
            }
            sw.Stop();
            var el = sw.Elapsed;
            var asdds = 123;
        }
    }
}
