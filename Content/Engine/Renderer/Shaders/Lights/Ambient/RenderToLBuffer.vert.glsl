/*
#attribute vs_Position Position 0
#attribute vs_TexCoord TextureCoordinate 0
*/

layout(location = 0) in vec4 vs_Position;
layout(location = 1) in vec2 vs_TexCoord;

layout(location = 0) out vec2 ps_TexCoord;

void main()
{
    gl_Position = vs_Position;
    ps_TexCoord = vs_TexCoord;
}
