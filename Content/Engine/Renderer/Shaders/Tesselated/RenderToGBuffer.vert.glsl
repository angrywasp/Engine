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

layout(location = 0) out vec4 tc_Position;
layout(location = 1) out vec2 tc_TexCoord;
layout(location = 2) out vec3 tc_Normal;
layout(location = 3) out vec3 tc_Binormal;
layout(location = 4) out vec3 tc_Tangent;

void main() {
    tc_Position = vs_Position;
    tc_TexCoord = vs_TexCoord;
    tc_Normal = vs_Normal;
    tc_Binormal = vs_Binormal;
    tc_Tangent = vs_Tangent;
}
