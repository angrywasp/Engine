/*
#include helpers Helpers.glsl
#include lbuffer LBuffer.glsl
#include pbr Pbr.glsl
#sampler GAlbedoBuffer Point Clamp
#sampler GNormalBuffer Point Clamp
#sampler GPbrBuffer Point Clamp
#sampler GDepthBuffer Point Clamp
#sampler ShadowMap Point Clamp
*/

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;
layout(location = 2) in vec4 ps_TexCoordScreenSpace;
layout(location = 3) in vec4 ps_LightPos;

#include lbuffer
#include pbr
#include helpers

uniform vec4 LightColor;
uniform vec3 CameraPosition;

uniform vec3 LightPosition;
uniform float InvLightRadiusSqr;

uniform sampler2D GAlbedoBuffer;
uniform sampler2D GNormalBuffer;
uniform sampler2D GPbrBuffer;
uniform sampler2D GDepthBuffer;

float ComputeAttenuation(vec3 lDir) {
    return 1.0 - clamp(dot(lDir, lDir) * InvLightRadiusSqr, 0.0, 1.0);
}

void main() {
    vec2 screenPos = PostProjectionSpaceToScreenSpace(ps_TexCoordScreenSpace);
    vec4 gDepth = texture(GDepthBuffer, screenPos);
    float linearDepth = gDepth.r;
    vec3 worldPos = gDepth.gba;

    if(-linearDepth + 0.9999 < 0.0)
        discard;

    vec3 lDir = LightPosition - worldPos;
    float attenuation = ComputeAttenuation(lDir);
    lDir = normalize(lDir);

    vec3 worldSpaceNormal = texture(GNormalBuffer, screenPos).rgb;
    float nDotL = dot(worldSpaceNormal, lDir);

    if(nDotL < 0.00001f)
        discard;

    //Direct lighting calculation

    vec3 albedo = texture(GAlbedoBuffer, screenPos).rgb;
    vec4 pbr = texture(GPbrBuffer, screenPos);

    float metalness = pbr.r;
    float roughness = pbr.b;

    vec3 radiance = (LightColor.rgb * LightColor.a) * attenuation;
    
    vec3 v = normalize(CameraPosition - worldPos);
    vec3 h = normalize(v + lDir);
    float nDotV = dot(worldSpaceNormal, v);

    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, albedo, metalness);

    vec3 f = F(F0, max(dot(v, h), 0.0));
    float d = D(roughness, dot(worldSpaceNormal, h));
    float g = G(roughness, nDotL, nDotV);

    vec3 specular = (f * d * g) / (PI * (max(nDotL, 0.0) * max(nDotV, 0.0) + 0.0001));
    vec3 kS = f;
    vec3 kD = vec3(1.0) - kS;
    kD *= (1.0 - metalness);
    vec3 Lo = (kD * albedo / PI + specular) * radiance * max(nDotL, 0.0);

    rt_LRadiance = vec4(Lo, nDotL);
    rt_LAmbient = vec4(0);
}
