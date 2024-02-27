/*
#attribute vs_Position Position 0
#attribute vs_Color Color 0
*/

layout(location = 0) in vec4 vs_Position;
layout(location = 1) in vec4 vs_Color;

layout(location = 0) out vec4 ps_Color;
layout(location = 1) out vec4 ps_ClipDistances;

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;
uniform vec4 ClipPlane;

void main() {
    mat4 worldView = (View * World);
    mat4 worldViewProjection = (Projection * worldView);

    gl_Position = worldViewProjection * vs_Position;
    gl_Position.y = -gl_Position.y;

    ps_Color = vs_Color;
    ps_ClipDistances = vec4(0);
    ps_ClipDistances.x = dot(gl_Position, ClipPlane);
}
