//using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Render
{
    public static class FrustumCalling
    {
        // Define the frustum planes
        public static Plane[] Planes { get; private set; }

        // Initialize the frustum planes
        public static void Initialize(OpenTK.Mathematics.Matrix4 viewProjectionMatrix)
        {
            if (Planes ==null)
             Planes = new Plane[6];
            // Extract the planes from the view-projection matrix
            // This is a common way to calculate frustum planes from a combined matrix
            Planes[0] = new Plane(viewProjectionMatrix.M14 + viewProjectionMatrix.M11,
                                    viewProjectionMatrix.M24 + viewProjectionMatrix.M21,
                                    viewProjectionMatrix.M34 + viewProjectionMatrix.M31,
                                    viewProjectionMatrix.M44 + viewProjectionMatrix.M41);

            Planes[1] = new Plane(viewProjectionMatrix.M14 - viewProjectionMatrix.M11,
                                    viewProjectionMatrix.M24 - viewProjectionMatrix.M21,
                                    viewProjectionMatrix.M34 - viewProjectionMatrix.M31,
                                    viewProjectionMatrix.M44 - viewProjectionMatrix.M41);

            Planes[2] = new Plane(viewProjectionMatrix.M14 + viewProjectionMatrix.M12,
                                    viewProjectionMatrix.M24 + viewProjectionMatrix.M22,
                                    viewProjectionMatrix.M34 + viewProjectionMatrix.M32,
                                    viewProjectionMatrix.M44 + viewProjectionMatrix.M42);

            Planes[3] = new Plane(viewProjectionMatrix.M14 - viewProjectionMatrix.M12,
                                    viewProjectionMatrix.M24 - viewProjectionMatrix.M22,
                                    viewProjectionMatrix.M34 - viewProjectionMatrix.M32,
                                    viewProjectionMatrix.M44 - viewProjectionMatrix.M42);

            Planes[4] = new Plane(viewProjectionMatrix.M14 + viewProjectionMatrix.M13,
                                    viewProjectionMatrix.M24 + viewProjectionMatrix.M23,
                                    viewProjectionMatrix.M34 + viewProjectionMatrix.M33,
                                    viewProjectionMatrix.M44 + viewProjectionMatrix.M43);

            Planes[5] = new Plane(viewProjectionMatrix.M14 - viewProjectionMatrix.M13,
                                    viewProjectionMatrix.M24 - viewProjectionMatrix.M23,
                                    viewProjectionMatrix.M34 - viewProjectionMatrix.M33,
                                    viewProjectionMatrix.M44 - viewProjectionMatrix.M43);

            // Normalize the planes (important for accurate distance calculations)
            for (int i = 0; i < 6; i++)
            {
              Planes[i] = Plane.Normalize(Planes[i]);
            }
        }

        // Check if a point is inside the frustum
        public static bool IsPointInside(Vector3 point)
        {
            // Check if the point is on the positive side of all planes
            foreach (Plane plane in Planes)
            {
                if (Vector3.Dot(plane.Normal, point) + plane.D < 0)
                {
                    return false; // Point is outside at least one plane
                }
            }
            return true; // Point is inside all planes
        }

        // Check if a sphere is inside the frustum
        public static bool IsSphereInside(Vector3 center, float radius)
        {
            // Check if the sphere's center is on the positive side of all planes, 
            // considering the sphere's radius
            foreach (Plane plane in Planes)
            {
                if (Vector3.Dot(plane.Normal, center) + plane.D < -radius)
                {
                    return false; // Sphere is outside at least one plane
                }
            }
            return true; // Sphere is inside all planes
        }

        internal static bool IsSphereInside(OpenTK.Mathematics.Vector3 position, float radius)
        {
            var center = new Vector3(position.X, position.Y, position.Z);
            // considering the sphere's radius
            foreach (Plane plane in Planes)
            {
                if (Vector3.Dot(plane.Normal, center) + plane.D < -radius)
                {
                    return false; // Sphere is outside at least one plane
                }
            }
            return true; // Sphere is inside all planes
        }
    }
}
