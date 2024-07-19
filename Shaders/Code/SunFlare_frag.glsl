#version 330 core

out vec4 FragColor;

in vec2 uv;


uniform vec3 sunPosition;


void main()
{
    float sunIntensity= 1.0f;
    float flareStrength = 0.5f;
    float flareSize = 2.0f;
    // Calculate distance from world position to sun position
    vec3 distance = vec3(0,0,0) - sunPosition;
    float distSqr = dot(distance, distance);

    // Calculate flare intensity based on distance
    float flareIntensity = flareStrength * exp(-distSqr * flareSize);

    // Blend the screen texture with the flare
    FragColor = mix( vec4(0.0f, 0.0f, 0.0f, 0.0f), vec4(1.0f, 1.0f, 0.1f, 1.0f), flareIntensity);

    // Apply sun intensity to the final color
    FragColor.rgb *= sunIntensity;
}
