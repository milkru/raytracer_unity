using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class RayTracingPrimitives {

	#region Sphere
	public struct SphereRT {
		public static int GetStride() {
			return Marshal.SizeOf(typeof(SphereRT));
		}

		public SphereRT(Vector3 position, float radius, Vector3 albedo, Vector3 specular) {
			this.position = position;
			this.radius = radius;
			this.albedo = albedo;
			this.specular = specular;
		}

		public Vector3 position;
		public float radius;
		public Vector3 albedo;
		public Vector3 specular;
	};
	#endregion

	#region Mesh
	public struct MeshRT {
		public static int GetStride() {
			return Marshal.SizeOf(typeof(MeshRT));
		}

		public MeshRT(Transform trans, ref List<Vector3> vertices, ref List<int> indices, ref List<Vector3> normals, Mesh mesh, Matrix4x4 localToWorldMatrix, Matrix4x4 worldToLocalMatrix, Vector3 albedo, Vector3 specular) {
			var previousVertexCount = vertices.Count;
			vertices.AddRange(mesh.vertices);

			var previousIndexCount = indices.Count;
			var subMeshIndices = mesh.GetIndices(0);
			for (int index = 0; index < subMeshIndices.Length; subMeshIndices[index++] += previousVertexCount);
			indices.AddRange(subMeshIndices);

			var localMeshNormals = mesh.normals;
			var worldMeshNormals = new List<Vector3>();
			for (int index = 0; index < localMeshNormals.Length; ++index) {
				var localNormal = localMeshNormals[index];
				var extendedLocalNormal = new Vector4(localNormal.x, localNormal.y, localNormal.z, 0);
				var extendedWorldNormal = localToWorldMatrix * extendedLocalNormal;
				var worldNormal = new Vector3(extendedWorldNormal.x, extendedWorldNormal.y, extendedWorldNormal.z);
				worldMeshNormals.Add(trans.TransformDirection(localMeshNormals[index]));
			}
			normals.AddRange(worldMeshNormals);

			this.localToWorldMatrix = localToWorldMatrix;
			this.worldToLocalMatrix = worldToLocalMatrix;
			indexOffset = previousIndexCount;
			indexCount = subMeshIndices.Length;
			this.albedo = albedo;
			this.specular = specular;
		}

		public Matrix4x4 localToWorldMatrix;
		public Matrix4x4 worldToLocalMatrix;
		public int indexOffset;
		public int indexCount;
		public Vector3 albedo;
		public Vector3 specular;
	}
	#endregion

}
