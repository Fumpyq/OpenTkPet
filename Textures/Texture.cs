using OpenTK.Graphics.OpenGL4;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
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
        public PixelFormat pixelFormat;
        public static readonly string SourcePath = AppDomain.CurrentDomain.BaseDirectory;
        private Image image;
        public Texture(string RelativePath)
        {
            image = null;
            //id = GL.GenTexture();
            try
            {
                //  var ddd =Image.Identify(RelativePath);   
                //  ddd.
                //var ad=  Image.DetectFormat(SourcePath + "\\" + RelativePath);
                //var asd = ad.Name;

                //var ads= Image.Load(SourcePath + "\\" + RelativePath);

                image = Image.Load(SourcePath+"\\"+RelativePath);
                //var tt = image.GetType();
                //  image.get
            }
            catch(Exception ex) {
                bool LogError = true;
                if (ex.Message == "The value cannot be an empty string. (Parameter 'path')") LogError = false;
                // if (LogError &&) LogError = false;
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

            //var _IMemoryGroup = image.GetPixelMemoryGroup();
            //var _MemoryGroup = _IMemoryGroup.ToArray()[0];
            //var PixelData = MemoryMarshal.AsBytes(_MemoryGroup.Span).ToArray();

            Width = image.Width;
            Height = image.Height;
            id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);
            if (image is Image<Rgb24> RightFormat)
            {
                var a = RightFormat; pixelFormat =  PixelFormat.Rgb;
                Rgb24[] pixelArray = new Rgb24[image.Width * image.Height];
                a.CopyPixelDataTo(pixelArray);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, pixelFormat, PixelType.UnsignedByte, pixelArray);

            }
            if (image is Image<Rgba32> RightFormat2)
            {
                var a = RightFormat2; pixelFormat = PixelFormat.Rgba;
                Rgba32[] pixelArray = new Rgba32[image.Width * image.Height];
                a.CopyPixelDataTo(pixelArray);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, pixelFormat, PixelType.UnsignedByte, pixelArray);

            }
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
        /// <summary>
        /// X,Y,index,out color as vec4
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="getPixelColor"></param>
        public void GenerateFromCode(int width,int height,Func<int,int,int,Rgba32> getPixelColor)
        {
            image = new Image<Rgba32>(width, height);
            Width = width;
            Height = height;
            if (image is Image<Rgba32> RightFormat2)
            {

                int ind = 0;
                for(int x= width-1;x>0;x--)
                for(int y= height - 1; y > 0; y--)
                    {
                        var clr = getPixelColor(x, y, ind);
                        RightFormat2[x, y] = clr;
                        ind++;
                    }

                RightFormat2.SaveAsPng("Testing.png");
                GL.BindTexture(TextureTarget.Texture2D, id);

                var a = RightFormat2; pixelFormat = PixelFormat.Rgba;
                
                Rgba32[] pixelArray = new Rgba32[image.Width * image.Height];
                a.CopyPixelDataTo(pixelArray);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, pixelFormat, PixelType.UnsignedByte, pixelArray);

            }
        }
        public Texture(int width, int height, int id)
        {
            Width = width;
            Height = height;
            this.id = id;
        }
        /// <summary>
        /// It will LOSE all content
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Resize(int width,int height, bool PreserveData=true)
        {
            GL.BindTexture(TextureTarget.Texture2D, id);
            switch (pixelFormat)
            {
                case PixelFormat.Rgba:
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, pixelFormat, PixelType.UnsignedByte, new Rgba32[width*height]);break;
                case PixelFormat.Rgb:
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, pixelFormat, PixelType.UnsignedByte, new Rgb24[width*height]);break;
            }
            Width = width;
            Height = height;
           
            
            //if(PreserveData) GL.CopyTexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, 0, 0, Width, Height); // Copy and resize 


        }
        public void Use(int unit=0)
        {
            
            GL.ActiveTexture((TextureUnit)((int)TextureUnit.Texture0+ unit));
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
        //public static byte[] GetTypeInfo(this SixLabors.ImageSharp.Image imageIn, out )
        //{
        //    imageIn.Configuration.ImageFormats
        //    var pixelFormat = imageIn.PixelType;
        //    if(pixelFormat == SixLabors.ImageSharp.PixelFormats.A8.Equals())
        //    {

        //    }
        //    // OpenGL texture format mapping based on pixel format properties
        //    PixelInternalFormat internalFormat;
        //    PixelFormat pixelFormatGL; // Note: This is for OpenGL, not the image's pixel format
        //    PixelType pixelDataType;

        //    if (pixelFormat.BitsPerPixel == 32 && pixelFormat. == 4) // Rgba32
        //    {
        //        internalFormat = PixelInternalFormat.Rgba;
        //        pixelFormatGL = PixelFormat.Bgra; // Bgra for OpenGL
        //        pixelDataType = PixelType.UnsignedByte;
        //    }
        //    else if (pixelFormat.BitsPerPixel == 24 && pixelFormat.Components == 3) // Rgb24
        //    {
        //        internalFormat = PixelInternalFormat.Rgb;
        //        pixelFormatGL = PixelFormat.Bgr; // Bgr for OpenGL
        //        pixelDataType = PixelType.UnsignedByte;
        //    }
        //    else if (pixelFormat.BitsPerPixel == 8 && pixelFormat.Components == 1) // L8 (grayscale)
        //    {
        //        internalFormat = PixelInternalFormat.Luminance;
        //        pixelFormatGL = PixelFormat.Luminance;
        //        pixelDataType = PixelType.UnsignedByte;
        //    }
        //    else
        //    {
        //        throw new ArgumentException($"Unsupported pixel format: {pixelFormat.BitsPerPixel} bits per pixel, {pixelFormat.Components} components");
        //    }
        //}
    }
}
