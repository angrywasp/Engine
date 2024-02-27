/**/

layout(location = 0) in vec4 ps_Color;
layout(location = 1) in vec2 ps_TexCoord;

layout(location = 0) out vec4 result;

void main() {
    result = vec4(ps_Color.rgb * ps_Color.a, ps_Color.a);
}
