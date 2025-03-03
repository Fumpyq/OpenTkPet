﻿using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Render
{
    public class DirectLight
    {
        public Camera cam;
        public DepthBuffer depthBuffer;
        public Transform transform { get =>cam.transform; set=>cam.transform = value; }

        public DirectLight(Vector3 position,Vector3 rotation,int Resolution = 2048*2)
        {
            //cam = new Camera(position,rotation,Camera.PerspectiveType.Orthographic);
            cam = new Camera(position,Vector3.Zero,60);
            cam.Width = Resolution;
            cam.Height = Resolution;
            cam.name = "DitLightCam";
            depthBuffer = new DepthBuffer("DirLight depth buffer",Resolution, Resolution);
        }
        public void Resize(int width, int height) {
            cam.Resize(width, height);
            depthBuffer.Resize(width, height);
        }
    }
}
