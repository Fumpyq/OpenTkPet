#version 460 core
layout(std140, binding = 0) uniform CameraData {
    mat4 view;
    mat4 projection;
    mat4 viewProjection;
};




layout(location = 4) uniform mat4 transform;


layout(location = 0) in mat4 instanceModel;
layout (location = 4) in vec3 aPos;
layout (location = 5) in vec2 aTexCoord;
out vec2 texCoord;

 void main(){
texCoord = aTexCoord;
gl_Position = vec4(aPos, 1.0)* instanceModel * viewProjection;
//gl_Position =  vec4(aPos, 1.0) * model * view* projection;
//gl_Position =   vec4(aPos, 1.0);
//gl_Position =     vec4(aPos, 1.0)* transform;

}