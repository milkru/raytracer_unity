using UnityEngine;

public class InputManager : MonoBehaviour {

	#region Public Fields
	[HideInInspector] public float HorizontalMovement;
	[HideInInspector] public float VerticalMovement;
	[HideInInspector] public float HorizontalLook;
	[HideInInspector] public float VerticalLook;
	[HideInInspector] public bool SwitchRendering;
	[HideInInspector] public bool TakeSnapshot;
	#endregion

	#region Unity Built In
	private void Update() {
		HorizontalMovement = Input.GetAxis("Horizontal");
		VerticalMovement = Input.GetAxis("Vertical");
		HorizontalLook = Input.GetAxis("Mouse X");
		VerticalLook = Input.GetAxis("Mouse Y");
		SwitchRendering = Input.GetKeyDown(KeyCode.Space);
		TakeSnapshot = Input.GetKeyDown(KeyCode.C);
	}
	#endregion

}
