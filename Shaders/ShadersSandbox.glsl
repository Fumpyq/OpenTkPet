#version 460 core
layout (location = 0) in
float3 aPosition;


void main()
{
    gl_Position = vec4(aPosition, 1.0);
}