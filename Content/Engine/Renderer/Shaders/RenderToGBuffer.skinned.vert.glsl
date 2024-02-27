/*
#attribute vs_Position Position 0
#attribute vs_TexCoord TextureCoordinate 0
#attribute vs_Normal Normal 0
#attribute vs_Binormal Binormal 0
#attribute vs_Tangent Tangent 0
#attribute vs_Indices BlendIndices 0
#attribute vs_Weights BlendWeight 0
*/

layout(location = 0) in vec4 vs_Position;
layout(location = 1) in vec2 vs_TexCoord;
layout(location = 2) in vec3 vs_Normal;
layout(location = 3) in vec3 vs_Binormal;
layout(location = 4) in vec3 vs_Tangent;
layout(location = 5) in vec4 vs_Indices;
layout(location = 6) in vec4 vs_Weights;

layout(location = 0) out vec4 ps_Position;
layout(location = 1) out vec2 ps_TexCoord;
layout(location = 2) out float ps_Depth;
layout(location = 3) out mat3 ps_WorldNormal;

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;
uniform mat4 Bones[72];

void main() {
    vec4 localPos = vec4(0);
    vec4 localNorm = vec4(0);
    vec4 localTan = vec4(0);
    vec4 localBinorm = vec4(0);

    for (int i = 0; i < 4; i++)
    {
        mat4 boneTransform = Bones[int(vs_Indices[i])];

        vec4 posePosition = boneTransform * vec4(vs_Position.xyz, 1.0);
        vec4 poseNormal = boneTransform * vec4(vs_Normal.xyz, 1.0);
        vec4 poseTangent = boneTransform * vec4(vs_Tangent.xyz, 1.0);
        vec4 poseBinormal = boneTransform * vec4(vs_Binormal.xyz, 1.0);

        localPos += posePosition * vs_Weights[i];
        localNorm += poseNormal * vs_Weights[i];
        localTan += poseTangent * vs_Weights[i];
        localBinorm += poseBinormal * vs_Weights[i];
    }

    mat4 worldView = (View * World);
    mat4 worldViewProjection = (Projection * worldView);
    vec3 viewSpacePosition = vec3((worldView * localPos));

    gl_Position = worldViewProjection * localPos;
    gl_Position.y = -gl_Position.y;

    ps_Position = World * localPos;
    ps_TexCoord = vs_TexCoord;
    ps_Depth = viewSpacePosition.z;

    ps_WorldNormal = mat3(
        vec3(normalize((World * vec4(localTan.xyz, 0.0)))),
        vec3(normalize((World * vec4(localBinorm.xyz, 0.0)))),
        vec3(normalize((World * vec4(localNorm.xyz, 0.0))))
    );
}
