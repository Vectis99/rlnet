#version 330 core

// Position
layout(location = 0) in vec2 aPosition;

// Texture
layout(location = 1) in vec2 aTexCoord;
out vec2 texCoord;

// Transformation: https://opentk.net/learn/chapter1/8-coordinate-systems.html
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
 
void
main()
{
    //gl_Position = vec4(aPosition, 0.0, 1.0);
    gl_Position = projection * view * model * vec4(aPosition, 0.0, 1.0);
}