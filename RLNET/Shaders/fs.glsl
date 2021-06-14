#version 330
 
out vec4 outputColor;

in vec2 texCoord;

in vec3 foregroundColor;

in vec3 backgroundColor;

uniform sampler2D texture0;
 
void main()
{
    vec4 raw = texture(texture0, texCoord);
    outputColor = vec4(
        (1 - raw.w) * backgroundColor.x + raw.w * foregroundColor.x,
        (1 - raw.w) * backgroundColor.y + raw.w * foregroundColor.y,
        (1 - raw.w) * backgroundColor.z + raw.w * foregroundColor.z,
        1.0
    );
}