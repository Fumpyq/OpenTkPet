﻿using ConsoleApp1_Pet.Render;
using ConsoleApp1_Pet.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Materials
{
    public class ScreenSpaceShadows : Material
    {
        public DirectLight light;

        public ScreenSpaceShadows(DirectLight light)
        {
            this.light = light;
            shader = ShaderManager.CompileShader(@"Shaders\Code\DepthTextureDisplay_vert.glsl", @"Shaders\Code\ScreenSpaceShadows.glsl");
        }

        public override void Use()
        {
            shader.Use();
            var invLightCameraVP = light.cam.ViewProjectionMatrix;
           
            var camVP = Camera.main.ViewProjectionMatrix;
            shader.SetUniform("lightCameraVP", invLightCameraVP);
            invLightCameraVP.Invert();
            shader.SetUniform("invLightCameraVP", invLightCameraVP);
            shader.SetTexture("lightDepth", light.depthBuffer);
            shader.SetUniform("mainCameraVP", camVP);
            shader.SetUniform("mainCameraView", Camera.main.ViewMatrix);
            camVP.Invert();
            shader.SetUniform("invMainCameraVP", camVP);
            shader.SetTexture(Shader.CameraDepth, MainGameWindow.instance.depthBuffer);
        }
    }
    public class ScreenSpaceSunFlare : Material
    {
        public DirectLight light;

        public ScreenSpaceSunFlare(DirectLight light)
        {
            this.light = light;
            shader = ShaderManager.CompileShader(@"Shaders\Code\DepthTextureDisplay_vert.glsl", @"Shaders\Code\SunFlare_frag.glsl");
        }

        public override void Use()
        {
            shader.Use();
            shader.SetUniform("sunPosition", light.transform.position);

            shader.SetTexture(Shader.CameraDepth, MainGameWindow.instance.depthBuffer);
        }
    }
}
