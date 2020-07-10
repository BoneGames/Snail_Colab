#ifndef LIGHTWEIGHT_SHADER_VARIABLES_INCLUDED
#define LIGHTWEIGHT_SHADER_VARIABLES_INCLUDED


real4 LD;
real4 LDI[2];

#endif 

#ifndef LIGHTWEIGHT_LIGHTING_INCLUDED
#define LIGHTWEIGHT_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ImageBasedLighting.hlsl"
#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Shadows.hlsl"


struct Light
{
    half3   direction;
    half    distanceAttenuation;
    half    shadowAttenuation;
};

int GetPerObjectLightIndex(int index)
{

    half2 lightIndex2 = (index < 2.0h) ? LDI[0].xy : LDI[0].zw;
    half i_rem = (index < 2.0h) ? index : index - 2.0h;
    return (i_rem < 1.0h) ? lightIndex2.x : lightIndex2.y;
}


float DistanceAttenuation(float distanceSqr, half2 distanceAttenuation)
{

    float lightAtten = rcp(distanceSqr);

#if SHADER_HINT_NICE_QUALITY

    half factor = distanceSqr * distanceAttenuation.x;
    half smoothFactor = saturate(1.0h - factor * factor);
    smoothFactor = smoothFactor * smoothFactor;
#else

    half smoothFactor = saturate(distanceSqr * distanceAttenuation.x + distanceAttenuation.y);
#endif

    return lightAtten * smoothFactor;
}


Light GetMainLight()
{
    Light light;
    light.direction = _MainLightPosition.xyz;
    light.distanceAttenuation = LD.z;
    #if defined(LIGHTMAP_ON)
        light.distanceAttenuation *= unity_ProbesOcclusion.x;
    #endif
    light.shadowAttenuation = 1.0;

    return light;
}

Light GetMainLight(float4 shadowCoord)
{
    Light light = GetMainLight();
    light.shadowAttenuation = MainLightRealtimeShadow(shadowCoord);
    return light;
}

Light GetAdditionalLight(int i, float3 positionWS)
{
    int perObjectLightIndex = GetPerObjectLightIndex(i);


    float3 lightPositionWS = _AdditionalLightsPosition[perObjectLightIndex].xyz;
    half4 distanceAndSpotAttenuation = _AdditionalLightsAttenuation[perObjectLightIndex];
    half4 spotDirection = _AdditionalLightsSpotDir[perObjectLightIndex];

    float3 lightVector = lightPositionWS - positionWS;
    float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);

    half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));
    half attenuation = DistanceAttenuation(distanceSqr, distanceAndSpotAttenuation.xy) * AngleAttenuation(spotDirection.xyz, lightDirection, distanceAndSpotAttenuation.zw);

    Light light;
    light.direction = lightDirection;
    light.distanceAttenuation = attenuation;
    light.shadowAttenuation = AdditionalLightRealtimeShadow(perObjectLightIndex, positionWS);


#if defined(LIGHTMAP_ON)

    half4 lightOcclusionProbeInfo = _AdditionalLightsOcclusionProbes[perObjectLightIndex];


    int probeChannel = lightOcclusionProbeInfo.x;


    half lightProbeContribution = lightOcclusionProbeInfo.y;

    half probeOcclusionValue = unity_ProbesOcclusion[probeChannel];
    light.distanceAttenuation *= max(probeOcclusionValue, lightProbeContribution);
#endif

    return light;
}

#endif 

void MainLightNode_float(float3 worldPos, out float LightAttenuation)
{
	Light mainLight = GetMainLight();
	float4 shadowCoord;
	#ifdef LIGHTWEIGHT_SHADOWS_INCLUDED
	#if SHADOWS_SCREEN
		float4 clipPos = TransformWorldToHClip(worldPos);
		shadowCoord = ComputeScreenPos(clipPos);
	#else
		shadowCoord = TransformWorldToShadowCoord(worldPos);
	#endif
	#endif
	LightAttenuation = MainLightRealtimeShadow(shadowCoord);
}