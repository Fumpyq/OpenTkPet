using OpenTK.Graphics.OpenGL4;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Color = SixLabors.ImageSharp.Color;

namespace ConsoleApp1_Pet.Textures
{
    public class Texture
    {
        public int Width;
        public int Height;
        public int id;

        public Texture(string RelativePath)
        {
            Image<Rgba32> image = null;
            //id = GL.GenTexture();
            try
            {
                image = (Image<Rgba32>?)Image.Load(RelativePath);
            }
            catch(Exception ex) {
                
                image = new Image<Rgba32>(4, 4);
                int r = 0;
                image.Mutate(c => c.ProcessPixelRowsAsVector4(row =>
                {
                    for (int x = 0; x < row.Length; x++)
                    {
                        row[x] = ((r + x) % 2 == 0 ?new Vector4(0,0.1f,0,1): new Vector4(0.26f, 0.02f, 0.32f, 1));
                        // row[x] = new Vector4(0, 0.5f, 0, 1);
                    }
                    r++;
                }));
            }
            Rgba32[] pixelArray = new Rgba32[image.Width * image.Height];
            image.CopyPixelDataTo(pixelArray);
            //var _IMemoryGroup = image.GetPixelMemoryGroup();
            //var _MemoryGroup = _IMemoryGroup.ToArray()[0];
            //var PixelData = MemoryMarshal.AsBytes(_MemoryGroup.Span).ToArray();

            Width = image.Width;
            Height = image.Height;
            id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixelArray);
            //Use();
            //GL.TextureParameter(id, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            //GL.TextureParameter(id, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            //GL.TextureParameter(id, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            //GL.TextureParameter(id, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            //GL.TextureParameter(id, TextureParameterName.TextureMaxLevel, (int)1);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            // var data = new byte[image.Width * image.Height];
            //image.Save(data);

            //GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        public Texture(int width, int height, int id)
        {
            Width = width;
            Height = height;
            this.id = id;
        }

        public void Use()
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, id);
        }
    }
    public static class TextureExtansions {
        public static byte[] ToArray(this SixLabors.ImageSharp.Image imageIn)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                imageIn.Save(ms, PngFormat.Instance);
                return ms.ToArray();
            }
        }
    }
}
