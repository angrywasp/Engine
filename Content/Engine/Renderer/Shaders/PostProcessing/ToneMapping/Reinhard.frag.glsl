/* Metadata
#sampler RenderTarget Linear Wrap
*/

uniform float LinearExposure;

uniform sampler2D RenderTarget;

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;

layout(location = 0) out vec4 result;

vec3 Reinhard(vec3 x) {
  return x / (1.0 + x);
}

vec3 Reinhard2(vec3 x) {
  const float L_white = 4.0;
  return (x * (1.0 + x / (L_white * L_white))) / (1.0 + x);
}

void main() {
    vec4 color = texture(RenderTarget, ps_TexCoord);
    result = vec4(Reinhard2(color.rgb), color.a);
}
