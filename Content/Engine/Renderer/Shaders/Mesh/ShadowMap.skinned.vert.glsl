/*
#attribute vs_Position Position 0
#attribute vs_TexCoord TextureCoordinate 0
#attribute vs_Indices BlendIndices 0
#attribute vs_Weights BlendWeight 0
*/

layout(location = 0) in vec4 vs_Position;
layout(location = 1) in vec2 vs_TexCoord;
layout(location = 2) in vec4 vs_Indices;
layout(location = 3) in vec4 vs_Weights;

layout(location = 0) out vec2 ps_TexCoord;
layout(location = 1) out vec2 ps_Depth;

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;
uniform mat4 Bones[72];

void main() {
    vec4 localPos = vec4(0);
    for (int i = 0; i < 4; i++)
    {
        mat4 boneTransform = Bones[int(vs_Indices[i])];
        vec4 posePosition = boneTransform * vec4(vs_Position.xyz, 1.0);
        localPos += posePosition * vs_Weights[i];
    }

    mat4 worldView = (View * World);
    mat4 worldViewProjection = (Projection * worldView);

    vec4 clipPos = worldViewProjection * localPos;
    clipPos.z = max(clipPos.z, 0.0);

    gl_Position = clipPos;
    gl_Position.y = -gl_Position.y;

    ps_TexCoord = vs_TexCoord;
    ps_Depth = gl_Position.zw;
}