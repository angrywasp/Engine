/*
#attribute vs_Position Position 0
#attribute vs_TexCoord TextureCoordinate 0
*/

layout(location = 0) in vec4 vs_Position;
layout(location = 1) in vec2 vs_TexCoord;

layout(location = 0) out vec2 ps_TexCoord;
layout(location = 1) out vec2 ps_Depth;

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

void main() {
    mat4 worldView = (View * World);
    mat4 worldViewProjection = (Projection * worldView);

    vec4 clipPos = worldViewProjection * vs_Position;
    clipPos.z = max(clipPos.z, 0.0);

    gl_Position = clipPos;
    gl_Position.y = -gl_Position.y;

    ps_TexCoord = vs_TexCoord;
    ps_Depth = gl_Position.zw;
}