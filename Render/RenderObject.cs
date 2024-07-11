using ConsoleApp1_Pet.Materials;
using ConsoleApp1_Pet.Meshes;
using ConsoleApp1_Pet.Shaders;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
            // var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(Game.instance._stopwatch.Elapsed.TotalSeconds * 35));
            // model *= Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(Game.instance._stopwatch.Elapsed.TotalSeconds * 25));
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
        public Vector3 position;
        public Quaternion rotation = Quaternion.Identity;
        public Vector3 scale = Vector3.One;
        private Matrix4 _model;
        public Transform parent;
        public bool IsValid = false;



        public Vector3 Forward
        {
            get => Vector3.Transform(Vector3.UnitZ, rotation); set
            {
                rotation = Quaternion.FromAxisAngle(Vector3.Cross(Vector3.UnitZ, value.Normalized()).Normalized(),
                                      Vector3.CalculateAngle(Vector3.UnitZ, value.Normalized()));

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
            get => Vector3.Transform(-Vector3.UnitX, rotation);
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
            
        
        public static implicit operator Matrix4 (Transform t)
        {
            return t.model;
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

        public Transform()
        {
        }

        public Transform(Vector3 position, Vector3 scale)
        {
            this.position = position;
            
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
        public Matrix4 model { get
            {

                if (IsValid)
                {
                    return parent == null ? _model : parent.model * _model;
                }
                else
                {
                    return parent == null ? (_model = BuildMatrix()) : parent.model *( _model = BuildMatrix());
                }
            }
        }
        private Matrix4 BuildMatrix()
        {
            Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(rotation);

            // Create a scaling matrix.
            Matrix4 scaleMatrix = Matrix4.CreateScale(scale);

            // Create a translation matrix.
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);

            // Combine the matrices in the correct order: scale, rotate, then translate.
            IsValid = true;
            return translationMatrix * rotationMatrix * scaleMatrix;
        }
        public void Invalidate()
        {
            IsValid = false;
        }


        public void LookAt(Vector3 target, Vector3 up = default)
        {
            // If no up vector is specified, use the default up direction
            if (up == default)
            {
                up = Vector3.UnitY;
            }
            Matrix4 viewMatrix = Matrix4.LookAt(target, target, up);
            // Calculate the rotation needed to look at the target
            rotation = viewMatrix.ExtractRotation();

            // Update the transformation matrix
            Invalidate();
        }
    }
}
