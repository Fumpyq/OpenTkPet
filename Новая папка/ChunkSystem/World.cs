using BepuPhysics.Trees;
using ConsoleApp1_Pet.Meshes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Новая_папка.ChunkSystem
{
    internal class World
    {
        private Dictionary<Vector2i, Chunk> loadedChunks = new Dictionary<Vector2i, Chunk>();

    }
    public class Chunk
    {
        

        public int X { get; private set; }
        public int Z { get; private set; }

        public const int Height =88;
        public const int Width = 16;
        public const int DataSize = Height * Width*Width;

        // Array to store block IDs in the chunk
        public int[] data;

        // Dictionary to store block properties by ID (static for global access)
        public  static Dictionary<int, BlockProperties> blockProperties = new Dictionary<int, BlockProperties>();

        // Dictionary to store custom block properties
        private Dictionary<int, Dictionary<string, object>> customProperties = new Dictionary<int, Dictionary<string, object>>();

        // Initialize the block properties dictionary (once at game start)
        public Chunk()
        {
            data = new int[DataSize]; // 16x16x256 blocks
        }
        public Chunk(Vector2i coords)
        {
            X = coords.X;
            Z = coords.Y;
            data = new int[DataSize]; // 16x16x256 blocks
        }

        public Chunk(int x, int z)
        {
            X = x;
            Z = z;
            data = new int[DataSize]; // 16x16x256 blocks
        }
        public static Vector3i IndexToVector3i(int index)
        {
            int x = index % Chunk.Width;
            int y = (index / Chunk.Width) % Chunk.Height;
            int z = index / (Chunk.Height * Chunk.Width);

            return new Vector3i(x, y, z);
        }
        public static void IndexToVector3i(int index,out int x, out int y, out int z)
        {
            x = index % Chunk.Width;
            y = (index / Chunk.Width) % Chunk.Height;
            z = index / (Chunk.Height * Chunk.Width);

            
        }
        // Convert 3D Vector3i to 1D index
        public static int Vector3iToIndex(Vector3i position)
        {
            return (position.Z * Chunk.Width * Chunk.Height) + (position.Y * Chunk.Width) + position.X;
        }
        // Get the block ID at the specified coordinates within the chunk
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBlockId(int x, int y, int z)
        {
            int index = (x + z * Width) * Height + y;
            return data[index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBlockId(int index)
        {
            return data[index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Set the block ID at the specified coordinates within the chunk
        public void SetBlockId(int x, int y, int z, int blockId)
        {
            int index = (x + z * Width) * Height + y;
            data[index] = blockId;
        }
        // Set the block ID at the specified coordinates within the chunk
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlockId(int index, int blockId)
        {
           
            data[index] = blockId;
        }
        // Get basic block properties by ID
        public static BlockProperties GetBlockProperties(int blockId)
        {
            if (blockProperties.TryGetValue(blockId, out var bb))
            {
                return bb;
            }
            else
            {
                return new BlockProperties(); // Or throw an exception if the ID is invalid
            }
        }

        // Get custom block properties by ID and property name
        public object GetCustomProperty(int blockId, string propertyName)
        {
            if (customProperties.ContainsKey(blockId) && customProperties[blockId].ContainsKey(propertyName))
            {
                return customProperties[blockId][propertyName];
            }
            else
            {
                return null; // Or throw an exception if the property is not found
            }
        }

        // Set custom block properties by ID, property name, and value
        public void SetCustomProperty(int blockId, string propertyName, object value)
        {
            if (customProperties.ContainsKey(blockId))
            {
                customProperties[blockId][propertyName] = value;
            }
            else
            {
                customProperties[blockId] = new Dictionary<string, object> { { propertyName, value } };
            }
        }
    }

    // Structure for basic block properties
    public struct BlockProperties
    {
        public int TextureId;
        public int Hardness;
        public int LightLevel;
        public bool IsTransparent;
        public bool IsSolid;
        public Mesh mesh;
    }
    public class Block
    {

    }
}
