/* Metadata
#include defines Defines.glsl
#include smaa SMAA.h
#sampler EdgesTex Linear Clamp
#sampler AreaTex Linear Clamp
#sampler SearchTex Point Clamp
*/

#define SMAA_INCLUDE_VS 0

#include defines
#include smaa

uniform sampler2D EdgesTex;
uniform sampler2D AreaTex;
uniform sampler2D SearchTex;

layout(location = 0) in vec2 ps_TexCoord;
layout(location = 1) in vec2 ps_PixCoord;
layout(location = 2) in vec4[3] ps_Offsets;

layout(location = 0) out vec4 result;

void main()
{
    result = SMAABlendingWeightCalculationPS(ps_TexCoord, ps_PixCoord, ps_Offsets, EdgesTex, AreaTex, SearchTex, ivec4(0));
}