/* Metadata
#include clipping Clipping.glsl
#include helpers Helpers.glsl
#sampler DiffuseMap Linear Wrap
#sampler DiffuseMap0 Linear Wrap
#sampler DiffuseMap1 Linear Wrap
#sampler DiffuseMap2 Linear Wrap
#sampler DiffuseMap3 Linear Wrap
#sampler BlendTexture Linear Clamp
#sampler GDepthBuffer Point Clamp
#sampler LColorBuffer Point Clamp
#sampler LParamsBuffer Point Clamp
*/

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;
layout(location = 2) in vec4 ps_TexCoordScreenSpace;

layout(location = 0) out vec4 result;

#include clipping
#include helpers

uniform vec2 TexScale;
uniform vec4 AverageRgb;
uniform vec4 AverageRgb0;
uniform vec4 AverageRgb1;
uniform vec4 AverageRgb2;
uniform vec4 AverageRgb3;

uniform sampler2D GDepthBuffer;
uniform sampler2D LColorBuffer;
uniform sampler2D LParamsBuffer;

uniform sampler2D DiffuseMap;
uniform sampler2D DiffuseMap0;
uniform sampler2D DiffuseMap1;
uniform sampler2D DiffuseMap2;
uniform sampler2D DiffuseMap3;

uniform sampler2D BlendTexture;

vec3 blendDiffuse(vec4 blend, vec2 texCoord)
{
    texCoord *= TexScale;
    vec3 color = texture(DiffuseMap, texCoord).rgb;
    
    color = mix(color, texture(DiffuseMap0, texCoord).rgb, blend.r);
    color = mix(color, texture(DiffuseMap1, texCoord).rgb, blend.g);
    color = mix(color, texture(DiffuseMap2, texCoord).rgb, blend.b);
    color = mix(color, texture(DiffuseMap3, texCoord).rgb, blend.a);

    return color;
}

vec3 blendColor(vec4 blend)
{
    vec3 avg = AverageRgb.rgb;
    //avg = mix(avg, AverageRgb0.rgb, blend.r);
    //avg = mix(avg, AverageRgb1.rgb, blend.g);
    //avg = mix(avg, AverageRgb2.rgb, blend.b);
    //avg = mix(avg, AverageRgb3.rgb, blend.a);
    return avg;
}

void main()
{
    if(Clip(ps_Position.y))
        discard;

    vec4 blend = texture(BlendTexture, ps_TexCoord);

    vec3 diffuse = blendDiffuse(blend, ps_TexCoord);
    //vec3 avg = blendColor(blend);

    vec2 screenPos = PostProjectionSpaceToScreenSpace(ps_TexCoordScreenSpace);

    //vec4 gDepth = texture(GDepthBuffer, screenPos);
    vec4 lColor = texture(LColorBuffer, screenPos);
    vec4 lParams = texture(LParamsBuffer, screenPos);

    //diffuse = mix(diffuse, avg, gDepth.r);

    float nl = lParams.g;

    float ambient = 0.25f;
    nl += ambient;

    vec3 color = ((lColor.rgb * nl) * diffuse);
    float luma = Rgb2Luma(color);
    result = vec4(color, luma);
}
