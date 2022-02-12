using UnityEngine;
using UnityEngine.UI;

public class RayTracingToggleUI : MonoBehaviour
{

	#region Properties
	public Text RayTracingUI;
	#endregion

	#region Unity Built In
	private void Update() {
		RayTracingUI.text = RayTracingController.toggleRayTracing ? "Ray Tracing On" : "Ray Tracing Off";
	}
	#endregion

}
