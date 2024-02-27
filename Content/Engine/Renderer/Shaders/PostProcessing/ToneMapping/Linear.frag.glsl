/* Metadata
#sampler RenderTarget Linear Wrap
*/

uniform float LinearExposure;

uniform sampler2D RenderTarget;

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;

layout(location = 0) out vec4 result;

vec3 Linear(vec3 linearColor) {
    linearColor *= vec3(LinearExposure);
    float factor = 1.0 / 2.2;
    return pow(linearColor, vec3(factor));
}

void main() {
    vec4 color = texture(RenderTarget, ps_TexCoord);
    result = vec4(Linear(color.rgb), color.a);
}
