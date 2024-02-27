/* Metadata
#sampler RenderTarget Linear Wrap
#sampler DepthBuffer Point Clamp
*/

uniform vec3 FogColor;
uniform float FogDensity;

uniform sampler2D RenderTarget;
uniform sampler2D DepthBuffer;

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;

layout(location = 0) out vec4 result;

void main() {
    vec4 color = texture(RenderTarget, ps_TexCoord);
    float depth = texture(DepthBuffer, ps_TexCoord).r * FogDensity * 5.0;

    depth = clamp(depth, 0.0, FogDensity);

    result = vec4(mix(color.rgb, FogColor, depth), 1.0);
}
