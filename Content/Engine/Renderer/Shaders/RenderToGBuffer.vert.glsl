/*
#attribute vs_Position Position 0
#attribute vs_TexCoord TextureCoordinate 0
#attribute vs_Normal Normal 0
#attribute vs_Binormal Binormal 0
#attribute vs_Tangent Tangent 0
*/

layout(location = 0) in vec4 vs_Position;
layout(location = 1) in vec2 vs_TexCoord;
layout(location = 2) in vec3 vs_Normal;
layout(location = 3) in vec3 vs_Binormal;
layout(location = 4) in vec3 vs_Tangent;

layout(location = 0) out vec4 ps_Position;
layout(location = 1) out vec2 ps_TexCoord;
layout(location = 2) out float ps_Depth;
layout(location = 3) out mat3 ps_WorldNormal;

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

void main() {
    mat4 worldView = (View * World);
    mat4 worldViewProjection = (Projection * worldView);
    vec3 viewSpacePosition = vec3(worldView * vs_Position);

    gl_Position = worldViewProjection * vs_Position;
    gl_Position.y = -gl_Position.y;

    ps_Position = World * vs_Position;
    ps_TexCoord = vs_TexCoord;
    ps_Depth = viewSpacePosition.z;

    ps_WorldNormal = mat3(
        vec3(normalize((World * vec4(vs_Tangent, 0.0)))),
        vec3(normalize((World * vec4(vs_Binormal, 0.0)))),
        vec3(normalize((World * vec4(vs_Normal, 0.0))))
    );
}
