/* Metadata
#sampler ReflectionTexture Linear Mirror
#sampler RefractionTexture Linear Mirror
#sampler WaveMap0 Linear Mirror
#sampler WaveMap1 Linear Mirror
*/

uniform vec4 LightColor;
uniform vec3 LightDirection;
uniform float LightSpecularPower;
uniform float LightSpecularIntensity;
uniform vec4 WaterColor;
uniform vec3 CameraPosition;
const float R0 = 0.02037;

uniform sampler2D ReflectionTexture;
uniform sampler2D RefractionTexture;
uniform sampler2D WaveMap0;
uniform sampler2D WaveMap1;

layout(location = 0) in vec3 ps_PosW;
layout(location = 1) in vec3 ps_ToEyeW;
layout(location = 2) in vec2 ps_Tex0;
layout(location = 3) in vec2 ps_Tex1;
layout(location = 4) in vec4 ps_ProjTexC;
layout(location = 5) in vec2 ps_TexCoord;

layout(location = 0) out vec4 result;

void main() {
    vec3 toEyeW = ps_ToEyeW;
    vec2 tex0 = ps_Tex0;
    vec2 tex1 = ps_Tex1;
    vec4 projTexC = ps_ProjTexC;
    vec3 pos = ps_PosW;

    projTexC.xyz /= projTexC.w;
    projTexC.x = 0.5 * projTexC.x + 0.5;
    projTexC.y = -0.5 * projTexC.y + 0.5;
    projTexC.z = 0.1 / projTexC.z; //refract more based on distance from the camera

    toEyeW = normalize(toEyeW);

    // Light vector is opposite the direction of the light.
    vec3 lightVecW = -LightDirection;

	// Sample normal map.
    vec4 normalT0 = texture(WaveMap0, tex0);
    vec4 normalT1 = texture(WaveMap1, tex1);

    //unroll the normals retrieved from the normalmaps
    normalT0.yz = normalT0.zy;
    normalT1.yz = normalT1.zy;

    normalT0 = 2.0 * normalT0 - 1.0;
    normalT1 = 2.0 * normalT1 - 1.0;

    vec4 normalT = normalize(0.5 * (normalT0 + normalT1));
    vec3 n1 = vec3(0.0, 1.0, 0.0); //we'll just use the y unit vector for spec reflection.

	//get the reflection vector from the eye
    vec3 R = normalize(reflect(vec4(toEyeW, 1.0), normalT)).xyz;

	//compute the fresnel term to blend reflection and refraction maps
    float ang = clamp(dot(-toEyeW, n1), 0.0, 1.0);
    float f = R0 + (1.0 - R0) * pow(1.0 - ang, 5.0);
    f += 0.015 * CameraPosition.y;

    f = clamp(f, 0.0, 1.0);

    vec3 sunlight = LightSpecularIntensity * pow(clamp(dot(R, lightVecW), 0.0, 1.0), LightSpecularPower) * LightColor.rgb;

    vec4 refl = texture(ReflectionTexture, projTexC.xy + projTexC.z * normalT.xz);
    vec4 refr = texture(RefractionTexture, projTexC.xy - projTexC.z * normalT.xz);

	//only use the refraction map if we're under water
    if(CameraPosition.y < pos.y)
        f = 0.0;

    result = vec4((WaterColor.rgb * mix(refr, refl, f).rgb) + sunlight, 1.0);
}