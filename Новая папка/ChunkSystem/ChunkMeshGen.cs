using ConsoleApp1_Pet.Meshes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Новая_папка.ChunkSystem
{
    public class ChunkMeshGen
    {
        public const int N = Chunk.Width;
        public const int N2 = N*N;
        public static Mesh GenerateMesh(Chunk ck)
        {
            //var data = ck.data.AsSpan();
            bool IsBlock(int ind)
            {
                //int x = ind % Chunk.Width;
                //int y = (ind / Chunk.Width) % Chunk.Width;
                //int z = ind / (Chunk.Width * Chunk.Width);
                return ck.data[ind] != 0;
            }
            bool IsTopFree(int ind)
            {

                //if (!((ind - N2) > 0 && IsBlock(ind - N2))) return false;
                if (((ind + N) < Chunk.DataSize && IsBlock(ind + N))) return false;

                return true;
            }
            bool IsBotFree(int ind)
            {

                //if (!((ind - N2) > 0 && IsBlock(ind - N2))) return false;
                if (((ind - N) > 0 && IsBlock(ind - N))) return false;

                return true;
            }
            bool IsEnclosed(int ind)
            {
                int x = ind % N;
                int y = (ind / N) % N;
                int z = ind / (N * N);

                var Deb = false;
                if (x > 0 && (Deb = ! IsBlock(ind - 1))) return false;  // Left
                if (x < N - 1 && (Deb = ! IsBlock(ind + 1))) return false;   // Right
                if (y > 0 && (Deb = !IsBlock(ind - N))) return false;   // Down
                if (y < Chunk.Height - 1 && (Deb = !IsBlock(ind + N))) return false;  // Up
                if (z > 0 && (Deb = !IsBlock(ind - (N2)))) return false;   // Back
                if (z < N - 1 && (Deb = !IsBlock(ind + (N2)))) return false;   // Forward



                return true;
            }
           
            
            List<Vertex> MeshVertices = new List<Vertex>();
            List<uint> tis = new List<uint>();
            uint trisOffest = 0;
            
            for (int i = Chunk.DataSize-1;i>=0;i--)
            {
                var bid = ck.GetBlockId(i);
                if (bid == 0)
                {
                    continue;
                }
                else
                {
                    Vector3 offest = Chunk.IndexToVector3i(i);

                    if (!IsEnclosed(i))
                    {
                      
                      var prp =  Chunk.GetBlockProperties(bid);
                        var V=new Vertex[prp.mesh.vertices.Length];
                        var tris = new uint[prp.mesh.triangles.Length];
                        Array.Copy(prp.mesh.vertices, V,V.Length);
                        Array.Copy(prp.mesh.triangles, tris, tris.Length);
                        V = OffestArray(V, offest);
                        if (trisOffest > 0)
                            tris= OffestArray(tris, trisOffest);
                        trisOffest += (uint)V.Length;
                        MeshVertices.AddRange(V);
                        tis.AddRange(tris);
                    }
                }
            }
            var Mesh = new Mesh(MeshVertices.ToArray(), tis.ToArray());
            return Mesh;
        }
        public static Vector3[] OffestArray(Vector3[] arr, Vector3 offest)
        {
            for (int i = arr.Length-1;i>=0;i--)
            {
                arr[i] += offest;
            }
            return arr;
        }
        public static Vertex[] OffestArray(Vertex[] arr, Vector3 offest)
        {
            for (int i = arr.Length - 1; i >= 0; i--)
            {
                arr[i].Position += offest;
            }
            return arr;
        }
        public static uint[] OffestArray( uint[] arr, uint offest)
        {
            for (int i = arr.Length - 1; i >= 0; i--)
            {
                arr[i] += offest;
            }
            return arr;
        }
    }
}
