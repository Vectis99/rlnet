#version 330 core

// Position
layout(location = 0) in vec2 aPosition;

// Texture
layout(location = 1) in vec2 aTexCoord;

// Foreground color
layout(location = 2) in vec4 aForegroundColor;

// Background color
layout(location = 3) in vec4 aBackgroundColor;

out vec2 texCoord;
out vec4 foregroundColor;
out vec4 backgroundColor;

// Transformation: https://opentk.net/learn/chapter1/8-coordinate-systems.html
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
 
void
main()
{
    texCoord = aTexCoord;
    foregroundColor = aForegroundColor;
    backgroundColor = aBackgroundColor;
    gl_Position = projection * view * model * vec4(aPosition, 0.0, 1.0);
}