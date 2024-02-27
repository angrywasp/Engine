/**/

layout(location = 0) in vec4 ps_Color;
layout(location = 1) in vec4 ps_ClipDistances;

layout(location = 0) out vec4 result;

uniform vec4 ColorMultiplier;
uniform bool DoClip;

void main() {
    if(DoClip && ps_ClipDistances.x < 0.0)
        discard;

    result = vec4((ps_Color * ColorMultiplier).rgb, 1);
}
