using System.Collections;
using System.Collections.Generic;
using Exoa.Cameras;
using UnityEngine;

[AddComponentMenu("Exoa/Demo/OrbitDemo")]
public class OrbitDemo : MonoBehaviour
{
    public Transform targetObj;
    private CameraBase cam;

    void Start()
    {
        cam = GetComponent<CameraBase>();
        cam.DisableTranslations = true;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 manualOffset = cam.FinalOffset;
        Quaternion manualRotation = cam.FinalRotation;
        float manualZoom = cam.FinalDistance;

        cam.SetGroundHeight(targetObj.position.y);
        cam.MoveCameraToInstant(targetObj.position);
    }
}
