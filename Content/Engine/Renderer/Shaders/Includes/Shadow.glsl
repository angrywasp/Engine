uniform vec2 ShadowMapPixelSize;
uniform vec2 ShadowMapSize;

uniform sampler2D ShadowMap; //point clamp

const float DepthBias = 0.0;

float NoFilter(float nl, vec2 shadowTexCoord, float ourdepth)
{
	// Get the current depth stored in the shadow map
	float shadowdepth = texture(ShadowMap, shadowTexCoord).r;

	// Check to see if this pixel is in front or behind the value in the shadow map
	nl = shadowdepth < ourdepth ? 0.0 : nl;

	return nl;
}

float SimpleFilter(float nl, vec2 shadowTexCoord, float ourdepth)
{
    // Get the current depth stored in the shadow map
    float samples[4];
    samples[0] = texture(ShadowMap, shadowTexCoord).r < ourdepth ? 0.0 : 1.0;
    samples[1] = texture(ShadowMap, shadowTexCoord + vec2(0.0, 2.0) * ShadowMapPixelSize).r < ourdepth ? 0.0 : 1.0;
    samples[2] = texture(ShadowMap, shadowTexCoord + vec2(2.0, 0.0) * ShadowMapPixelSize).r < ourdepth ? 0.0 : 1.0;
    samples[3] = texture(ShadowMap, shadowTexCoord + vec2(2.0, 2.0) * ShadowMapPixelSize).r < ourdepth ? 0.0 : 1.0;

    // Determine the lerp amounts           
    vec2 lerps = fract(shadowTexCoord * ShadowMapSize);
    // lerp between the shadow values to calculate our light amount
    float shadow = mix(mix(samples[0], samples[1], lerps.y), mix(samples[2], samples[3], lerps.y), lerps.x);

    return nl * shadow;
}

 vec2 poissonDisk[24] = { 
    vec2(0.5713538, 0.7814451),
    vec2(0.2306823, 0.6228884),
    vec2(0.1000122, 0.9680607),
    vec2(0.947788, 0.2773731),
    vec2(0.2837818, 0.303393),
    vec2(0.6001099, 0.4147638),
    vec2(-0.2314563, 0.5434746),
    vec2(-0.08173513, 0.0796717),
    vec2(-0.4692954, 0.8651238),
    vec2(0.2768489, -0.3682062),
    vec2(-0.5900795, 0.3607553),
    vec2(-0.1010569, -0.5284956),
    vec2(-0.4741178, -0.2713854),
    vec2(0.4067073, -0.00782522),
    vec2(-0.4603044, 0.0511527),
    vec2(0.9820454, -0.1295522),
    vec2(0.8187376, -0.4105208),
    vec2(-0.8115796, -0.106716),
    vec2(-0.4698426, -0.6179109),
    vec2(-0.8402727, -0.4400948),
    vec2(-0.2302377, -0.879307),
    vec2(0.2748472, -0.708988),
    vec2(-0.7874522, 0.6162704),
    vec2(-0.9310728, 0.3289311)
};

#define TOTAL_SAMPLES 20

float Filter(float nl, vec2 shadowTexCoord, float ourdepth)
{	
    // Get the current depth stored in the shadow map
    float samples[TOTAL_SAMPLES]; 

    float shadow = 0;
    float sampleDiscSize = 1.7;
    vec2 pixelSize = ShadowMapPixelSize * sampleDiscSize;

    // Sample the texture at various offsets

    for (int i = 0; i < TOTAL_SAMPLES; i++)
    {
        int idx = i;
        samples[idx] = texture(ShadowMap, shadowTexCoord + poissonDisk[idx] * pixelSize).r > ourdepth ? 1 : 0;
        shadow += samples[idx];
    }

    shadow = shadow / (TOTAL_SAMPLES + 1);
    return nl * shadow;
}
