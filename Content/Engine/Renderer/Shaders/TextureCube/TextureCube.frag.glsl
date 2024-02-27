/* Metadata
#sampler AlbedoMap Linear Clamp
*/

uniform samplerCube AlbedoMap;

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec3 ps_TexCoord;

layout(location = 0) out vec4 result;

void main()
{
    result = texture(AlbedoMap, normalize(ps_TexCoord));
}
