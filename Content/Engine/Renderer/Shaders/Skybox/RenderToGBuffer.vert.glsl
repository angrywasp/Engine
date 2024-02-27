/*
#attribute vs_Position Position 0
*/

layout(location = 0) in vec4 vs_Position;

layout(location = 0) out vec4 ps_Position;
layout(location = 1) out vec3 ps_TexCoord;

uniform mat4 World;
uniform mat4 WorldViewProjection;
uniform vec3 CameraPosition;

void main() {
    vec4 hPos = vec4(vs_Position.xyz, 0.0);

    gl_Position = (WorldViewProjection * hPos).xyww;
    gl_Position.y = -gl_Position.y;

    vec4 VertexPosition = World * vs_Position;
    ps_TexCoord = VertexPosition.rgb - CameraPosition;
    ps_Position = VertexPosition;
}
