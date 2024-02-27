/* Metadata
#sampler RenderTarget Linear Wrap
*/

uniform sampler2D RenderTarget;

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;

layout(location = 0) out vec4 result;

void main() {
    vec4 color = texture(RenderTarget, ps_TexCoord);
    float luma = sqrt(dot(color.rgb, vec3(0.299, 0.587, 0.114)));
    result = vec4(vec3(luma), 1);
}
