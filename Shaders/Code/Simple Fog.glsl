#version 460 core
out vec4 FragColor;

//uniform mat4 mainCameraView; 
uniform mat4 mainCameraVP; 
uniform sampler2D _camDepth;
uniform sampler2D _screenTexture;
uniform float UfogDensity;
uniform float UfogStart;
uniform float UfogEnd;

in vec2 uv;
vec3 WorldSpaceFromDepth(float depth, vec2 uv, mat4 invViewProj, float bias) {
    {
        depth = (depth - bias) * 2.0 - 1.0 - bias;

        vec4 clipSpacePosition = vec4(uv * 2.0 - 1.0, depth, 1.0) - bias;

        vec4 worldSpacePosition = invViewProj * clipSpacePosition;

        return worldSpacePosition.xyz / worldSpacePosition.w;
    }
}
vec3 WorldSpaceFromDepth(sampler2D depthTex, vec2 screenUV, mat4 invViewProj, float bias) {
    return WorldSpaceFromDepth(texture(depthTex, screenUV).r, screenUV, invViewProj, bias);
}
void main(){
vec2 TexCoord = uv;
	float fogDensity  =24.0f; //  =UfogDensity;
    float fogNear     =1.0f; //  =UfogStart;
    float fogFar      =120.0f; //  =UfogEnd;
    vec3 fogColor = vec3(0.5, 0.6, 0.8);
    // Get texture color
    vec4 texColor = texture(_screenTexture, TexCoord);

    // Get depth from depth texture
    float depth = texture(_camDepth, TexCoord).r;

    // Reconstruct linear depth from normalized depth
    float linearDepth = (2.0 * fogNear * fogFar) / (fogFar + fogNear - depth * (fogFar - fogNear));

    // Calculate fog factor based on depth and density 
    float fogFactor = clamp((linearDepth - fogNear) / (fogFar - fogNear), 0.0, 1.0);
    fogFactor = pow(fogFactor, fogDensity); // Adjust fog density for more control

    // Blend texture color with fog color
    FragColor = mix(texColor, vec4(fogColor, texColor.a), fogFactor);
} 