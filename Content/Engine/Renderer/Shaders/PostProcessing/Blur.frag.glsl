/* Metadata
#sampler RenderTarget Linear Wrap
*/

uniform float blurAmt;

uniform sampler2D RenderTarget;

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;

layout(location = 0) out vec4 result;

void main() {
    vec4 color = texture(RenderTarget, vec2(ps_TexCoord.x + blurAmt, ps_TexCoord.y + blurAmt));
    color += texture(RenderTarget, vec2(ps_TexCoord.x - blurAmt, ps_TexCoord.y - blurAmt));
    color += texture(RenderTarget, vec2(ps_TexCoord.x + blurAmt, ps_TexCoord.y - blurAmt));
    color += texture(RenderTarget, vec2(ps_TexCoord.x - blurAmt, ps_TexCoord.y + blurAmt));
    color /= 4.0;
    result = color;
}
