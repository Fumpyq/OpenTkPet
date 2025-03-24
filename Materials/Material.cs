using ConsoleApp1_Pet.Render;
using ConsoleApp1_Pet.Shaders;
using ConsoleApp1_Pet.Textures;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApp1_Pet.Materials
{
    public sealed class Material : IDisposable, ICloneable
    {
        public static int idCounter;
        public int id;
        private readonly Dictionary<string, UniformValue> _uniforms = new();
        private readonly Dictionary<string, TextureBinding> _textures = new();
        private Shader _shader;
        private bool _disposed;
        private readonly Dictionary<string, Func<object>> _dynamicUniforms = new();
        public event Action<Material> OnUpdated;

        public Shader Shader
        {
            get => _shader;
            set
            {
                _shader = value ?? throw new ArgumentNullException(nameof(value));
                Invalidate();
            }
        }
 

        public Material AddDynamicUniform<T>(string name, Func<object> valueProvider)
        {
            _dynamicUniforms[name] =  valueProvider;
            return this;
        }
        public Material(Shader shader)
        {
            id = Interlocked.Increment(ref idCounter);
            _shader = shader ?? throw new ArgumentNullException(nameof(shader));
        }

        public Material SetUniform<T>(string name, T value) where T : unmanaged
        {
            _uniforms[name] = UniformValue.Create(value);
            Invalidate();
            return this;
        }

        public Material SetTexture(Texture texture, string name= "texture0",  int unit = -1)
        {
            _textures[name] = new TextureBinding(
                texture ?? throw new ArgumentNullException(nameof(texture)),
                unit
            );
            Invalidate();
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Use()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Material));

            Shader.Use();

            foreach (var (name, provider) in _dynamicUniforms)
            {
                dynamic value = provider();
                switch (value)
                {
                    case Matrix4 mat4:
                        SetUniform(name, mat4);
                        break;
                    case Texture tex:
                        SetTexture(tex,name);
                        break;
                    case float f:
                        SetUniform(name, f);
                        break;
                    case Vector3 v:
                        Shader.SetUniform(name, v);
                        break;
                        // Add other types as needed
                }
            }

            ApplyUniforms();
            BindTextures();
        }

        private void ApplyUniforms()
        {
            foreach (var (name, value) in _uniforms)
            {
                value.Apply(Shader, name);
            }
        }

        private void BindTextures()
        {
            foreach (var (name, binding) in _textures)
            {
                int unit = binding.Unit >= 0 ? binding.Unit : Shader.GetTextureUnit(name);
                binding.Texture.Bind(unit);
                Shader.SetUniform(name, unit);
            }
        }
        public Material SetFrameBufferAttachment(string uniformName, FrameBuffer buffer,
       AttachmentType type = AttachmentType.Color, int index = 0)
        {
            var texture = type switch
            {
                AttachmentType.Color => buffer.GetColorAttachment(index),
                AttachmentType.Depth => buffer.GetDepthAttachment(),
                _ => throw new ArgumentOutOfRangeException()
            };

            return SetTexture(texture, uniformName);
        }
        public Material Clone()
        {
            var clone = new Material(Shader);

            foreach (var (name, value) in _uniforms)
                clone._uniforms[name] = value.Clone();

            foreach (var (name, binding) in _textures)
                clone._textures[name] = binding.Clone();

            return clone;
        }

        private void Invalidate() => OnUpdated?.Invoke(this);

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            OnUpdated = null;
            // Add texture/shader disposal logic if needed
        }

        object ICloneable.Clone() => Clone();

        private readonly struct TextureBinding(Texture texture, int unit)
        {
            public Texture Texture { get; } = texture;
            public int Unit { get; } = unit;

            public TextureBinding Clone() => new(Texture, Unit);
        }

        private abstract class UniformValue
        {
            public static UniformValue Create<T>(T value) where T : unmanaged =>
                typeof(T) switch
                {
                    _ when typeof(T) == typeof(float) => new FloatValue(Unsafe.As<T, float>(ref value)),
                    _ when typeof(T) == typeof(int) => new IntValue(Unsafe.As<T, int>(ref value)),
                    _ when typeof(T) == typeof(Matrix4) => new Matrix4Value(Unsafe.As<T, Matrix4>(ref value)),
                    _ => throw new NotSupportedException($"Uniform type {typeof(T)} not supported")
                };

            public abstract void Apply(Shader shader, string name);
            public abstract UniformValue Clone();

            private sealed class FloatValue(float value) : UniformValue
            {
                public override void Apply(Shader shader, string name) => shader.SetUniform(name, value);
                public override UniformValue Clone() => new FloatValue(value);
            }

            private sealed class IntValue(int value) : UniformValue
            {
                public override void Apply(Shader shader, string name) => shader.SetUniform(name, value);
                public override UniformValue Clone() => new IntValue(value);
            }

            private sealed class Matrix4Value(Matrix4 value) : UniformValue
            {
                public override void Apply(Shader shader, string name) => shader.SetUniform(name, value);
                public override UniformValue Clone() => new Matrix4Value(value);
            }
        }
    }


}
