#ifndef RAY_INCLUDED
#define RAY_INCLUDED

#include "Includes/Constants.hlsl"
#include "Includes/Structs.hlsl"

float4x4 _CameraToWorld;
float4x4 _CameraInverseProjection;

inline Ray CreateRay(float3 origin, float3 direction) {
	Ray ray;
	ray.origin = origin;
	ray.direction = direction;
	ray.energy = ONE;
	return ray;
}

inline Ray CreateCameraRay(float2 homoCoord) {
	float3 origin = mul(_CameraToWorld, float4(0.0f, 0.0f, 0.0f, 1.0f)).xyz;
	float3 direction = mul(_CameraInverseProjection, float4(homoCoord, 0.0f, 1.0f)).xyz;
	direction = mul(_CameraToWorld, float4(direction, 0.0f)).xyz;
	direction = normalize(direction);
	return CreateRay(origin, direction);
}

inline RayHit CreateRayHit() {
	RayHit rayHit;
	rayHit.position = ZERO;
	rayHit.normal = ZERO;
	rayHit.distance = INFINITY;
	rayHit.albedo = ZERO;
	rayHit.specular = ZERO;
	return rayHit;
}

#endif /* RAY_INCLUDED */