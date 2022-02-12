#ifndef LIGHTING_INCLUDED
#define LIGHTING_INCLUDED

#include "Includes/Constants.hlsl"
#include "Includes/Structs.hlsl"
#include "Includes/Ray.hlsl"

float _AmbientOcclusionRange;
float _AmbientOcclusionIntensity;
float3 _AmbientLight;
float3 _DirectionalLightDirection;
float3 _DirectionalLightColor;
StructuredBuffer<PointLight> _PointLights;
Texture2D<float4> _SkyboxTexture;
SamplerState sampler_SkyboxTexture;
StructuredBuffer<Sphere> _Spheres;

inline RayHit Trace(Ray ray) {
	RayHit rayHit = CreateRayHit();
	uint count, stride, index;

	_Spheres.GetDimensions(count, stride);
	for (index = 0; index < count; ++index) {
		IntersectSphere(ray, rayHit, _Spheres[index]);
	}

	_Meshes.GetDimensions(count, stride);
	for (index = 0; index < count; ++index) {
		IntersectMesh(ray, rayHit, _Meshes[index]);
	}

	return rayHit;
}

inline float3 CalculateSkyboxColor(float3 direction) {
	float theta = acos(direction.y) / -PI;
	float phi = atan2(direction.x, -direction.z) / -PI * 0.5f + 0.75f;
	return _SkyboxTexture.SampleLevel(sampler_SkyboxTexture, float2(phi, theta), 0.0f).xyz;
}

inline float CalculateFersnelFactor(Ray ray, RayHit rayHit) {
	float3 hitToCamera = normalize(rayHit.position - ray.origin);
	return FERSNEL_CONSTANT * pow(dot(rayHit.normal, hitToCamera) * 0.5f + 0.5f, FERSNEL_CONSTANT);
}

inline float CalculateShadowFactor(RayHit rayHit, float3 lightSource) {
	Ray shadowRay = CreateRay(rayHit.position + rayHit.normal * SMALL_OFFSET, lightSource);
	RayHit shadowRayHit = Trace(shadowRay);
	return shadowRayHit.distance < INFINITY ? ZERO : ONE;
}

inline float3 CalculatePointLightPart(RayHit rayHit, PointLight pointLight) {
	float3 hitToLight = pointLight.position - rayHit.position;
	float pointLightShadowFactor = CalculateShadowFactor(rayHit, normalize(hitToLight));
	float pointLightIntensity = dot(normalize(rayHit.normal), normalize(hitToLight));
	float pointLightDistance = length(hitToLight);
	return pointLightShadowFactor * pointLightIntensity * rayHit.albedo * pointLight.color / pointLightDistance;
}

inline float3 CalculateDiffusePart(RayHit rayHit) {
	float3 color = ZERO;
	uint count, stride;
	_PointLights.GetDimensions(count, stride);
	for (uint index = 0; index < count; ++index) {
		color += CalculatePointLightPart(rayHit, _PointLights[index]);
	}
	
	float directionalLightShadowFactor = CalculateShadowFactor(rayHit, -_DirectionalLightDirection);
	float directionalLightIntensity = -dot(rayHit.normal, _DirectionalLightDirection);
	color += directionalLightShadowFactor * saturate(directionalLightIntensity * rayHit.albedo * _DirectionalLightColor);
	return color;
}

inline float3 CalculateAmbientPart(RayHit rayHit) {
	Ray ambientOcclusionRay = CreateRay(rayHit.position + rayHit.normal * SMALL_OFFSET, rayHit.normal);
	RayHit ambientOcclusionRayHit = Trace(ambientOcclusionRay);
	float ambientOcclusionShade = min(_AmbientOcclusionRange, ambientOcclusionRayHit.distance) / _AmbientOcclusionRange;
	float3 color = rayHit.albedo * _AmbientLight;
	return lerp(color, color * ambientOcclusionShade, _AmbientOcclusionIntensity);
}

inline float3 CalculateSpecularPart(Ray ray, RayHit rayHit) {
	float fersnelFactor = CalculateFersnelFactor(ray, rayHit);
	return saturate(rayHit.specular + fersnelFactor);
}

inline float3 Shade(inout Ray ray, RayHit rayHit) {
	if (rayHit.distance >= INFINITY) {
		ray.energy = ZERO;
		return CalculateSkyboxColor(ray.direction);
	}

	float3 specularPart = CalculateSpecularPart(ray, rayHit);
	ray.energy *= specularPart;
	ray.origin = rayHit.position + rayHit.normal * SMALL_OFFSET;
	ray.direction = reflect(ray.direction, rayHit.normal);
	
	float3 diffusePart = CalculateDiffusePart(rayHit);
	float3 ambientPart = CalculateAmbientPart(rayHit);

	return saturate(diffusePart + ambientPart);
}

#endif /* LIGHTING_INCLUDED */