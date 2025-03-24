using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Processing;

namespace ConsoleApp1_Pet.Textures
{
    public static class TextureGenerator
    {
        public static Texture CreateProcedural(int width, int height,
            Func<int, int, int, Rgba32> generator,
            TextureFormat format = TextureFormat.RGBA8,
            bool mipmaps = false)
        {
            var texture = new Texture(width, height, format);
            GenerateFromFunction(texture, generator, mipmaps);
            return texture;
        }
        public static void Checkerboard(in Texture texture, int cellSize,
    Rgba32 color1, Rgba32 color2)
        {
            GenerateFromFunction(texture, (x, y, _) =>
            {
                bool cellX = (x / cellSize) % 2 == 0;
                bool cellY = (y / cellSize) % 2 == 0;
                return (cellX ^ cellY) ? color1 : color2;
            },false);
        }
        public static Texture Checkerboard(int size, int cellSize,
            Rgba32 color1, Rgba32 color2,
            TextureFormat format = TextureFormat.RGBA8)
        {
            return CreateProcedural(size, size, (x, y, _) =>
            {
                bool cellX = (x / cellSize) % 2 == 0;
                bool cellY = (y / cellSize) % 2 == 0;
                return (cellX ^ cellY) ? color1 : color2;
            }, format);
        }

        public static Texture SolidColor(int width, int height,
            Rgba32 color,
            TextureFormat format = TextureFormat.RGBA8)
        {
            return CreateProcedural(width, height, (_, _, _) => color, format);
        }

        public static Texture LinearGradient(int width, int height,
            Rgba32 start, Rgba32 end,
            bool horizontal = true)
        {
            return CreateProcedural(width, height, (x, y, _) =>
            {
                float t = horizontal ? x / (float)width : y / (float)height;
                return Lerp(start, end, t);
            }, TextureFormat.RGBA8);
        }

        public static Texture Border(int width, int height,
            int borderSize, Rgba32 borderColor, Rgba32 fillColor)
        {
            return CreateProcedural(width, height, (x, y, _) =>
            {
                bool isBorder = x < borderSize || x >= width - borderSize ||
                              y < borderSize || y >= height - borderSize;
                return isBorder ? borderColor : fillColor;
            }, TextureFormat.RGBA8);
        }

        //public static Texture PerlinNoise(int width, int height,
        //    float scale = 0.1f, int seed = 0)
        //{
        //    var noise = new FastNoiseLite(seed);
        //    noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        //    return CreateProcedural(width, height, (x, y, _) =>
        //    {
        //        float value = noise.GetNoise(x * scale, y * scale);
        //        value = (value + 1) * 0.5f; // Remap from [-1,1] to [0,1]
        //        return new Rgba32(value, value, value, 1f);
        //    }, TextureFormat.RGBA8);
        //}

        private static void GenerateFromFunction(Texture texture,
            Func<int, int, int, Rgba32> generator, bool mipmaps)
        {
            using var image = new Image<Rgba32>(texture.Width, texture.Height);

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < accessor.Width; x++)
                    {
                        int index = y * accessor.Width + x;
                        row[x] = generator(x, y, index);
                    }
                }
            });

            //image.Mutate(ctx =>
            //{
            //    //ctx.ProcessPixelRowsAsVector4(accessor =>
            //    //{
            //    //    for (int y = 0; y < image.Height; y++)
            //    //    {
            //    //        var row = accessor;
            //    //        for (int x = 0; x < image.Width; x++)
            //    //        {
            //    //            int index = y * image.Width + x;
            //    //            row[x] = generator(x, y, index);
            //    //        }
            //    //    }
            //    //});
            //    //for (int y = 0; y < image.Height; y++)
            //    //{
            //    //    for (int x = 0; x < image.Width; x++)
            //    //    {
            //    //        int index = y * image.Width + x;
            //    //        image[x, y] = generator(x, y, index);
            //    //    }
            //    //}


            //    ctx.Flip(FlipMode.Vertical);
            //});

            texture.LoadImageData(image);
            if (mipmaps) texture.GenerateMipmaps();
        }

        private static Rgba32 Lerp(Rgba32 a, Rgba32 b, float t)
        {
            return new Rgba32(
                (byte)(a.R + (b.R - a.R) * t),
                (byte)(a.G + (b.G - a.G) * t),
                (byte)(a.B + (b.B - a.B) * t),
                (byte)(a.A + (b.A - a.A) * t)
            );
        }
    }
}
