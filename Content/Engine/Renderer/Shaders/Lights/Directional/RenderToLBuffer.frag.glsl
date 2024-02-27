/* MetadataLightDir
#include lbuffer LBuffer.glsl
#include shadow Shadow.glsl
#include pbr Pbr.glsl
#sampler GAlbedoBuffer Point Clamp
#sampler GNormalBuffer Point Clamp
#sampler GPbrBuffer Point Clamp
#sampler GDepthBuffer Point Clamp
#sampler ShadowMap Point Clamp
*/

layout(location = 0) in vec2 ps_TexCoord;
layout(location = 1) in vec3 ps_FrustumRay;

#include lbuffer
#include shadow
#include pbr

uniform vec4 LightColor;
uniform vec3 CameraPosition;

uniform vec3 LightDir;
uniform mat4 LightView[3];
uniform mat4 LightProjection[3];
uniform vec2 ClipPlanes[3];
uniform vec3 CascadeDistances;

uniform sampler2D GAlbedoBuffer;
uniform sampler2D GNormalBuffer;
uniform sampler2D GPbrBuffer;
uniform sampler2D GDepthBuffer;

void main() {
    vec4 gDepth = texture(GDepthBuffer, ps_TexCoord);
    float linearDepth = gDepth.r;
    vec3 worldPos = gDepth.gba;

    if(-linearDepth + 0.9999 < 0.0)
        discard;

    vec3 worldSpaceNormal = texture(GNormalBuffer, ps_TexCoord).rgb;
    float nDotL = dot(worldSpaceNormal, LightDir);
    nDotL *= pow(LightColor.a, 2.2);

    // Shadow calculation

    vec3 viewPos = ps_FrustumRay * linearDepth;
    vec3 weights = vec3((lessThan(vec3(viewPos.z), CascadeDistances)));
    weights.xy -= weights.yz;

    mat4 lvp0 = LightProjection[0] * LightView[0];
    mat4 lvp1 = LightProjection[1] * LightView[1];
    mat4 lvp2 = LightProjection[2] * LightView[2];

    mat4 lightViewProj = lvp0 * weights.x + lvp1 * weights.y + lvp2 * weights.z;
    float fOffset = weights.y * 0.33333 + weights.z * 0.666666;
    vec4 lightingPosition = lightViewProj * vec4(worldPos, 1.0);
    vec2 shadowTexCoord = 0.5 * lightingPosition.xy / lightingPosition.w + vec2(0.5, 0.5);

    shadowTexCoord.x = shadowTexCoord.x * 0.33333 + fOffset;
    shadowTexCoord.y = 1.0 - shadowTexCoord.y;
    shadowTexCoord += ShadowMapPixelSize;

    float ourdepth = (lightingPosition.z / lightingPosition.w) - DepthBias;
    float shadowSkip = float(ClipPlanes[2].y > viewPos.z);
    nDotL = nDotL * shadowSkip + Filter(nDotL, shadowTexCoord, ourdepth) * (1.0 - shadowSkip);

    //Direct lighting calculation

    vec3 albedo = texture(GAlbedoBuffer, ps_TexCoord).rgb;
    vec4 pbr = texture(GPbrBuffer, ps_TexCoord);

    float metalness = pbr.r;
    float roughness = pbr.b;

    //directional light doesn't have attenuation
    vec3 radiance = (LightColor.rgb * LightColor.a);
    
    vec3 v = normalize(CameraPosition - worldPos);
    vec3 h = normalize(v + LightDir);
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
