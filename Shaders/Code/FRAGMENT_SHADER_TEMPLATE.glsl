#version 460 core
out vec4 FragColor;

//uniform mat4 mainCameraView; 
//uniform mat4 mainCameraVP; 
//uniform sampler2D _camDepth;

in vec2 uv;

void main(){
	
 FragColor = vec4(uv,0.0f,0.0f);

} 