/* Metadata
#include defines Defines.glsl
#include smaa SMAA.h
#sampler ColorTex Linear Clamp
#sampler BlendTex Linear Clamp
*/

#define SMAA_INCLUDE_VS 0

#include defines
#include smaa

uniform sampler2D ColorTex;
uniform sampler2D BlendTex;

layout(location = 0) in vec2 ps_TexCoord;
layout(location = 1) in vec4 ps_Offset;

layout(location = 0) out vec4 result;

void main()
{
    result = SMAANeighborhoodBlendingPS(ps_TexCoord, ps_Offset, ColorTex, BlendTex);
}