using BepuPhysics.Trees;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
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

        // Array to store block IDs in the chunk
        private int[] blocks;

        // Dictionary to store block properties by ID (static for global access)
        private static Dictionary<int, BlockProperties> blockProperties = new Dictionary<int, BlockProperties>();

        // Dictionary to store custom block properties
        private Dictionary<int, Dictionary<string, object>> customProperties = new Dictionary<int, Dictionary<string, object>>();

        // Initialize the block properties dictionary (once at game start)
        public Chunk()
        {
        
        }

        public Chunk(int x, int z)
        {
            X = x;
            Z = z;
            blocks = new int[16 * 16 * 256]; // 16x16x256 blocks
        }

        // Get the block ID at the specified coordinates within the chunk
        public int GetBlockId(int x, int y, int z)
        {
            int index = (x + z * 16) * 256 + y;
            return blocks[index];
        }

        // Set the block ID at the specified coordinates within the chunk
        public void SetBlockId(int x, int y, int z, int blockId)
        {
            int index = (x + z * 16) * 256 + y;
            blocks[index] = blockId;
        }

        // Get basic block properties by ID
        public BlockProperties GetBlockProperties(int blockId)
        {
            if (blockProperties.ContainsKey(blockId))
            {
                return blockProperties[blockId];
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
    }
    public class Block
    {

    }
}
