/* Metadata
#sampler GDepthBuffer Point Clamp
#sampler SSAOBuffer Linear Clamp
*/

#define NUM_SAMPLERS 4

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec4 ps_TexCoord;
layout(location = 2) in vec2 ps_TexCoordHalfBuffer;

layout(location = 0) out vec4 result;

uniform float FarClip;
uniform vec2 BlurDirection;

uniform sampler2D SSAOBuffer;
uniform sampler2D GDepthBuffer;

void main() {
    float depth = texture(GDepthBuffer, ps_TexCoord.xy).r;

    if(-depth + 0.9999 < 0.0)
        discard;

    float divisor = 1;
    float occ = 0;

    if (depth < 1)
    {
        occ = texture(SSAOBuffer, ps_TexCoord.zw).r;

        for (int i = -NUM_SAMPLERS; i <= NUM_SAMPLERS; i++)
		{
			vec2 uvOcc = ps_TexCoord.zw + BlurDirection * i;
			float newOcc = texture(SSAOBuffer, uvOcc).r;
			vec2 uvScreen = ps_TexCoord.xy + BlurDirection * i;
			float newDepth = texture(GDepthBuffer, uvScreen).r;
			float depthDif = abs(depth - newDepth) * FarClip;
			float sz = max(0, 0.2 - depthDif) * 5;
			divisor += sz;
			occ+=newOcc*sz;
		}		
	
		occ /= divisor;
    }

    result = vec4(vec3(1 - occ), 1.0);
}
