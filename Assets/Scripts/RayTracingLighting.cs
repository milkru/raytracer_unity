using UnityEngine;
using System.Runtime.InteropServices;

public class RayTracingLighting : MonoBehaviour
{

	#region PointLight
	public struct PointLightRT {
		public static int GetStride() {
			return Marshal.SizeOf(typeof(PointLightRT));
		}

		public PointLightRT(Vector3 position, Vector3 color) {
			this.position = position;
			this.color = color;
		}

		public Vector3 position;
		public Vector3 color;
	}
	#endregion

}
