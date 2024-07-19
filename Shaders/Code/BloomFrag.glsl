#version 460 core
out vec4 FragColor;

//uniform mat4 mainCameraView; 
//uniform mat4 mainCameraVP; 
//uniform sampler2D _camDepth;

uniform float UbloomThreshold;
uniform float UbloomIntensity;
uniform sampler2D _screenTexture;

in vec2 uv;

void main(){
	float bloomThreshold = 1;
    float bloomIntensity = 1.f;
    // Get the color from the screen texture
    vec4 color = texture(_screenTexture, uv);

    // Calculate bloom contribution
    float bloomFactor = max(color.r - bloomThreshold, 0.0) +
                       max(color.g - bloomThreshold, 0.0) +
                       max(color.b - bloomThreshold, 0.0);
    bloomFactor = clamp(bloomFactor, 0.0, 1.0);

    // Apply bloom effect
    color.rgb += bloomIntensity * bloomFactor * color.rgb;

    FragColor = color*2 ;

} 