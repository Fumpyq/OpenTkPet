using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Новая_папка
{
    public static class MatrixExtensions
    {
        public static string ToTransformString(this Matrix4 model)
        {
            var Pos     =  model.ExtractTranslation();
            var Rot     =  model.ExtractRotation();
            var Scale   =  model.ExtractScale();
            return $"P:{Pos},Q:{Rot.ToEulerAngles()},S:{Scale}";

        }
        public static string ToStringShort(this Vector3 vec)
        {
            
            return $"({vec.X.ToString("f2")}, {vec.Y.ToString("f2")}, {vec.Z.ToString("f2")})";

        }
    }

    public static class QuaternionExtensions
    {
        // Extension method to create a quaternion that rotates a transform
        // to look at a given target direction, similar to Unity's Quaternion.LookRotation
        public static Quaternion LookRotation(Vector3 forward, Vector3 up = default)
        {
            var d = Vector3.Dot(forward.Normalized(), up.Normalized());
            if (up == Vector3.Zero || d == 0)
            {
                up = Vector3.UnitY; // Use Y-axis as default 'up'
            }
            if (d == 1)
            {
                return Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(90f));
            }
            return Matrix4.LookAt(Vector3.Zero, forward, up).ExtractRotation().Normalized();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Quaternion ExtractRotation(this Matrix4 Mat, bool rowNormalize = true)
        { // Assume Matrix4 is stored in column-major order with contiguous memory
            float* matrix = &Mat.Row0.X;
            {
                // 1. Direct memory access for matrix rows
                Vector3 vector = *(Vector3*)&matrix[0];
                Vector3 vector2 = *(Vector3*)&matrix[4];
                Vector3 vector3 = *(Vector3*)&matrix[8];

                // 2. Fast normalization using SIMD-enabled hardware intrinsics
                if (rowNormalize)
                {
                    vector = Vector3.Normalize(vector);
                    vector2 = Vector3.Normalize(vector2);
                    vector3 = Vector3.Normalize(vector3);
                }

                // 3. Precompute and cache all components
                float m00 = vector.X, m11 = vector2.Y, m22 = vector3.Z;
                float trace = m00 + m11 + m22;

                // 4. Branchless threshold check
                if (trace > 0.999f)
                {
                    // Handle identity/quaternion singularity case first
                    return Quaternion.Identity;
                }

                Quaternion result = default;

                // 5. Unified calculation with reduced branching
                float s = 2.0f * MathF.Sqrt(trace + 1.0f);
                float invS = 1.0f / s;

                result.W = 0.25f * s;
                result.X = (vector2.Z - vector3.Y) * invS;
                result.Y = (vector3.X - vector.Z) * invS;
                result.Z = (vector.Y - vector2.X) * invS;

                // 6. Fast reciprocal square root approximation for normalization
                float length = MathF.ReciprocalSqrtEstimate(result.W * result.W + result.X * result.X + result.Y * result.Y + result.Z * result.Z);
                result.W *= length;
                result.X *= length;
                result.Y *= length;
                result.Z *= length;

                return result;
            }
        }
    }

    public static class Vector3Extensions
    {        // Snaps a Vector3 to a grid with a given float size.
        public static Vector3 SnapToGrid(this Vector3 point, float gridSize)
        {
            return new Vector3(
                MathF.Round(point.X / gridSize) * gridSize,
                MathF.Round(point.Y / gridSize) * gridSize,
                MathF.Round(point.Z / gridSize) * gridSize
            );
        }

        // Snaps a Vector3 to a grid with a given Vector3 size.
        public static Vector3 SnapToGrid(this Vector3 point, Vector3 gridSize)
        {
            return new Vector3(
                MathF.Round(point.X / gridSize.X) * gridSize.X,
                MathF.Round(point.Y / gridSize.Y) * gridSize.Y,
                MathF.Round(point.Z / gridSize.Z) * gridSize.Z
            );
        }
        /// <summary>
        /// Transforms a Vector3 by a given Matrix4.
        /// This method is equivalent to OpenTK's Vector3.Transform(Matrix4).
        /// </summary>
        /// <param name="vector">The Vector3 to transform.</param>
        /// <param name="matrix">The Matrix4 to transform by.</param>
        /// <returns>The transformed Vector3.</returns>
        public static Vector3 Transform(this Vector3 vector, Matrix4 matrix)
        {
            // Extract the relevant components from the matrix
            float x = (matrix.M11 * vector.X) + (matrix.M12 * vector.Y) + (matrix.M13 * vector.Z) + matrix.M14;
            float y = (matrix.M21 * vector.X) + (matrix.M22 * vector.Y) + (matrix.M23 * vector.Z) + matrix.M24;
            float z = (matrix.M31 * vector.X) + (matrix.M32 * vector.Y) + (matrix.M33 * vector.Z) + matrix.M34;

            return new Vector3(x, y, z);
        }
        public static Vector3 Swap(this System.Numerics.Vector3 vector)
        {
            
            return new Vector3(vector.X,vector.Y,vector.Z);
        }
        public static System.Numerics.Vector3 Swap(this Vector3  vector)
        {

            return new System.Numerics.Vector3(vector.X, vector.Y, vector.Z);
        }
        public static Quaternion Swap(this System.Numerics.Quaternion q)
        {

            return new Quaternion(q.X,q.Y,q.Z,q.W);
        }
    }
}
