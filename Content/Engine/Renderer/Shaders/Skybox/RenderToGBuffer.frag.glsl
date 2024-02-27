/* Metadata
#include clipping Clipping.glsl
#include gbuffer GBuffer.glsl
#sampler AlbedoMap Linear Clamp
*/

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec3 ps_TexCoord;

#include clipping
#include gbuffer

uniform samplerCube AlbedoMap;

void main() {
    if(Clip(ps_Position.y))
        discard;

    rt_GAlbedo = texture(AlbedoMap, ps_TexCoord);
    rt_GNormal = vec4(0.5, 0.5, 1.0, 1.0);
    rt_GDepth.r = 1.0;
    rt_GDepth.gba = ps_Position.xyz;
}
