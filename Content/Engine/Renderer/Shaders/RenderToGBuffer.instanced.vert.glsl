/*
#attribute vs_Position Position 0
#attribute vs_TexCoord TextureCoordinate 0
#attribute vs_Normal Normal 0
#attribute vs_Binormal Binormal 0
#attribute vs_Tangent Tangent 0
#attribute vs_InstanceTransform0 Position 1
#attribute vs_InstanceTransform1 Position 2
#attribute vs_InstanceTransform2 Position 3
#attribute vs_InstanceTransform3 Position 4
*/

layout(location = 0) in vec4 vs_Position;
layout(location = 1) in vec2 vs_TexCoord;
layout(location = 2) in vec3 vs_Normal;
layout(location = 3) in vec3 vs_Binormal;
layout(location = 4) in vec3 vs_Tangent;
layout(location = 5) in vec4 vs_InstanceTransform0;
layout(location = 6) in vec4 vs_InstanceTransform1;
layout(location = 7) in vec4 vs_InstanceTransform2;
layout(location = 8) in vec4 vs_InstanceTransform3;

layout(location = 0) out vec4 ps_Position;
layout(location = 1) out vec2 ps_TexCoord;
layout(location = 2) out float ps_Depth;
layout(location = 3) out mat3 ps_WorldNormal;

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
    vec3 viewSpacePosition = vec3((worldView * vs_Position));

    gl_Position = worldViewProjection * vs_Position;
    gl_Position.y = -gl_Position.y;

    ps_Position = it * vs_Position;
    ps_TexCoord = vs_TexCoord;
    ps_Depth = viewSpacePosition.z;

    ps_WorldNormal = mat3(
        vec3(normalize((it * vec4(vs_Tangent, 0.0)))),
        vec3(normalize((it * vec4(vs_Binormal, 0.0)))),
        vec3(normalize((it * vec4(vs_Normal, 0.0))))
    );
}
