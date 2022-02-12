#ifndef INTERSECTIONS_INCLUDED
#define INTERSECTIONS_INCLUDED

#include "Includes/Constants.hlsl"
#include "Includes/Structs.hlsl"

StructuredBuffer<Mesh> _Meshes;
StructuredBuffer<float3> _Vertices;
StructuredBuffer<int> _Indices;
StructuredBuffer<float3> _Normals;

inline void IntersectSphere(Ray ray, inout RayHit rayHit, Sphere sphere) {
	float3 oc = ray.origin - sphere.position;
	float b = 2.0f * dot(oc, ray.direction);
	float c = dot(oc, oc) - sphere.radius * sphere.radius;
	float d = b * b - 4 * c;
	if (d < 0) {
		return;
	}

	float t = (-b - sqrt(d)) / 2.0f;
	if (t > 0 && t < rayHit.distance) {
		rayHit.position = ray.origin + ray.direction * t;
		rayHit.normal = normalize(rayHit.position - sphere.position);
		rayHit.distance = t;
		rayHit.specular = sphere.specular;
		rayHit.albedo = sphere.albedo;
	}
}

inline bool IntersectTriangle(Ray ray, float3 vert0, float3 vert1, float3 vert2,
    inout float t, inout float u, inout float v) {
    float3 edge1 = vert1 - vert0;
    float3 edge2 = vert2 - vert0;
    float3 pvec = cross(ray.direction, edge2);
    float d = dot(edge1, pvec);
    if (d < EPSILON) {
        return false;
	}
	
    float3 tvec = ray.origin - vert0;
    u = dot(tvec, pvec) / d;
    if (u < 0.0 || u > 1.0f) {
        return false;
	}
	
    float3 qvec = cross(tvec, edge1);
    v = dot(ray.direction, qvec) / d;
    if (v < 0.0 || u + v > 1.0f) {
        return false;
	}
	
    t = dot(edge2, qvec) / d;
    return true;
}

inline void IntersectMesh(Ray ray, inout RayHit rayHit, Mesh mesh) {
    uint indexOffsetStart = mesh.indexOffset;
    uint indexOffsetEnd = indexOffsetStart + mesh.indexCount;
	
	Ray tempRay;
	tempRay.origin = (mul(mesh.worldToLocalMatrix, float4(ray.origin, 1))).xyz;
	tempRay.direction = (mul(mesh.worldToLocalMatrix, float4(ray.direction, 0))).xyz;
    
	for (uint index = indexOffsetStart; index < indexOffsetEnd; index += 3)
    {
        float t, u, v;
		int i0 = _Indices[index];
		int i1 = _Indices[index + 1];
		int i2 = _Indices[index + 2];
		float3 v0 = _Vertices[i0];
		float3 v1 = _Vertices[i1];
		float3 v2 = _Vertices[i2];
        if (IntersectTriangle(tempRay, v0, v1, v2, t, u, v)) {
            if (t > 0 && t < rayHit.distance) {
                rayHit.distance = t;
                rayHit.position = ray.origin + t * ray.direction;
				float3 n0 = _Normals[i0];
				float3 n1 = _Normals[i1];
				float3 n2 = _Normals[i2];
                rayHit.normal = u * n0 + v * n1 + (1 - u - v) * n2;
                rayHit.albedo = mesh.albedo;
				rayHit.specular = mesh.specular;
            }
        }
    }
}

#endif /* INTERSECTIONS_INCLUDED */