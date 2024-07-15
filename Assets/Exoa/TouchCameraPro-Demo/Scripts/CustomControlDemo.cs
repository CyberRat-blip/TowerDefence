using System.Collections;
using System.Collections.Generic;
using Exoa.Cameras;
using Exoa.Common;
using UnityEngine;

[AddComponentMenu("Exoa/Demo/CustomControlDemo")]
public class CustomControlDemo : MonoBehaviour
{
    private CameraPerspective cam;
    private Vector3 manualOffset;
    private Quaternion manualRotation;
    private float manualZoom;
    public float moveSpeed = 10f;
    public float rotationSpeed = 1f;
    public float zoomSpeed = 1f;

    void Start()
    {
        cam = GetComponent<CameraPerspective>();

    }

    // Update is called once per frame
    void Update()
    {
        manualOffset = cam.FinalOffset;
        manualRotation = cam.FinalRotation;
        manualZoom = cam.FinalDistance;

        // Example code to move the camera based on keyboard arrow keys
        float h = BaseTouchInput.KeyboardXAxis();
        float v = BaseTouchInput.KeyboardYAxis();

        manualOffset += transform.right.SetY(0).normalized * h * moveSpeed * Time.unscaledDeltaTime;
        manualOffset += transform.forward.SetY(0).normalized * v * moveSpeed * Time.unscaledDeltaTime;

        // Example code to rotate the camera based on I/O/K/L keyboards keys
        float hRotation = BaseTouchInput.GetKeyIsHeld(KeyCode.I) ? rotationSpeed : (BaseTouchInput.GetKeyIsHeld(KeyCode.O) ? -rotationSpeed : 0);
        float vRotation = BaseTouchInput.GetKeyIsHeld(KeyCode.K) ? rotationSpeed : (BaseTouchInput.GetKeyIsHeld(KeyCode.L) ? -rotationSpeed : 0);
        manualRotation = Quaternion.Euler(0, vRotation, 0) * manualRotation * Quaternion.Euler(hRotation, 0, 0);

        // Example code to change the zoom with R/T keys
        float zoomInput = BaseTouchInput.GetKeyIsHeld(KeyCode.R) ? zoomSpeed : (BaseTouchInput.GetKeyIsHeld(KeyCode.T) ? -zoomSpeed : 0);
        manualZoom += zoomInput;

        // You can apply translation, rotation and zoom at the same time with
        // cam.MoveCameraToInstant(manualOffset, manualZoom, manualRotation);
        // but here we are going to separate them just for the sake of the demo
        // and to show that they can be run at the same time

        if (h != 0 || v != 0)
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

        if (BaseTouchInput.GetKeyIsHeld(KeyCode.Space))
        {
            cam.RotateAround(new Vector2(0.5f, 0.75f), 30f * Time.deltaTime);
        }
    }
}
