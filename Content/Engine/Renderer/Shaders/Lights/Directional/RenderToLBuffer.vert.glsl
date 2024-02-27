/*
#attribute vs_Position Position 0
#attribute vs_TexCoord TextureCoordinate 0
*/

layout(location = 0) in vec4 vs_Position;
layout(location = 1) in vec2 vs_TexCoord;

layout(location = 0) out vec2 ps_TexCoord;
layout(location = 1) out vec3 ps_FrustumRay;

uniform vec3 FrustumCorners[4];

vec3 GetFrustumRay(vec2 texCoord)
{
    float index = texCoord.x + (texCoord.y * 2.0);
    return FrustumCorners[int(index)];
}

void main()
{
    gl_Position = vs_Position;

    ps_TexCoord = vs_TexCoord;
    ps_FrustumRay = GetFrustumRay(vs_TexCoord);
}
