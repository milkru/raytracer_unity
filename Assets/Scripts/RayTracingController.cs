using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEditor;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(InputManager))]
public class RayTracingController : MonoBehaviour {

	#region Properties
	[Header("Configurations")]
	public RayTracingConfig Config;
	#endregion

	#region Fields
	private InputManager inputManager;

	private Camera mainCamera;
	private RenderTexture renderTexture;

	private ComputeBuffer cubeBuffer;
	private ComputeBuffer sphereBuffer;
	private ComputeBuffer cylinderBuffer;
	private ComputeBuffer planeBuffer;
	private ComputeBuffer meshBuffer;
	private ComputeBuffer vertexBuffer;
	private ComputeBuffer indexBuffer;
	private ComputeBuffer normalBuffer;
	private ComputeBuffer pointLightBuffer;

	private int threadGroupsX;
	private int threadGroupsY;

	public static bool toggleRayTracing;
	private bool takeSnapshot;
	
	private int sampleCount;
	#endregion

	#region Compute Buffer
	
	private void CreateComputeBuffer<T>(ref ComputeBuffer buffer, List<T> data, int stride) where T : struct {
		buffer?.Release();
		buffer = null;

#pragma warning disable
		if (data.Count > 0) {
			buffer = new ComputeBuffer(data.Count, stride);
			buffer.SetData(data);
		} else {
			buffer = new ComputeBuffer(1, stride);
		}
#pragma warning restore
	}
	
	private void SetComputeBuffer(string name, ComputeBuffer buffer) {
		if (buffer != null) {
			Config.RayTracingShader.SetBuffer(0, name, buffer);
		}
	}

	private void SafeReleaseComputeBuffer(ComputeBuffer buffer) {
		buffer?.Release();
	}
	#endregion

	#region Color Conversion
	private Vector3 ColorToVector3(Color color) {
		return new Vector3(color.r, color.g, color.b);
	}

	private Vector3 ColorToVector3(Color color, float intensity) {
		return new Vector3(color.r, color.g, color.b) * intensity;
	}
	#endregion

	#region Initialization
	private void InitializeObjects() {
		var rayTracingTags = FindObjectsOfType<RayTracingTag>();
		var spheres = new List<RayTracingPrimitives.SphereRT>();
		var meshes = new List<RayTracingPrimitives.MeshRT>();
		var vertices = new List<Vector3>();
		var indices = new List<int>();
		var normals = new List<Vector3>();

		foreach (var rayTracingTag in rayTracingTags) {
			var rayTracingObject = rayTracingTag.gameObject;
			var transform = rayTracingObject.GetComponent<Transform>();
			var meshFilter = rayTracingObject.GetComponent<MeshFilter>();
			var mesh = meshFilter.sharedMesh;
			var meshRenderer = rayTracingObject.GetComponent<MeshRenderer>();
			var material = meshRenderer.material;
			var albedoColor = material.GetColor("_Color");
			var albedo = ColorToVector3(albedoColor);
			var specularColor = material.GetColor("_SpecColor");
			var specular = ColorToVector3(specularColor);
			albedo *= (1.0f - specularColor.grayscale);

			switch (rayTracingObject.tag) {
				case "Sphere":
					spheres.Add(new RayTracingPrimitives.SphereRT(transform.position, transform.localScale.x / 2, albedo, specular));
					break;
				default:
					meshes.Add(new RayTracingPrimitives.MeshRT(transform, ref vertices, ref indices, ref normals, mesh, transform.localToWorldMatrix, transform.worldToLocalMatrix, albedo, specular));
					break;
			}
		}

		CreateComputeBuffer(ref sphereBuffer, spheres, RayTracingPrimitives.SphereRT.GetStride());
		SetComputeBuffer("_Spheres", sphereBuffer);
		CreateComputeBuffer(ref meshBuffer, meshes, RayTracingPrimitives.MeshRT.GetStride());
		SetComputeBuffer("_Meshes", meshBuffer);
		CreateComputeBuffer(ref vertexBuffer, vertices, Marshal.SizeOf(typeof(Vector3)));
		SetComputeBuffer("_Vertices", vertexBuffer);
		CreateComputeBuffer(ref indexBuffer, indices, Marshal.SizeOf(typeof(int)));
		SetComputeBuffer("_Indices", indexBuffer);
		CreateComputeBuffer(ref normalBuffer, normals, Marshal.SizeOf(typeof(Vector3)));
		SetComputeBuffer("_Normals", normalBuffer);
	}
	
	private void InitializeLightning() {
		var lights = FindObjectsOfType<Light>();
		var pointLights = new List<RayTracingLighting.PointLightRT>();

		foreach (var light in lights) {
			switch (light.type) {
				case LightType.Directional:
					Vector3 DirectionalLightDirection = light.transform.forward.normalized;
					Config.RayTracingShader.SetVector("_DirectionalLightDirection", DirectionalLightDirection);
					Vector3 DirectionalLightColor = ColorToVector3(light.color, light.intensity);
					Config.RayTracingShader.SetVector("_DirectionalLightColor", DirectionalLightColor);
					break;
				case LightType.Point:
					Vector3 PointLightPosition = light.transform.position;
					Vector3 PointLightColor = ColorToVector3(light.color, light.intensity);
					pointLights.Add(new RayTracingLighting.PointLightRT(PointLightPosition, PointLightColor));
					break;
			}
		}

		CreateComputeBuffer(ref pointLightBuffer, pointLights, RayTracingLighting.PointLightRT.GetStride());
		SetComputeBuffer("_PointLights", pointLightBuffer);

		Config.RayTracingShader.SetTexture(0, "_SkyboxTexture", Config.SkyboxTexture);
		Config.RayTracingShader.SetInt("_ReflectionDepth", Config.ReflectionDepth);
		Config.RayTracingShader.SetVector("_AmbientLight", ColorToVector3(RenderSettings.ambientLight, RenderSettings.ambientIntensity));
		Config.RayTracingShader.SetFloat("_AmbientOcclusionRange", Config.AmbientOcclusionRange);
		Config.RayTracingShader.SetFloat("_AmbientOcclusionIntensity", Config.AmbientOcclusionIntensity);
	}

	private void InitializeScene() {
		InitializeObjects();
		InitializeLightning();
	}
	#endregion

	#region Unity Built In
	private void OnDestroy() {
		SafeReleaseComputeBuffer(sphereBuffer);
		SafeReleaseComputeBuffer(meshBuffer);
		SafeReleaseComputeBuffer(vertexBuffer);
		SafeReleaseComputeBuffer(indexBuffer);
		SafeReleaseComputeBuffer(pointLightBuffer);
	}

	private void Awake() {
		if (renderTexture != null) {
			renderTexture.Release();
		}

		renderTexture = new RenderTexture(Screen.width, Screen.height, 0,
			RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
		renderTexture.enableRandomWrite = true;
		renderTexture.Create();
		Config.RayTracingShader.SetTexture(0, "_Result", renderTexture);

		inputManager = GetComponent<InputManager>();

		mainCamera = GetComponent<Camera>();

		Cursor.visible = false;

		threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
		threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);

		InitializeScene();
	}

	private void Update() {
		if (inputManager.TakeSnapshot) {
			takeSnapshot = true;
		}
		
		if (inputManager.SwitchRendering) {
			toggleRayTracing = !toggleRayTracing;
			sampleCount = 0;
		} else if (mainCamera.transform.hasChanged) {
			mainCamera.transform.hasChanged = false;
			sampleCount = 0;
		} else {
			++sampleCount;
		}
	}

	private void SaveRenderTextureAsPNG(RenderTexture rt) {
		RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        RenderTexture.active = null;

        byte[] bytes;
        bytes = tex.EncodeToPNG();
        
        string path = "Image" + Random.Range(0, 1000) + ".png";
        System.IO.File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path);
        Debug.Log("Saved to " + path);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination) {
		if (toggleRayTracing) {
			Config.RayTracingShader.SetMatrix("_CameraToWorld", mainCamera.cameraToWorldMatrix);
			Config.RayTracingShader.SetMatrix("_CameraInverseProjection", mainCamera.projectionMatrix.inverse);
			Config.RayTracingShader.SetVector("_RandomOffset", new Vector2(Random.Range(0.25f, 0.75f), Random.Range(0.0f, 1.0f)));
			Config.AntiAliasingMaterial.SetInt("_SampleCount", sampleCount);

			Config.RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
			
			if (takeSnapshot) {
				takeSnapshot = false;
				SaveRenderTextureAsPNG(renderTexture);
			}
			
			Graphics.Blit(renderTexture, destination);
		} else {
			if (takeSnapshot) {
				takeSnapshot = false;
				SaveRenderTextureAsPNG(source);
			}
			
			Graphics.Blit(source, destination);
		}
	}
	#endregion

}
