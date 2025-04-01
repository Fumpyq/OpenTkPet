using BepuPhysics.Collidables;
using ConsoleApp1_Pet.Meshes;
using ConsoleApp1_Pet.Render;
using ConsoleApp1_Pet.Новая_папка.Resources;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mesh = ConsoleApp1_Pet.Meshes.Mesh;

namespace ConsoleApp1_Pet.Новая_папка.ChunkSystem
{
    public class ExpirementalChunk
    {
        public class ChunkGenerator
        {
            private const int N = 16; // Chunk size
            private const float frequency = 0.1f;
            private const int seed = 12345;
            private float? ExpectedMin;
            private float? ExpectedMax;
            public Mesh GenerateChunk(Vector3 chunkCoords, int lod = 0)
            {
                int xOffset = (int)chunkCoords.X * N;
                int yOffset = (int)chunkCoords.Y * N;
                int zOffset = (int)chunkCoords.Z * N;

                // Generate extended noise grid with 1-block padding
                int extendedN = N + 2;
                float[] extendedNoise = new float[extendedN * extendedN * extendedN];
                var mm= MainGameWindow.instance.TerrainNoise.GenUniformGrid3D(
                    extendedNoise,
                    xOffset - 1, yOffset - 1, zOffset - 1,
                    extendedN, extendedN, extendedN,
                    frequency, seed
                );

                // Calculate noise threshold

                ExpectedMin ??=  mm.min;
                ExpectedMax ??=  mm.max;

                float min = ExpectedMin.Value, max = ExpectedMax.Value;
                //for (int z = 1; z <= N; z++)
                //{
                //    for (int y = 1; y <= N; y++)
                //    {
                //        for (int x = 1; x <= N; x++)
                //        {
                //            float val = extendedNoise[x + y * extendedN + z * extendedN * extendedN];
                //            min = Math.Min(min, val);
                //            max = Math.Max(max, val);
                //        }
                //    }
                //}
                float middle = (max - min) / 2 + min;

                List<Vertex> vertices = new List<Vertex>();
                List<uint> triangles = new List<uint>();

                for (int z = 0; z < N; z++)
                {
                    for (int y = 0; y < N; y++)
                    {
                        for (int x = 0; x < N; x++)
                        {
                            // Get noise value from extended grid
                            float noise = extendedNoise[
                                (x + 1) +
                                (y + 1) * extendedN +
                                (z + 1) * extendedN * extendedN
                            ];

                            if (noise < middle) continue;

                            // LOD processing (simple example)
                            bool isBorder = x == 0 || x == N - 1 ||
                                          y == 0 || y == N - 1 ||
                                          z == 0 || z == N - 1;

                            if (lod > 0 && !isBorder)
                            {
                                if ((x % (lod + 1)) != 0 ||
                                    (y % (lod + 1)) != 0 ||
                                    (z % (lod + 1)) != 0) continue;
                            }

                            // Check neighbors and generate faces
                            CheckFace(x, y, z, Direction.Left, extendedNoise, extendedN, middle,
                                    vertices, triangles);
                            CheckFace(x, y, z, Direction.Right, extendedNoise, extendedN, middle,
                                    vertices, triangles);
                            CheckFace(x, y, z, Direction.Down, extendedNoise, extendedN, middle,
                                    vertices, triangles);
                            CheckFace(x, y, z, Direction.Up, extendedNoise, extendedN, middle,
                                    vertices, triangles);
                            CheckFace(x, y, z, Direction.Back, extendedNoise, extendedN, middle,
                                    vertices, triangles);
                            CheckFace(x, y, z, Direction.Front, extendedNoise, extendedN, middle,
                                    vertices, triangles);
                        }
                    }
                }

                return new Mesh(vertices.ToArray(), triangles.ToArray());
            }

            private enum Direction { Left, Right, Down, Up, Back, Front }

            private void CheckFace(int x, int y, int z, Direction dir, float[] noise, int size,
                                 float threshold, List<Vertex> vertices, List<uint> triangles)
            {
                int adjX = x + 1, adjY = y + 1, adjZ = z + 1;
                switch (dir)
                {
                    case Direction.Left: adjX--; break;
                    case Direction.Right: adjX++; break;
                    case Direction.Down: adjY--; break;
                    case Direction.Up: adjY++; break;
                    case Direction.Back: adjZ--; break;
                    case Direction.Front: adjZ++; break;
                }

                // Check adjacent block in extended noise grid
                float adjNoise = noise[adjX + adjY * size + adjZ * size * size];
                if (adjNoise >= threshold) return;

                // Generate face vertices
                Vector3 localPos = new Vector3(x, y, z);
                Vector3[] corners = GetFaceCorners(dir, localPos);
                Vector2[] uvs = new Vector2[]
                {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
                };

                uint baseIndex = (uint)vertices.Count;
                foreach (var corner in corners)
                    vertices.Add(new Vertex(corner, uvs[vertices.Count % 4]));

                triangles.AddRange(new uint[]
                {
            baseIndex, baseIndex + 1, baseIndex + 2,
            baseIndex, baseIndex + 2, baseIndex + 3
                });
            }

            private Vector3[] GetFaceCorners(Direction dir, Vector3 localPos)
            {
                return dir switch
                {
                    Direction.Left => new[]
                    {
                localPos + new Vector3(0, 0, 0),
                localPos + new Vector3(0, 0, 1),
                localPos + new Vector3(0, 1, 1),
                localPos + new Vector3(0, 1, 0)
            },
                    Direction.Right => new[]
                    {
                localPos + new Vector3(1, 0, 0),
                localPos + new Vector3(1, 1, 0),
                localPos + new Vector3(1, 1, 1),
                localPos + new Vector3(1, 0, 1)
            },
                    Direction.Down => new[]
                    {
                localPos + new Vector3(0, 0, 0),
                localPos + new Vector3(1, 0, 0),
                localPos + new Vector3(1, 0, 1),
                localPos + new Vector3(0, 0, 1)
            },
                    Direction.Up => new[]
                    {
                localPos + new Vector3(0, 1, 0),
                localPos + new Vector3(0, 1, 1),
                localPos + new Vector3(1, 1, 1),
                localPos + new Vector3(1, 1, 0)
            },
                    Direction.Back => new[]
                    {
                localPos + new Vector3(0, 0, 0),
                localPos + new Vector3(0, 1, 0),
                localPos + new Vector3(1, 1, 0),
                localPos + new Vector3(1, 0, 0)
            },
                    Direction.Front => new[]
                    {
                localPos + new Vector3(0, 0, 1),
                localPos + new Vector3(1, 0, 1),
                localPos + new Vector3(1, 1, 1),
                localPos + new Vector3(0, 1, 1)
            },
                    _ => throw new ArgumentOutOfRangeException(nameof(dir))
                };
            }
        }

        public static void Run()
        {
            int size = 8;
            var ck = new ChunkGenerator();
            for (int x = -size; x <= size; x++)
            {
                for (int z = -size; z <= size; z++)
                {
                    var mesh =  ck.GenerateChunk(new Vector3(x,0,z),0);
                    mesh.CreateBuffers();
                    var resMat = MainGameWindow.instance.RockMaterial;
                    var rr3 = new RenderComponent(mesh, resMat);
                    rr3.WithSelfGamobject();
                    rr3.gameObject.transform.position = new Vector3(x, 0, z) * Chunk.Width;
                    //  MainGameWindow.instance.renderer.AddToRender(rr3);
                    //  var c = MainGameWindow.instance.cg.GenerateChunk(new OpenTK.Mathematics.Vector2i(x, z));
                    //  var mesh = ChunkMeshGen.GenerateMesh(c);
                    //  mesh.CreateBuffers();
                    //  var resMat = MainGameWindow.instance.RockMaterial;
                    //  var rr3 = new RenderComponent(mesh, resMat);
                    //  rr3.WithSelfGamobject();
                    //  rr3.gameObject.transform.position = new Vector3(x,0,z)*Chunk.Width;
                    ////  MainGameWindow.instance.renderer.AddToRender(rr3);
                }
            }
           
        }
    }
}
