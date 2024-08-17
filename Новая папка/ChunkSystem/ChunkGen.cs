using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Новая_папка.ChunkSystem
{
    public class ChunkGen
    {
        public FastNoise2 BaseShapeNoise;
        public int seed;
        public float freq;

        public ChunkGen(FastNoise2 baseShapeNoise, int seed, float freq)
        {
            BaseShapeNoise = baseShapeNoise;
            this.seed = seed;
            this.freq = freq;
        }
        float[] buffer = new float[Chunk.DataSize];
        public Chunk GenerateChunk(Vector2i pos)
        {
            var mm = BaseShapeNoise.GenUniformGrid3D(buffer, pos.X*Chunk.Width, 0, pos.Y * Chunk.Width, Chunk.Width, Chunk.Height, Chunk.Width, freq, seed);
       
            var AirLevel = mm.min + ((mm.max- mm.min) * 0.4f);

            var chunk = new Chunk(pos);

            for(int i = Chunk.DataSize - 1; i >= 0; i--)
            {
                chunk.SetBlockId(i, buffer[i] > AirLevel?0:1);
            }


            //int length = Chunk.DataSize;

            //// Process elements in batches of 4
            //for (int i = length - 5; i >= 3; i -= 4)
            //{
            //    // Load 4 elements into a Vector<byte>
            //    var data = new Vector<float>(buffer.AsSpan(i,4));

            //    // Compare each element with airLevel
            //    var result = Vector.GreaterThan(data, new Vector<float>(AirLevel));

            //    // Set block IDs based on comparison
            //    var blockIds = Vector.ConditionalSelect(result, new Vector<int>(0), new Vector<int>(1));

            //    // Store the results back into the chunk's block IDs
            //    blockIds.CopyTo(chunk.data,i);
            //}

            //// Handle any remaining elements (less than 4)
            //for (int i = Math.Min(3, length - 1); i >= 0; i--)
            //{
            //    chunk.data[i] = (buffer[i] > AirLevel ? 0 : 1);
            //}
            return chunk;
        }

    }
}
