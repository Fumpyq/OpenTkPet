#version 460
uniform mat4 viewProjection1; // View-Projection matrix for screen 1
uniform mat4 viewProjection2; // View-Projection matrix for screen 2

in vec2 screenPos1; // Screen-space position on screen 1 (normalized [0, 1])



void main()
{
    // Convert screen-space coordinates to clip-space
    vec4 clipPos1 = vec4(screenPos1 * 2.0 - 1.0, 0.0, 1.0);

    // Transform from clip-space to world-space
    vec4 worldPos1 = inverse(viewProjection1) * clipPos1;

    // Homogeneous divide (perspective division)
    worldPos1.xyz /= worldPos1.w;

    // Output the world-space position
    worldPos = worldPos1;

    // Convert world-space point to screen-space coordinates for screen 2
    vec4 screenPos2 = viewProjection2 * worldPos;

    // Homogeneous divide (perspective division)
    screenPos2.xyz /= screenPos2.w;

    // Convert from clip-space to screen-space
    screenPos2.xy = screenPos2.xy * 0.5 + 0.5;
    screenPos2.z = screenPos2.z * 0.5 + 0.5;

    // Output the screen-space position on screen 2
    outPos = screenPos2;
}