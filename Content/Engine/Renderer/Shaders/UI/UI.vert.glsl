/*
#attribute vs_Position Position 0
#attribute vs_Color Color 0
#attribute vs_TexCoord TextureCoordinate 0
*/

layout(location = 0) in vec4 vs_Position;
layout(location = 1) in vec4 vs_Color;
layout(location = 2) in vec2 vs_TexCoord;

layout(location = 0) out vec4 ps_Color;
layout(location = 1) out vec2 ps_TexCoord;

uniform mat4 ViewProjection;

void main() {
    gl_Position = ViewProjection * vs_Position;
    ps_Color = vs_Color;
    ps_TexCoord = vs_TexCoord;
}
