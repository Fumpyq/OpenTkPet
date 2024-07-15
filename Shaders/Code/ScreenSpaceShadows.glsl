#version 460


uniform mat4 mainCameraVP; // View-Projection matrix for screen 1
uniform mat4 invMainCameraVP; // View-Projection matrix for screen 1
uniform mat4 lightCameraVP; // View-Projection matrix for screen 2
uniform mat4 invLightCameraVP; // View-Projection matrix for screen 2

in vec2 uv; // Screen-space position on screen 1 (normalized [0, 1])

uniform sampler2D _camDepth;
uniform sampler2D lightDepth;
vec3 worldPosFromDepth(float depth, vec2 screenUV, mat4 invViewProj) {
  vec4 ndc = vec4(screenUV * 2.0 - 1.0, depth, 1.0);
  vec4 worldPos = invViewProj * ndc;
  return worldPos.xyz / worldPos.w;
}
vec3 worldPosFromDepthTexture(sampler2D depthTex, vec2 screenUV, mat4 invViewProj) {
  float depth = texture(depthTex, screenUV).r;
  return worldPosFromDepth(depth, screenUV, invViewProj);
}
float distance(vec2 a, vec2 b) {
    return length(a - b);
}
vec2 worldToCameraUV(vec3 worldPos, mat4 viewProj) {
  vec4 clipPos = viewProj * vec4(worldPos, 1.0);
  vec2 ndc = clipPos.xy / clipPos.w;
  return ndc * 0.5 + 0.5;
}
float fastDistance(vec3 a, vec3 b) {
  return abs(a.x - b.x) + abs(a.y - b.y) + abs(a.z - b.z);
}
vec3 calculate_world_position(vec2 texture_coordinate, float depth_from_depth_buffer, mat4 invViewProj)
{
    vec4 clip_space_position = vec4(texture_coordinate * 2.0 - vec2(1.0), 2.0 * depth_from_depth_buffer - 1.0, 1.0);

    vec4 position = invViewProj * clip_space_position; // Use this for world space

    return(position.xyz / position.w);
}
vec3 calculate_world_position(sampler2D depthTex, vec2 UV, mat4 invViewProj)
{
    return calculate_world_position(UV,texture(depthTex, UV).r,invViewProj);
} 

out vec4 FragColor;
void main()
{
    vec3 mainCameraPixelWorldSpace = worldPosFromDepthTexture(_camDepth,uv,transpose(invMainCameraVP));
    vec2 lightCameraUv = worldToCameraUV(mainCameraPixelWorldSpace,lightCameraVP);

    vec3 lightCameraPixelWorldSpace = worldPosFromDepthTexture(lightDepth,lightCameraUv,invLightCameraVP  );

    float dist = distance(mainCameraPixelWorldSpace,lightCameraPixelWorldSpace);

    bool cond = dist >0.4f;
    //bool cond = mainCameraPixelWorldSpace.r>1 && mainCameraPixelWorldSpace.g>1;
    FragColor = cond ? vec4(0.1f,0.2f,0.4f,0.4f):vec4(0.9f,0.92f,0.91f,0.6f);

}