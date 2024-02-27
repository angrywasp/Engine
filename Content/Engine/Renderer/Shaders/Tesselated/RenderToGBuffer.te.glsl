/*
#attribute te_Position Position 0
#attribute te_TexCoord TextureCoordinate 0
#attribute te_Normal Normal 0
#attribute te_Binormal Binormal 0
#attribute te_Tangent Tangent 0
*/

layout(triangles, equal_spacing) in;

layout(location = 0) in vec4 te_Position[];
layout(location = 1) in vec2 te_TexCoord[];
layout(location = 2) in vec3 te_Normal[];
layout(location = 3) in vec3 te_Binormal[];
layout(location = 4) in vec3 te_Tangent[];

layout(location = 0) out vec4 ps_Position;
layout(location = 1) out vec2 ps_TexCoord;
layout(location = 2) out float ps_Depth;
layout(location = 3) out vec3 ps_Normal;
layout(location = 4) out vec3 ps_Tangent;
layout(location = 5) out vec3 ps_Binormal;

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

uniform sampler2D PbrMap;
const float DisplacementPower = 0.1;

vec2 lerp2D(vec2 v0, vec2 v1, vec2 v2)
{
    return vec2(gl_TessCoord.x) * v0 + vec2(gl_TessCoord.y) * v1 + vec2(gl_TessCoord.z) * v2;
}

vec3 lerp3D(vec3 v0, vec3 v1, vec3 v2)
{
    return vec3(gl_TessCoord.x) * v0 + vec3(gl_TessCoord.y) * v1 + vec3(gl_TessCoord.z) * v2;
}

void main() {
    vec2 tc = lerp2D(te_TexCoord[0], te_TexCoord[1], te_TexCoord[2]);
    float height = texture(PbrMap, tc).a * DisplacementPower;

    vec3 p1 = te_Position[0].xyz;
    vec3 p2 = te_Position[1].xyz;
    vec3 p3 = te_Position[2].xyz;

    vec3 n1 = te_Normal[0];
    vec3 n2 = te_Normal[1];
    vec3 n3 = te_Normal[2];

    p1 += n1 * height;
    p2 += n2 * height;
    p3 += n3 * height;

    vec4 pos = vec4(lerp3D(p1, p2, p3), 1.0);
    
    vec3 norm = normalize(lerp3D(n1, n2, n3));
    vec3 binorm = normalize(lerp3D(te_Binormal[0], te_Binormal[1], te_Binormal[2]));
    vec3 tang = normalize(lerp3D(te_Tangent[0], te_Tangent[1], te_Tangent[2]));

    mat4 worldView = (View * World);
    mat4 worldViewProjection = (Projection * worldView);
    vec3 viewSpacePosition = vec3((worldView * pos));

    gl_Position = worldViewProjection * pos;
    gl_Position.y = -gl_Position.y;

    ps_Position = World * pos;
    ps_TexCoord = tc;
    ps_Depth = viewSpacePosition.z;
    ps_Normal = vec3(normalize((worldView * vec4(norm, 0.0))));
    ps_Tangent = vec3(normalize((worldView * vec4(tang, 0.0))));
    ps_Binormal = vec3(normalize((worldView * vec4(binorm, 0.0))));
}
