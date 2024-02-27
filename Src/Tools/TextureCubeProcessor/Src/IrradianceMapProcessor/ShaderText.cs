namespace TextureCubeProcessor.IrradianceMapProcessor {
public static class IrradianceMapShaderText {

public const string PixelShader =

@"/* Metadata
#sampler AlbedoMap Linear Clamp
*/

uniform samplerCube AlbedoMap;

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec3 ps_TexCoord;

layout(location = 0) out vec4 result;

const float PI = 3.14159265359;

// Compute the irradiance of the environment map at the given surface position
vec3 computeIrradiance(samplerCube envMap, vec3 normal)
{
    vec3 irradiance = vec3(0);

    vec3 up = vec3(0.0, 1.0, 0.0);
    vec3 right = normalize(cross(up, normal));
    up = normalize(cross(normal, right));

    float sampleDelta = 0.025;
    float nrSamples = 0.0; 
    for(float phi = 0.0; phi < 2.0 * PI; phi += sampleDelta)
    {
        for(float theta = 0.0; theta < 0.5 * PI; theta += sampleDelta)
        {
            vec3 tangentSample = vec3(sin(theta) * cos(phi),  sin(theta) * sin(phi), cos(theta));
            vec3 sampleVec = tangentSample.x * right + tangentSample.y * up + tangentSample.z * normal; 

            irradiance += texture(AlbedoMap, sampleVec).rgb * cos(theta) * sin(theta);
            nrSamples++;
        }
    }
    irradiance = PI * irradiance * (1.0 / float(nrSamples));
    return irradiance;
}

void main()
{
    result = vec4(computeIrradiance(AlbedoMap, normalize(ps_TexCoord)), 1);
}";

}}