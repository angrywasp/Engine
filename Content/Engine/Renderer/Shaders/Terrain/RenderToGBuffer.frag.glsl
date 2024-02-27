/* Metadata
#include clipping Clipping.glsl
#include gbuffer GBuffer.glsl
#include helpers Helpers.glsl
#include encoding NormalEncoding.glsl
#sampler NormalMap Linear Wrap
#sampler NormalMap0 Linear Wrap
#sampler NormalMap1 Linear Wrap
#sampler NormalMap2 Linear Wrap
#sampler NormalMap3 Linear Wrap
#sampler BlendTexture Linear Clamp
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

uniform sampler2D NormalMap;
uniform sampler2D NormalMap0;
uniform sampler2D NormalMap1;
uniform sampler2D NormalMap2;
uniform sampler2D NormalMap3;

uniform sampler2D BlendTexture;

vec3 blend(vec2 texCoord) {
    vec4 blend = texture(BlendTexture, texCoord);
    texCoord *= TexScale;

    vec3 normal = NormalMapToSpaceNormal(texture(NormalMap, texCoord).rgb, ps_Normal, ps_Binormal, ps_Tangent);

    vec3 normal0 = NormalMapToSpaceNormal(texture(NormalMap0, texCoord).rgb, ps_Normal, ps_Binormal, ps_Tangent);
    vec3 normal1 = NormalMapToSpaceNormal(texture(NormalMap1, texCoord).rgb, ps_Normal, ps_Binormal, ps_Tangent);
    vec3 normal2 = NormalMapToSpaceNormal(texture(NormalMap2, texCoord).rgb, ps_Normal, ps_Binormal, ps_Tangent);
    vec3 normal3 = NormalMapToSpaceNormal(texture(NormalMap3, texCoord).rgb, ps_Normal, ps_Binormal, ps_Tangent);

    normal = mix(normal, normal0, blend.r);
    normal = mix(normal, normal1, blend.g);
    normal = mix(normal, normal2, blend.b);
    normal = mix(normal, normal3, blend.a);

    return normal;
}

void main() {
    if(Clip(ps_Position.y))
        discard;

    vec3 normalViewSpace = blend(ps_TexCoord);

    float depth = -ps_Depth / FarClip;

    //vec3 defaultNormal = NormalMapToSpaceNormal(vec3(0.5, 0.5, 1.0), ps_Normal, ps_Binormal, ps_Tangent);

    //if (depth > 0.1)
    //    normalViewSpace = defaultNormal;

    //normalViewSpace = mix(normalViewSpace, defaultNormal, depth * 10);

    rt_GParams.rg = EncodeNormal(normalize(normalViewSpace));
    rt_GParams.b = 0.0;
    rt_GParams.a = 1.0;

    rt_GDepth.r = depth;
    rt_GDepth.gba = ps_Position.xyz;
}