using Unity.Mathematics;
using UnityEngine;

namespace FirstPerson
{
	public class FirstPersonLook : MonoBehaviour
	{
		[SerializeField] private FirstPersonMovement character;
		[SerializeField] private float sensitivity = 2;
		[SerializeField] private float smoothing = 1.5f;

		private float2 _vel;
		private float2 _frameVelocity;
		private IA_FirstPerson _inputAction;
		
		void Start()
		{
			// Lock the mouse cursor to the game screen.
			Cursor.lockState = CursorLockMode.Locked;
			character = GetComponentInParent<FirstPersonMovement>();
			_inputAction = character.InputActions;
		}

		void Update()
		{
#if LEGACY_INPUT_MANAGER && !ENABLE_INPUT_SYSTEM			
			// Get smooth velocity.
			float2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
#else
			float2 mouseDelta = _inputAction.Player.MouseView.ReadValue<Vector2>();
#endif
			float2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity);
			_frameVelocity = math.lerp(_frameVelocity, rawFrameVelocity, 1 / smoothing);
			_vel += _frameVelocity;
			_vel.y = math.clamp(_vel.y, -90, 90);

			// Rotate camera up-down and controller left-right from velocity.
			transform.localRotation = Quaternion.AngleAxis(-_vel.y, Vector3.right);
			character.transform.localRotation = Quaternion.AngleAxis(_vel.x, Vector3.up);
		}
	}
}