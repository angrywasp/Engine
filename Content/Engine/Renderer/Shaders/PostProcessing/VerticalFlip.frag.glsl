/* Metadata
#sampler RenderTarget Linear Wrap
*/

uniform sampler2D RenderTarget;

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;

layout(location = 0) out vec4 result;

void main() {
    result = texture(RenderTarget, vec2(ps_TexCoord.x, 1.0 - ps_TexCoord.y));
}
