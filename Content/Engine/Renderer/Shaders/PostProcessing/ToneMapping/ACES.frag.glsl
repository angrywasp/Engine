/* Metadata
#sampler RenderTarget Linear Wrap
*/

uniform sampler2D RenderTarget;

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;

layout(location = 0) out vec4 result;

vec3 ACES(vec3 x) {
  const float a = 2.51;
  const float b = 0.03;
  const float c = 2.43;
  const float d = 0.59;
  const float e = 0.14;
  return clamp((x * (a * x + b)) / (x * (c * x + d) + e), 0.0, 1.0);
}
void main() {
    vec4 color = texture(RenderTarget, ps_TexCoord);
    vec3 tone = ACES(color.rgb);
    result = vec4(tone, color.a);
}
