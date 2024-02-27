/* Metadata
#attribute vs_Position Position 0
*/

layout(location = 0) in vec4 vs_Position;

layout(location = 0) out vec2 ps_TexCoord;

uniform vec2 GBufferPixelSize;

void main() {
    gl_Position = vs_Position;

    ps_TexCoord = vs_Position.xy * 0.5 + vec2(0.5, 0.5);
    ps_TexCoord += GBufferPixelSize;
}
