/*
#include clipping Clipping.glsl
#include helpers Helpers.glsl
#sampler AlbedoMap Linear Clamp
*/

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec3 ps_TexCoord;

layout(location = 0) out vec4 result;

#include clipping
#include helpers

uniform samplerCube AlbedoMap;

void main() {
    if(Clip(ps_Position.y))
        discard;

    vec4 albedo = texture(AlbedoMap, ps_TexCoord);

    vec3 color = albedo.rgb;
    result = vec4(color, Rgb2Luma(color));
}