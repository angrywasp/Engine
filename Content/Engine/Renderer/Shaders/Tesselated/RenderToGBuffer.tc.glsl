/*
#attribute tc_Position Position 0
#attribute tc_TexCoord TextureCoordinate 0
#attribute tc_Normal Normal 0
#attribute tc_Binormal Binormal 0
#attribute tc_Tangent Tangent 0
*/

layout(vertices = 3) out;

layout(location = 0) in vec4 tc_Position[];
layout(location = 1) in vec2 tc_TexCoord[];
layout(location = 2) in vec3 tc_Normal[];
layout(location = 3) in vec3 tc_Binormal[];
layout(location = 4) in vec3 tc_Tangent[];

layout(location = 0) out vec4 te_Position[];
layout(location = 1) out vec2 te_TexCoord[];
layout(location = 2) out vec3 te_Normal[];
layout(location = 3) out vec3 te_Binormal[];
layout(location = 4) out vec3 te_Tangent[];

const float TessellationLevel = 4;

void main() {
    te_Position[gl_InvocationID] = tc_Position[gl_InvocationID];
    te_TexCoord[gl_InvocationID] = tc_TexCoord[gl_InvocationID];
    te_Normal[gl_InvocationID] = tc_Normal[gl_InvocationID];
    te_Binormal[gl_InvocationID] = tc_Binormal[gl_InvocationID];
    te_Tangent[gl_InvocationID] = tc_Tangent[gl_InvocationID];

    gl_TessLevelOuter[0] = TessellationLevel;
    gl_TessLevelOuter[1] = TessellationLevel;
    gl_TessLevelOuter[2] = TessellationLevel;

    gl_TessLevelInner[0] = TessellationLevel;
}
