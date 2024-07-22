using ConsoleApp1_Pet.Materials;
using ConsoleApp1_Pet.Meshes;
using ConsoleApp1_Pet.Shaders;
using ConsoleApp1_Pet.Новая_папка;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace ConsoleApp1_Pet.Render
{
    public class RenderObject
    {
        public Material material { get => materials.Count > 0 ? materials[0] : null;set { 
            if(materials.Count > 0) materials[0] = value;
                else { materials.Add(value); }
            }  }
        public List<Material> materials = new List<Material>();

        public Mesh mesh;

        public Transform transform = new Transform();

        public RenderObject(Mesh mesh, Transform transform,params Material[] materials)
        {
            this.materials = new List<Material>(materials);
            this.mesh = mesh;
            this.transform = transform;
        }

        public RenderObject(Mesh mesh, params Material[] materials)
        {
            this.materials = new List<Material>(materials);
            this.mesh = mesh;
            //this.transform = new Transform();
        }

        public void DirectDraw(Matrix4 view,Matrix4 project)
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
        private Vector3 _localPosition      = Vector3.Zero;
        private Quaternion _localRotation   = Quaternion.Identity;
        private Vector3 _localScale         = Vector3.One;

        // World space properties
        private Vector3 _worldPosition      = Vector3.Zero;
        private Quaternion _worldRotation   = Quaternion.Identity;
        private Vector3 _worldScale         = Vector3.One;
        // Parent transform
        private Transform _parent;

        public Matrix4 matrixCache;
        /// <summary>
        /// Initializes a new instance of the <see cref="Transform"/> class.
        /// </summary>
        public Transform()
        {
            //_localPosition 
            //_localRotation 
            //_localScale 
        }

        /// <summary>
        /// Gets or sets the position of the transform in local space.
        /// </summary>
        public Vector3 LocalPosition
        {
            get => _localPosition;
            set
            {
                _localPosition = value;
                UpdateWorldTransform();
            }
        }

        /// <summary>
        /// Gets or sets the rotation of the transform in local space.
        /// </summary>
        public Quaternion LocalRotation
        {
            get => _localRotation;
            set
            {
                _localRotation = value;
                UpdateWorldTransform();
            }
        }

        /// <summary>
        /// Gets or sets the scale of the transform in local space.
        /// </summary>
        public Vector3 LocalScale
        {
            get => _localScale;
            set
            {
                _localScale = value;
                UpdateWorldTransform();
            }
        }

        /// <summary>
        /// Gets the position of the transform in world space.
        /// </summary>
        public Vector3 WorldPosition => _worldPosition;

        /// <summary>
        /// Gets the rotation of the transform in world space.
        /// </summary>
        public Quaternion WorldRotation => _worldRotation;

        /// <summary>
        /// Gets the scale of the transform in world space.
        /// </summary>
        public Vector3 WorldScale => _worldScale;
        public Vector3 position
        {
            get => _worldPosition;
            set
            {
                if (_parent != null)
                {
                    // Calculate local position relative to parent
                    _localPosition = value.Transform(Matrix4.Invert(_parent.WorldMatrix));
                }
                else
                {
                    _localPosition = value;
                }
                UpdateWorldTransform();
            }
        }

        public Quaternion rotation
        {
            get => _worldRotation;
            set
            {
                if (_parent != null)
                {
                    // Calculate local rotation relative to parent
                    _localRotation = Quaternion.Multiply(Quaternion.Invert(_parent.WorldRotation), value);
                }
                else
                {
                    _localRotation = value;
                }
                UpdateWorldTransform();
            }
        }

        public Vector3 scale
        {
            get => _worldScale;
            set
            {
                if (_parent != null)
                {
                    // Calculate local scale relative to parent
                    _localScale = Vector3.Divide(value, _parent.WorldScale);
                }
                else
                {
                    _localScale = value;
                }
                UpdateWorldTransform();
            }
        }
        public Transform parent { get=> Parent; set => Parent = value; }
        /// <summary>
        /// Gets or sets the parent transform.
        /// </summary>
        public Transform Parent
        {
            get => _parent;
            set
            {
                if (_parent != null)
                {
                    _parent.OnTransformChanged -= UpdateWorldTransform;
                }

                _parent = value;

                if (_parent != null)
                {
                    _parent.OnTransformChanged += UpdateWorldTransform;
                    UpdateWorldTransform();
                }
            }
        }

        /// <summary>
        /// Event that is raised when the transform changes.
        /// </summary>
        public event Action OnTransformChanged;

        /// <summary>
        /// Updates the world space transform based on the local space transform and parent transform.
        /// </summary>
        private void UpdateWorldTransform()
        {
            // Calculate world space position, rotation, and scale
            Matrix4 localTransform = Matrix4.CreateScale(_localScale) * Matrix4.CreateFromQuaternion(_localRotation) * Matrix4.CreateTranslation(_localPosition);

            if (_parent != null)
            {
                localTransform *= _parent.WorldMatrix;
            }

            _worldPosition = localTransform.ExtractTranslation();
            _worldRotation = localTransform.ExtractRotation();
            _worldScale = localTransform.ExtractScale();
            matrixCache = localTransform;

            // Raise the transform changed event
            OnTransformChanged?.Invoke();
        }

        /// <summary>
        /// Gets the world space matrix for the transform.
        /// </summary>
        public Matrix4 WorldMatrix
        {
            get
            {
               // return Matrix4.CreateScale(_worldScale) * Matrix4.CreateFromQuaternion(_worldRotation) * Matrix4.CreateTranslation(_worldPosition);
                return matrixCache;
            }
        }
        public Vector3 Forward
        {
            get => Vector3.Transform(Vector3.UnitZ, rotation); set
            {
                rotation = QuaternionExtensions.LookRotation(value);
            }
        }
        public Vector3 Up
        {
            get => Vector3.Transform(Vector3.UnitY, rotation);
            set
            {
                rotation = Quaternion.FromAxisAngle(Vector3.Cross(Vector3.UnitY, value.Normalized()).Normalized(),
                                      Vector3.CalculateAngle(Vector3.UnitY, value.Normalized()));

            }
        }
        public Vector3 Right
        {
            get => Vector3.Normalize(Vector3.Transform(-Vector3.UnitX, rotation));
            set
            {
                Vector3 newUp = Vector3.Cross(value.Normalized(), Forward).Normalized();
                rotation = Quaternion.FromAxisAngle(Vector3.Cross(Vector3.UnitX, value.Normalized()).Normalized(),
                                      Vector3.CalculateAngle(Vector3.UnitX, value.Normalized())) *
                Quaternion.FromAxisAngle(Vector3.Cross(Vector3.UnitY, newUp).Normalized(),
                                          Vector3.CalculateAngle(Vector3.UnitY, newUp));

            }
        }
        public Vector3 Left => -Right;


        public static implicit operator Matrix4(Transform t)
        {
            return t.worldSpaceModel;
        }
        public Transform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public Transform(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }


        public Transform(Vector3 position, Vector3 rotation)
        {
            this.position = position;
            this.rotation = Quaternion.FromEulerAngles(rotation).Normalized();
            //this.scale = scale;
        }
        public Transform(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = Quaternion.FromEulerAngles(rotation);
            this.scale = scale;
        }

        public Transform(Vector3 position)
        {
            this.position = position;
        }
        /// <summary>
        /// Uses invalidate strategy
        /// if (IsValid) return _model;
        ///else return  (_model = BuildMatrix()) ;
        /// </summary>
        public Matrix4 worldSpaceModel => WorldMatrix;
        public override string ToString()
        {
            return position.ToStringShort() + $"\nQ:{rotation}\nF:{Forward.ToStringShort()} \nR:{Right.ToStringShort()}  \nU:{Up.ToStringShort()}";
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
