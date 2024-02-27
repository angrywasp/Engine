/* Metadata
#sampler RenderTarget Linear Wrap
*/

uniform sampler2D RenderTarget;

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;

layout(location = 0) out vec4 result;

void main() {
    vec4 color = 1.0 - texture(RenderTarget, ps_TexCoord);
    color.a = 1.0;
    result = color;
}
