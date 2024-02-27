/* Metadata
#sampler Texture MinLinearMagPointMipPoint Clamp
*/

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TextureCoordinate;
layout(location = 2) in vec4 ps_Color;

layout(location = 0) out vec4 result;

uniform sampler2D Texture;

void main() {
    vec4 color = texture(Texture, ps_TextureCoordinate) * ps_Color;
    if(color.a == 0.0)
        discard;

    result = color;
}
