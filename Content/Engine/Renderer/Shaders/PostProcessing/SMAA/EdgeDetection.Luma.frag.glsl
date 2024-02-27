/* Metadata
#include defines Defines.glsl
#include smaa SMAA.h
#sampler ColorTex Linear Clamp
*/

#define SMAA_INCLUDE_VS 0

#include defines
#include smaa

uniform sampler2D ColorTex;

layout(location = 0) in vec2 ps_TexCoord;
layout(location = 1) in vec4[3] ps_Offsets;

layout(location = 0) out vec4 result;

void main()
{
    vec2 ps = SMAALumaEdgeDetectionPS(ps_TexCoord, ps_Offsets, ColorTex);
    result = vec4(ps, 0, 1);
}