#ifndef STRUCTS_INCLUDED
#define STRUCTS_INCLUDED

struct Ray {
	float3 origin;
	float3 direction;
	float3 energy;
};

struct RayHit {
	float3 position;
	float3 normal;
	float distance;
	float3 albedo;
	float3 specular;
};

struct Sphere {
	float3 position;
	float radius;
	float3 albedo;
	float3 specular;
};

struct Mesh {
	float4x4 localToWorldMatrix;
	float4x4 worldToLocalMatrix;
	int indexOffset;
	int indexCount;
	float3 albedo;
	float3 specular;
};

struct PointLight {
	float3 position;
	float3 color;
};

#endif /* STRUCTS_INCLUDED */