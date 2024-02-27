/* Metadata
#include defines Defines.glsl
#include smaa SMAA.h
#attribute vs_Position Position 0
#attribute vs_TexCoord TextureCoordinate 0
*/

#define SMAA_INCLUDE_PS 0

#include defines
#include smaa

layout(location = 0) in vec4 vs_Position;
layout(location = 1) in vec2 vs_TexCoord;

layout(location = 0) out vec2 ps_TexCoord;
layout(location = 1) out vec4[3] ps_Offsets;

void main()
{
    gl_Position = vs_Position;
    ps_TexCoord = vs_TexCoord;
    SMAAEdgeDetectionVS(vs_TexCoord, ps_Offsets);
}