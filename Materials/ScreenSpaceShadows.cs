using ConsoleApp1_Pet.Render;
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
            invLightCameraVP.Invert();
            var camVP = Game.instance.mainCamera.ViewMatrix;
            shader.SetUniform("lightCameraVP",light.cam.ViewProjectionMatrix);
            shader.SetUniform("invLightCameraVP", invLightCameraVP);
            shader.SetTexture("lightDepth", light.depthBuffer);
            shader.SetUniform("mainCameraVP", camVP);
            camVP.Invert();
            shader.SetUniform("invMainCameraVP", camVP);
            shader.SetTexture(Shader.CameraDepth, Game.instance.depthBuffer);
        }
    }
}
