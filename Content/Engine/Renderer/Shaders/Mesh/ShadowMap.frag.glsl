/* Metadata
#sampler AlbedoMap Linear Wrap
*/

layout(location = 0) in vec2 ps_TexCoord;
layout(location = 1) in vec2 ps_Depth;

layout(location = 0) out vec4 result;

uniform vec2 TexScale;

uniform sampler2D AlbedoMap;

void main() {
	
    vec4 albedoMap = texture(AlbedoMap, ps_TexCoord * TexScale);

    if(albedoMap.a < 0.5)
        discard;

    float depth = ps_Depth.x / ps_Depth.y;
    result = vec4(depth, 0.0, 0.0, 1.0);
}
