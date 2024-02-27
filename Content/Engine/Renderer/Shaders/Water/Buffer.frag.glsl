/* Metadata
#sampler ReflectionSource Linear Wrap
#sampler RefractionSource Linear Wrap
#sampler DepthBuffer Point Clamp
*/

uniform vec4 WaterColor;
uniform float WaterHeight;
uniform float MaximumLightExtinction;
uniform vec3 CameraPosition;

uniform sampler2D DepthBuffer;
uniform sampler2D ReflectionSource;
uniform sampler2D RefractionSource;

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;

layout(location = 0) out vec4 rt_Reflection;
layout(location = 1) out vec4 rt_Refraction;

//todo: could copy the tonemapping from the post process chain, but linear will do for now
vec3 LinearToneMap(vec3 linearColor) {
    float factor = 1.0 / 2.2;
    return pow(linearColor, vec3(factor));
}

void main() {
    if(CameraPosition.y > WaterHeight) {
        float pixelDepth = WaterHeight - texture(DepthBuffer, ps_TexCoord).b;

        if(pixelDepth > 0.0) {
            float scaled = clamp(pixelDepth / WaterHeight, 0.0, MaximumLightExtinction);
            vec3 pix = LinearToneMap(texture(RefractionSource, ps_TexCoord).rgb);

            rt_Refraction = vec4(mix(pix, WaterColor.rgb, scaled), 1.0);
        }
    } else
        rt_Refraction = vec4(LinearToneMap(texture(RefractionSource, ps_TexCoord).rgb), 1.0);

    rt_Reflection = texture(ReflectionSource, ps_TexCoord);
}
