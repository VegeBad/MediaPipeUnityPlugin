using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    public float speed = 5;

    [Header("Running (Legacy)")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 9;
    public KeyCode runningKey = KeyCode.LeftShift;

    private Rigidbody _rb;
    /// <summary> Functions to override movement speed. Will use the last added override. </summary>
    public List<System.Func<float>> SpeedOverrides = new List<System.Func<float>>();

    void Start()
    {
        // Get the rigidbody on this.
        _rb = GetComponent<Rigidbody>();
    }
    
#if ENABLE_INPUT_SYSTEM
    public IA_FirstPerson InputActions;

    private void OnEnable()
    {
	    InputActions = new IA_FirstPerson();
	    InputActions.Enable();
    }

    private void OnDisable()
    {
	    InputActions.Disable();
    }
#endif
	
    void FixedUpdate()
    {
#if LEGACY_INPUT_MANAGER && !ENABLE_INPUT_SYSTEM
        // Update IsRunning from input.
        IsRunning = canRun && Input.GetKey(runningKey);
        // Get targetMovingSpeed.
        float targetSpeed = IsRunning ? runSpeed : speed;
#else
	    float targetSpeed = InputActions.Player.Running.ReadValue<float>() > 0 ? runSpeed : speed;
#endif
	    if (SpeedOverrides.Count > 0)
	    {
		    targetSpeed = SpeedOverrides[^1]();
	    }

	    var targetVel = new float2(InputActions.Player.Movement.ReadValue<Vector2>()) * targetSpeed;;
		_rb.linearVelocity = transform.rotation * new Vector3(targetVel.x, _rb.linearVelocity.y, targetVel.y);
    }
}