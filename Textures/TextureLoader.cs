using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Processing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.Fonts;

namespace ConsoleApp1_Pet.Textures
{
    public enum TexturePreset
    {
        Auto,
        Albedo,
        Normal,
        Metallic,
        Roughness,
        Height,
        AmbientOcclusion,
        Emissive,
        UI
    }
    public static class TextureLoaderExtensions
    {
        private static readonly Dictionary<string, TexturePreset> _pathKeywords = new()
    {
        { "_albedo", TexturePreset.Albedo },
        { "_basecolor", TexturePreset.Albedo },
        { "_normal", TexturePreset.Normal },
        { "_nrm", TexturePreset.Normal },
        { "_metal", TexturePreset.Metallic },
        { "_roughness", TexturePreset.Roughness },
        { "_rgh", TexturePreset.Roughness },
        { "_height", TexturePreset.Height },
        { "_disp", TexturePreset.Height },
        { "_ao", TexturePreset.AmbientOcclusion },
        { "_emissive", TexturePreset.Emissive },
        { "_emi", TexturePreset.Emissive }
    };

        public static void LoadFromFile(this Texture texture, string path,
       bool preserveParams = true, bool generateMipmaps = false)
        {
            try
            {
                // Save current parameters
                var originalWrap = texture.WrapMode;
                var originalMin = texture.MinFilter;
                var originalMag = texture.MagFilter;
                var originalBorder = texture.BorderColor;

                using var image = Image.Load(path);
           
                // Resize texture if needed
                if (texture.Width != image.Width || texture.Height != image.Height)
                {
                    texture.Resize(image.Width, image.Height, preserveData: false);
                }

                // Upload pixel data with automatic conversion
                image.Mutate(x => x.Flip(FlipMode.Vertical));
                UploadImageData(texture, image);

                // Restore parameters if requested
                if (preserveParams)
                {
                    texture.SetWrapMode(originalWrap);
                    texture.SetMinFilter(originalMin);
                    texture.SetMagFilter(originalMag);
                    if (originalWrap == TextureWrapMode.ClampToBorder)
                    {
                        texture.SetBorderColor(originalBorder);
                    }
                }

                if (generateMipmaps)
                    texture.GenerateMipmaps();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ReplaceWithCheckers(texture);
               
            }
        }
        private static void ReplaceWithCheckers(Texture tex)
        {
            TextureGenerator.Checkerboard(tex, 1, Color.Purple, Color.DarkGray);
        }
        private static Texture CreateTexture(Image image,
       (TextureFormat format, TextureWrapMode wrap,
        TextureMinFilter minFilter, Color4? border) config,
       bool generateMipmaps)
        {
            var texture = new Texture(
                image.Width,
                image.Height,
                config.format,
                config.wrap,
                config.minFilter,
                borderColor: config.border
            );

            UploadImageData(texture, image);

            if (generateMipmaps)
                texture.GenerateMipmaps();

            return texture;
        }

        private static void UploadImageData(Texture texture, Image image)
        {
            switch (image)
            {
                case Image<Rgba32> rgbaImage:
                    texture.LoadImageData(rgbaImage);
                    break;
                case Image<Rgb24> rgbImage:
                    texture.LoadImageData(rgbImage);
                    break;
                case Image<L8> grayImage:
                    texture.LoadImageData(grayImage);
                    break;
                case Image<La16> laImage:
                    texture.LoadImageData(laImage);
                    break;
                default:
                    // Convert unsupported formats to RGBA32
                    var converted = image.CloneAs<Rgba32>();
                    texture.LoadImageData(converted);
                    break;
            }
        }

        public static Texture CreateTextureFromFile(
            string path,
            TexturePreset preset = TexturePreset.Auto,
            bool generateMipmaps = true)
        {
            try
            {
                using var image = Image.Load(path);
                return CreateTextureFromImage(image, path, preset, generateMipmaps);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return TextureGenerator.Checkerboard(128, 128 / 8, Color.Violet, Color.DarkGray);
            }
            
        }

        public static Texture CreateTextureFromImage(
            Image image,
            string pathHint = "",
            TexturePreset preset = TexturePreset.Auto,
            bool generateMipmaps = true)
        {
            if (preset == TexturePreset.Auto)
            {
                preset = DetectPresetFromPath(pathHint) ?? DetectPresetFromImage(image);
            }

            var config = GetPresetConfiguration(preset, image, generateMipmaps);
            return CreateTexture(image, config, generateMipmaps);
        }

        private static TexturePreset? DetectPresetFromPath(string path)
        {
            var lowerPath = path.ToLowerInvariant();
            foreach (var (key, preset) in _pathKeywords)
            {
                if (lowerPath.Contains(key)) return preset;
            }
            return null;
        }

        private static TexturePreset DetectPresetFromImage(Image image) => image switch
        {
            Image<Rgba32> => TexturePreset.Albedo,
            Image<Rgb24> => TexturePreset.Albedo,
            Image<L8> => TexturePreset.Height,
            Image<La16> => TexturePreset.AmbientOcclusion,
            _ => TexturePreset.Albedo
        };

        private static (TextureFormat format, TextureWrapMode wrap,
                        TextureMinFilter minFilter, Color4? border)
            GetPresetConfiguration(TexturePreset preset, Image image, bool mipmaps)
        {
            return preset switch
            {
                TexturePreset.Albedo => (
                    image is Image<Rgba32> ? TextureFormat.SRGBA8 : TextureFormat.SRGB8,
                    TextureWrapMode.Repeat,
                    mipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear,
                    null
                ),

                TexturePreset.Normal => (
                    TextureFormat.RGB8,
                    TextureWrapMode.ClampToEdge,
                    TextureMinFilter.Linear,
                    null
                ),

                TexturePreset.Metallic or TexturePreset.Roughness => (
                    TextureFormat.R8,
                    TextureWrapMode.Repeat,
                    TextureMinFilter.Linear,
                    null
                ),

                TexturePreset.Height => (
                    TextureFormat.R8,
                    TextureWrapMode.Repeat,
                    mipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear,
                    null
                ),

                TexturePreset.Emissive => (
                    TextureFormat.SRGB8,
                    TextureWrapMode.ClampToEdge,
                    TextureMinFilter.Linear,
                    null
                ),

                TexturePreset.UI => (
                    TextureFormat.SRGBA8,
                    TextureWrapMode.ClampToEdge,
                    TextureMinFilter.Linear,
                    Color4.Transparent
                ),

                _ => (
                    image is Image<Rgba32> ? TextureFormat.SRGBA8 : TextureFormat.SRGB8,
                    TextureWrapMode.Repeat,
                    TextureMinFilter.Linear,
                    null
                )
            };
        }



        // Preset-specific quickload methods
        public static Texture LoadAsAlbedo(string path, bool mipmaps = true)
            => CreateTextureFromFile(path, TexturePreset.Albedo, mipmaps);

        public static Texture LoadAsNormalMap(string path)
            => CreateTextureFromFile(path, TexturePreset.Normal, generateMipmaps: false);

        public static Texture LoadAsMetallicMap(string path)
            => CreateTextureFromFile(path, TexturePreset.Metallic);

        public static Texture LoadAsHeightMap(string path, bool mipmaps = true)
            => CreateTextureFromFile(path, TexturePreset.Height, mipmaps);

        //public static Texture LoadAsUIElement(string path)
        //    => CreateTextureFromFile(path, TexturePreset.UI);
        //public static Texture CreateTextureFromFile(
        //    string path,
        //    TextureWrapMode wrapMode = TextureWrapMode.Repeat,
        //    TextureMinFilter minFilter = TextureMinFilter.Linear,
        //    TextureMagFilter magFilter = TextureMagFilter.Linear,
        //    bool generateMipmaps = false,
        //    bool srgb = true)
        //{
        //    using var image = Image.Load(path);
        //    return CreateTextureFromImage(image, wrapMode, minFilter, magFilter, generateMipmaps, srgb);
        //}

        public static Texture CreateTextureFromImage(
            Image image,
            TextureWrapMode wrapMode = TextureWrapMode.Repeat,
            TextureMinFilter minFilter = TextureMinFilter.Linear,
            TextureMagFilter magFilter = TextureMagFilter.Linear,
            bool generateMipmaps = false,
            bool srgb = true)
        {
            // Flip image vertically for OpenGL coordinate system
            image.Mutate(x => x.Flip(FlipMode.Vertical));

            // Determine texture format based on image type
            var format = image switch
            {
                Image<Rgba32> => srgb ? TextureFormat.SRGBA8 : TextureFormat.RGBA8,
                Image<Rgb24> => srgb ? TextureFormat.SRGB8 : TextureFormat.RGB8,
                Image<L8> => TextureFormat.R8,
                Image<La16> => TextureFormat.RG8,
                _ => throw new NotSupportedException($"Unsupported pixel format: {image.GetType().Name}")
            };

            // Create texture with determined format
            var texture = new Texture(image.Width, image.Height, format,
                wrapMode, minFilter, magFilter,null, generateMipmaps);

            // Upload pixel data
            switch (image)
            {
                case Image<Rgba32> rgbaImg:
                    UploadPixelData(texture, rgbaImg);
                    break;
                case Image<Rgb24> rgbImg:
                    UploadPixelData(texture, rgbImg);
                    break;
                case Image<L8> grayImg:
                    UploadPixelData(texture, grayImg);
                    break;
                case Image<La16> laImg:
                    UploadPixelData(texture, laImg);
                    break;
            }

            if (generateMipmaps)
                texture.GenerateMipmaps();

            return texture;
        }

        private static void UploadPixelData<T>(Texture texture, Image<T> image) where T : unmanaged, IPixel<T>
        {
            var pixels = new T[image.Width * image.Height];
            image.CopyPixelDataTo(pixels);
            texture.SetTextureData(pixels);
        }

        //public static void LoadImageData<T>(
        //    this Texture texture,
        //    string path,
        //    bool generateMipmaps = false,
        //    bool flipVertical = true) where T : unmanaged, IPixel<T>
        //{
        //    using var image = Image.Load<T>(path);
        //    LoadImageData(texture, image, generateMipmaps, flipVertical);
        //}

        public static void LoadImageData<T>(
            this Texture texture,
            Image<T> image,
            bool generateMipmaps = false,
            bool flipVertical = true) where T : unmanaged, IPixel<T>
        {
            if (flipVertical)
                image.Mutate(x => x.Flip(FlipMode.Vertical));

            if (texture.Width != image.Width || texture.Height != image.Height)
                texture.Resize(image.Width, image.Height);

            var pixels = new T[image.Width * image.Height];
            image.CopyPixelDataTo(pixels);
            texture.SetTextureData(pixels);

            if (generateMipmaps)
                texture.GenerateMipmaps();
        }
    }
}
