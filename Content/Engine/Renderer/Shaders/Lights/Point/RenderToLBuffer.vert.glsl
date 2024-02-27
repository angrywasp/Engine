/*
#attribute vs_Position Position 0
#attribute vs_TexCoord TextureCoordinate 0
*/

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

layout(location = 0) in vec4 vs_Position;
layout(location = 1) in vec2 vs_TexCoord;

layout(location = 0) out vec4 ps_Position;
layout(location = 1) out vec2 ps_TexCoord;
layout(location = 2) out vec4 ps_TexCoordScreenSpace;
layout(location = 3) out vec4 ps_LightPos;

void main() {
    mat4 worldView = (View * World);
    mat4 worldViewProjection = (Projection * worldView);

    gl_Position = worldViewProjection * vs_Position;
    gl_Position.y = -gl_Position.y;

    ps_TexCoord = vs_TexCoord;
    ps_TexCoordScreenSpace = gl_Position;
    ps_LightPos = worldView[3];
}