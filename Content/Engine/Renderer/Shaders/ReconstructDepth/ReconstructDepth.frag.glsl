/* Metadata
#sampler GDepthBuffer Point Clamp
*/

layout(location = 0) in vec2 ps_TexCoord;

uniform float FarClip;
uniform vec2 ProjectionValues;

uniform sampler2D GDepthBuffer;

void main() {
    float depthValue = -texture(GDepthBuffer, ps_TexCoord).r * FarClip;
    float z = ProjectionValues.x * depthValue + ProjectionValues.y;

    gl_FragDepth = -z / depthValue;
}
