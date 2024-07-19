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
    public abstract class FrameBuffer
    {
        public Texture texture;
        public int id;
        public int Width;
        public int Height;
        public string name;

        public FrameBuffer(string name,int width, int height)
        {
            Width = width;
            Height = height;
            this.name = name;   
            Init();
            
        }

        public abstract void Init();
        public virtual void Resize(int width, int height)
        {
            texture.Resize(width, height);
            Width = width; Height = height;
        }
        public void Use()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);
            texture.Use();
        }
        public static implicit operator Texture(FrameBuffer t)
        {
            return t.texture;
        }
    }

    public class DepthBuffer : FrameBuffer
    {
        public DepthBuffer(string name, int width, int height) : base(name, width, height)
        {
        }
        public override void Resize(int width, int height)
        {
            base.Resize(width, height);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);
            GL.BindTexture(TextureTarget.Texture2D, texture.id);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent,
                           width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        }

        public override void Init()
        {
            
                int fbo = GL.GenFramebuffer();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
                GL.ObjectLabel(ObjectLabelIdentifier.Framebuffer, fbo, name.Length, name);
                // Create the depth texture
                int depthTexture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, depthTexture);
                GL.ObjectLabel(ObjectLabelIdentifier.Texture, fbo, name.Length, name);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent,
                               Game.instance.ClientSize.X, Game.instance.ClientSize.Y, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            // GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            //  GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthTexture, 0);
                
                // Check if FBO is complete
                FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
                if (status != FramebufferErrorCode.FramebufferComplete)
                {
                    Console.WriteLine("Framebuffer incomplete!");
                }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            texture = new Texture(Game.instance.ClientSize.X, Game.instance.ClientSize.Y, depthTexture);
            id = fbo;
            // Disable color buffer writing
            //GL.ColorMask(false, false, false, false);

        }


    }

    public class ScreenBuffer : FrameBuffer
    {
        public ScreenBuffer(string name, int width, int height) : base(name, width, height)
        {
        }
        public override void Resize(int width, int height)
        {
            base.Resize(width, height);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);
            GL.BindTexture(TextureTarget.Texture2D, texture.id);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
                           width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        }

        public override void Init()
        {

            int fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.ObjectLabel(ObjectLabelIdentifier.Framebuffer, fbo, name.Length, name);
            // Create the depth texture
            int tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);
            GL.ObjectLabel(ObjectLabelIdentifier.Texture, fbo, name.Length, name);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
                           Game.instance.ClientSize.X, Game.instance.ClientSize.Y, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            // GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            //  GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, tex, 0);

            // Check if FBO is complete
            FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
            {
                Console.WriteLine("Framebuffer incomplete!");
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            texture = new Texture(Game.instance.ClientSize.X, Game.instance.ClientSize.Y, tex);
            id = fbo;
            // Disable color buffer writing
            //GL.ColorMask(false, false, false, false);

        }


    }
}
