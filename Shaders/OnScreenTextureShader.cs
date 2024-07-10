using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Shaders
{
    public class OnScreenTextureShader:Shader2d
    {

        protected override string FragPrefix => base.FragPrefix + @"layout(location = 0)uniform sampler2D texture0;";
        protected override string FragMain => @$"void main(){{
FragColor = texture(texture0, uv);
}}";
    }
}
