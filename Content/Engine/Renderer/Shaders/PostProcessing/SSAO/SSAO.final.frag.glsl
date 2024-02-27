/* Metadata
#sampler SSAOBuffer Linear Clamp
*/

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec4 ps_TexCoord;
layout(location = 2) in vec2 ps_TexCoordHalfBuffer;

layout(location = 0) out vec4 result;

uniform sampler2D SSAOBuffer;
uniform float SSAOIntensity;

uniform sampler2D RenderTarget;

void main() {
    float ssao = texture(SSAOBuffer, ps_TexCoord.xy).r * SSAOIntensity;
    vec4 color = texture(RenderTarget, ps_TexCoord.xy);
    result = vec4(color.rgb * (1 - vec3(ssao)), 1.0);
}
