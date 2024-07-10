using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Shaders
{
    public class OnScreenTextureShader:Shader2d
    {
        public override string Name => "Texture 2D Shader";
        protected override string FragPrefix => base.FragPrefix + @"
uniform sampler2D texture0;";
        protected override string FragMain => @$"void main(){{
float depth = texture(texture0, uv).r;
float normalizedDepth = (depth - 0.1f) / (100 - 0.1f); 
FragColor = vec4(normalizedDepth,normalizedDepth,normalizedDepth,1.0f );
//FragColor = texture(texture0, uv);
//FragColor = vec4(uv,0.0f,1.0f);
}}";
    }
}
