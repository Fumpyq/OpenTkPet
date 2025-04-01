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
    {    /// <summary>
         /// Creates a rotation matrix that orients the Z axis in the specified direction.
         /// </summary>
         /// <param name="forward">The direction the Z axis should point.</param>
         /// <param name="up">The approximate up direction.  This is used to calculate a true up direction perpendicular to forward.</param>
         /// <returns>A rotation matrix that aligns the Z axis with the specified direction.</returns>
        public static Matrix3 LookRotationMat(Vector3 forward, Vector3 up = default)
        {
            if (up == default)
            {
                up = Vector3.UnitY; // Use Y-axis as default 'up'
            }
            // Normalize the forward vector.  Important for stable results
            forward = forward.Normalized();

            // If forward and up are parallel, Cross product will be very small or zero.  Avoid divide by zero / NaNs.
            //  This is a tricky edge case that can happen frequently depending on the application.
            //  A good default direction to use in this case is Vector3.UnitY, but it really depends on the context.
            if (Vector3.Cross(forward, up).LengthSquared == 0)
            {
                //  Handle the case where the forward and up vectors are (nearly) parallel or antiparallel.
                //  If they are exactly opposite, the Cross product will be exactly zero.
                //  Choose a different up vector that is guaranteed to be different from forward
                up = Vector3.UnitY;
                if (Vector3.Cross(forward, up).LengthSquared == 0)
                {
                    up = Vector3.UnitX;  //If forward is (nearly) Vector3.UnitY or -Vector3.UnitY
                }

                // At this point, up should be safe to use.
            }


            Vector3 right = Vector3.Cross(up, forward).Normalized(); // Side vector
            up = Vector3.Cross(forward, right).Normalized(); // True up vector

            // Create rotation matrix
            Matrix3 rotation = new Matrix3(
                right.X, up.X, forward.X,
                right.Y, up.Y, forward.Y,
                right.Z, up.Z, forward.Z
            );

            return rotation;
        }

        /// <summary>
        /// Creates a quaternion that orients the Z axis in the specified direction.
        /// </summary>
        /// <param name="forward">The direction the Z axis should point.</param>
        /// <param name="up">The approximate up direction.  This is used to calculate a true up direction perpendicular to forward.</param>
        /// <returns>A quaternion that aligns the Z axis with the specified direction.</returns>
        public static Quaternion LookRotationQuat(Vector3 forward, Vector3 up = default)
        {
            if (up == default)
            {
                up = Vector3.UnitY; // Use Y-axis as default 'up'
            }
            // Implementation using Matrix3 and Quaternion.FromMatrix
            return Quaternion.FromMatrix(LookRotationMat(forward, up));

            //  Alternative Implementation -  Much faster!  But may require careful handling of edge cases.
            //  The edge cases are same as above with LookRotation.

            //forward = forward.Normalized();

            //Vector3 side = Vector3.Cross(forward, up).Normalized();
            //up = Vector3.Cross(forward, side).Normalized();

            //float w = (float)Math.Sqrt((1.0f + side.X + up.Y + forward.Z)) / 2.0f;
            //float w4 = (4.0f * w);
            //float x = (side.Y - up.Z) / w4;
            //float y = (forward.X - side.Z) / w4;
            //float z = (up.X - forward.Y) / w4;

            //return new Quaternion(x, y, z, w);

        }
        // Extension method to create a quaternion that rotates a transform
        // to look at a given target direction, similar to Unity's Quaternion.LookRotation
        public static Quaternion LookRotation(Vector3 forward, Vector3 up = default)
        {
            if (up == default)
            {
                up = Vector3.UnitY; // Use Y-axis as default 'up'
            }
            var d = Vector3.Dot(forward.Normalized(), up.Normalized());

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
