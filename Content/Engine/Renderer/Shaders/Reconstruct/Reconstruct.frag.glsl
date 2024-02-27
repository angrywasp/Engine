/* Metadata
#sampler GDepthBuffer Point Clamp
#sampler LRadianceBuffer Point Clamp
#sampler LAmbientBuffer Point Clamp
*/

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec2 ps_TexCoord;
layout(location = 2) in vec2 ps_DepthTexCoord;

layout(location = 0) out vec4 result;

uniform float FarClip;
uniform vec2 ProjectionValues;

uniform sampler2D GDepthBuffer;
uniform sampler2D LRadianceBuffer;
uniform sampler2D LAmbientBuffer;

void main() {
    float depthValue = -texture(GDepthBuffer, ps_DepthTexCoord).r * FarClip;
    float z = ProjectionValues.x * depthValue + ProjectionValues.y;

    vec3 radiance = texture(LRadianceBuffer, ps_TexCoord).rgb;
    vec3 ambient = texture(LAmbientBuffer, ps_TexCoord).rgb;

    vec3 color = vec3(pow(radiance, vec3(2.2)) + ambient);
    float luma = sqrt(dot(color.rgb, vec3(0.299, 0.587, 0.114)));
    result = vec4(color, luma);
    gl_FragDepth = -z / depthValue;
}
