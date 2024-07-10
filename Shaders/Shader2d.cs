using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Shaders
{
    public class Shader2d:Shader
    {
        public override string Name => "Default 2D Shader "; 
        protected override string VertexPrefix => base.VertexPrefix + $@"
layout(location = 1) in vec2 uvin;
out vec2 uv;
";
        protected override string FragPrefix => base.FragPrefix +$@"
in vec2 uv;
";
        protected override string VertexMain => $@"
void main(){{
uv = uvin;
gl_Position = vec4(aPos, 1.0);
}}";
    }
}
