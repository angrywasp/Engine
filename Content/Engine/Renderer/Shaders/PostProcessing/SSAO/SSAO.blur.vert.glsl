/* Metadata
#attribute vs_Position Position 0
#attribute vs_TexCoord TextureCoordinate 0
*/

layout(location = 0) in vec4 vs_Position;
layout(location = 1) in vec2 vs_TexCoord;

layout(location = 0) out vec4 ps_Position;
layout(location = 1) out vec4 ps_TexCoord;
layout(location = 2) out vec2 ps_TexCoordHalfBuffer;

uniform vec2 GBufferPixelSize;
uniform vec2 TempBufferRes;

void main() {
    gl_Position = vs_Position;

    ps_Position = vs_Position;
    ps_TexCoord.xy = vs_TexCoord + GBufferPixelSize;
    ps_TexCoord.zw = vs_TexCoord + 0.5f / TempBufferRes;
    ps_TexCoordHalfBuffer = vec2(0);
}