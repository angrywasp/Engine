/* Metadata
#include lbuffer LBuffer.glsl
#include pbr Pbr.glsl
#sampler GAlbedoBuffer Point Clamp
#sampler GNormalBuffer Point Clamp
#sampler GPbrBuffer Point Clamp
#sampler GDepthBuffer Point Clamp
#sampler IrradianceMap Point Clamp
#sampler PreFilterMap Point Clamp
#sampler BrdfLutMap LinearMipPoint Clamp
*/

layout(location = 0) in vec2 ps_TexCoord;

#include lbuffer
#include pbr

uniform vec3 CameraPosition;

uniform sampler2D GAlbedoBuffer;
uniform sampler2D GNormalBuffer;
uniform sampler2D GPbrBuffer;
uniform sampler2D GDepthBuffer;
uniform samplerCube IrradianceMap;
uniform samplerCube PreFilterMap;
uniform sampler2D BrdfLutMap;

void main()
{
    vec4 gDepth = texture(GDepthBuffer, ps_TexCoord);
    float linearDepth = gDepth.r;
    vec3 worldPos = gDepth.gba;

    vec3 albedo = texture(GAlbedoBuffer, ps_TexCoord).rgb;

    if(-linearDepth + 0.9999 < 0.0)
    {
        rt_LAmbient = vec4(albedo, 1);
        rt_LRadiance = vec4(0);
        return;
    }

    vec3 worldSpaceNormal = texture(GNormalBuffer, ps_TexCoord).rgb;

    vec4 pbr = texture(GPbrBuffer, ps_TexCoord);

    float metalness = pbr.r;
    float ao = 1.0; //ao not implemented
    float roughness = pbr.b;

    vec3 v = normalize(CameraPosition - worldPos);
    vec3 r = reflect(-v, worldSpaceNormal);
    float nDotV = dot(worldSpaceNormal, v);

    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, albedo, metalness);

    vec3 f = F_Roughness(F0, max(nDotV, 0.0), roughness);
    vec3 kS = f;
    vec3 kD = vec3(1.0) - kS;
    kD *= (1.0 - metalness);
    vec3 irradiance = texture(IrradianceMap, r).rgb;
    vec3 diffuse = irradiance * albedo;

    // sample both the pre-filter map and the BRDF lut and combine them together
    // as per the Split-Sum approximation to get the IBL specular part.

    const float MAX_REFLECTION_LOD = 4.0;
    vec3 prefilterColor = textureLod(PreFilterMap, r, roughness * MAX_REFLECTION_LOD).rgb;
    vec2 brdf = texture(BrdfLutMap, vec2(max(nDotV, 0.0), roughness)).rg;
    vec3 specular = prefilterColor * (f * brdf.x + brdf.y);
    
    rt_LAmbient = vec4((kD * diffuse + specular) * ao, 1);
    rt_LRadiance = vec4(0);
}
