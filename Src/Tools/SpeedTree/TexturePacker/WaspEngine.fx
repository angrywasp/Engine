void TexturePacking(STexturePackingInput sInput, inout STexturePackingOutput sOutput)
{
	const float3 vBlack = float3(0,0,0);
	const float3 vWhite = float3(1,1,1);
	const float3 vFlippedGreenNormal = float3(sInput.m_vNormal.r, 1.0 - sInput.m_vNormal.g, sInput.m_vNormal.b);
	const float3 vCombinedSubsurface = sInput.m_vSubsurfaceColor * sInput.m_fSubsurfaceAmount;
	const float fNoisyOpacity = sInput.m_fOpacity * (sInput.m_fNoise * 2.0 + 1.0) / 3.0;

	sOutput.m_vTexture0.r = sInput.m_vColor.r;
	sOutput.m_vTexture0.g = sInput.m_vColor.g;
	sOutput.m_vTexture0.b = sInput.m_vColor.b;
	sOutput.m_vTexture0.a = sInput.m_fOpacity;
	sOutput.m_vTexture0 = saturate(sOutput.m_vTexture0);
	sOutput.m_vTexture0.rgb = LinearTosRgb(sOutput.m_vTexture0.rgb);

	sOutput.m_vTexture1.r = sInput.m_vNormal.r;
	sOutput.m_vTexture1.g = vFlippedGreenNormal.g;
	sOutput.m_vTexture1.b = sInput.m_vNormal.b;
	sOutput.m_vTexture1.a = sInput.m_fGloss;
	sOutput.m_vTexture1 = saturate(sOutput.m_vTexture1);

	sOutput.m_vTexture2.r = sInput.m_fMetallic;
	sOutput.m_vTexture2.g = sInput.m_fAmbientOcclusion;
	sOutput.m_vTexture2.b = 1.0 - sInput.m_fGloss;
	sOutput.m_vTexture2.a = sInput.m_fHeight;
	sOutput.m_vTexture2 = saturate(sOutput.m_vTexture3);

}
