using ConsoleApp1_Pet.Новая_папка;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ConsoleApp1_Pet.Render.Camera;

namespace ConsoleApp1_Pet.Render
{
    public class Camera
    {
        public string name;
        public enum PerspectiveType
        {
            Orthographic,
            Perspective
        }
        // Position
        public Transform transform;

        // Field of view (in radians)
        public float FOV  = 60; // 45 degrees MathF.PI / 4

        public PerspectiveType perspectiveType = PerspectiveType.Perspective;

        // Near and far clipping planes
        public float nearPlane = 0.1f;
        public float farPlane = 100.0f;
        public int Width, Height;
        // Rotation around the X axis (radians)
        public float Pitch;

        // Rotation around the Y axis (radians)
        public float Yaw = -MathHelper.PiOver2; // Without this, you would be started rotated 90 degrees right.


        public Camera(Vector3 position, Vector3 rotation, float fOV, PerspectiveType perspectiveType)
        {
            this.transform = new Transform(position, rotation);
    
            FOV = fOV;
            this.perspectiveType = perspectiveType;
            OnCreate();
        }
        public void Resize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public Camera(Vector3 position, Vector3 rotation, float fOV, PerspectiveType perspectiveType, float nearPlane, float farPlane) : this(position, rotation, fOV, perspectiveType)
        {
            this.nearPlane = nearPlane;
            this.farPlane = farPlane;
        }

        public Camera(Vector3 position, Vector3 rotation, PerspectiveType perspectiveType)
        {
            this.transform = new Transform(position, rotation);
            this.perspectiveType = perspectiveType;
            OnCreate();
        }

        public Camera(Vector3 position, Vector3 rotation, float fOV)
        {
            this.transform = new Transform(position, rotation);
            FOV = fOV;
            OnCreate();
        }

        public Camera(Vector3 position, Vector3 rotation, float fOV, float nearPlane, float farPlane) : this(position, rotation, fOV)
        {
            this.nearPlane = nearPlane;
            this.farPlane = farPlane;
            
        }

        protected void OnCreate()
        {
            Game.instance.allCameras.Add(this);
            Width = Game.instance.ClientSize.X;
            Height = Game.instance.ClientSize.Y;
        }

        // Calculate the view matrix (camera to world space)
        public Matrix4 ViewMatrix
        {
            get
            {
                return Matrix4.LookAt(transform.position, transform.position+transform.Forward, Vector3.UnitY);
            }
        }
        public static implicit operator Matrix4(Camera c)
        {
            return c.ViewProjectionMatrix;
        }
        // Calculate the projection matrix (3D space to 2D screen)
        public Matrix4 ProjectionMatrix
        {
            get
            {
                switch (perspectiveType)
                {
                    case PerspectiveType.Perspective: return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FOV),(float)  Width/Height, nearPlane, farPlane);
                    case PerspectiveType.Orthographic: return Matrix4.CreateOrthographicOffCenter(0.0f, 12f, 0.0f, 12f, 0.1f, 100.0f);
                }
                return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FOV), (float)Width / Height, nearPlane, farPlane);
            }
        }

        // Calculate the combined view-projection matrix
        public Matrix4 ViewProjectionMatrix
        {
            get
            {
                return ViewMatrix* ProjectionMatrix;
            }
        }
       
    }
}
