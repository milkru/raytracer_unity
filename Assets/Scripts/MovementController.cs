using UnityEngine;

[RequireComponent(typeof(InputManager))]
public class MovementController : MonoBehaviour {

	#region Properties
	[Header("Configurations")]
	public MovementConfig Config;
	#endregion

	#region Fields
	private InputManager inputManager;
	private Vector2 rotation;
	#endregion

	#region Unity Built In
	private void Awake() {
		inputManager = GetComponent<InputManager>();
	}

	private void Update() {
		rotation.y += inputManager.HorizontalLook;
		rotation.x += -inputManager.VerticalLook;
		transform.eulerAngles = rotation * Config.LookSensitivity;

		Vector3 velocity = new Vector3(inputManager.HorizontalMovement, 0, inputManager.VerticalMovement);
		velocity *= Config.MaxSpeed * Time.deltaTime;
		transform.Translate(velocity);
	}
	#endregion

}
