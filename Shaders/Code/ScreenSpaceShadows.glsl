#version 460

uniform mat4 mainCameraView; // View-Projection matrix for screen 1
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
float distance(vec3 a, vec3 b) {
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
vec3 WorldSpaceFromDepth(float depth, vec2 uv, mat4 invViewProj, float bias) {
    {
        depth = (depth- bias) * 2.0 - 1.0 - bias;

        vec4 clipSpacePosition = vec4(uv * 2.0 - 1.0, depth, 1.0) - bias;

        vec4 worldSpacePosition = invViewProj * clipSpacePosition;

        return worldSpacePosition.xyz / worldSpacePosition.w;  
    }
} 
vec3 WorldSpaceFromDepth(sampler2D depthTex, vec2 screenUV, mat4 invViewProj, float bias) {
   return WorldSpaceFromDepth(texture(depthTex, screenUV).r, screenUV, invViewProj, bias);
}

const vec3 sphereCenter = vec3(6.0, 6.0, 6.0); // Center in world space
const float sphereRadius = 2.0f; // Radius in world space    
out vec4 FragColor;
vec4 DrawSphere() {
    vec2 fragCoord = uv;
    // Get the screen space position in NDC (normalized device coordinates)
    vec2 ndcCoord = fragCoord * 2.0 - 1.0; 

    // Construct a ray in world space
    // We will use the camera position and direction based on the fragment's NDC position
    vec3 cameraPos = vec3(0.0, 0.0, 0.0); // Assuming camera at world origin
    vec3 rayDir = vec3(ndcCoord, -1.0); // Assuming perspective projection

    // Transform the ray direction to world space
    vec3 worldSpaceRayDir = (transpose(invMainCameraVP) * vec4(rayDir, 0.0)).xyz;

    // Calculate the intersection point of the ray with the sphere
    vec3 sphereToRayOrigin = cameraPos - sphereCenter; 
    float a = dot(worldSpaceRayDir, worldSpaceRayDir);      
    float b = 2.0 * dot(sphereToRayOrigin, worldSpaceRayDir);   
    float c = dot(sphereToRayOrigin, sphereToRayOrigin) - sphereRadius * sphereRadius;

    // Solve quadratic equation for intersection points
    float discriminant = b * b - 4.0 * a * c;

    // Check for intersection
    if (discriminant >= 0.0) {
        // Calculate the closest intersection point
        float t = (-b - sqrt(discriminant)) / (2.0 * a);
        vec3 intersectionPoint = cameraPos + t * worldSpaceRayDir;

        // Calculate the distance from the intersection point to the sphere center
        float distance = length(intersectionPoint - sphereCenter);

        // Check if the intersection point is inside the sphere
        if (distance <= sphereRadius) {
            // Set color to red if inside
            return vec4(1.0, 0.0, 0.0, 1.0);
        }
        else {
            // Set color to blue if outside
            return vec4(0.0, 0.0, 1.0, 1.0);
        }
    }
    else {
        // No intersection, set color to blue
        return vec4(0.0, 0.0, 1.0, 1.0);
    }
}
vec3 extractTranslation(mat4 matrix) {
    return matrix[3].xyz;
}
void main()
{
    float dd = texture(_camDepth, uv).r;  
    vec3 mainCameraPixelWorldSpace = WorldSpaceFromDepth(dd , uv,transpose(invMainCameraVP),  0.0f);
    vec3 camPos = extractTranslation(mainCameraView);
    //vec3 mainCameraPixelWorldSpace = new vec3(5,5,5);  
    vec2 lightCameraUv = worldToCameraUV(mainCameraPixelWorldSpace, transpose(lightCameraVP));
    float ldd = texture(lightDepth, lightCameraUv).r;
    vec3 lightCameraPixelWorldSpace = WorldSpaceFromDepth(ldd,lightCameraUv, transpose(invLightCameraVP),0.0f);
      
    float camdist = distance(camPos, mainCameraPixelWorldSpace);
   // float dist = distance(mainCameraPixelWorldSpace,lightCameraPixelWorldSpace);     
    float dist = fastDistance(mainCameraPixelWorldSpace,lightCameraPixelWorldSpace); 
    dist -=0.25f;  
    //dist /= dd * 4;
    //dist = sqrt(dist-0.1f) ;
    // bool cond = dist < dd*4;
   // bool cond = mainCameraPixelWorldSpace.r>1 && mainCameraPixelWorldSpace.g>1 && mainCameraPixelWorldSpace.b > 1
    //    && mainCameraPixelWorldSpace.r < 5 && mainCameraPixelWorldSpace.g < 5  && mainCameraPixelWorldSpace.b < 5;
  //  FragColor = cond ? vec4(0.02f, 0.04f, 0.01f, 0.4f):vec4(0.9f,0.92f,0.91f,0.6f);
     FragColor = vec4(0.02f, 0.04f, 0.01f, dd >= 1.0f ? 0 : min(dist*  sqrt(camdist), 0.8f));
    //FragColor = vec4(mainCameraPixelWorldSpace, 1.0f);      
   // FragColor = vec4(lightCameraPixelWorldSpace, 1.0f);
   // FragColor = DrawSphere();
    // FragColor = vec4(lightCameraPixelWorldSpace, 0.8f); 
}