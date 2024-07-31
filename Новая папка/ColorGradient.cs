using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Новая_папка
{
    public static class ColorGradient
    {
        public static Rgba32 GetGradientColor(
            this IEnumerable<(float Value, Rgba32 Color)> gradientSource,
            float value)
        {
            // Find the two closest gradient points.
            var (lowerValue, lowerColor) = gradientSource.LastOrDefault(x => x.Value <= value);
            var (upperValue, upperColor) = gradientSource.FirstOrDefault(x => x.Value >= value);

            // If we're outside the gradient range, return the closest color.
            if (lowerValue == default && upperValue == default)
            {
                return lowerColor;
            }
            else if (lowerValue == default)
            {
                return upperColor;
            }
            else if (upperValue == default)
            {
                return lowerColor;
            }

            // Calculate the interpolation factor.
            var factor = (value - lowerValue) / (upperValue - lowerValue);

            // Interpolate the color components.
            var r = (byte)(lowerColor.R * (1 - factor) + upperColor.R * factor);
            var g = (byte)(lowerColor.G * (1 - factor) + upperColor.G * factor);
            var b = (byte)(lowerColor.B * (1 - factor) + upperColor.B * factor);
            var a = (byte)(lowerColor.A * (1 - factor) + upperColor.A * factor);

            // Return the interpolated color.
            return new Rgba32(r, g, b, a);
        }
        public static Rgba32 GetLowerColor(
    this IEnumerable<(float Value, Rgba32 Color)> gradientSource,
    float value)
        {
            // Find the two closest gradient points.
            var (lowerValue, lowerColor) = gradientSource.LastOrDefault(x => x.Value <= value);
            var (upperValue, upperColor) = gradientSource.FirstOrDefault(x => x.Value >= value);

            // If we're outside the gradient range, return the closest color.
            if (lowerValue == default && upperValue == default)
            {
                return lowerColor;
            }
            else if (lowerValue == default)
            {
                return upperColor;
            }
            else if (upperValue == default)
            {
                return lowerColor;
            }

            // Calculate the interpolation factor.
         

            // Return the interpolated color.
            return lowerColor;
        }
        public static Rgba32 GetCloseColor(
this IEnumerable<(float Value, Rgba32 Color)> gradientSource,
float value)
        {
            // Find the two closest gradient points.
            var (lowerValue, lowerColor) = gradientSource.LastOrDefault(x => x.Value <= value);
            var (upperValue, upperColor) = gradientSource.FirstOrDefault(x => x.Value >= value);

            // If we're outside the gradient range, return the closest color.
            if (lowerValue == default && upperValue == default)
            {
                return lowerColor;
            }
            else if (lowerValue == default)
            {
                return upperColor;
            }
            else if (upperValue == default)
            {
                return lowerColor;
            }

            var dif1 = MathF.Abs(lowerValue - value); var dif2 = MathF.Abs(upperValue - value);
            if (dif1 < dif2) return lowerColor;
            return upperColor;
        }
        public static Rgba32 GetPiorityLerpColor(
this IEnumerable<(float Value, Rgba32 Color)> gradientSource,
float value
            , float priority)
        {
            var (lowerValue, lowerColor) = gradientSource.LastOrDefault(x => x.Value <= value);
            var (upperValue, upperColor) = gradientSource.FirstOrDefault(x => x.Value >= value);

            // If we're outside the gradient range, return the closest color.
            if (lowerValue == default && upperValue == default)
            {
                return lowerColor;
            }
            else if (lowerValue == default)
            {
                return upperColor;
            }
            else if (upperValue == default)
            {
                return lowerColor;
            }

            // Calculate the interpolation factor.
            var factor = (value - lowerValue) / (upperValue - lowerValue);
            // factor = MathF.Pow(factor, priority) * (1 - MathF.Pow(1 - factor, 1 - priority));
            /// var sigmoidFactor = 1 / (1 + Math.Exp(-(factor - 0.5f) * 10 * (priority - 0.5f)));
            var easeInOutFactor = factor < 0.5f
            ? 4 * factor * factor * factor
    : (factor - 1) * (2 * factor - 2) * (2 * factor - 2) + 1;

            // Adjust factor based on priority
           // easeInOutFactor = easeInOutFactor * priority + (1 - easeInOutFactor) * (1 - priority);

            // Interpolate the color components.
            var r = (byte)(lowerColor.R * (1 - easeInOutFactor) + upperColor.R * (easeInOutFactor));
            var g = (byte)(lowerColor.G * (1 - easeInOutFactor) + upperColor.G * (easeInOutFactor));
            var b = (byte)(lowerColor.B * (1 - easeInOutFactor) + upperColor.B * (easeInOutFactor));
            var a = (byte)(lowerColor.A * (1 - easeInOutFactor) + upperColor.A * (easeInOutFactor));

            // Return the interpolated color.
            return new Rgba32(r, g, b, a);
        }
        // Helper method to convert Color to Rgba32.
        public static Rgba32 ToRgba32(this Color color)
        {
            return new Rgba32(color.R, color.G, color.B, color.A);
        }
    }
}
