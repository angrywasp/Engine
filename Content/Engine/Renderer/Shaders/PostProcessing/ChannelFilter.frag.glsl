/* Metadata
#sampler RenderTarget Linear Wrap
*/

uniform sampler2D RenderTarget;

uniform bool ShowRed;
uniform bool ShowGreen;
uniform bool ShowBlue;
uniform bool ShowAlpha;

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;

layout(location = 0) out vec4 result;

void main() {
    vec4 color = texture(RenderTarget, ps_TexCoord);

    result = vec4(0);

    if(ShowRed)
        result.r = color.r;

    if(ShowGreen)
        result.g = color.g;

    if(ShowBlue)
        result.b = color.b;

    if(ShowAlpha)
        result.a = color.a;
}
