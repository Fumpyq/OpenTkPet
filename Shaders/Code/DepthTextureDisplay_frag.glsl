#version 460 core
out vec4 FragColor;

in vec2 uv;

uniform sampler2D texture0;
void main(){
	float depth = texture(texture0, uv).r;
	float normalizedDepth = pow((depth-0.80f) *5,50);      
	FragColor = vec4(normalizedDepth,normalizedDepth,normalizedDepth,1.0f );
	//FragColor = vec4(0.0f,0.0f,0.0f,1-normalizedDepth );
	//FragColor = texture(texture0, uv); 
	//FragColor = vec4(uv,0.0f,1.0f);  
} 