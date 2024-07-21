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
        // No intersection, set color to blue
        return vec4(0.0, 0.0, 1.0, 1.0);
    }
}
vec3 extractTranslation(mat4 matrix) {
    return matrix[3].xyz;
}
void main()
{
    float penumbraScale = 1.52f; // Controls the size of the soft shadow
    float minPenumbraSize = 1f; // Minimum size of the penumbra

    float dd = texture(_camDepth, uv).r;

    vec3 mainCameraPixelWorldSpace = WorldSpaceFromDepth(dd, uv, transpose(invMainCameraVP), 0.0f);
    vec3 camPos = extractTranslation(mainCameraView);
    //vec3 mainCameraPixelWorldSpace = new vec3(5,5,5);  
    vec2 lightCameraUv = worldToCameraUV(mainCameraPixelWorldSpace, transpose(lightCameraVP));

    vec2 shadowTexelSize = vec2(1 / 4096.0f, 1 / 4096.0f);

    float depthGradient =
        abs(
            texture(lightDepth, lightCameraUv.xy + vec2(shadowTexelSize.x, 0.0)).r -
            texture(lightDepth, lightCameraUv.xy - vec2(shadowTexelSize.x, 0.0)).r
        ) +
        abs(
            texture(lightDepth, lightCameraUv.xy + vec2(0.0, shadowTexelSize.y)).r -
            texture(lightDepth, lightCameraUv.xy - vec2(0.0, shadowTexelSize.y)).r
        );

    float penumbraSize = max(minPenumbraSize, depthGradient * penumbraScale);
    float shadowValue = 0.0;
    float penumbraSamples = 0.0;
    float MinDist = 53636;
    vec3 MinPosition = vec3(42525);
    int steps = 4;
    for (int i = -int(penumbraSize); i <= int(penumbraSize); i++) {
        for (int j = -int(penumbraSize); j <= int(penumbraSize); j++) {
            vec2 lUv = lightCameraUv + vec2(1 / 4096.0f * i, 1 / 4096.0f * j);
            float ldd = texture(lightDepth, lUv).r;
            vec3 lightCameraPixelWorldSpace = WorldSpaceFromDepth(ldd, lUv, transpose(invLightCameraVP), 0.0f);
            float dist = distance(mainCameraPixelWorldSpace, lightCameraPixelWorldSpace);
            //penumbraSamples += 1.0;
            //float camdist = distance(camPos, mainCameraPixelWorldSpace);
           // shadowValue+=dist;
            if (MinDist > dist) {
                MinDist = dist;
                MinPosition = lightCameraPixelWorldSpace;
                penumbraSamples += 1.0;
                //shadowValue += dist;
                //shadowValue /= 2;
            }
        }
    }
    shadowValue /= penumbraSamples;
    float dist = distance(mainCameraPixelWorldSpace, MinPosition);
    dist = exp(dist) - 1;
    //  dist = dist < 0.13f ? 0+ rand(vec2(MinDist, MinDist * 1.02f))* dist : dist;

     // dist = clamp(dist, 0.497f, 2f);
     // dist = (dist + shadowValue) /2;
    float ldd = texture(lightDepth, lightCameraUv).r;
    vec3 lightCameraPixelWorldSpace = WorldSpaceFromDepth(ldd, lightCameraUv, transpose(invLightCameraVP), 0.0f);
    //   
    float camdist = distance(camPos, mainCameraPixelWorldSpace);
    //// float dist = distance(mainCameraPixelWorldSpace,lightCameraPixelWorldSpace);     
    // float dist = fastDistance(mainCameraPixelWorldSpace,lightCameraPixelWorldSpace); 
     //dist -=0.25f;  
     //dist /= dd * 4;
     //dist = sqrt(dist-0.1f) ;
     // bool cond = dist < dd*4;
    bool cond = dist < 0.00002f;
    // bool cond = mainCameraPixelWorldSpace.r>1 && mainCameraPixelWorldSpace.g>1 && mainCameraPixelWorldSpace.b > 1
     //    && mainCameraPixelWorldSpace.r < 5 && mainCameraPixelWorldSpace.g < 5  && mainCameraPixelWorldSpace.b < 5;
    // FragColor = cond ? vec4(0.02f, 0.04f, 0.01f, 0.4f):vec4(0.4f,0.42f,0.41f,0.6f);
    //  FragColor = vec4(dist, 0.04f, 0.01f, dist<0.08f? 0:1f); 

    FragColor = vec4(0.02f, 0.04f, 0.01f, dd >= 1.0f ? 0 : min((dist  - 0.077f) * 25, 0.8f));


    //vec2 CameraUv = worldToCameraUV(mainCameraPixelWorldSpace, mainCameraVP);

  // FragColor = vec4(MinPosition, 1.0f);

 //  FragColor = vec4(lightCameraUv, 0.0f, 1.0f);
   //FragColor = vec4(CameraUv -lightCameraUv, 0.0f, 1.0f);
  // FragColor = vec4(mainCameraPixelWorldSpace, 1.0f);
  // FragColor = vec4(vec3(dist), 1.0f);

  // vec2 Test = worldToCameraUV(lightCameraPixelWorldSpace, transpose(lightCameraVP));
  // ldd = texture(lightDepth, Test).r;
   //if (ldd >= 0.99f) ldd = 1;
   //vec3 lightCameraPixelWorldSpace2 = WorldSpaceFromDepth(ldd, Test, transpose(invLightCameraVP), 0.0f);

  // FragColor = vec4(lightCameraPixelWorldSpace2, 1.0f);
  //FragColor = vec4(texture(lightDepth, Test).r!=1f ? 0.7f:0, 0.0f, 0.0f, 1.0f);
  // FragColor = DrawSphere();
   // FragColor = vec4(lightCameraPixelWorldSpace, 0.8f); 
}