#version 460
#pragma STDGL precision highp float;
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
        depth = (depth - bias) * 2.0 - 1.0 - bias;

        vec4 clipSpacePosition = vec4(uv * 2.0 - 1.0, depth, 1.0) - bias;

        vec4 worldSpacePosition = invViewProj * clipSpacePosition;

        return worldSpacePosition.xyz / worldSpacePosition.w;
    }
}
vec3 WorldSpaceFromDepth(sampler2D depthTex, vec2 screenUV, mat4 invViewProj, float bias) {
    return WorldSpaceFromDepth(texture(depthTex, screenUV).r, screenUV, invViewProj, bias);
}
float rand(vec2 co) {
    return fract(sin(dot(co.xy, vec2(12.9898, 78.233))) * 43758.5453);
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
    discard;
        // No intersection, set color to blue
        //return vec4(0.0, 0.0, 1.0, 1.0);
    }
}
vec3 extractTranslation(mat4 matrix) {
    return matrix[3].xyz;
}
vec2 snapToGrid(vec2 position, float gridSize) {
  // Calculate the grid cell coordinates.
  vec2 gridCell = floor(position / gridSize);

  // Snap the position to the center of the grid cell.
  return (gridCell) * gridSize;
}vec3 snapToGrid(vec3 position, float gridSize) {
  // Calculate the grid cell coordinates.
  vec3 gridCell = floor(position / gridSize);

  // Snap the position to the center of the grid cell.
  return (gridCell) * gridSize;
}
vec4 simpleShadowPass(){
    float dd = texture(_camDepth, uv).r;
    vec3 mainCameraPixelWorldSpace = WorldSpaceFromDepth(dd, uv, transpose(invMainCameraVP), 0.0f);
    
    vec3 camPos = extractTranslation(mainCameraView);
    
    vec2 lightCameraUv = worldToCameraUV(mainCameraPixelWorldSpace, transpose(lightCameraVP));
        float ldd = texture(lightDepth, lightCameraUv).r;
    vec3 lightCameraPixelWorldSpace = WorldSpaceFromDepth(ldd, lightCameraUv, transpose(invLightCameraVP), 0.0f);
    vec2 texelSize = 1.0f / textureSize(lightDepth, 0);
     vec2 texelSize2 = 1.0f / textureSize(_camDepth, 0);

    float shadow  =0;
    int steps=4;
   // int steps2=steps*2+1;
    int TotalSteps=0;
    for(int x = -steps; x <= steps; ++x)
    {
        for(int y = -steps; y <= steps; ++y)
        { 
            //dd = texture(_camDepth, uv + vec2(x, y) * texelSize2).r;
            //mainCameraPixelWorldSpace = WorldSpaceFromDepth(dd, uv+ vec2(x, y) * texelSize2, transpose(invMainCameraVP), 0.0f);
            vec2 Luv = lightCameraUv + vec2(x, y) * texelSize;
            float ldd = texture(lightDepth, Luv).r;
            lightCameraPixelWorldSpace = WorldSpaceFromDepth(ldd, Luv , transpose(invLightCameraVP), 0.0f);
            float dist2 = distance(mainCameraPixelWorldSpace, lightCameraPixelWorldSpace);

           // float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r; 
            shadow += dist2; 
           TotalSteps+= 1;
               //int(length(lightCameraPixelWorldSpace)/2);
        }   
    }
    shadow /= TotalSteps* 3;
    float dist = shadow;
    // dist = distance(mainCameraPixelWorldSpace, lightCameraPixelWorldSpace);
    // dist = distance(mainCameraPixelWorldSpace, MinPos);
    // dist +=rand();
    if (dist < 0.07f)
        return vec4(0.97f, 0.92, 0.89, 0.04f);
     return vec4(0.02f, 0.04f, 0.01f, dd >= 1.0f ? 0 : min((dist  - 0.077f) * 25, 0.7f));
}
vec4 MoreComplexShadows(){
float penumbraScale =3.0f; // Controls the size of the soft shadow
    float minPenumbraSize = 1.0f; // Minimum size of the penumbra

    float dd = texture(_camDepth, uv).r;

    vec3 mainCameraPixelWorldSpace = WorldSpaceFromDepth(dd, uv, transpose(invMainCameraVP), 0.0f);
    vec2 lightCameraUv = worldToCameraUV(mainCameraPixelWorldSpace, transpose(lightCameraVP));

    vec2 shadowTexelSize = 1.0f / textureSize(lightDepth, 0);

    float depthGradient =
        abs(
            (texture(lightDepth, lightCameraUv + vec2(shadowTexelSize.x*5, 0.0)).r * 2.0 - 1.0) -
            (texture(lightDepth, lightCameraUv - vec2(shadowTexelSize.x*5, 0.0)).r * 2.0 - 1.0)
        ) +
        abs(
            (texture(lightDepth, lightCameraUv + vec2(0.0, shadowTexelSize.y*5)).r * 2.0 - 1.0) -
            (texture(lightDepth, lightCameraUv - vec2(0.0, shadowTexelSize.y*5)).r * 2.0 - 1.0)
        );

    float penumbraSize = max(minPenumbraSize, depthGradient * penumbraScale);
    bool IsSame = penumbraSize != minPenumbraSize;
    float shadowValue = 0.0;
    float penumbraSamples = 0.0;
    float MinDist = 53636;
    vec3 MinPosition = vec3(42525);

    for (int i = -int(penumbraSize); i <= int(penumbraSize); i++) {
        for (int j = -int(penumbraSize); j <= int(penumbraSize); j++) {
            vec2 lUv = lightCameraUv + vec2(i, j) * shadowTexelSize;
            float ldd = texture(lightDepth, lUv).r;
            vec3 lightCameraPixelWorldSpace = WorldSpaceFromDepth(ldd, lUv, transpose(invLightCameraVP), 0.0f);
            float dist = distance(mainCameraPixelWorldSpace, lightCameraPixelWorldSpace);
            penumbraSamples += 1.0;
            shadowValue+=dist-0.001f;
            if (MinDist > dist) {
                MinDist = dist;
                MinPosition = lightCameraPixelWorldSpace;
                //penumbraSamples += 1.0;

            }
        }
    }
    shadowValue /= penumbraSamples;    
    float dist = shadowValue;
    //dist = distance(mainCameraPixelWorldSpace, MinPosition);
    dist = exp(dist) - 1;
    
    return vec4(0.02f, 0.04f, 0.01f, dd >= 1.0f ? 0 : min((dist  - 0.077f) * 25, 0.8f));
    //return vec4(IsSame?0.8f:0.0f, 0.04f, 0.01f, IsSame?0.8f:0.0f);
   /*
    return vec4( abs( (texture(lightDepth, lightCameraUv + vec2(shadowTexelSize.x*5, 0.0)).r * 2.0 - 1.0) -
            (texture(lightDepth, lightCameraUv - vec2(shadowTexelSize.x*5, 0.0)).r * 2.0 - 1.0))==0?0.25f:0.0f,
            0.0f, 0.0f, 1.0f);
            */
     
     
}
void main()
{
 //FragColor = MoreComplexShadows();
 FragColor = simpleShadowPass();
 //FragColor = vec4(1.0f, 1.0f, 1.0f, 1.0f);

    
}
