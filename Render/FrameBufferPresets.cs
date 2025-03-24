using ConsoleApp1_Pet.Textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Render
{
    public static class FrameBufferPresets
    {
        public static FrameBuffer CreateGBuffer(int width, int height)
        {
            var buffer = new FrameBuffer("GBuffer", width, height)
            {
                // Position (RGB32F)
                new FrameBufferAttachment(
                    new Texture(width, height, TextureFormat.RGB32F),
                    AttachmentType.Color, 0),

                // Normal (RGB16F)
                new FrameBufferAttachment(
                    new Texture(width, height, TextureFormat.RGB16F),
                    AttachmentType.Color, 1),

                // Albedo + Specular (RGBA8)
                new FrameBufferAttachment(
                    new Texture(width, height, TextureFormat.RGBA8),
                    AttachmentType.Color, 2),

                // Depth (24-bit)
                new FrameBufferAttachment(
                    new Texture(width, height, TextureFormat.Depth24),
                    AttachmentType.Depth)
            };
            
            //buffer.SetDrawBuffers(0, 1, 2);
            buffer.Initialize();
            return buffer;
        }

        public static FrameBuffer CreateShadowOrDepthMap(int width, int height)
        {
            var buffer = new FrameBuffer("ShadowMap", width, height)
                {
                    new FrameBufferAttachment(
                        new Texture(width, height, TextureFormat.Depth32F,
                            wrapMode: TextureWrapMode.ClampToBorder,
                            borderColor: Color4.White
                        ),
                        AttachmentType.Depth)
                };
         
            //buffer.SetDrawBuffers();
            buffer.Initialize();
            return buffer;
        }

        public static FrameBuffer CreateHDRBuffer(int width, int height)
        {
            var buffer = new FrameBuffer("HDR", width, height)
        {
            new FrameBufferAttachment(
                new Texture(width, height, TextureFormat.RGBA16F),
                AttachmentType.Color, 0),

            new FrameBufferAttachment(
                new Texture(width, height, TextureFormat.Depth24),
                AttachmentType.Depth)
        };
            buffer.Initialize();
            return buffer;
        }

        public static FrameBuffer CreatePostProcessing(int width, int height)
        {
            var buffer=  new FrameBuffer("PostProcessing", width, height)
            {
                new FrameBufferAttachment(
                    new Texture(width, height, TextureFormat.RGBA8),
                    AttachmentType.Color, 0)
            };
            buffer.Initialize();
            return buffer;
        }

        public static FrameBuffer CreateMultiSampled(int width, int height, int samples = 4)
        {
            var buffer = new FrameBuffer("MultiSampled", width, height);

            var colorTex = new Texture(width, height, TextureFormat.RGBA8)
                .SetMultiSampled(samples);

            var depthTex = new Texture(width, height, TextureFormat.Depth24)
                .SetMultiSampled(samples);

            buffer.AddAttachment(new FrameBufferAttachment(colorTex, AttachmentType.Color, 0));
            buffer.AddAttachment(new FrameBufferAttachment(depthTex, AttachmentType.Depth));
            buffer.Initialize();
            return buffer;
        }

        public static FrameBuffer CreateBasic(int width, int height)
        {
            var buffer= new FrameBuffer("Basic", width, height)
        {
            new FrameBufferAttachment(
                new Texture(width, height, TextureFormat.RGBA8),
                AttachmentType.Color, 0),

            new FrameBufferAttachment(
                new Texture(width, height, TextureFormat.Depth24),
                AttachmentType.Depth)
        };
            buffer.Initialize();
            return buffer;
        }
    }

    // Texture extension for multisampling
    public static class TextureExtensions
    {
        public static Texture SetMultiSampled(this Texture tex, int samples)
        {
            GL.BindTexture(TextureTarget.Texture2DMultisample, tex.Handle);
            GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample,
                samples,
                (PixelInternalFormat)Texture.FormatMap[tex.Format].Item1,
                tex.Width,
                tex.Height,
                true);
            return tex;
        }
    }
}
