/* Metadata
#sampler RenderTarget Linear Wrap
*/

uniform float blurAmt;

uniform sampler2D RenderTarget;
uniform sampler2D DepthBuffer;

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;

layout(location = 0) out vec4 result;

void main() {
    vec4 color = texture(RenderTarget, vec2(ps_TexCoord.x, ps_TexCoord.y));
    float depth = texture(DepthBuffer, ps_TexCoord).r;

    color += texture(RenderTarget, vec2(ps_TexCoord.x + depth, ps_TexCoord.y + depth));
    color += texture(RenderTarget, vec2(ps_TexCoord.x - depth, ps_TexCoord.y - depth));
    color += texture(RenderTarget, vec2(ps_TexCoord.x + depth, ps_TexCoord.y - depth));
    color += texture(RenderTarget, vec2(ps_TexCoord.x - depth, ps_TexCoord.y + depth));
    color /= 5.0;
    result = color;
}
