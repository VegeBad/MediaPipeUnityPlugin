using UnityEngine;

public class Jump : MonoBehaviour
{
    private Rigidbody _rb;
    public float jumpStrength = 2;
    public event System.Action Jumped;

    [SerializeField, Tooltip("Prevents jumping when the transform is in mid-air.")]
    GroundCheck groundCheck;
	
    private FirstPersonMovement _character;
    private IA_FirstPerson _inputAction;
    
    void Reset()
    {
        // Try to get groundCheck.
        groundCheck = GetComponentInChildren<GroundCheck>();
    }

    void Start()
    {
        // Get rigidbody.
        _rb = GetComponent<Rigidbody>();
        
        _character = GetComponentInParent<FirstPersonMovement>();
        _inputAction = _character.InputActions;
    }

    void LateUpdate()
    {
#if Legacy_Input_Manager && !ENABLE_INPUT_SYSTEM	    
        // Jump when the Jump button is pressed and we are on the ground.
        if (Input.GetButtonDown("Jump") && (!groundCheck || groundCheck.isGrounded))
        {
            rigidbody.AddForce(Vector3.up * (100 * jumpStrength));
            Jumped?.Invoke();
        }
#else
	    if (!(_inputAction.Player.Jump.ReadValue<float>() >= .5f) || (groundCheck && !groundCheck.isGrounded)) return;
	    _rb.AddForce(Vector3.up * (100 * jumpStrength));
	    Jumped?.Invoke();
#endif
    }
}
