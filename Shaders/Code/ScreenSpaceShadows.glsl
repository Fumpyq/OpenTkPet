#version 460
#pragma STDGL precision highp float;

uniform mat4 mainCameraView;
uniform mat4 mainCameraVP;
uniform mat4 invMainCameraVP;
uniform mat4 lightCameraVP;
uniform vec3 lightWorldPos;
uniform mat4 invLightCameraVP;
uniform sampler2D _camDepth;
uniform sampler2D lightDepth;
in vec2 uv;
out vec4 FragColor;

// Color constants (matches original)
const vec3 shadowColor = vec3(0.02, 0.04, 0.01);  // Dark shadow areas
const vec3 litColor = vec3(0.97, 0.92, 0.89);     // Bright lit areas
const float shadowThreshold = 0.07;                // Original threshold

// Sun sphere parameters
const float sunAngularSize = 0.02;                  // 2% of screen
const vec3 sunColor = vec3(1.0, 0.9, 0.75);
const float sunGlowFalloff = 4.0;
vec3 WorldSpaceFromDepth(float depth, vec2 uv) {
    vec4 ndc = vec4(uv * 2.0 - 1.0, depth * 2.0 - 1.0, 1.0);
    vec4 worldPos = invMainCameraVP * ndc;
    return worldPos.xyz / worldPos.w;
}

// World position reconstruction
vec3 WorldSpaceFromDepth(float depth, vec2 screenUV, mat4 invViewProj) {
    vec4 ndc = vec4(screenUV * 2.0 - 1.0, depth * 2.0 - 1.0, 1.0);
    return (invViewProj * ndc).xyz / (invViewProj * ndc).w;
}

vec2 worldToCameraUV(vec3 worldPos, mat4 viewProj) {
    vec4 clipPos = viewProj * vec4(worldPos, 1.0);
    return (clipPos.xy / clipPos.w) * 0.5 + 0.5;
}

// Poisson samples for soft shadows
const vec2 PoissonSamples[16] = vec2[](
    vec2(-0.942, -0.399), vec2(0.945, -0.382), vec2(-0.094, -0.759),
    vec2(0.344, -0.792), vec2(-0.915, 0.054), vec2(0.833, 0.054),
    vec2(-0.275, 0.224), vec2(0.380, 0.392), vec2(-0.418, -0.599),
    vec2(0.448, -0.596), vec2(-0.753, 0.672), vec2(0.744, 0.660),
    vec2(-0.313, 0.945), vec2(0.304, 0.949), vec2(-0.678, -0.008),
    vec2(0.659, 0.007)
    );

vec4 CalculateShadows() {
    float depth = texture(_camDepth, uv).r;
    //if (depth >= 1.0) discard;

    // Reconstruct world positions
    vec3 worldPos = WorldSpaceFromDepth(depth, uv, invMainCameraVP);
    vec2 lightUV = worldToCameraUV(worldPos, lightCameraVP);

    // Early exit for outside light frustum
    //if (any(lessThan(lightUV, vec2(0.0))) || any(greaterThan(lightUV, vec2(1.0))))
    //    return vec4(litColor, 0.04); // Original lit alpha

    // Shadow calculation
    vec2 texelSize = 1.0 / textureSize(lightDepth, 0);
    float visibility = 0.0;
    float penumbra = 2.0;

    for (int i = 0; i < 8; i++) {
        vec2 sampleUV = lightUV + PoissonSamples[i] * texelSize * penumbra;
        vec3 lightPos = WorldSpaceFromDepth(texture(lightDepth, sampleUV).r, sampleUV, invLightCameraVP);
        float dist = distance(worldPos, lightPos);

        // Corrected visibility calculation (1 when NOT in shadow)
        visibility += step(shadowThreshold, dist); // Reverse comparison
    }
    visibility /= 8.0;

    // Final color and alpha (matches original behavior)
    vec3 color = mix(litColor,shadowColor, visibility);
    float alpha = mix(0.04,0.7, visibility); // Higher alpha in shadows

    return vec4(color, alpha);
}
vec3 AddWorldSpaceSun(vec3 sceneColor, vec2 uv) {
    // Project sun to screen space
    vec4 sunClipPos = mainCameraVP * vec4(lightWorldPos, 1.0);
    vec3 sunNDC = sunClipPos.xyz / sunClipPos.w;

    // Convert to screen coordinates
    vec2 sunUV = sunNDC.xy * 0.5 + 0.5;

    // Calculate distance and depth
    float distToSun = length(uv - sunUV);
    float sunDepth = sunNDC.z * 0.5 + 0.5;

    // Get scene depth at sun position
    float sceneDepth = texture(_camDepth, sunUV).r;
    vec3 scenePos = WorldSpaceFromDepth(sceneDepth, sunUV);

    // Only show sun if nothing is in front
    if (sunDepth < sceneDepth) {
        // Sun disk with smooth edges
        float sunMask = smoothstep(sunAngularSize, sunAngularSize * 0.8, distToSun);
        float glow = pow(1.0 - smoothstep(0.0, sunAngularSize * 2.0, distToSun), sunGlowFalloff);

        // Combine effects
        vec3 sunEffect = mix(sunColor, sceneColor, sunMask);
        sunEffect += sunColor * glow * 0.5;
        return mix(sceneColor, sunEffect, glow);
    }
    return sceneColor;
}


void main() {
    vec4 shadowResult = CalculateShadows();

    // Add sun effect with proper blending
    shadowResult.rgb = AddWorldSpaceSun(shadowResult.rgb, uv);

    //// Preserve subtle background glow
    //vec2 centerVec = uv - vec2(0.5);
    //float baseGlow = pow(1.0 - dot(centerVec, centerVec) * 4.0, 4.0);
    //shadowResult.rgb += litColor * baseGlow * 0.05;  // Reduced intensity

    FragColor = clamp(shadowResult, 0.0, 1.0);
}