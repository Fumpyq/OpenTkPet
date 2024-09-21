using BepuPhysics;
using ConsoleApp1_Pet.Architecture;
using ConsoleApp1_Pet.Render;
using ConsoleApp1_Pet.Новая_папка;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ConsoleApp1_Pet.Architecture.GameObject;

namespace ConsoleApp1_Pet.Scripts.DebugScripts
{
    public class DebugDrawer
    {
        public static Queue<BoundingBox> CubesPool;

       
        public static BoundingBox GetBoundingBox(Bounds bounds, float lineWidth)
        {
            GameObject gg = new GameObject("BoundingBox",bounds.min,Vector3.Zero);
            BoundingBox bb = new BoundingBox();
            bb.parent = gg;
            bb.Bounds = bounds;
            void CreateCubeEdge(Vector3 start, Vector3 end)
            {
                // Calculate the edge direction
                Vector3 direction = end - start;

                if (direction == new Vector3(0, 1, 0))
                {

                }

                // Calculate the cube size
                Vector3 cubeSize = new Vector3(lineWidth, lineWidth, direction.Length);

                // Create a new cube game object
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

                // Set the cube's position
                cube.transform.position = start + direction * 0.5f;

                // Set the cube's scale
                cube.transform.LocalScale = cubeSize;

                // Set the cube's rotation
                cube.transform.rotation = QuaternionExtensions.LookRotation(direction, Vector3.UnitY);
                cube.transform.parent = gg.transform;
                
            }
            Vector3 origin = bounds.min;
            Vector3 size = bounds.size;
           // Bot face
           CreateCubeEdge(origin, origin + new Vector3(size.X, 0, 0));
           CreateCubeEdge(origin + new Vector3(size.X, 0, 0), origin + new Vector3(size.X, 0, size.Z));
           CreateCubeEdge(origin + new Vector3(size.X, 0, size.Z), origin + new Vector3(0, 0, size.Z));
           CreateCubeEdge(origin + new Vector3(0, 0, size.Z), origin);

            // Top face
            CreateCubeEdge(origin + new Vector3(0, size.Y, 0), origin + new Vector3(size.X, size.Y, 0));
            CreateCubeEdge(origin + new Vector3(size.X, size.Y, 0), origin + new Vector3(size.X, size.Y, size.Z));
            CreateCubeEdge(origin + new Vector3(size.X, size.Y, size.Z), origin + new Vector3(0, size.Y, size.Z));
            CreateCubeEdge(origin + new Vector3(0, size.Y, size.Z), origin + new Vector3(0, size.Y, 0));

            // Connecting edges
            CreateCubeEdge(origin, origin + new Vector3(0, size.Y, 0));
            CreateCubeEdge(origin + new Vector3(size.X, 0, 0), origin + new Vector3(size.X, size.Y, 0));
            CreateCubeEdge(origin + new Vector3(size.X, 0, size.Z), origin + new Vector3(size.X, size.Y, size.Z));
            CreateCubeEdge(origin + new Vector3(0, 0, size.Z), origin + new Vector3(0, size.Y, size.Z));

            return bb;

        }
        public struct Bounds
        {
            public Vector3 min;
            public Vector3 max;
            public Vector3 center;
            public Vector3 size;

            // Constructor
            public Bounds(Vector3 min, Vector3 max)
            {
                this.min = min;
                this.max = max;
                this.center = (min + max) * 0.5f;
                this.size = max - min;
            }

            // Methods
            public Vector3 GetClosestPoint(Vector3 point)
            {
                // Clamp the point to the bounds
                return new Vector3(
                    Math.Clamp(point.X, min.X, max.X),
                    Math.Clamp(point.Y, min.Y, max.Y),
                    Math.Clamp(point.Z, min.Z, max.Z)
                );
            }

            public bool Contains(Vector3 point)
            {
                // Check if the point is within the bounds
                return point.X >= min.X && point.X <= max.X &&
                       point.Y >= min.Y && point.Y <= max.Y &&
                       point.Z >= min.Z && point.Z <= max.Z;
            }

            public bool Intersects(Bounds otherBounds)
            {
                // Check if the minimum point of one bounds is less than the maximum point of the other bounds in all dimensions
                return otherBounds.max.X >= min.X && otherBounds.min.X <= max.X &&
                       otherBounds.max.Y >= min.Y && otherBounds.min.Y <= max.Y &&
                       otherBounds.max.Z >= min.Z && otherBounds.min.Z <= max.Z;
            }

            public bool Intersects(Vector3 point, float radius)
            {
                // Check if the sphere defined by the point and radius intersects the bounds
                return point.X - radius <= max.X && point.X + radius >= min.X &&
                       point.Y - radius <= max.Y && point.Y + radius >= min.Y &&
                       point.Z - radius <= max.Z && point.Z + radius >= min.Z;
            }
        }
        public class BoundingBox
        {
            public GameObject parent;
            public Bounds Bounds;

        }
    }
}
