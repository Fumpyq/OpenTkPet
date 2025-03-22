using ConsoleApp1_Pet.Textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Render
{
    // ======================
    // Improved FrameBuffer System
    // ======================
    public enum AttachmentType
    {
        Color,
        Depth,
        Stencil,
        DepthStencil
    }

    public sealed class FrameBufferAttachment
    {
        public Texture Texture { get; }
        public AttachmentType Type { get; }
        public int AttachmentPoint { get; }

        public FrameBufferAttachment(Texture texture, AttachmentType type, int attachmentPoint = 0)
        {
            Texture = texture;
            Type = type;
            AttachmentPoint = attachmentPoint;
        }
    }

    public class FrameBuffer : IDisposable, IEnumerable<FrameBufferAttachment>, IList<FrameBufferAttachment>
    {
        private readonly List<FrameBufferAttachment> _attachments = new();
        public string Name { get; }
        public int Width;
        public int Height;
        public int Handle { get; private set; }

        public int Count => ((ICollection<FrameBufferAttachment>)_attachments).Count;

        public bool IsReadOnly => ((ICollection<FrameBufferAttachment>)_attachments).IsReadOnly;

        public FrameBufferAttachment this[int index] { get => ((IList<FrameBufferAttachment>)_attachments)[index]; set => ((IList<FrameBufferAttachment>)_attachments)[index] = value; }

        public FrameBuffer(string name, int width, int height)
        {
            Name = name;
            Width = width;
            Height = height;
            Handle = GL.GenFramebuffer();
            Initialize();
        }

        private void Initialize()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);

            foreach (var attachment in _attachments)
            {
                Attach(attachment);
            }

            CheckStatus();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void AddAttachment(FrameBufferAttachment attachment)
        {
            _attachments.Add(attachment);
            if (GL.IsFramebuffer(Handle))
                Attach(attachment);
        }

        private void Attach(FrameBufferAttachment attachment)
        {
            switch (attachment.Type)
            {
                case AttachmentType.Color:
                    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                        FramebufferAttachment.ColorAttachment0 + attachment.AttachmentPoint,
                        TextureTarget.Texture2D, attachment.Texture.id, 0);
                    break;

                case AttachmentType.Depth:
                    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                        FramebufferAttachment.DepthAttachment,
                        TextureTarget.Texture2D, attachment.Texture.id, 0);
                    break;

                    // Handle other attachment types...
            }
        }
        public Texture GetColorAttachment(int index = 0)
        {
            return _attachments
                .Where(a => a.Type == AttachmentType.Color)
                .ElementAtOrDefault(index)?.Texture;
        }

        public Texture GetDepthAttachment()
        {
            return _attachments
                .FirstOrDefault(a => a.Type == AttachmentType.Depth)?.Texture;
        }
        public void Resize(int width, int height)
        {
            Width = width;
            Height = height;

            foreach (var attachment in _attachments)
            {
                attachment.Texture.Resize(width, height);
            }

            GL.Viewport(0, 0, width, height);
        }
        public void SetDrawBuffers(params int[] colorAttachmentIndices)
        {
            using (new FrameBufferBinder(this))
            {
                if (colorAttachmentIndices.Length == 0)
                {
                    GL.DrawBuffer(DrawBufferMode.None);
                }
                else
                {
                    var buffers = colorAttachmentIndices
                        .Select(i => DrawBuffersEnum.ColorAttachment0 + i)
                        .ToArray();

                    GL.DrawBuffers(buffers.Length, buffers);
                }
            }
        }
        public IDisposable Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);
            GL.Viewport(0, 0, Width, Height);
            return new FrameBufferBinder(this);
        }

        private void CheckStatus()
        {
            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
            {
                throw new Exception($"Framebuffer {Name} incomplete: {status}");
            }
        }

        public void Dispose()
        {
            GL.DeleteFramebuffer(Handle);
            //foreach (var attachment in _attachments)
            //{
            //    attachment.Texture.Dispose();
            //}
        }

        public IEnumerator<FrameBufferAttachment> GetEnumerator()
        {
            return ((IEnumerable<FrameBufferAttachment>)_attachments).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_attachments).GetEnumerator();
        }

        public int IndexOf(FrameBufferAttachment item)
        {
            return ((IList<FrameBufferAttachment>)_attachments).IndexOf(item);
        }

        public void Insert(int index, FrameBufferAttachment item)
        {
            ((IList<FrameBufferAttachment>)_attachments).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<FrameBufferAttachment>)_attachments).RemoveAt(index);
        }

        public void Add(FrameBufferAttachment item)
        {
            ((ICollection<FrameBufferAttachment>)_attachments).Add(item);
        }

        public void Clear()
        {
            ((ICollection<FrameBufferAttachment>)_attachments).Clear();
        }

        public bool Contains(FrameBufferAttachment item)
        {
            return ((ICollection<FrameBufferAttachment>)_attachments).Contains(item);
        }

        public void CopyTo(FrameBufferAttachment[] array, int arrayIndex)
        {
            ((ICollection<FrameBufferAttachment>)_attachments).CopyTo(array, arrayIndex);
        }

        public bool Remove(FrameBufferAttachment item)
        {
            return ((ICollection<FrameBufferAttachment>)_attachments).Remove(item);
        }

        private class FrameBufferBinder : IDisposable
        {
            private readonly FrameBuffer _parent;
            public FrameBufferBinder(FrameBuffer parent) => _parent = parent;
            public void Dispose() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    }

    // ======================
    // Preconfigured Buffers
    // ======================
    //public static class FrameBufferPresets
    //{
    //    //public static FrameBuffer CreateGBuffer(int width, int height)
    //    //{
    //    //    var buffer = new FrameBuffer("GBuffer", width, height)
    //    //{
    //    //    new FrameBufferAttachment(
    //    //        new Texture(width, height, TextureFormat.RGB32F),
    //    //        AttachmentType.Color, 0),

    //    //    new FrameBufferAttachment(
    //    //        new Texture(width, height, TextureFormat.RGB16F),
    //    //        AttachmentType.Color, 1),

    //    //    new FrameBufferAttachment(
    //    //        new Texture(width, height, TextureFormat.RGBA8),
    //    //        AttachmentType.Color, 2),

    //    //    new FrameBufferAttachment(
    //    //        new Texture(width, height, TextureFormat.Depth32F),
    //    //        AttachmentType.Depth)
    //    //};

    //    //    return buffer;
    //    //}

    //    //public static FrameBuffer CreateShadowOrDepthMap(int width, int height)
    //    //{
    //    //    var buffer = new FrameBuffer("ShadowMap", width, height)
    //    //{
    //    //    new FrameBufferAttachment(
    //    //        new Texture(width, height, TextureFormat.Depth32F,wrapMode: TextureWrapMode.ClampToBorder),
    //    //        AttachmentType.Depth)
    //    //};

    //    //    GL.DrawBuffer(DrawBufferMode.None);
    //    //    GL.ReadBuffer(ReadBufferMode.None);

    //    //    return buffer;
    //    //}
    //}
}
