#version 460 core
layout (location = 0) in vec3 aPos;
layout(location = 2) in vec2 aTexCoord;
layout(location = 3) in mat4 instaceTransform;

layout(location = 0) uniform mat4 model;
layout(location = 1) uniform mat4 view;
layout(location = 2) uniform mat4 projection;
layout(location = 3) uniform mat4 viewProjection;
layout(location = 4) uniform mat4 transform;

out vec2 texCoord;

 void main(){
texCoord = aTexCoord;
//gl_Position = vec4(aPos, 1.0)* instaceTransform * viewProjection;
gl_Position = vec4(aPos, 1.0)* model * viewProjection;
//gl_Position =  vec4(aPos, 1.0) * model * view* projection;
//gl_Position =   vec4(aPos, 1.0);
//gl_Position =     vec4(aPos, 1.0)* transform;

}