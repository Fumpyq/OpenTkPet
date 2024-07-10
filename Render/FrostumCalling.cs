//using OpenTK.Mathematics;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Render
{
    public static class FrustumCalling
    {
        private static Vector4[] _planes = new Vector4[6];

        // Initialize the frustum planes
        public static void Initialize(Matrix4 viewProjectionMatrix)
        {
            // Extract the planes from the view-projection matrix
            // This is a common way to calculate frustum planes from a combined matrix
            _planes[0] = new Vector4(viewProjectionMatrix.M14 + viewProjectionMatrix.M11,
                                     viewProjectionMatrix.M24 + viewProjectionMatrix.M21,
                                     viewProjectionMatrix.M34 + viewProjectionMatrix.M31,
                                     viewProjectionMatrix.M44 + viewProjectionMatrix.M41);

            _planes[1] = new Vector4(viewProjectionMatrix.M14 - viewProjectionMatrix.M11,
                                     viewProjectionMatrix.M24 - viewProjectionMatrix.M21,
                                     viewProjectionMatrix.M34 - viewProjectionMatrix.M31,
                                     viewProjectionMatrix.M44 - viewProjectionMatrix.M41);

            _planes[2] = new Vector4(viewProjectionMatrix.M14 + viewProjectionMatrix.M12,
                                     viewProjectionMatrix.M24 + viewProjectionMatrix.M22,
                                     viewProjectionMatrix.M34 + viewProjectionMatrix.M32,
                                     viewProjectionMatrix.M44 + viewProjectionMatrix.M42);

            _planes[3] = new Vector4(viewProjectionMatrix.M14 - viewProjectionMatrix.M12,
                                     viewProjectionMatrix.M24 - viewProjectionMatrix.M22,
                                     viewProjectionMatrix.M34 - viewProjectionMatrix.M32,
                                     viewProjectionMatrix.M44 - viewProjectionMatrix.M42);

            _planes[4] = new Vector4(viewProjectionMatrix.M14 + viewProjectionMatrix.M13,
                                     viewProjectionMatrix.M24 + viewProjectionMatrix.M23,
                                     viewProjectionMatrix.M34 + viewProjectionMatrix.M33,
                                     viewProjectionMatrix.M44 + viewProjectionMatrix.M43);

            _planes[5] = new Vector4(viewProjectionMatrix.M14 - viewProjectionMatrix.M13,
                                     viewProjectionMatrix.M24 - viewProjectionMatrix.M23,
                                     viewProjectionMatrix.M34 - viewProjectionMatrix.M33,
                                     viewProjectionMatrix.M44 - viewProjectionMatrix.M43);

            // Normalize the planes (important for accurate distance calculations)
            for (int i = 0; i < 6; i++)
            {
                _planes[i] = Vector4.Normalize(_planes[i]);
            }
        }

        // Check if a point is inside the frustum
        public static bool IsPointInside(Vector3 point)
        {
            // Check if the point is on the positive side of all planes
            for (int i = 0; i < 6; i++)
            {
                if (Vector3.Dot(_planes[i].Xyz, point) + _planes[i].W < 0)
                {
                    return false; // Point is outside at least one plane
                }
            }
            return true; // Point is inside all planes
        }
        public static float Dot(Vector4 left, Vector3 right)
        {
            return left.X * right.X + left.Y * right.Y + left.Z * right.Z;
        }
        // Check if a sphere is inside the frustum
        public static bool IsSphereInside(Vector3 center, float radius)
        {
            // Check if the sphere's center is on the positive side of all planes, 
            // considering the sphere's radius
            for (int i = 0; i < 6; i++)
            {
                if (Dot(_planes[i], center) + _planes[i].W < -radius)
                {
                    return false; // Sphere is outside at least one plane
                }
            }
            return true; // Sphere is inside all planes
        }
    }
}
