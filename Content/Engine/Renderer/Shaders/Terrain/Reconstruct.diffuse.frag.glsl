/* Metadata
#include clipping Clipping.glsl
#include helpers Helpers.glsl
#sampler DiffuseMap Linear Wrap
#sampler DiffuseMap0 Linear Wrap
#sampler DiffuseMap1 Linear Wrap
#sampler DiffuseMap2 Linear Wrap
#sampler DiffuseMap3 Linear Wrap
#sampler BlendTexture Linear Clamp
*/

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;
layout(location = 2) in vec4 ps_TexCoordScreenSpace;

layout(location = 0) out vec4 result;

#include clipping
#include helpers

uniform vec2 TexScale;

uniform sampler2D DiffuseMap;
uniform sampler2D DiffuseMap0;
uniform sampler2D DiffuseMap1;
uniform sampler2D DiffuseMap2;
uniform sampler2D DiffuseMap3;

uniform sampler2D BlendTexture;

vec3 blend(vec2 texCoord) {
    vec4 blend = texture(BlendTexture, texCoord);
    texCoord *= TexScale;
    vec3 color = texture(DiffuseMap, texCoord).rgb;

    color = mix(color, texture(DiffuseMap0, texCoord).rgb, blend.r);
    color = mix(color, texture(DiffuseMap1, texCoord).rgb, blend.g);
    color = mix(color, texture(DiffuseMap2, texCoord).rgb, blend.b);
    color = mix(color, texture(DiffuseMap3, texCoord).rgb, blend.a);

    return color;
}

void main() {
    if(Clip(ps_Position.y))
        discard;

    vec3 color = blend(ps_TexCoord);
    float luma = Rgb2Luma(color);
    result = vec4(color, luma);
}
