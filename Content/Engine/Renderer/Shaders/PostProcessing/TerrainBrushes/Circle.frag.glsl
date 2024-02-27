/**/

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;

layout(location = 0) out vec4 result;

void main() {
    float d = distance(vec2(0.5, 0.5), ps_TexCoord);

    if(d > 0.5)
        result = vec4(0.0);
    else
        result = vec4(1.0);
}
