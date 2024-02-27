/* Metadata
#attribute vs_Position Position 0
#attribute vs_TexCoord TextureCoordinate 0
*/

layout(location = 0) in vec4 vs_Position;
layout(location = 1) in vec2 vs_TexCoord;

layout(location = 0) out vec4 ps_Position;
layout(location = 1) out vec4 ps_TexCoord;
layout(location = 2) out vec2 ps_TexCoordHalfBuffer;

uniform vec2 SSAORes;

void main() {
    gl_Position = vs_Position;

    ps_Position = vs_Position;
    ps_TexCoord.xy = vs_TexCoord + 0.5f / SSAORes;
    ps_TexCoord.zw = vec2(0);
    ps_TexCoordHalfBuffer = vec2(0);
}