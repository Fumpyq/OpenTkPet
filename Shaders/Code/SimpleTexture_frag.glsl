﻿#version 460 core
out vec4 FragColor;

in vec2 texCoord;
uniform sampler2D texture0;

 void main(){
FragColor = texture(texture0, texCoord);
//FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);  
}


