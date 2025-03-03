﻿using OpenTK.Mathematics;
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
        public static Quaternion LookRotation( Vector3 forward, Vector3 up = default)
        {
            var d = Vector3.Dot(forward.Normalized(), up.Normalized());
            if (up == Vector3.Zero || d == 0)
            {
                up = Vector3.UnitY; // Use Y-axis as default 'up'
            }
            if (d == 1)
            {
                return Quaternion.FromAxisAngle(Vector3.UnitX,MathHelper.DegreesToRadians(90f));
            }
            return Matrix4.LookAt(Vector3.Zero, forward, up).ExtractRotation().Normalized();
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
