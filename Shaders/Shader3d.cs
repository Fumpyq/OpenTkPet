using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1_Pet.Shaders
{
    public class Shader3d:Shader
    {
        protected override string VertexPrefix => base.VertexPrefix+$@"
layout(location = 0) uniform mat4 model;
layout(location = 1) uniform mat4 view;   
layout(location = 2) uniform mat4 projection; 
layout(location = 3) uniform mat4 viewProjection; 
layout(location = 4) uniform mat4 transform;
layout(location = 1) in vec2 aTexCoord;
out vec2 texCoord;
";
        protected override string VertexMain => $@"void main(){{
texCoord = aTexCoord;
gl_Position = vec4(aPos, 1.0)* model * viewProjection;
//gl_Position =  vec4(aPos, 1.0) * model * view* projection;
//gl_Position =   vec4(aPos, 1.0);
//gl_Position =     vec4(aPos, 1.0)* transform;

}}";
        protected override string FragMain => $@"void main(){{
FragColor = texture(texture0, texCoord);
//FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
}}";
        protected override string FragPrefix => $@"{base.FragPrefix}
in vec2 texCoord;
uniform sampler2D texture0;
";
    }
}
