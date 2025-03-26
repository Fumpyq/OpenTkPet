using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
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
    public enum TextureFormat
    {
        R8,
        RG8,        
        RGB8,
        SRGB8,      
        RGBA8,
        SRGBA8,     
        RGB16F,
        RGB32F,
        RGBA16F,
        RGBA32F,
        Depth16,
        Depth24,
        Depth32F
    }

    public class Texture
    {
        public int id { get => Handle; private set => Handle = value; }
        public int Handle { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public TextureTarget Target { get; private set; }
        public TextureFormat Format { get; private set; }
        public TextureWrapMode WrapMode { get; private set; }
        public Color4 BorderColor { get; private set; }
        public TextureMinFilter MinFilter { get; private set; }
        public TextureMagFilter MagFilter { get; private set; }
        public bool IsArray => Target == TextureTarget.Texture2DArray;

        public static readonly Dictionary<TextureFormat, (SizedInternalFormat, PixelFormat, PixelType)> FormatMap = new()
        {

            [TextureFormat.R8] = (SizedInternalFormat.R8, PixelFormat.Red, PixelType.UnsignedByte),
            [TextureFormat.RG8] = (SizedInternalFormat.Rg8, PixelFormat.Rg, PixelType.UnsignedByte),
            [TextureFormat.RGB8] = (SizedInternalFormat.Rgb8, PixelFormat.Rgb, PixelType.UnsignedByte),
            [TextureFormat.RGBA8] = (SizedInternalFormat.Rgba8, PixelFormat.Rgba, PixelType.UnsignedByte),
            [TextureFormat.SRGB8] = (SizedInternalFormat.Srgb8, PixelFormat.Rgb, PixelType.UnsignedByte),
            [TextureFormat.SRGBA8] = (SizedInternalFormat.Srgb8Alpha8, PixelFormat.Rgba, PixelType.UnsignedByte),
            [TextureFormat.RGB16F] = (SizedInternalFormat.Rgb16f, PixelFormat.Rgb, PixelType.HalfFloat),
            [TextureFormat.RGB32F] = (SizedInternalFormat.Rgb32f, PixelFormat.Rgb, PixelType.Float),
            [TextureFormat.RGBA16F] = (SizedInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.HalfFloat),
            [TextureFormat.RGBA32F] = (SizedInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float),
            [TextureFormat.Depth16] = (SizedInternalFormat.DepthComponent16, PixelFormat.DepthComponent, PixelType.UnsignedShort),
            [TextureFormat.Depth24] = (SizedInternalFormat.DepthComponent24, PixelFormat.DepthComponent, PixelType.UnsignedInt),
            [TextureFormat.Depth32F] = (SizedInternalFormat.DepthComponent32f, PixelFormat.DepthComponent, PixelType.Float)
        };

        public Texture(int width, int height, TextureFormat format,
                     TextureWrapMode wrapMode = TextureWrapMode.Repeat,
                     TextureMinFilter minFilter = TextureMinFilter.Linear,
                     TextureMagFilter magFilter = TextureMagFilter.Linear,
                     Color4? borderColor = null,
                     bool generateMipmaps = false)
        {
            CreateTexture2D(width, height, format, wrapMode, minFilter, magFilter, generateMipmaps);
            borderColor = borderColor ?? Color4.Black;
            _BindedSetBorderColor(borderColor.Value);
        }
        public Texture InitWithEmptyStorage()
        {
            GL.TexStorage2D(TextureTarget2d.Texture2D,
                1, // Mipmap levels
                FormatMap[Format].Item1,
                Width,
                Height
            );
            return this;
        }
        private void CreateTexture2D(int width, int height, TextureFormat format,
                                    TextureWrapMode wrapMode, TextureMinFilter minFilter,
                                    TextureMagFilter magFilter, bool generateMipmaps)
        {
            Handle = GL.GenTexture();
            Width = width;
            Height = height;
            Format = format;
            Target = TextureTarget.Texture2D;
           
            var (internalFormat, pixelFormat, pixelType) = FormatMap[format];
            
            GL.BindTexture(Target, Handle);
            GL.TexImage2D(Target, 0, (PixelInternalFormat)internalFormat, width, height, 0, pixelFormat, pixelType, IntPtr.Zero);

            SetParameters(wrapMode, minFilter, magFilter);

            if (generateMipmaps) GL.GenerateMipmap((GenerateMipmapTarget)Target);
        }

        // Texture array constructor
        public Texture(int width, int height, int layers, TextureFormat format,
                     TextureWrapMode wrapMode = TextureWrapMode.Repeat,
                     TextureMinFilter minFilter = TextureMinFilter.Linear,
                     TextureMagFilter magFilter = TextureMagFilter.Linear)
        {
            Handle = GL.GenTexture();
            Width = width;
            Height = height;
            Format = format;
            Target = TextureTarget.Texture2DArray;

            var (internalFormat, pixelFormat, pixelType) = FormatMap[format];

            GL.BindTexture(Target, Handle);
            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, internalFormat, width, height, layers);
            SetParameters(wrapMode, minFilter, magFilter);
        }

        private void SetParameters(TextureWrapMode wrapMode,
                                  TextureMinFilter minFilter,
                                  TextureMagFilter magFilter)
        {

            WrapMode = wrapMode;
            MinFilter = minFilter;
            MagFilter = magFilter;
            
            GL.TexParameter(Target, TextureParameterName.TextureWrapS, (int)wrapMode);
            GL.TexParameter(Target, TextureParameterName.TextureWrapT, (int)wrapMode);
            GL.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)magFilter);
        }

        public void SetTextureData<T>(Span<T> data) where T : struct
        {
            GL.BindTexture(Target, Handle);
            GL.TexSubImage2D(Target, 0, 0, 0, Width, Height,
                             FormatMap[Format].Item2,
                             FormatMap[Format].Item3,
                             ref data[0]);
        }

        public void GenerateMipmaps()
        {
            GL.BindTexture(Target, Handle);
            GL.GenerateMipmap((GenerateMipmapTarget)Target);
        }

        public void Bind(int unit = 0)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + unit);
            GL.BindTexture(Target, Handle);
        }

        public void Dispose()
        {
            GL.DeleteTexture(Handle);
            Handle = 0;
        }

        // Advanced functionality
        public void SetWrapMode(TextureWrapMode wrapMode)
        {
            GL.BindTexture(Target, Handle);
            GL.TexParameter(Target, TextureParameterName.TextureWrapS, (int)wrapMode);
            GL.TexParameter(Target, TextureParameterName.TextureWrapT, (int)wrapMode);
            WrapMode = wrapMode;
        }

        public void SetMinFilter(TextureMinFilter filter)
        {
            GL.BindTexture(Target, Handle);
            GL.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)filter);
            MinFilter = filter;
        }

        public void SetMagFilter(TextureMagFilter filter)
        {
            GL.BindTexture(Target, Handle);
            GL.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)filter);
            MagFilter = filter;
        }

        public void SetBorderColor(Color4 color)
        {
            GL.BindTexture(Target, Handle);
            GL.TexParameter(Target, TextureParameterName.TextureBorderColor,
                new[] { color.R, color.G, color.B, color.A });
            BorderColor = color;
        }
        private void _BindedSetBorderColor(Color4 color)
        {
            GL.TexParameter(Target, TextureParameterName.TextureBorderColor, new[] { color.R, color.G, color.B, color.A });
        }
        public void Resize(int newWidth, int newHeight, bool preserveData = false)
        {
            if (Target != TextureTarget.Texture2D)
                throw new NotSupportedException("Resize only supported for 2D textures");
            if (Width == newWidth && Height == newHeight) return;

       
            if (preserveData)
            {
                int tempFBO = GL.GenFramebuffer();
                int tempTex = GL.GenTexture();
                try
                {
                   

                    GL.BindTexture(TextureTarget.Texture2D, tempTex);
                    GL.CopyImageSubData(
                        Handle, ImageTarget.Texture2D, 0, 0, 0, 0,
                        tempTex, ImageTarget.Texture2D, 0, 0, 0, 0,
                        Width, Height, 1
                    );

                    // Reallocate original texture storage
                    GL.BindTexture(TextureTarget.Texture2D, Handle);
                    var (internalFormat, pixelFormat, pixelType) = FormatMap[Format];
                    GL.TexImage2D(
                        TextureTarget.Texture2D, 0, (PixelInternalFormat)internalFormat,
                        newWidth, newHeight, 0, pixelFormat, pixelType, IntPtr.Zero
                    );


                    GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, tempFBO);
                    GL.FramebufferTexture2D(
                        FramebufferTarget.ReadFramebuffer,
                        FramebufferAttachment.ColorAttachment0,
                        TextureTarget.Texture2D, tempTex, 0
                    );

                    GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, Handle);
                    GL.BlitFramebuffer(
                        0, 0, Width, Height,
                        0, 0, newWidth, newHeight,
                        ClearBufferMask.ColorBufferBit,
                        BlitFramebufferFilter.Linear
                    );
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    GL.DeleteTexture(tempTex);
                    GL.DeleteFramebuffer(tempFBO);

                }
            }
            else
            {
                GL.BindTexture(Target, Handle);
                var (internalFormat, pixelFormat, pixelType) = FormatMap[Format];
                GL.TexImage2D(Target, 0, (PixelInternalFormat)internalFormat, newWidth, newHeight, 0,
                             pixelFormat, pixelType, IntPtr.Zero);
            }
            InitWithEmptyStorage();
            Width = newWidth;
            Height = newHeight;

        }
        public static Texture CreateDepthTexture(int width, int height)
        {
            var tex= new Texture(width, height, TextureFormat.Depth32F,
                wrapMode: TextureWrapMode.ClampToBorder,
                minFilter: TextureMinFilter.Linear,
                magFilter: TextureMagFilter.Linear);

            tex.SetBorderColor(Color4.White);
            return tex;
        }
    }
}
