using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Render
{
    public class DirectLight: ICamera
    {
        public Camera cam;
        public FrameBuffer depthBuffer;
        public Transform transform { get =>cam.transform; set=>cam.transform = value; }
        public Matrix4 LightProjectionMatrix { get; private set; }
        public Vector3 Direction => transform.Forward.Normalized();

        public Vector3 position => ((ICamera)cam).position;

        public Matrix4 ProjectionMatrix => ((ICamera)cam).ProjectionMatrix;

        public Matrix4 ViewMatrix => ((ICamera)cam).ViewMatrix;

        public Matrix4 ViewProjectionMatrix => ((ICamera)cam).ViewProjectionMatrix;

        public DirectLight(Vector3 position,Vector3 rotation,int Resolution = 2048*2)
        {
            cam = new Camera(position,rotation,180,Camera.PerspectiveType.Orthographic);
           // cam = new Camera(position, rotation, 60);
            cam.Width = Resolution;
            cam.Height = Resolution;
            cam.name = "DitLightCam";
            //depthBuffer = new DepthBuffer("DirLight depth buffer",Resolution, Resolution);
            depthBuffer = FrameBufferPresets.CreateShadowOrDepthMap(Resolution, Resolution);
        }
        public void Resize(int width, int height) {
            cam.Resize(width, height);
            depthBuffer.Resize(width, height);
        }



    }
}
