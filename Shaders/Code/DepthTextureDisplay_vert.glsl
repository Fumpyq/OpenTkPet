﻿#version 460 core
layout (location = 0) in
vec3 aPos;

layout(location = 1) in vec2 uvin;
out vec2 uv;

void main(){
uv = uvin;  
gl_Position = vec4(aPos, 1.0); 
}   