using UnityEngine;

[CreateAssetMenu(menuName = "Ray Tracing/Config", fileName = "RayTracingConfig")]
public class RayTracingConfig : ScriptableObject {

	#region Properties
	[Header("Main Assets")]
	public ComputeShader RayTracingShader;
	public Texture SkyboxTexture;
	public Material AntiAliasingMaterial;
	
	[Header("Reflections")]
	public int ReflectionDepth;

	[Header("Ambient Occlusion")]
	public float AmbientOcclusionRange;
	[Range(0, 1)]
	public float AmbientOcclusionIntensity;
	#endregion

}
