using Unity.Mathematics;
using UnityEngine;

//[ExecuteInEditMode]
public class Zoom : MonoBehaviour
{
    private Camera _cam;
    public float defaultFOV = 60;
    public float maxZoomFOV = 15;
    [Range(0, 1)]
    public float currentZoom;
    public float sensitivity = 1;
	
    private FirstPersonMovement _character;
    private IA_FirstPerson _inputAction;
    
    void Start()
    {
        // Get the camera on this gameObject and the defaultZoom.
        _cam = GetComponent<Camera>();
        if (_cam)
        {
            defaultFOV = _cam.fieldOfView;
        }
        
        _character = GetComponentInParent<FirstPersonMovement>();
        _inputAction = _character.InputActions;
    }

    void Update()
    {
#if Legacy_Input_Manager && !ENABLE_INPUT_SYSTEM	    
        // Update the currentZoom and the camera's fieldOfView.
        currentZoom += Input.mouseScrollDelta.y * sensitivity * .05f;
#else
	    currentZoom += _inputAction.Player.MouseScroll.ReadValue<Vector2>().y * sensitivity * .05f;
#endif
        currentZoom = math.clamp(currentZoom,0,1);
        _cam.fieldOfView = math.lerp(defaultFOV, maxZoomFOV, currentZoom);
    }
}
