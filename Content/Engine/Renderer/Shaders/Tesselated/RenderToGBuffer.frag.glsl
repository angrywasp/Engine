/* Metadata
#include clipping Clipping.glsl
#include gbuffer GBuffer.glsl
#include helpers Helpers.glsl
#include encoding NormalEncoding.glsl
#sampler DiffuseMap Linear Wrap
#sampler NormalMap Linear Wrap
*/

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;
layout(location = 2) in float ps_Depth;
layout(location = 3) in vec3 ps_Normal;
layout(location = 4) in vec3 ps_Tangent;
layout(location = 5) in vec3 ps_Binormal;

#include clipping
#include gbuffer
#include encoding
#include helpers

uniform vec2 TexScale;
uniform float FarClip;
uniform bool ReceiveShadows;

uniform sampler2D DiffuseMap;
uniform sampler2D NormalMap;


void main() {
    if(Clip(ps_Position.y))
        discard;

    vec4 diffuseMap = texture(DiffuseMap, ps_TexCoord * TexScale);

    if(diffuseMap.a < 0.5)
        discard;

    vec4 normalMap = texture(NormalMap, ps_TexCoord * TexScale);
    vec3 normalViewSpace = NormalMapToSpaceNormal(normalMap.rgb, ps_Normal, ps_Binormal, ps_Tangent);

    rt_GParams.rg = EncodeNormal(normalize(normalViewSpace));
    rt_GParams.b = 0.0;
    rt_GParams.a = float(ReceiveShadows);

    rt_GDepth.r = -ps_Depth / FarClip;
    rt_GDepth.gba = ps_Position.xyz;
}
