/* Metadata
#attribute vs_Position Position 0
#attribute vs_TexCoord TextureCoordinate 0
*/

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;
uniform vec3 CameraPosition;
uniform vec2 WaveMapOffset0;
uniform vec2 WaveMapOffset1;
uniform float TexScale;

layout(location = 0) in vec4 vs_Position;
layout(location = 1) in vec2 vs_TexCoord;

layout(location = 0) out vec3 ps_PosW;
layout(location = 1) out vec3 ps_ToEyeW;
layout(location = 2) out vec2 ps_Tex0;
layout(location = 3) out vec2 ps_Tex1;
layout(location = 4) out vec4 ps_ProjTexC;
layout(location = 5) out vec2 ps_TexCoord;

void main() {
    mat4 worldView = (View * World);
    mat4 worldViewProjection = (Projection * worldView);
    vec3 posW = (World * vs_Position).xyz;

    gl_Position = worldViewProjection * vs_Position;
    gl_Position.y = -gl_Position.y;

    ps_ToEyeW = posW - CameraPosition;
    ps_ToEyeW.z = -ps_ToEyeW.z; //hack: align the specular reflection to the directional light direction
    ps_Tex0 = (vs_TexCoord * TexScale) + WaveMapOffset0;
    ps_Tex1 = (vs_TexCoord * TexScale) + WaveMapOffset1;
    ps_ProjTexC = worldViewProjection * vs_Position;
    ps_PosW = posW;
    ps_TexCoord = vs_TexCoord;
}