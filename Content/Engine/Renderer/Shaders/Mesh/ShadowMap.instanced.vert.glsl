/*
#attribute vs_Position Position 0
#attribute vs_TexCoord TextureCoordinate 0
#attribute vs_InstanceTransform0 Position 1
#attribute vs_InstanceTransform1 Position 2
#attribute vs_InstanceTransform2 Position 3
#attribute vs_InstanceTransform3 Position 4
*/

layout(location = 0) in vec4 vs_Position;
layout(location = 1) in vec2 vs_TexCoord;
layout(location = 2) in vec4 vs_InstanceTransform0;
layout(location = 3) in vec4 vs_InstanceTransform1;
layout(location = 4) in vec4 vs_InstanceTransform2;
layout(location = 5) in vec4 vs_InstanceTransform3;

layout(location = 0) out vec2 ps_TexCoord;
layout(location = 1) out vec2 ps_Depth;

uniform mat4 View;
uniform mat4 Projection;

void main() {
    mat4 it = mat4(0.0f);
    it[0] = vs_InstanceTransform0;
    it[1] = vs_InstanceTransform1;
    it[2] = vs_InstanceTransform2;
    it[3] = vs_InstanceTransform3;

    mat4 worldView = (View * it);
    mat4 worldViewProjection = (Projection * worldView);

    vec4 clipPos = worldViewProjection * vs_Position;
    clipPos.z = max(clipPos.z, 0.0);

    gl_Position = clipPos;
    gl_Position.y = -gl_Position.y;

    ps_TexCoord = vs_TexCoord;
    ps_Depth = gl_Position.zw;
}