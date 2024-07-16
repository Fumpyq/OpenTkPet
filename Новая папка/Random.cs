using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Новая_папка
{
    public static class Random
    {
        //private static Random random
        public static Vector3 InsideSphere(float minRadius, float maxRadius)
        {
            float radius = (float)System.Random.Shared.NextDouble() * (maxRadius - minRadius) + minRadius;

            // Generate random spherical coordinates (theta, phi)
            float theta = (float)System.Random.Shared.NextDouble() * 2 * MathF.PI;
            float phi = (float)System.Random.Shared.NextDouble() * MathF.PI;

            // Convert spherical coordinates to Cartesian coordinates
            float x = radius * MathF.Sin(phi) * MathF.Cos(theta);
            float y = radius * MathF.Sin(phi) * MathF.Sin(theta);
            float z = radius * MathF.Cos(phi);

            return new Vector3(x, y, z);
        }
    }
}
