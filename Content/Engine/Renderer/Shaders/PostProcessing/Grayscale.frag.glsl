/* Metadata
#sampler RenderTarget Linear Wrap
*/

uniform sampler2D RenderTarget;

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;

layout(location = 0) out vec4 result;

void main() {
    vec3 color = texture(RenderTarget, ps_TexCoord).rgb;
    float gray = dot(color, vec3(0.3, 0.59, 0.11));
    result = vec4(vec3(gray), 1.0);
}
