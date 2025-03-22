using ConsoleApp1_Pet.Shaders;

namespace ConsoleApp1_Pet.Materials
{
    public class SimpleFogMaterial
    {
        public SimpleFogMaterial()
        {

            shader = MainGameWindow.instance.resources.CreateShader("PostProcessing_Fog", @"Shaders\Code\DepthTextureDisplay_vert.glsl", @"Shaders\Code\Simple Fog.glsl");
        }

        public override void Use()
        {
            shader.Use();
            shader.SetTexture(Shader.ScreenTexture, MainGameWindow.instance.prePostProcessingGBuffer);
            shader.SetTexture(Shader.CameraDepth, MainGameWindow.instance.depthBuffer);
        }
    }
}