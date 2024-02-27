/* Metadata
#include clipping Clipping.glsl
#include helpers Helpers.glsl
#include pbr Pbr.glsl
#sampler Emissive Linear Wrap
#sampler LRadianceBuffer Point Clamp
#sampler LAmbientBuffer Point Clamp
*/

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;
layout(location = 2) in vec4 ps_TexCoordScreenSpace;

layout(location = 0) out vec4 result;

#include clipping
#include helpers
#include pbr

uniform vec2 TexScale;

uniform sampler2D EmissiveMap;

uniform sampler2D LRadianceBuffer;
uniform sampler2D LAmbientBuffer;

void main() {
    if(Clip(ps_Position.y))
        discard;

    vec4 emissive = texture(EmissiveMap, ps_TexCoord * TexScale);

    vec2 screenPos = PostProjectionSpaceToScreenSpace(ps_TexCoordScreenSpace);

    vec4 radiance = texture(LRadianceBuffer, screenPos);
    vec4 ambient = texture(LAmbientBuffer, screenPos);

    result.rgb = vec3(pow(radiance.rgb, vec3(2.2)) + ambient.rgb);
    result.a = clamp(radiance.a, 0.1, 1.0);
}
