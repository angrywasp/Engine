/*
#sampler DiffuseMap Point Clamp
*/

layout(location = 0) in vec4 ps_Color;
layout(location = 1) in vec2 ps_TexCoord;

layout(location = 0) out vec4 result;

uniform sampler2D DiffuseMap;

void main() {
    vec4 diffuseMap = texture(DiffuseMap, ps_TexCoord);
    vec4 vertColor = vec4(ps_Color.rgb * ps_Color.a, ps_Color.a);
    result = (diffuseMap * diffuseMap.a) * vertColor;
}
