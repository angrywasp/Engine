vec3 NormalMapToSpaceNormal(vec3 nMap, vec3 normal, vec3 binormal, vec3 tangent)
{
	nMap = nMap * 2.0 - 1.0;
	nMap = vec3(normal * nMap.z + nMap.x * tangent + nMap.y * binormal);
	return nMap;
}

vec2 PostProjectionSpaceToScreenSpace(vec4 pos)
{
	vec2 screenPos = pos.xy / pos.w;
	return (0.5f * (vec2(screenPos.x, screenPos.y) + vec2(1.0)));
}

float Rgb2Luma(vec3 rgb)
{
    return sqrt(dot(rgb, vec3(0.299, 0.587, 0.114)));
}
