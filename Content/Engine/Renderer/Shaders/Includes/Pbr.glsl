const float PI = 3.141592;
const vec3 fDielectric = vec3(0.04);

vec3 F(vec3 f0, float xDotY)
{
    return f0 + (1.0 - f0) * pow(1.0 - xDotY, 5.0);
}

vec3 F_Roughness(vec3 f0, float xDotY, float roughness)
{
    return f0 + (max(vec3(1.0 - roughness), f0) - f0) * pow(clamp(1.0 - xDotY, 0.0, 1.0), 5.0);
} 

float G_SchlickGGX(float roughness, float nDotX)
{
    float r = (roughness + 1.0);
    float k = (r * r) / 8.0;

    return nDotX / (nDotX * (1.0 - k) + k);
}

float D(float roughness, float nDotH)
{
    float nDotH2 = nDotH * nDotH;
    float m2 = roughness * roughness;
    float x = ((nDotH * nDotH) * (m2 - 1.0)) + 1.0;
    float d = m2 / (PI * x * x);

    return d;
}


float G(float roughness, float nDotL, float nDotV)
{
    float ggx2  = G_SchlickGGX(roughness, nDotV);
    float ggx1  = G_SchlickGGX(roughness, nDotL);
	
    return ggx1 * ggx2;
}
