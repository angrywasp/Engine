/* Metadata
#sampler RenderTarget Linear Wrap
#sampler DepthBuffer Point Clamp
*/

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;

layout(location = 0) out vec4 result;

uniform vec3 FogColor;
uniform float FogDensity;
uniform float WaterHeight;

uniform sampler2D RenderTarget;
uniform sampler2D DepthBuffer;

void main() {
    vec4 depth = texture2D(DepthBuffer, ps_TexCoord);
    float linearDepth = depth.r;
    float yDepth = 10 - depth.b;
    vec4 color = texture2D(RenderTarget, ps_TexCoord);

    float density = clamp(linearDepth, 0.0f, yDepth);
    result = vec4(mix(color.rgb, FogColor, density), 1.0f);
}
