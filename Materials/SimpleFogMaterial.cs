using ConsoleApp1_Pet.Shaders;

namespace ConsoleApp1_Pet.Materials
{
    public class SimpleFogMaterial : Material
    {
        public SimpleFogMaterial()
        {

            shader = ShaderManager.CompileShader(@"Shaders\Code\DepthTextureDisplay_vert.glsl", @"Shaders\Code\Simple Fog.glsl");
        }

        public override void Use()
        {
            shader.Use();
            shader.SetTexture(Shader.ScreenTexture, MainGameWindow.instance.prePostProcessingBuffer);
            shader.SetTexture(Shader.CameraDepth, MainGameWindow.instance.depthBuffer);
        }
    }
}