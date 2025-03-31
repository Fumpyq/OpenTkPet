using ConsoleApp1_Pet.Architecture;
using ConsoleApp1_Pet.Materials;
using ConsoleApp1_Pet.Meshes;
using ConsoleApp1_Pet.Scripts.Core;
using ConsoleApp1_Pet.Shaders;
using ConsoleApp1_Pet.Новая_папка;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Profiling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace ConsoleApp1_Pet.Render
{
    public class RenderComponent : GOComponent, IOctreeNode
    {

        public Material material
        {
            get => materials.Count > 0 ? materials[0] : null; set
            {
                if (materials.Count > 0) materials[0] = value;
                else { materials.Add(value); }
            }
        }

        Vector3 IOctreeNode.position { get => transform.position; }
        Vector2 IOctreeNode.position2d { get => throw new NotImplementedException(); }

        public List<Material> materials = new List<Material>();

        public Mesh mesh;
        public event Action<Material> OnMaterialChange;

        //public RenderComponent(Mesh mesh, Transform transform,params Material[] materials)
        //{
        //    this.materials = new List<Material>(materials);
        //    this.mesh = mesh;
        //    this.transform = transform;
        //}
        public override void OnInit(GameObject go)
        {
            base.OnInit(go);
            MainGameWindow.instance.renderer.AddToRender(this);
        }
        public RenderComponent(Mesh mesh, params Material[] materials)
        {
            //if(selfHosted) WithSelfGamobject();
            this.materials = new List<Material>(materials);
            this.mesh = mesh;
            //this.transform = new Transform();
        }
        public RenderComponent WithSelfGamobject()
        {
            this.gameObject = new GameObject("");
            this.gameObject.AddComponent(this);
            return this;
        }

        public void DirectDraw(Matrix4 view, Matrix4 project)
        {
            material.Use();
            //var mtrx = transform * view * project;
            // var worldSpaceModel = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(Game.instance._stopwatch.Elapsed.TotalSeconds * 35));
            // worldSpaceModel *= Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(Game.instance._stopwatch.Elapsed.TotalSeconds * 25));
            material.shader.SetMatrix(0, transform);
            material.shader.SetMatrix(1, view);
            material.shader.SetMatrix(2, project);
            //material.shader.SetMatrix(3, mtrx);

            mesh.FillBuffers();
            GL.BindVertexArray(mesh.VAO);

            GL.DrawElements(PrimitiveType.Triangles, mesh.triangles.Length, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }
    }
    public class Transform
    {
        // Local space properties
        private Vector3 _localPosition = Vector3.Zero;
        private Quaternion _localRotation = Quaternion.Identity;
        private Vector3 _localScale = Vector3.One;

        // World space cache
        private Vector3 _worldPosition = Vector3.Zero;
        private Quaternion _worldRotation = Quaternion.Identity;
        private Vector3 _worldScale = Vector3.One;
        private Matrix4 _worldMatrix = Matrix4.Identity;
        private bool _isDirty = true;
        private bool _parentChanged = false;

        // Hierarchy
        private Transform _parent;
        public List<Transform> childs = new List<Transform>();
        public GameObject gameObject;

        public event Action OnTransformChanged;
        private bool _isUpdating = false;
        public Transform() { }

        // Constructors
        public Transform(Vector3 position, Quaternion rotation, Vector3 scale) : this()
        {
            _localPosition = position;
            _localRotation = rotation;
            _localScale = scale;
            _isDirty = true;
        }
        public Transform(Vector3 position, Vector3 rotation) : this(position, Quaternion.FromEulerAngles(rotation).Normalized()) { }
        public Transform(Vector3 position, Quaternion rotation) : this(position, rotation, Vector3.One) { }
        public Transform(Vector3 position) : this(position, Quaternion.Identity) { }
        public Transform(GameObject gameObject) : this() => this.gameObject = gameObject;

        // Local properties
        public Vector3 LocalPosition
        {
            get => _localPosition;
            set { _localPosition = value; MarkDirty(); }
        }

        public Quaternion LocalRotation
        {
            get => _localRotation;
            set { _localRotation = value; MarkDirty(); }
        }

        public Vector3 LocalScale
        {
            get => _localScale;
            set { _localScale = value; MarkDirty(); }
        }

        // World properties
        public Vector3 WorldPosition => UpdateTransformIfNeeded()._worldPosition;
        public Quaternion WorldRotation => UpdateTransformIfNeeded()._worldRotation;
        public Vector3 WorldScale => UpdateTransformIfNeeded()._worldScale;

        // Parent-relative properties
        public Vector3 position
        {
            get => WorldPosition;
            set
            {
                _localPosition = _parent != null
                    ? value.Transform(Matrix4.Invert(_parent.WorldMatrix))
                    : value;
                MarkDirty();
            }
        }

        public Quaternion rotation
        {
            get => WorldRotation;
            set
            {
                _localRotation = _parent != null
                    ? Quaternion.Multiply(Quaternion.Invert(_parent.WorldRotation), value)
                    : value;
                MarkDirty();
            }
        }

        public Vector3 scale
        {
            get => WorldScale;
            set
            {
                _localScale = _parent != null
                    ? Vector3.Divide(value, _parent.WorldScale)
                    : value;
                MarkDirty();
            }
        }

        // Parent management
        public Transform parent
        {
            get => _parent;
            set
            {
                if (_parent == value) return;

                if (_parent != null)
                {
                    _parent.OnTransformChanged -= HandleParentTransformChanged;
                    _parent.RemoveChild(this);
                }

                _parent = value;
                _parentChanged = true;

                if (_parent != null)
                {
                    _parent.AddChild(this, false);
                    _parent.OnTransformChanged += HandleParentTransformChanged;
                }

                MarkDirty(true);
            }
        }

        // Matrix access
        public Matrix4 WorldMatrix => UpdateTransformIfNeeded()._worldMatrix;
        public Matrix4 worldSpaceModel => WorldMatrix;
        public static implicit operator Matrix4(Transform t) => t.WorldMatrix;

        // Direction vectors
        public Vector3 Forward
        {
            get => Vector3.Transform(Vector3.UnitZ, rotation);
            set => rotation = QuaternionExtensions.LookRotation(value);
        }

        public Vector3 Up
        {
            get => Vector3.Transform(Vector3.UnitY, rotation);
            set => rotation = Quaternion.FromAxisAngle(
                Vector3.Cross(Vector3.UnitY, value.Normalized()).Normalized(),
                Vector3.CalculateAngle(Vector3.UnitY, value.Normalized()));
        }

        public Vector3 Right => Vector3.Normalize(Vector3.Transform(-Vector3.UnitX, rotation));
        public Vector3 Left => -Right;

        // Child management
        public void AddChild(Transform child, bool changeParent = true)
        {
            if (child.parent != this && !childs.Contains(child))
            {
                if (changeParent) child.parent = this;
                else childs.Add(child);

                child.MarkDirty(true);
            }
        }

        public void RemoveChild(Transform child) => childs.Remove(child);

        // Bulk update method
        public void SetPositionAndRotation(Vector3 pos, Quaternion rot)
        {
            if (_parent != null)
            {
                _localPosition = pos.Transform(Matrix4.Invert(_parent.WorldMatrix));
                _localRotation = Quaternion.Multiply(Quaternion.Invert(_parent.WorldRotation), rot);
            }
            else
            {
                _localPosition = pos;
                _localRotation = rot;
            }
            MarkDirty();
        }

        private void MarkDirty(bool forceChildUpdate = false)
        {
            if (!_isDirty)
            {
                _isDirty = true;
                OnTransformChanged?.Invoke();
            }

            if (forceChildUpdate || childs.Count > 0)
            {
                foreach (var child in childs)
                {
                    child.MarkDirty(true, silent: true);
                }
            }
        }

        private void MarkDirty(bool forceChildUpdate, bool silent)
        {
            _isDirty = true;
            if (!silent) OnTransformChanged?.Invoke();

            if (forceChildUpdate || childs.Count > 0)
            {
                foreach (var child in childs)
                {
                    child.MarkDirty(true, silent: true);
                }
            }
        }
        static long totalTime;
        static int totalRecords;
        // Transform update logic
        private Transform UpdateTransformIfNeeded()
        {
            if (_isUpdating) return this;
            _isUpdating = true;

            try
            {
              
                if (!_isDirty && !_parentChanged && (_parent == null || !_parent._isDirty))
                    return this;
      
                if (_parentChanged)
                {
                    if (_parent != null)
                    {
                        _localPosition = position.Transform(Matrix4.Invert(_parent.WorldMatrix));
                        _localRotation = Quaternion.Multiply(Quaternion.Invert(_parent.WorldRotation), rotation);
                        _localScale = Vector3.Divide(scale, _parent.WorldScale);
                    }
                    _parentChanged = false;
                }

                //var localTransform = Matrix4.CreateScale(_localScale)
                //                   * Matrix4.CreateFromQuaternion(_localRotation)
                //                   * Matrix4.CreateTranslation(_localPosition);
                var localTransform = MatrixOptimization.CreateTransformationMatrixVectorizedOptimized2(_localScale,LocalRotation, _localPosition);

                _worldMatrix = _parent != null
                    ? localTransform * _parent.WorldMatrix
                    : localTransform;

                //_worldMatrix.ExtractTransform(out _worldScale, out _worldRotation, out _worldPosition);
                
                _worldPosition = _worldMatrix.ExtractTranslation();

               // var sw = Stopwatch.StartNew();

                _worldRotation = _worldMatrix.ExtractRotation();  //QuaternionExtensions.ExtractRotation(_worldMatrix);

                //sw.Stop();
                //if (totalRecords > 5000)
                //    totalTime += sw.ElapsedTicks;
                //totalRecords++;
                //if (totalRecords >= 15000)
                //{
                //    var total = TimeSpan.FromTicks(totalTime);// avg 0.0081
                //                                              //default matrix avg 0.009
                //}

                //_worldRotation = _worldMatrix.ExtractRotation();
                _worldScale = _worldMatrix.ExtractScale();
              
                _isDirty = false;
              


                return this;
            }
            finally
            {
                _isUpdating = false;
            }
        }

        // Event handlers
        private void HandleParentTransformChanged()
        {
            if (!_isDirty && !_isUpdating)
            {
                _isDirty = true;
                // Do NOT invoke OnTransformChanged here
                foreach (var child in childs)
                {
                    child.MarkDirty(true);
                }
            }
        }

        public override string ToString() =>
            $"{position.ToStringShort()}\nQ:{rotation}\nF:{Forward.ToStringShort()}\nR:{Right.ToStringShort()}\nU:{Up.ToStringShort()}";
    }



    


    public static class MatrixOptimization
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 CreateTransformationMatrixSafe(
    in Vector3 scale, in Quaternion rotation, in Vector3 translation)
        {
            Vector3 axis = new Vector3(rotation.X, rotation.Y, rotation.Z);
            float w = rotation.W;

            Vector3 twoAxis = 2 * axis;
            float xx2 = twoAxis.X * axis.X;
            float yy2 = twoAxis.Y * axis.Y;
            float zz2 = twoAxis.Z * axis.Z;

            float xy2 = twoAxis.X * axis.Y;
            float xz2 = twoAxis.X * axis.Z;
            float yz2 = twoAxis.Y * axis.Z;

            float wx2 = twoAxis.X * w;
            float wy2 = twoAxis.Y * w;
            float wz2 = twoAxis.Z * w;

            return new Matrix4(
                (1 - yy2 - zz2) * scale.X, (xy2 - wz2) * scale.Y, (xz2 + wy2) * scale.Z, translation.X,
                (xy2 + wz2) * scale.X, (1 - xx2 - zz2) * scale.Y, (yz2 - wx2) * scale.Z, translation.Y,
                (xz2 - wy2) * scale.X, (yz2 + wx2) * scale.Y, (1 - xx2 - yy2) * scale.Z, translation.Z,
                0, 0, 0, 1
            );
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Matrix4 CreateTransformationMatrixAvx2Fixed(
    in Vector3 scale, in Quaternion rotation, in Vector3 translation)
        {
            // Quaternion components
            float x = rotation.X, y = rotation.Y, z = rotation.Z, w = rotation.W;

            // Precompute all 2* terms using AVX2
            var squaredTerms = Avx2.Multiply(
                Vector256.Create(x, y, z, x, y, z, w, w),
                Vector256.Create(x, y, z, y, z, w, x, y)
            );

            float* terms = stackalloc float[8];
            Avx.Store(terms, Avx2.Multiply(squaredTerms, Vector256.Create(2f)));

            // Rotation matrix columns with scaling
            Vector128<float> col0 = Vector128.Create(
                (1 - terms[1] - terms[2]) * scale.X,  // M11: (1 - 2y² - 2z²) * Sx
                (terms[3] + terms[7]) * scale.X,       // M21: (2xy + 2zw) * Sx
                (terms[4] - terms[6]) * scale.X,       // M31: (2xz - 2yw) * Sx
                0f                                     // M41
            );

            Vector128<float> col1 = Vector128.Create(
                (terms[3] - terms[7]) * scale.Y,       // M12: (2xy - 2zw) * Sy
                (1 - terms[0] - terms[2]) * scale.Y,   // M22: (1 - 2x² - 2z²) * Sy
                (terms[5] + terms[4]) * scale.Y,       // M32: (2yz + 2xw) * Sy
                0f                                     // M42
            );

            Vector128<float> col2 = Vector128.Create(
                (terms[4] + terms[6]) * scale.Z,       // M13: (2xz + 2yw) * Sz
                (terms[5] - terms[3]) * scale.Z,       // M23: (2yz - 2xw) * Sz
                (1 - terms[0] - terms[1]) * scale.Z,   // M33: (1 - 2x² - 2y²) * Sz
                0f                                     // M43
            );

            Vector128<float> col3 = Vector128.Create(
                translation.X,                         // M14: Tx
                translation.Y,                         // M24: Ty
                translation.Z,                         // M34: Tz
                1f                                     // M44
            );

            // Store to matrix memory with proper 4x4 layout
            Matrix4 result = default;
            float* matrixPtr = (float*)Unsafe.AsPointer(ref result);

            Avx.Store(matrixPtr, col0);  // First column:  M11, M21, M31, M41
            Avx.Store(matrixPtr + 4, col1);  // Second column: M12, M22, M32, M42
            Avx.Store(matrixPtr + 8, col2);  // Third column:  M13, M23, M33, M43
            Avx.Store(matrixPtr + 12, col3);  // Fourth column: M14, M24, M34, M44

            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 CreateTransformationMatrixVectorizedOptimized2(
    in Vector3 scale, in Quaternion rotation, in Vector3 translation)
        {
            // Extract rotation components (hot path ensures register allocation)
            float x = rotation.X, y = rotation.Y, z = rotation.Z, w = rotation.W;

            // Precompute all 2* terms first (minimizes redundant calculations)
            float xx = x * x * 2f, yy = y * y * 2f, zz = z * z * 2f;
            float xy = x * y * 2f, xz = x * z * 2f, xw = x * w * 2f;
            float yz = y * z * 2f, yw = y * w * 2f, zw = z * w * 2f;

            // Calculate diagonal terms using precomputed values
            Vector3 diag = new Vector3(
                1f - yy - zz,  // 1 - 2y² - 2z²
                1f - xx - zz,  // 1 - 2x² - 2z²
                1f - xx - yy   // 1 - 2x² - 2y²
            );

            // Calculate off-diagonal terms using precomputed values
            Vector3 off0 = new Vector3(xy + zw, xz - yw, xy - zw);
            Vector3 off1 = new Vector3(yz + xw, yz - xw, xz + yw);

            // Construct scaled columns using vector operations
            Vector4 col0 = new Vector4(diag.X * scale.X, off0.X * scale.X, off0.Y * scale.X, 0f);
            Vector4 col1 = new Vector4(off0.Z * scale.Y, diag.Y * scale.Y, off1.X * scale.Y, 0f);
            Vector4 col2 = new Vector4(off1.Z * scale.Z, off1.Y * scale.Z, diag.Z * scale.Z, 0f);
            Vector4 col3 = new Vector4(translation, 1f);

            return new Matrix4(col0, col1, col2, col3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 CreateTransformationMatrixVectorizedOptimized(Vector3 scale, Quaternion rotation, Vector3 translation)
        {
            float x = rotation.X, y = rotation.Y, z = rotation.Z, w = rotation.W;
            float x2 = x * x, y2 = y * y, z2 = z * z;
            float xy = x * y, xz = x * z, xw = x * w;
            float yz = y * z, yw = y * w, zw = z * w;

            // Compute each column as a Vector4 with integrated scaling and rotation
            Vector4 col0 = new Vector4(
                (1 - 2 * (y2 + z2)) * scale.X,
                (2 * (xy + zw)) * scale.X,
                (2 * (xz - yw)) * scale.X,
                0f
            );

            Vector4 col1 = new Vector4(
                (2 * (xy - zw)) * scale.Y,
                (1 - 2 * (x2 + z2)) * scale.Y,
                (2 * (yz + xw)) * scale.Y,
                0f
            );

            Vector4 col2 = new Vector4(
                (2 * (xz + yw)) * scale.Z,
                (2 * (yz - xw)) * scale.Z,
                (1 - 2 * (x2 + y2)) * scale.Z,
                0f
            );

            Vector4 col3 = new Vector4(translation.X, translation.Y, translation.Z, 1f);

            // Construct the matrix directly from Vector4 columns
            return new Matrix4(col0, col1, col2, col3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 CreateTransformationMatrixVectorized3(Vector3 scale, Quaternion rotation, Vector3 translation)
        {
            // Quaternion to rotation matrix (optimized for SIMD)
            float x = rotation.X, y = rotation.Y, z = rotation.Z, w = rotation.W;
            float x2 = x * x, y2 = y * y, z2 = z * z;
            float xy = x * y, xz = x * z, xw = x * w;
            float yz = y * z, yw = y * w, zw = z * w;

            // Rotation Calculations (inlined)
            float col0x = (1 - 2 * (y2 + z2));
            float col0y = (2 * (xy + zw));
            float col0z = (2 * (xz - yw));

            float col1x = (2 * (xy - zw));
            float col1y = (1 - 2 * (x2 + z2));
            float col1z = (2 * (yz + xw));

            float col2x = (2 * (xz + yw));
            float col2y = (2 * (yz - xw));
            float col2z = (1 - 2 * (x2 + y2));

            // Scaling Calculations (directly into matrix construction)

            // Construct the matrix directly (column-major order) avoiding Vector3 allocation
            return new Matrix4(
                scale.X * col0x, scale.X * col0y, scale.X * col0z, 0,  // Column 0
                scale.Y * col1x, scale.Y * col1y, scale.Y * col1z, 0,  // Column 1
                scale.Z * col2x, scale.Z * col2y, scale.Z * col2z, 0,  // Column 2
                translation.X, translation.Y, translation.Z, 1       // Column 3
            );
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 CreateTransformationMatrixVectorized2(Vector3 scale, Quaternion rotation, Vector3 translation)
        {
            // Quaternion to rotation matrix (optimized for SIMD)
            float x = rotation.X, y = rotation.Y, z = rotation.Z, w = rotation.W;
            float x2 = x * x, y2 = y * y, z2 = z * z;
            float xy = x * y, xz = x * z, xw = x * w;
            float yz = y * z, yw = y * w, zw = z * w;

            //Rotation Calculations
            Vector3 col0Rotation = new Vector3((1 - 2 * (y2 + z2)), (2 * (xy + zw)), (2 * (xz - yw)));
            Vector3 col1Rotation = new Vector3((2 * (xy - zw)), (1 - 2 * (x2 + z2)), (2 * (yz + xw)));
            Vector3 col2Rotation = new Vector3((2 * (xz + yw)), (2 * (yz - xw)), (1 - 2 * (x2 + y2)));

            // Scaling Calculations
            Vector3 col0 = col0Rotation * scale.X;
            Vector3 col1 = col1Rotation * scale.Y;
            Vector3 col2 = col2Rotation * scale.Z;


            // Construct the matrix directly (column-major order)
            return new Matrix4(col0.X, col0.Y, col0.Z, 0,
                               col1.X, col1.Y, col1.Z, 0,
                               col2.X, col2.Y, col2.Z, 0,
                               translation.X, translation.Y, translation.Z, 1);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 CreateTransformationMatrixVectorized(Vector3 scale, Quaternion rotation, Vector3 translation)
        {
            // Quaternion to rotation matrix (optimized for SIMD)
            float x = rotation.X, y = rotation.Y, z = rotation.Z, w = rotation.W;
            float x2 = x * x, y2 = y * y, z2 = z * z;
            float xy = x * y, xz = x * z, xw = x * w;
            float yz = y * z, yw = y * w, zw = z * w;

            // Column 0
            Vector4 col0 = new Vector4(scale.X * (1 - 2 * (y2 + z2)),
                                      scale.X * (2 * (xy + zw)),
                                      scale.X * (2 * (xz - yw)),
                                      0);

            // Column 1
            Vector4 col1 = new Vector4(scale.Y * (2 * (xy - zw)),
                                      scale.Y * (1 - 2 * (x2 + z2)),
                                      scale.Y * (2 * (yz + xw)),
                                      0);

            // Column 2
            Vector4 col2 = new Vector4(scale.Z * (2 * (xz + yw)),
                                      scale.Z * (2 * (yz - xw)),
                                      scale.Z * (1 - 2 * (x2 + y2)),
                                      0);

            // Column 3 (Translation)
            Vector4 col3 = new Vector4(translation.X, translation.Y, translation.Z, 1);

            // Construct the matrix directly (column-major order)
            return new Matrix4(col0.X, col0.Y, col0.Z, col0.W,
                               col1.X, col1.Y, col1.Z, col1.W,
                               col2.X, col2.Y, col2.Z, col2.W,
                               col3.X, col3.Y, col3.Z, col3.W);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 CreateTransformationMatrix(Vector3 scale, Quaternion rotation, Vector3 translation)
        {
            // Pre-calculate rotation components (common subexpressions)
            float x = rotation.X, y = rotation.Y, z = rotation.Z, w = rotation.W;
            float x2 = x * x, y2 = y * y, z2 = z * z;
            float xy = x * y, xz = x * z, xw = x * w;
            float yz = y * z, yw = y * w, zw = z * w;

            // Construct the matrix directly (column-major order) with pre-calculated values
            Matrix4 matrix = new Matrix4();
            matrix.M11 = scale.X * (1 - 2 * (y2 + z2));  // Scale * Rotation
            matrix.M21 = scale.X * (2 * (xy + zw));
            matrix.M31 = scale.X * (2 * (xz - yw));
            matrix.M41 = 0;

            matrix.M12 = scale.Y * (2 * (xy - zw));      // Scale * Rotation
            matrix.M22 = scale.Y * (1 - 2 * (x2 + z2));
            matrix.M32 = scale.Y * (2 * (yz + xw));
            matrix.M42 = 0;

            matrix.M13 = scale.Z * (2 * (xz + yw));      // Scale * Rotation
            matrix.M23 = scale.Z * (2 * (yz - xw));
            matrix.M33 = scale.Z * (1 - 2 * (x2 + y2));
            matrix.M43 = 0;

            matrix.M14 = translation.X;             // Translation
            matrix.M24 = translation.Y;
            matrix.M34 = translation.Z;
            matrix.M44 = 1;

            return matrix;
        }


        public static Matrix4 OriginalMatrixBuild(Vector3 localScale, Quaternion localRotation, Vector3 localPosition)
        {
            Matrix4 localTransform = Matrix4.CreateScale(localScale)
                                   * Matrix4.CreateFromQuaternion(localRotation)
                                   * Matrix4.CreateTranslation(localPosition);

            return localTransform;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExtractTransform(
    this ref Matrix4 matrix,
    out Vector3 scale,
    out Quaternion rotation,
    out Vector3 translation)
        {
            // Extract translation (direct copy from matrix)
            translation = new Vector3(matrix.M14, matrix.M24, matrix.M34);

            // Extract scale using vectorized column magnitudes
            Vector3 col0 = new Vector3(matrix.M11, matrix.M21, matrix.M31);
            Vector3 col1 = new Vector3(matrix.M12, matrix.M22, matrix.M32);
            Vector3 col2 = new Vector3(matrix.M13, matrix.M23, matrix.M33);

            scale = new Vector3(
                col0.Length,
                col1.Length,
                col2.Length
            );

            // Handle zero scale cases safely
            Vector3 safeScale = new Vector3(
                scale.X > 0f ? 1f / scale.X : 0f,
                scale.Y > 0f ? 1f / scale.Y : 0f,
                scale.Z > 0f ? 1f / scale.Z : 0f
            );

            // Remove scaling from rotation matrix
            Matrix3x3 rotMat = new Matrix3x3(
                col0 * safeScale.X,
                col1 * safeScale.Y,
                col2 * safeScale.Z
            );

            // Convert to quaternion using optimized method
            rotation = rotMat.ToQuaternion();
        }

        // Helper struct for 3x3 matrix operations
        internal struct Matrix3x3
        {
            public Vector3 Row0, Row1, Row2;

            public Matrix3x3(Vector3 col0, Vector3 col1, Vector3 col2)
            {
                Row0 = new Vector3(col0.X, col1.X, col2.X);
                Row1 = new Vector3(col0.Y, col1.Y, col2.Y);
                Row2 = new Vector3(col0.Z, col1.Z, col2.Z);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Quaternion ToQuaternion()
            {
                float trace = Row0.X + Row1.Y + Row2.Z;

                if (trace > 0f)
                {
                    float s = MathF.Sqrt(trace + 1f) * 2f;
                    return new Quaternion(
                        (Row1.Z - Row2.Y) / s,
                        (Row2.X - Row0.Z) / s,
                        (Row0.Y - Row1.X) / s,
                        s * 0.25f
                    );
                }
                else if (Row0.X > Row1.Y && Row0.X > Row2.Z)
                {
                    float s = MathF.Sqrt(1f + Row0.X - Row1.Y - Row2.Z) * 2f;
                    return new Quaternion(
                        s * 0.25f,
                        (Row0.Y + Row1.X) / s,
                        (Row2.X + Row0.Z) / s,
                        (Row1.Z - Row2.Y) / s
                    );
                }
                else if (Row1.Y > Row2.Z)
                {
                    float s = MathF.Sqrt(1f + Row1.Y - Row0.X - Row2.Z) * 2f;
                    return new Quaternion(
                        (Row0.Y + Row1.X) / s,
                        s * 0.25f,
                        (Row1.Z + Row2.Y) / s,
                        (Row2.X - Row0.Z) / s
                    );
                }
                else
                {
                    float s = MathF.Sqrt(1f + Row2.Z - Row0.X - Row1.Y) * 2f;
                    return new Quaternion(
                        (Row2.X + Row0.Z) / s,
                        (Row1.Z + Row2.Y) / s,
                        s * 0.25f,
                        (Row0.Y - Row1.X) / s
                    );
                }
            }
        }
    }
}

//public class Transform
//{
//    public Vector3 position { get=>GetWorldPosition(); set => SetWorldPosition(value); }
//    public Vector3 localPosition;
//    public Quaternion rotation = Quaternion.Identity;
//    public Vector3 scale = Vector3.One;
//    private Matrix4 _model;
//    private Transform _parent;
//    public Transform parent { get => _parent; set => SetParent(value);}
//    public bool IsValid = false;
//    public void RemoveFromParent()
//    {
//        //if (parent != null)
//       // {
//            parent = null;
//       // }
//    }
//    public void SetWorldPosition(Vector3 worldPosition)
//    {
//        // Traverse up the parent hierarchy to accumulate the inverse transformations
//        Transform parent = this.parent;
//        Matrix4 inverseTransform = Matrix4.Identity;
//        while (parent != null)
//        {
//            // Apply parent's inverse scale
//            inverseTransform *= Matrix4.CreateScale(Vector3.One / parent.scale);

//            // Apply parent's inverse rotation
//            inverseTransform *= Matrix4.CreateFromQuaternion(Quaternion.Invert(parent.rotation));

//            // Apply parent's inverse translation
//            inverseTransform *= Matrix4.CreateTranslation(-parent.position);

//            parent = parent.parent;
//        }

//        // Transform the world position to local space
//        localPosition = Vector3.Transform(localPosition, inverseTransform.ExtractRotation()) + inverseTransform.ExtractTranslation();
//        localPosition *= inverseTransform.ExtractScale();

//        // Set the local position
//        //this.localPosition  = localPosition;
//    }
//    public Vector3 GetWorldPosition()
//    {
//        // Get the local position
//        Vector3 localPosition = this.localPosition;

//        // Traverse up the parent hierarchy
//        Transform parent = this.parent;

//            localPosition = Vector3.Transform(localPosition, parent.rotation) + parent.position;
//            localPosition *= parent.scale;


//        // Return the world position
//        return localPosition;
//    }
//    public void SetParent(Transform newParent)
//    {
//        // If already has a parent, remove it
//        if (parent != null)
//        {
//            RemoveFromParent();
//        }

//        // Set the new parent


//        // Update local transform to preserve world space
//        if (newParent != null)
//        {
//            // Get the current world matrix
//            Matrix4 currentWorldMatrix = this.worldSpaceModel;
//            var rrrr1 = currentWorldMatrix.ToTransformString();
//            // Get the parent's world matrix
//            Matrix4 parentWorldMatrix = newParent.worldSpaceModel;
//            var rrrr2 = parentWorldMatrix.ToTransformString();
//            // Calculate the new local transform
//            Matrix4 newLocalMatrix = Matrix4.Invert(parentWorldMatrix) * currentWorldMatrix;

//            // Extract the new local position, rotation, and scale
//            position = newLocalMatrix.ExtractTranslation();
//            rotation = newLocalMatrix.ExtractRotation();
//            scale = newLocalMatrix.ExtractScale();
//            position = Vector3.TransformPosition(position, Matrix4.CreateScale(newParent.scale));
//            _parent = newParent;
//        }
//    }
//    public Vector3 Forward
//    {
//        get => Vector3.Transform(Vector3.UnitZ, rotation); set
//        {
//          rotation = QuaternionExtensions.LookRotation(value);
//        }
//    }
//    public Vector3 Up
//    {
//        get => Vector3.Transform(Vector3.UnitY, rotation);
//        set
//        {
//            rotation = Quaternion.FromAxisAngle(Vector3.Cross(Vector3.UnitY, value.Normalized()).Normalized(),
//                                  Vector3.CalculateAngle(Vector3.UnitY, value.Normalized()));

//        }
//    }
//    public Vector3 Right
//    {
//        get => Vector3.Normalize(Vector3.Transform(-Vector3.UnitX, rotation));
//        set
//        {
//            Vector3 newUp = Vector3.Cross(value.Normalized(), Forward).Normalized();
//            rotation = Quaternion.FromAxisAngle(Vector3.Cross(Vector3.UnitX, value.Normalized()).Normalized(),
//                                  Vector3.CalculateAngle(Vector3.UnitX, value.Normalized())) *
//            Quaternion.FromAxisAngle(Vector3.Cross(Vector3.UnitY, newUp).Normalized(),
//                                      Vector3.CalculateAngle(Vector3.UnitY, newUp));

//        }
//    }
//    public Vector3 Left => -Right;


//    public static implicit operator Matrix4 (Transform t)
//    {
//        return t.worldSpaceModel;
//    }
//    public Transform(Vector3 position, Quaternion rotation, Vector3 scale)
//    {
//        this.position = position;
//        this.rotation = rotation;
//        this.scale = scale;
//    }

//    public Transform(Vector3 position, Quaternion rotation)
//    {
//        this.position = position;
//        this.rotation = rotation;
//    }

//    public Transform()
//    {
//    }
//    public Transform(Vector3 position, Vector3 rotation)
//    {
//        this.position = position;
//        this.rotation = Quaternion.FromEulerAngles(rotation).Normalized();
//        //this.scale = scale;
//    }
//    public Transform(Vector3 position,Vector3 rotation, Vector3 scale)
//    {
//        this.position = position;
//        this.rotation = Quaternion.FromEulerAngles(rotation);
//        this.scale = scale;
//    }

//    public Transform(Vector3 position)
//    {
//        this.position = position;
//    }
//    /// <summary>
//    /// Uses invalidate strategy
//    /// if (IsValid) return _model;
//    ///else return  (_model = BuildMatrix()) ;
//    /// </summary>
//    public Matrix4 worldSpaceModel => parent == null ? localSpaceModel : localSpaceModel *parent.worldSpaceModel;

//    public Matrix4 localSpaceModel
//    {
//        get
//        {
//            Invalidate();
//            if (IsValid)
//            {
//                return _model;
//            }
//            else
//            {
//                return _model = BuildMatrix();
//            }
//        }
//    }
//    //public Matrix4 worldSpaceModel
//    //{
//    //    get
//    //    {
//    //        if (IsValid) return _model;
//    //        else return (_model = BuildMatrix());
//    //    }
//    //}
//    private Matrix4 BuildMatrix()
//    {
//        Matrix4 translationMatrix = Matrix4.CreateTranslation(localPosition);
//        Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(rotation);
//        Matrix4 scaleMatrix = Matrix4.CreateScale(scale);
//        Matrix4 localMatrix = translationMatrix * rotationMatrix * scaleMatrix;


//        // Combine the matrices in the correct order: scale, rotate, then translate.
//        IsValid = true;
//        return localMatrix;
//    }
//    public void Invalidate()
//    {
//        IsValid = false;
//    }


//    public void LookAt(Vector3 target, Vector3 up = default)
//    {
//        // If no up vector is specified, use the default up direction
//        if (up == default)
//        {
//            up = Vector3.UnitY;
//        }
//        Matrix4 viewMatrix = Matrix4.LookAt(target, target, up);
//        // Calculate the rotation needed to look at the target
//        rotation = viewMatrix.ExtractRotation();

//        // Update the transformation matrix
//        Invalidate();
//    }
//    public override string ToString()
//    {
//        return position.ToStringShort() + $"\nQ:{rotation}\nF:{Forward.ToStringShort()} \nR:{Right.ToStringShort()}  \nU:{Up.ToStringShort()}";
//    }
//}
//}
