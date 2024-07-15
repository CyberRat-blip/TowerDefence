using System.Collections;
using System.Collections.Generic;
using Exoa.Cameras;
using Exoa.Common;
using UnityEngine;

[AddComponentMenu("Exoa/Demo/CustomControlControllerSupportDemo")]
public class CustomControlControllerSupportDemo : MonoBehaviour
{
    private CameraPerspective cam;
    private Vector3 manualOffset;
    private Quaternion manualRotation;
    private float manualZoom;
    public float moveSpeed = 10f;
    public float rotationSpeed = 1f;
    public float zoomSpeed = 1f;

    private CameraInputActions _inputActions;

    private void OnDestroy()
    {
        if (_inputActions != null)
        {
            _inputActions.Cam.Disable();
        }
    }

    private void Awake()
    {
        _inputActions = new CameraInputActions();
        _inputActions.Cam.Enable();
    }

    void Start()
    {
        cam = GetComponent<CameraPerspective>();
    }

    void Update()
    {
        if (_inputActions == null || _inputActions.Cam.Move == null)
            return;

        manualOffset = cam.FinalOffset;
        manualRotation = cam.FinalRotation;
        manualZoom = cam.FinalDistance;

        // Example code to move the camera based on the left joystick or keyboard arrow keys
        Vector2 move = _inputActions.Cam.Move.ReadValue<Vector2>();
        Vector2 rotate = _inputActions.Cam.Rotate.ReadValue<Vector2>();
        Vector2 zoom = _inputActions.Cam.Zoom.ReadValue<Vector2>();

        manualOffset += transform.right.SetY(0).normalized * move.x * moveSpeed * Time.unscaledDeltaTime;
        manualOffset += transform.forward.SetY(0).normalized * move.y * moveSpeed * Time.unscaledDeltaTime;

        // Example code to rotate the camera based on the right joystick
        float hRotation = rotate.y * rotationSpeed;
        float vRotation = rotate.x * rotationSpeed;
        manualRotation = Quaternion.Euler(0, vRotation, 0) * manualRotation * Quaternion.Euler(hRotation, 0, 0);

        // Example code to change the zoom with R/T keys
        float zoomInput = zoom.y * zoomSpeed;
        manualZoom += zoomInput;

        // You can apply translation, rotation and zoom at the same time with
        // cam.MoveCameraToInstant(manualOffset, manualZoom, manualRotation);
        // but here we are going to separate them just for the sake of the demo
        // and to show that they can be run at the same time

        if (move.x != 0 || move.y != 0)
        {
            cam.MoveCameraToInstant(manualOffset);
        }
        if (hRotation != 0 || vRotation != 0)
        {
            cam.MoveCameraToInstant(manualRotation);
        }
        if (zoomInput != 0)
        {
            cam.MoveCameraToInstant(manualZoom);
        }
    }
}
