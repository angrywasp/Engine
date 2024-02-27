/* Metadata
#include defines Defines.glsl
#include smaa SMAA.h
#attribute vs_Position Position 0
#attribute vs_TexCoord TextureCoordinate 0
*/

#define SMAA_INCLUDE_PS 0

#include defines
#include smaa

layout(location = 0) in vec2 vs_Position;
layout(location = 1) in vec2 vs_TexCoord;

layout(location = 0) out vec2 ps_TexCoord;
layout(location = 1) out vec4 ps_Offset;

void main()
{
    SMAANeighborhoodBlendingVS(vs_TexCoord, ps_Offset);
    gl_Position = vec4(vs_Position, 0.0, 1.0);
    ps_TexCoord = vs_TexCoord;
}