/* Metadata
#sampler RenderTarget Linear Wrap
*/

uniform sampler2D RenderTarget;

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;

layout(location = 0) out vec4 result;

void main() {
    vec4 color = texture(RenderTarget, ps_TexCoord);
    result.r = (color.r * 0.393) + (color.g * 0.769) + (color.b * 0.189);
    result.g = (color.r * 0.349) + (color.g * 0.686) + (color.b * 0.168);
    result.b = (color.r * 0.272) + (color.g * 0.534) + (color.b * 0.131);
    result.a = 1.0;
}
