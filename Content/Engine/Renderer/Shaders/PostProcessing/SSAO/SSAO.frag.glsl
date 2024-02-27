/* Metadata
#include encoding NormalEncoding.glsl
#sampler GNormalBuffer Point Clamp
#sampler GDepthBuffer Point Clamp
#sampler RandomBuffer Point Clamp
#sampler RenderTarget Linear Wrap
*/

#define SAMPLE_COUNT 16
const vec3 RAND_SAMPLES[SAMPLE_COUNT] = 
{
    vec3( 0.5381, 0.1856,-0.4319), 
	vec3( 0.1379, 0.2486, 0.4430),
    vec3( 0.3371, 0.5679,-0.0057), 
	vec3(-0.6999,-0.0451,-0.0019),
    vec3( 0.0689,-0.1598,-0.8547), 
	vec3( 0.0560, 0.0069,-0.1843),
    vec3(-0.0146, 0.1402, 0.0762), 
	vec3( 0.0100,-0.1924,-0.0344),
    vec3(-0.3577,-0.5301,-0.4358), 
	vec3(-0.3169, 0.1063, 0.0158),
    vec3( 0.0103,-0.5869, 0.0046), 
	vec3(-0.0897,-0.4940, 0.3287),
    vec3( 0.7119,-0.0154,-0.0918), 
	vec3(-0.0533, 0.0596,-0.5411),
    vec3( 0.0352,-0.0631, 0.5460), 
	vec3(-0.4776, 0.2847,-0.0271)
};

layout(location = 0) in vec4 ps_Position;
layout(location = 1) in vec4 ps_TexCoord;
layout(location = 2) in vec2 ps_TexCoordHalfBuffer;

layout(location = 0) out vec4 result;

#include encoding

uniform vec2 Radius;
uniform float FarClip;
uniform float Bias;

uniform sampler2D GNormalBuffer;
uniform sampler2D GDepthBuffer;
uniform sampler2D RandomBuffer;

uniform sampler2D RenderTarget;

void main() {
    float depth = texture(GDepthBuffer, ps_TexCoord.xy).r;

    if(-depth + 0.9999 < 0.0)
        discard;

    float totalOcclusion = 0;

    depth *= FarClip;    
	//prevent near 0 divisions
	float scale = min(Radius.y, Radius.x / max(1, depth));

    vec3 normal = DecodeNormal(texture(GNormalBuffer, ps_TexCoord.xy).rg);
    normal.y = -normal.y;

    //this will be used to avoid self-shadowing		  
	vec3 normalScaled = normal * 0.25f;

	//pick a random normal, to add some "noise" to the output
	vec3 randNormal = (texture(RandomBuffer, ps_TexCoord.zw).rgb * 2.0 - 1.0);
			
	for (int i = 0; i < SAMPLE_COUNT; i++)
	{
		// reflect the pre-computed direction on the random normal 
		vec3 randomDirection = reflect(RAND_SAMPLES[i], randNormal);
			
		// Prevent it pointing inside the geometry
		randomDirection *= sign(dot(normal , randomDirection));

		// add that scaled normal
		randomDirection += normalScaled;
		
		//we use a modified depth in the tests
		float modifiedDepth = depth - (randomDirection.z * Radius.x);
		//according to the distance to the camera, we should scale the direction to account the perspective
		vec2 offset = randomDirection.xy * scale;
		
		// Sample depth at offset location
		float newDepth = texture(GDepthBuffer, ps_TexCoord.xy + offset).r * FarClip;
		   
		//we only care about samples in front of our original-modifies 
		float deltaDepth = clamp(modifiedDepth - newDepth, 0.0, 1.0);
			
		//ignore negative deltas
		totalOcclusion += (1 - deltaDepth) * (deltaDepth > Bias ? 1.0 : 0.0);
			      
	}

	totalOcclusion /= SAMPLE_COUNT;

    result = vec4(vec3(totalOcclusion), 1.0);
}
