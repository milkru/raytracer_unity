using UnityEngine;

[CreateAssetMenu(menuName = "Movement/Config", fileName = "MovementConfig")]
public class MovementConfig : ScriptableObject {

	#region Properties
	[Header("Main Parameters")]
	public float MaxSpeed;
	public float LookSensitivity;
	#endregion

}
