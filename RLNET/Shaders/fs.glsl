#version 330
 
out vec4 outputColor;

in vec2 texCoord;

in vec4 foregroundColor;

in vec4 backgroundColor;

uniform sampler2D texture0;
 
void main()
{
    vec4 raw = texture(texture0, texCoord);
    // We can do a "conditional" based on the alpha component, w.
    // If alpha = 0, use background
    // If alpha = 1, use foreground
    // Like so;
    // (1 - raw.w) * background
    // raw.w * foreground
    outputColor = vec4(
        (1 - raw.w) * backgroundColor.x + raw.w * foregroundColor.x,
        (1 - raw.w) * backgroundColor.y + raw.w * foregroundColor.y,
        (1 - raw.w) * backgroundColor.z + raw.w * foregroundColor.z,
        (1 - raw.w) * backgroundColor.w + raw.w * foregroundColor.w
    );
}