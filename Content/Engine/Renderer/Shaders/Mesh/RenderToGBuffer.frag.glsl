/* Metadata
#include clipping Clipping.glsl
#include gbuffer GBuffer.glsl
#include helpers Helpers.glsl
#include encoding NormalEncoding.glsl
#sampler AlbedoMap Linear Wrap
#sampler NormalMap Linear Wrap
#sampler PbrMap Linear Wrap
*/

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;
layout(location = 2) in float ps_Depth;
layout(location = 3) in mat3 ps_WorldNormal;

#include clipping
#include gbuffer
#include encoding
#include helpers

uniform vec2 TexScale;
uniform float FarClip;

uniform sampler2D AlbedoMap;
uniform sampler2D NormalMap;
uniform sampler2D PbrMap;

void main() {
    if(Clip(ps_Position.y))
        discard;

    vec4 albedo = texture(AlbedoMap, ps_TexCoord * TexScale);
    
    if(albedo.a < 0.5)
        discard;

    rt_GAlbedo.rgb = albedo.rgb;
    rt_GAlbedo.a = 1.0;

    vec3 normal = texture(NormalMap, ps_TexCoord * TexScale).rgb;
    normal = normal * 2.0 - 1.0;

    vec3 worldSpaceNormal = ps_WorldNormal * normal;
    rt_GNormal.rgb = normalize(worldSpaceNormal);

    rt_GPbr = texture(PbrMap, ps_TexCoord * TexScale);
    //todo: displacement mapping is save here, but not used. set to 1 so we can visualize the buffer
    rt_GPbr.a = 1.0;

    rt_GDepth.r = -ps_Depth / FarClip;
    rt_GDepth.gba = ps_Position.xyz;
}
