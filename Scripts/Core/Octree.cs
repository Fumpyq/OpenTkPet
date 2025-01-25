using BepuPhysics.Trees;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static BepuPhysics.Collidables.CompoundBuilder;

namespace ConsoleApp1_Pet.Scripts.Core
{
    public class Octree<T> where T : IOctreeNode
    {
        public int ObjectLimit;
        public OctreeNode root;
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Rebuild(Span<T> objects)
        {
            if (root != null) { BackToPool(root); }
            var OverAllBounds = new CenterSizeBounds(objects[0].position);
            var leng = objects.Length - 1;
            for (int i = leng; i >= 1; i--)
            {
                OverAllBounds.Encapsulate(objects[i].position);
            }

            root = GetNode(OverAllBounds, 0);
            for (int i = leng; i >= 0; i--)
            {
                root.Add(objects[i]);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Rebuild(IEnumerable<T> objects)
        {
            if (root != null) { BackToPool(root); }
            var OverAllBounds = new CenterSizeBounds(objects.First().position);

            foreach (var obj in objects)
            {
                OverAllBounds.Encapsulate(obj.position);
            }

            root = GetNode(OverAllBounds, 0);
            foreach (var obj in objects)
            {
                root.Add(obj);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Clear()
        {
            if(root==null)BackToPool(root);
            root = null;
        }


        private Queue<OctreeNode> pool = new Queue<OctreeNode>();
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public Octree(int objectLimit, int initialPool = 100)
        {
            ObjectLimit = objectLimit;
            for (int i = initialPool - 1; i >= 0; i--)
                pool.Enqueue(new OctreeNode(this));

        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private OctreeNode GetNode(CenterSizeBounds bound, int lvl)
        {
            if (pool.TryDequeue(out var node)) return node.PoolSetup(bound, lvl);

            node = new OctreeNode(this, bound, lvl);
            return node;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void BackToPool(OctreeNode node)
        {
            node.OnPoolUnload();
            pool.Enqueue(node);
        }
        // public class NodePool<T> where T : IOctreeNode { }
        public class OctreeNode
        {
            public Octree<T> tree;
            List<T> objects;

            OctreeNode[] childs = new OctreeNode[8];
            public bool HaveChilds => childs[0] != null;
            public CenterSizeBounds bounds;
            public int lvl;

            public OctreeNode(Octree<T> tree, CenterSizeBounds bounds, int lvl) : this(tree)
            {
                this.bounds = bounds;
                this.lvl = lvl;

            }

            public OctreeNode(Octree<T> tree)
            {
                this.tree = tree;
                objects = new List<T>(tree.ObjectLimit);
            }

            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            public void Add(T newObject)
            {
                RecursiveAdd(newObject);
            }
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            private void RecursiveAdd(T newObj)
            {
                if (HaveChilds)
                {
                    int side = DecideChild(newObj);

                    childs[side].RecursiveAdd(newObj);
                }
                else
                {
                    if (objects.Count >= tree.ObjectLimit - 1)
                    {
                        Split(newObj);
                    }
                    else
                    {
                        objects.Add(newObj);
                    }
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            public void Split(T newObject)
            {
                Vector3 halfSize = bounds.Size / 2f;
                Vector3 Quat = halfSize / 2f;
                int newLvl = lvl + 1;
                //float quarterSizeX = halfSize.X / 2f;
                //float quarterSizeY = halfSize.Y / 2f;
                //float quarterSizeZ = halfSize.Z / 2f;
                childs[0] = tree.GetNode(new CenterSizeBounds(bounds.Center + new Vector3(Quat.X, Quat.Y, Quat.Z), halfSize), newLvl);  // Top Right Front
                childs[1] = tree.GetNode(new CenterSizeBounds(bounds.Center + new Vector3(Quat.X, Quat.Y, -Quat.Z), halfSize), newLvl);  // Top Right Back
                childs[2] = tree.GetNode(new CenterSizeBounds(bounds.Center + new Vector3(Quat.X, -Quat.Y, Quat.Z), halfSize), newLvl);  // Bottom Right Front
                childs[3] = tree.GetNode(new CenterSizeBounds(bounds.Center + new Vector3(Quat.X, -Quat.Y, -Quat.Z), halfSize), newLvl);  // Bottom Right Back
                childs[4] = tree.GetNode(new CenterSizeBounds(bounds.Center + new Vector3(-Quat.X, Quat.Y, Quat.Z), halfSize), newLvl);  // Top Left Front
                childs[5] = tree.GetNode(new CenterSizeBounds(bounds.Center + new Vector3(-Quat.X, Quat.Y, -Quat.Z), halfSize), newLvl);  // Top Left Back
                childs[6] = tree.GetNode(new CenterSizeBounds(bounds.Center + new Vector3(-Quat.X, -Quat.Y, Quat.Z), halfSize), newLvl);  // Bottom Left Front
                childs[7] = tree.GetNode(new CenterSizeBounds(bounds.Center + new Vector3(-Quat.X, -Quat.Y, -Quat.Z), halfSize), newLvl);  // Bottom Left Back




                int side = DecideChild(newObject);
                childs[side].RecursiveAdd(newObject);

                foreach (var child in CollectionsMarshal.AsSpan(objects))
                {
                    side = DecideChild(child);
                    childs[side].RecursiveAdd(child);
                }
                objects.Clear();
            }
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            public int DecideChild(T obj)
            {
                var pos = obj.position;
                var cen = bounds.Center;

                return (pos.X >= cen.X ? 0 : 4) | (pos.Y >= cen.Y ? 0 : 2) | (pos.Z >= cen.Z ? 0 : 1);
            }
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            public OctreeNode PoolSetup(CenterSizeBounds bound, int lvl)
            {
                bounds = bound;
                this.lvl = lvl;
                return this;
            }
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            public void OnPoolUnload()
            {
                lvl = 0;
                objects.Clear();
                if (HaveChilds)
                {
                    tree.BackToPool(childs[0]); childs[0] = null;// since only 1-st element is checked no needs to clear other;
                    tree.BackToPool(childs[1]);
                    tree.BackToPool(childs[2]);
                    tree.BackToPool(childs[3]);
                    tree.BackToPool(childs[4]);
                    tree.BackToPool(childs[5]);
                    tree.BackToPool(childs[6]);
                    tree.BackToPool(childs[7]);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            public void GetAllObjects(ref List<T> Output)
            {
                if (HaveChilds)
                {
                    foreach (var c in childs)
                    {
                        c.GetAllObjects(ref Output);
                    }
                }
                else
                {
                    Output.AddRange(objects);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveOptimization)]
            public void DoFrostumCall(Frostum frost, ref List<T> res)
            {
                var CheckRes = frost.IsBBInsideCheap(bounds);
                switch (CheckRes)
                {
                    case Frostum.OverlapResult.Inside:
                        GetAllObjects(ref res);
                        break;
                    case Frostum.OverlapResult.Outside:
                        return;
                    case Frostum.OverlapResult.Overlap:
                        if (lvl > 3)
                        {
                            GetAllObjects(ref res);
                            break;
                        }
                        if (HaveChilds)
                        {
                            foreach (var c in childs)
                            {
                                c.DoFrostumCall(frost, ref res);
                            }
                        }
                        break;
                }
            }

        }
    }
    public struct FrostumPlane
    {
        public Vector3 vect;
        public float W;

        public FrostumPlane(float x, float y, float z, float w) : this()
        {
            W = w;
        }
        public float Magnitude()
        {
            return MathF.Sqrt(vect.X * vect.X + vect.Y * vect.Y + vect.Z * vect.Z + W * W);
        }

        public FrostumPlane Normalize()
        {
            float magnitude = Magnitude();
            if (magnitude == 0) // Avoid division by zero
            {
                return new FrostumPlane(0, 0, 0, 0); // Return zero vector if magnitude is zero
            }

            float invMagnitude = 1.0f / magnitude;
            return new FrostumPlane(vect.X * invMagnitude, vect.Y * invMagnitude, vect.Z * invMagnitude, W * invMagnitude);
        }
    }
    public class Frostum
    {
        public FrostumPlane[] _planes = new FrostumPlane[6];
        public OpenTK.Mathematics.Vector3 pos;
        public enum OverlapResult
        {
            /// <summary>
            /// Fully inside
            /// </summary>
            Inside,
            /// <summary>
            /// Fully outside
            /// </summary>
            Outside,
            /// <summary>
            /// Something inside, something outside
            /// </summary>
            Overlap
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        void Initialize(OpenTK.Mathematics.Matrix4 viewProjectionMatrix)
        {
            // Extract the planes from the view-projection matrix
            // This is a common way to calculate frustum planes from a combined matrix
            _planes[0] = new FrostumPlane(viewProjectionMatrix.M14 + viewProjectionMatrix.M11,
                                             viewProjectionMatrix.M24 + viewProjectionMatrix.M21,
                                             viewProjectionMatrix.M34 + viewProjectionMatrix.M31,
                                             viewProjectionMatrix.M44 + viewProjectionMatrix.M41);

            _planes[1] = new FrostumPlane(viewProjectionMatrix.M14 - viewProjectionMatrix.M11,
                                             viewProjectionMatrix.M24 - viewProjectionMatrix.M21,
                                             viewProjectionMatrix.M34 - viewProjectionMatrix.M31,
                                             viewProjectionMatrix.M44 - viewProjectionMatrix.M41);

            _planes[2] = new FrostumPlane(viewProjectionMatrix.M14 + viewProjectionMatrix.M12,
                                             viewProjectionMatrix.M24 + viewProjectionMatrix.M22,
                                             viewProjectionMatrix.M34 + viewProjectionMatrix.M32,
                                             viewProjectionMatrix.M44 + viewProjectionMatrix.M42);

            _planes[3] = new FrostumPlane(viewProjectionMatrix.M14 - viewProjectionMatrix.M12,
                                             viewProjectionMatrix.M24 - viewProjectionMatrix.M22,
                                             viewProjectionMatrix.M34 - viewProjectionMatrix.M32,
                                             viewProjectionMatrix.M44 - viewProjectionMatrix.M42);

            _planes[4] = new FrostumPlane(viewProjectionMatrix.M14 + viewProjectionMatrix.M13,
                                             viewProjectionMatrix.M24 + viewProjectionMatrix.M23,
                                             viewProjectionMatrix.M34 + viewProjectionMatrix.M33,
                                             viewProjectionMatrix.M44 + viewProjectionMatrix.M43);

            _planes[5] = new FrostumPlane(viewProjectionMatrix.M14 - viewProjectionMatrix.M13,
                                             viewProjectionMatrix.M24 - viewProjectionMatrix.M23,
                                             viewProjectionMatrix.M34 - viewProjectionMatrix.M33,
                                             viewProjectionMatrix.M44 - viewProjectionMatrix.M43);
            pos = viewProjectionMatrix.ExtractTranslation();
            // Normalize the planes (important for accurate distance calculations)
            for (int i = 0; i < 6; i++)
            {
                _planes[i] = _planes[i].Normalize();
            }
        }
        public OverlapResult IsBBInsideCheap(CenterSizeBounds bb)
        {
            var radius = bb.Size.LengthSquared;

            //if (Vector3.DistanceSquared(center, pos) > 10000) return false;
            OverlapResult res = OverlapResult.Outside;
            foreach (var p in _planes)
            {
                if (!(Dot(p.vect, bb.Center) + p.W < -radius))
                { // point is inside
                    res = OverlapResult.Overlap;
                    //return false;
                }
                else
                {//Point is outside
                    if (res == OverlapResult.Overlap)
                    {
                        return res;
                    }
                }
            }
            return res = OverlapResult.Inside; // Sphere is inside all planes 
        }
        public bool IsSphereInsideCheap(Vector3 center, float radius)
        {

            //if (Vector3.DistanceSquared(center, pos) > 10000) return false;
            foreach (var p in _planes)
            {
                if (Dot(p.vect, center) + p.W < -radius)
                {
                    return false;
                }
            }
            return true; // Sphere is inside all planes 
        }
        public bool IsSphereInsideCheapSqrRadius(Vector3 center, float radius)
        {

            //if (Vector3.DistanceSquared(center, pos) > 10000) return false;
            foreach (var p in _planes)
            {
                var sq = Dot(p.vect, center) + p.W;
                sq *= sq;
                if (sq < -radius)
                {
                    return false;
                }
            }
            return true; // Sphere is inside all planes 
        }
        public static float Dot(Vector3 left, Vector3 right)
        {
            left = left * right;
            return left.X + left.Y + left.Z;
        }
    }
    public class CenterSizeBounds
    {
        public Vector3 Center;
        public Vector3 Size;

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]

        public CenterSizeBounds(Vector3 center, Vector3 size)
        {
            Center = center;
            Size = size;
        }
        public CenterSizeBounds(Vector3 center)
        {
            Center = center;
            Size = Vector3.Zero;
        }
        public CenterSizeBounds(CenterSizeBounds other)
        {
            Center = other.Center;
            Size = other.Size;
        }


        public Vector3 Min { get => Center - Size * 0.5f; }

        public Vector3 Max { get => Center + Size * 0.5f; }
        public float Volume => Size.X * Size.Y * Size.Z;
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Encapsulate(Vector3 point)
        {
            Vector3 min = Min;
            Vector3 max = Max;
            //encapsulate min max, then generate new centre/size
            Vector3 newMin = Vector3.ComponentMin(min, point);
            Vector3 newMax = Vector3.ComponentMax(max, point);

            Vector3 newSize = newMax - newMin;
            Vector3 newCenter = (newMin + newMax) * 0.5f;

            Center = newCenter;
            Size = newSize;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void Encapsulate(CenterSizeBounds other)
        {
            Vector3 min = Min;
            Vector3 max = Max;
            //encapsulate min max, then generate new centre/size
            Vector3 newMin = Vector3.ComponentMin(min, other.Min);
            Vector3 newMax = Vector3.ComponentMax(max, other.Max);

            Vector3 newSize = newMax - newMin;
            Vector3 newCenter = (newMin + newMax) * 0.5f;

            Center = newCenter;
            Size = newSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public bool Contains(Vector3 point)
        {
            Vector3 min = Min;
            Vector3 max = Max;
            return point.X >= min.X && point.X <= max.X &&
                   point.Y >= min.Y && point.Y <= max.Y &&
                   point.Z >= min.Z && point.Z <= max.Z;
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public bool Intersects(CenterSizeBounds other)
        {
            Vector3 min = Min;
            Vector3 max = Max;
            Vector3 otherMin = other.Min;
            Vector3 otherMax = other.Max;

            return (max.X >= otherMin.X && min.X <= otherMax.X) &&
                   (max.Y >= otherMin.Y && min.Y <= otherMax.Y) &&
                   (max.Z >= otherMin.Z && min.Z <= otherMax.Z);
        }



        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        // Helper methods
        public Vector3 GetCorner(int index)
        {
            if (index < 0 || index > 7) throw new ArgumentOutOfRangeException("Index out of bounds. Must be between 0-7");

            bool x = (index & 1) != 0;
            bool y = (index & 2) != 0;
            bool z = (index & 4) != 0;

            Vector3 min = Min;
            Vector3 max = Max;

            return new Vector3(x ? max.X : min.X, y ? max.Y : min.Y, z ? max.Z : min.Z);
        }
        public override string ToString()
        {
            return $"Center: {Center}, Size: {Size}";
        }
    }
    public struct MinMaxBounds
    {
        public Vector3 Min { get; set; }
        public Vector3 Max { get; set; }
        private Vector3? _cachedCenter = null;

        public MinMaxBounds(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        public MinMaxBounds(MinMaxBounds other)
        {
            Min = other.Min;
            Max = other.Max;
        }

        public Vector3 Center
        {
            get
            {
                if (!_cachedCenter.HasValue)
                {
                    _cachedCenter = (Min + Max) * 0.5f;
                }
                return _cachedCenter.Value;
            }
        }
        public Vector3 Size => Max - Min;
        public float Volume => Size.X * Size.Y * Size.Z;

        public void Encapsulate(Vector3 point)
        {
            Min = Vector3.ComponentMin(Min, point);
            Max = Vector3.ComponentMax(Max, point);
            _cachedCenter = null; // Invalidate cache
        }

        public void Encapsulate(MinMaxBounds other)
        {
            Min = Vector3.ComponentMin(Min, other.Min);
            Max = Vector3.ComponentMax(Max, other.Max);
            _cachedCenter = null; // Invalidate cache
        }


        public bool Contains(Vector3 point)
        {
            return point.X >= Min.X && point.X <= Max.X &&
                   point.Y >= Min.Y && point.Y <= Max.Y &&
                   point.Z >= Min.Z && point.Z <= Max.Z;
        }


        public bool Intersects(MinMaxBounds other)
        {
            return (Max.X >= other.Min.X && Min.X <= other.Max.X) &&
                   (Max.Y >= other.Min.Y && Min.Y <= other.Max.Y) &&
                   (Max.Z >= other.Min.Z && Min.Z <= other.Max.Z);
        }


        // Helper methods
        public Vector3 GetCorner(int index)
        {
            if (index < 0 || index > 7) throw new ArgumentOutOfRangeException("Index out of bounds. Must be between 0-7");

            bool x = (index & 1) != 0;
            bool y = (index & 2) != 0;
            bool z = (index & 4) != 0;

            return new Vector3(x ? Max.X : Min.X, y ? Max.Y : Min.Y, z ? Max.Z : Min.Z);

        }

        public override string ToString()
        {
            return $"Min: {Min}, Max: {Max}";
        }
    }
    public interface IOctreeNode
    {
        public Vector3 position { get;}
        public Vector2 position2d { get; }
    }
}
